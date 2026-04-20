package com.jackypao.customer;

import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;
import org.springframework.stereotype.Repository;

import java.util.List;
import java.util.Optional;

@Repository("jpa")
public class EmployeeJPADataAccessService implements EmployeeDao {

    private final EmployeeRepository employeeRepository;

    public EmployeeJPADataAccessService(EmployeeRepository employeeRepository) {
        this.employeeRepository = employeeRepository;
    }

    @Override
    public List<Employee> selectAllEmployees() {
        Page<Employee> page = employeeRepository.findAll(Pageable.ofSize(1000));
        return page.getContent();
    }

    @Override
    public Optional<Employee> selectEmployeeById(Integer id) {
        return employeeRepository.findById(id);
    }

    @Override
    public void insertEmployee(Employee employee) {
        employeeRepository.save(employee);
    }

    @Override
    public boolean existsEmployeeWithEmail(String email) {
        return employeeRepository.existsEmployeeByEmail(email);
    }

    @Override
    public boolean existsEmployeeById(Integer id) {
        return employeeRepository.existsEmployeeById(id);
    }

    @Override
    public void deleteEmployeeById(Integer employeeId) {
        employeeRepository.deleteById(employeeId);
    }

    @Override
    public void updateEmployee(Employee update) {
        employeeRepository.save(update);
    }

    @Override
    public Optional<Employee> selectUserByEmail(String email) {
        return employeeRepository.findEmployeeByEmail(email);
    }

    @Override
    public void updateEmployeeProfileImageId(String profileImageId,
                                             Integer employeeId) {
        employeeRepository.updateProfileImageId(profileImageId, employeeId);
    }

}
