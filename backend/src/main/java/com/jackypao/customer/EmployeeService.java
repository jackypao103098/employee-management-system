package com.jackypao.customer;

import com.jackypao.exception.DuplicateResourceException;
import com.jackypao.exception.RequestValidationException;
import com.jackypao.exception.ResourceNotFoundException;
import com.jackypao.s3.S3Buckets;
import com.jackypao.s3.S3Service;
import org.apache.commons.lang3.StringUtils;
import org.springframework.beans.factory.annotation.Qualifier;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.stereotype.Service;
import org.springframework.web.multipart.MultipartFile;

import java.io.IOException;
import java.util.List;
import java.util.UUID;
import java.util.stream.Collectors;

@Service
public class EmployeeService {

    private final EmployeeDao employeeDao;
    private final EmployeeDTOMapper employeeDTOMapper;
    private final PasswordEncoder passwordEncoder;
    private final S3Service s3Service;
    private final S3Buckets s3Buckets;

    public EmployeeService(@Qualifier("jpa") EmployeeDao employeeDao,
                           EmployeeDTOMapper employeeDTOMapper,
                           PasswordEncoder passwordEncoder,
                           S3Service s3Service,
                           S3Buckets s3Buckets) {
        this.employeeDao = employeeDao;
        this.employeeDTOMapper = employeeDTOMapper;
        this.passwordEncoder = passwordEncoder;
        this.s3Service = s3Service;
        this.s3Buckets = s3Buckets;
    }

    public List<EmployeeDTO> getAllEmployees() {
        return employeeDao.selectAllEmployees()
                .stream()
                .map(employeeDTOMapper)
                .collect(Collectors.toList());
    }

    public EmployeeDTO getEmployee(Integer id) {
        return employeeDao.selectEmployeeById(id)
                .map(employeeDTOMapper)
                .orElseThrow(() -> new ResourceNotFoundException(
                        "employee with id [%s] not found".formatted(id)
                ));
    }

    public void addEmployee(EmployeeRegistrationRequest employeeRegistrationRequest) {
        // check if email exists
        String email = employeeRegistrationRequest.email();
        if (employeeDao.existsEmployeeWithEmail(email)) {
            throw new DuplicateResourceException(
                    "email already taken"
            );
        }

        // add
        Employee employee = new Employee(
                employeeRegistrationRequest.name(),
                employeeRegistrationRequest.email(),
                passwordEncoder.encode(employeeRegistrationRequest.password()),
                employeeRegistrationRequest.age(),
                employeeRegistrationRequest.gender());

        employeeDao.insertEmployee(employee);
    }

    public void deleteEmployeeById(Integer employeeId) {
        checkIfEmployeeExistsOrThrow(employeeId);
        employeeDao.deleteEmployeeById(employeeId);
    }

    private void checkIfEmployeeExistsOrThrow(Integer employeeId) {
        if (!employeeDao.existsEmployeeById(employeeId)) {
            throw new ResourceNotFoundException(
                    "employee with id [%s] not found".formatted(employeeId)
            );
        }
    }

    public void updateEmployee(Integer employeeId,
                               EmployeeUpdateRequest updateRequest) {
        Employee employee = employeeDao.selectEmployeeById(employeeId)
                .orElseThrow(() -> new ResourceNotFoundException(
                        "employee with id [%s] not found".formatted(employeeId)
                ));

        boolean changes = false;

        if (updateRequest.name() != null && !updateRequest.name().equals(employee.getName())) {
            employee.setName(updateRequest.name());
            changes = true;
        }

        if (updateRequest.age() != null && !updateRequest.age().equals(employee.getAge())) {
            employee.setAge(updateRequest.age());
            changes = true;
        }

        if (updateRequest.email() != null && !updateRequest.email().equals(employee.getEmail())) {
            if (employeeDao.existsEmployeeWithEmail(updateRequest.email())) {
                throw new DuplicateResourceException(
                        "email already taken"
                );
            }
            employee.setEmail(updateRequest.email());
            changes = true;
        }

        if (!changes) {
           throw new RequestValidationException("no data changes found");
        }

        employeeDao.updateEmployee(employee);
    }

    public void uploadEmployeeProfileImage(Integer employeeId,
                                           MultipartFile file) {
        checkIfEmployeeExistsOrThrow(employeeId);
        String profileImageId = UUID.randomUUID().toString();
        try {
            s3Service.putObject(
                    s3Buckets.getCustomer(),
                    "profile-images/%s/%s".formatted(employeeId, profileImageId),
                    file.getBytes()
            );
        } catch (IOException e) {
            throw new RuntimeException("failed to upload profile image", e);
        }
        employeeDao.updateEmployeeProfileImageId(profileImageId, employeeId);
    }

    public byte[] getEmployeeProfileImage(Integer employeeId) {
        var employee = employeeDao.selectEmployeeById(employeeId)
                .map(employeeDTOMapper)
                .orElseThrow(() -> new ResourceNotFoundException(
                        "employee with id [%s] not found".formatted(employeeId)
                ));

        if (StringUtils.isBlank(employee.profileImageId())) {
            throw new ResourceNotFoundException(
                    "employee with id [%s] profile image not found".formatted(employeeId));
        }

        byte[] profileImage = s3Service.getObject(
                s3Buckets.getCustomer(),
                "profile-images/%s/%s".formatted(employeeId, employee.profileImageId())
        );
        return profileImage;
    }
}
