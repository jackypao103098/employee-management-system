package com.jackypao.customer;

import com.jackypao.exception.DuplicateResourceException;
import com.jackypao.exception.RequestValidationException;
import com.jackypao.exception.ResourceNotFoundException;
import com.jackypao.s3.S3Buckets;
import com.jackypao.s3.S3Service;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.ArgumentCaptor;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;
import org.springframework.mock.web.MockMultipartFile;
import org.springframework.security.crypto.password.PasswordEncoder;
import org.springframework.web.multipart.MultipartFile;

import java.io.IOException;
import java.util.Optional;

import static org.assertj.core.api.Assertions.assertThat;
import static org.assertj.core.api.Assertions.assertThatThrownBy;
import static org.mockito.Mockito.*;

@ExtendWith(MockitoExtension.class)
class EmployeeServiceTest {

    @Mock
    private EmployeeDao employeeDao;
    @Mock
    private PasswordEncoder passwordEncoder;
    @Mock
    private S3Service s3Service;
    @Mock
    private S3Buckets s3Buckets;
    private EmployeeService underTest;
    private final EmployeeDTOMapper employeeDTOMapper = new EmployeeDTOMapper();

    @BeforeEach
    void setUp() {
        underTest = new EmployeeService(employeeDao, employeeDTOMapper, passwordEncoder, s3Service, s3Buckets);
    }

    @Test
    void getAllEmployees() {
        // When
        underTest.getAllEmployees();

        // Then
        verify(employeeDao).selectAllEmployees();
    }

    @Test
    void canGetCustomer() {
        // Given
        int id = 10;
        Employee employee = new Employee(id, "Alex", "alex@gmail.com", "password", 19, Gender.MALE);
        when(employeeDao.selectEmployeeById(id)).thenReturn(Optional.of(employee));

        EmployeeDTO expected = employeeDTOMapper.apply(employee);

        // When
        EmployeeDTO actual = underTest.getCustomer(id);

        // Then
        assertThat(actual).isEqualTo(expected);
    }

    @Test
    void willThrowWhenGetCustomerReturnEmptyOptional() {
        // Given
        int id = 10;

        when(employeeDao.selectEmployeeById(id)).thenReturn(Optional.empty());

        // When
        // Then
        assertThatThrownBy(() -> underTest.getCustomer(id)).isInstanceOf(ResourceNotFoundException.class).hasMessage("employee with id [%s] not found".formatted(id));
    }

    @Test
    void addEmployee() {
        // Given
        String email = "alex@gmail.com";

        when(employeeDao.existsEmployeeWithEmail(email)).thenReturn(false);

        EmployeeRegistrationRequest request = new EmployeeRegistrationRequest("Alex", email, "password", 19, Gender.MALE);

        String passwordHash = "¢5554ml;f;lsd";

        when(passwordEncoder.encode(request.password())).thenReturn(passwordHash);

        // When
        underTest.addEmployee(request);

        // Then
        ArgumentCaptor<Employee> customerArgumentCaptor = ArgumentCaptor.forClass(Employee.class);

        verify(employeeDao).insertEmployee(customerArgumentCaptor.capture());

        Employee capturedCustomer = customerArgumentCaptor.getValue();

        assertThat(capturedCustomer.getId()).isNull();
        assertThat(capturedCustomer.getName()).isEqualTo(request.name());
        assertThat(capturedCustomer.getEmail()).isEqualTo(request.email());
        assertThat(capturedCustomer.getAge()).isEqualTo(request.age());
        assertThat(capturedCustomer.getPassword()).isEqualTo(passwordHash);
    }

    @Test
    void willThrowWhenEmailExistsWhileAddingACustomer() {
        // Given
        String email = "alex@gmail.com";

        when(employeeDao.existsEmployeeWithEmail(email)).thenReturn(true);

        EmployeeRegistrationRequest request = new EmployeeRegistrationRequest("Alex", email, "password", 19, Gender.MALE);

        // When
        assertThatThrownBy(() -> underTest.addEmployee(request)).isInstanceOf(DuplicateResourceException.class).hasMessage("email already taken");

        // Then
        verify(employeeDao, never()).insertEmployee(any());
    }

    @Test
    void deleteEmployeeById() {
        // Given
        int id = 10;

        when(employeeDao.existsEmployeeById(id)).thenReturn(true);

        // When
        underTest.deleteEmployeeById(id);
        // Then
        verify(employeeDao).deleteEmployeeById(id);
    }

    @Test
    void willThrowDeleteCustomerByIdNotExists() {
        // Given
        int id = 10;

        when(employeeDao.existsEmployeeById(id)).thenReturn(false);

        // When
        assertThatThrownBy(() -> underTest.deleteEmployeeById(id)).isInstanceOf(ResourceNotFoundException.class).hasMessage("employee with id [%s] not found".formatted(id));

        // Then
        verify(employeeDao, never()).deleteEmployeeById(id);
    }

    @Test
    void canUpdateAllCustomersProperties() {
        // Given
        int id = 10;
        Employee employee = new Employee(id, "Alex", "alex@gmail.com", "password", 19, Gender.MALE);
        when(employeeDao.selectEmployeeById(id)).thenReturn(Optional.of(employee));

        String newEmail = "alexandro@jackypao.com";

        EmployeeUpdateRequest updateRequest = new EmployeeUpdateRequest("Alexandro", newEmail, 23);

        when(employeeDao.existsEmployeeWithEmail(newEmail)).thenReturn(false);

        // When
        underTest.updateCustomer(id, updateRequest);

        // Then
        ArgumentCaptor<Employee> customerArgumentCaptor = ArgumentCaptor.forClass(Employee.class);

        verify(employeeDao).updateCustomer(customerArgumentCaptor.capture());
        Employee capturedCustomer = customerArgumentCaptor.getValue();

        assertThat(capturedCustomer.getName()).isEqualTo(updateRequest.name());
        assertThat(capturedCustomer.getEmail()).isEqualTo(updateRequest.email());
        assertThat(capturedCustomer.getAge()).isEqualTo(updateRequest.age());
    }

    @Test
    void canUpdateOnlyCustomerName() {
        // Given
        int id = 10;
        Employee employee = new Employee(id, "Alex", "alex@gmail.com", "password", 19, Gender.MALE);
        when(employeeDao.selectEmployeeById(id)).thenReturn(Optional.of(employee));

        EmployeeUpdateRequest updateRequest = new EmployeeUpdateRequest("Alexandro", null, null);

        // When
        underTest.updateCustomer(id, updateRequest);

        // Then
        ArgumentCaptor<Employee> customerArgumentCaptor = ArgumentCaptor.forClass(Employee.class);

        verify(employeeDao).updateCustomer(customerArgumentCaptor.capture());
        Employee capturedCustomer = customerArgumentCaptor.getValue();

        assertThat(capturedCustomer.getName()).isEqualTo(updateRequest.name());
        assertThat(capturedCustomer.getAge()).isEqualTo(employee.getAge());
        assertThat(capturedCustomer.getEmail()).isEqualTo(employee.getEmail());
    }

    @Test
    void canUpdateOnlyCustomerEmail() {
        // Given
        int id = 10;
        Employee employee = new Employee(id, "Alex", "alex@gmail.com", "password", 19, Gender.MALE);
        when(employeeDao.selectEmployeeById(id)).thenReturn(Optional.of(employee));

        String newEmail = "alexandro@jackypao.com";

        EmployeeUpdateRequest updateRequest = new EmployeeUpdateRequest(null, newEmail, null);

        when(employeeDao.existsEmployeeWithEmail(newEmail)).thenReturn(false);

        // When
        underTest.updateCustomer(id, updateRequest);

        // Then
        ArgumentCaptor<Employee> customerArgumentCaptor = ArgumentCaptor.forClass(Employee.class);

        verify(employeeDao).updateCustomer(customerArgumentCaptor.capture());
        Employee capturedCustomer = customerArgumentCaptor.getValue();

        assertThat(capturedCustomer.getName()).isEqualTo(employee.getName());
        assertThat(capturedCustomer.getAge()).isEqualTo(employee.getAge());
        assertThat(capturedCustomer.getEmail()).isEqualTo(newEmail);
    }

    @Test
    void canUpdateOnlyCustomerAge() {
        // Given
        int id = 10;
        Employee employee = new Employee(id, "Alex", "alex@gmail.com", "password", 19, Gender.MALE);
        when(employeeDao.selectEmployeeById(id)).thenReturn(Optional.of(employee));

        EmployeeUpdateRequest updateRequest = new EmployeeUpdateRequest(null, null, 22);

        // When
        underTest.updateCustomer(id, updateRequest);

        // Then
        ArgumentCaptor<Employee> customerArgumentCaptor = ArgumentCaptor.forClass(Employee.class);

        verify(employeeDao).updateCustomer(customerArgumentCaptor.capture());
        Employee capturedCustomer = customerArgumentCaptor.getValue();

        assertThat(capturedCustomer.getName()).isEqualTo(employee.getName());
        assertThat(capturedCustomer.getAge()).isEqualTo(updateRequest.age());
        assertThat(capturedCustomer.getEmail()).isEqualTo(employee.getEmail());
    }

    @Test
    void willThrowWhenTryingToUpdateCustomerEmailWhenAlreadyTaken() {
        // Given
        int id = 10;
        Employee employee = new Employee(id, "Alex", "alex@gmail.com", "password", 19, Gender.MALE);
        when(employeeDao.selectEmployeeById(id)).thenReturn(Optional.of(employee));

        String newEmail = "alexandro@jackypao.com";

        EmployeeUpdateRequest updateRequest = new EmployeeUpdateRequest(null, newEmail, null);

        when(employeeDao.existsEmployeeWithEmail(newEmail)).thenReturn(true);

        // When
        assertThatThrownBy(() -> underTest.updateCustomer(id, updateRequest)).isInstanceOf(DuplicateResourceException.class).hasMessage("email already taken");

        // Then
        verify(employeeDao, never()).updateCustomer(any());
    }

    @Test
    void willThrowWhenCustomerUpdateHasNoChanges() {
        // Given
        int id = 10;
        Employee employee = new Employee(id, "Alex", "alex@gmail.com", "password", 19, Gender.MALE);
        when(employeeDao.selectEmployeeById(id)).thenReturn(Optional.of(employee));

        EmployeeUpdateRequest updateRequest = new EmployeeUpdateRequest(employee.getName(), employee.getEmail(), employee.getAge());

        // When
        assertThatThrownBy(() -> underTest.updateCustomer(id, updateRequest)).isInstanceOf(RequestValidationException.class).hasMessage("no data changes found");

        // Then
        verify(employeeDao, never()).updateCustomer(any());
    }

    @Test
    void canUploadProfileImage() {
        // Given
        int customerId = 10;

        when(employeeDao.existsEmployeeById(customerId)).thenReturn(true);

        byte[] bytes = "Hello World".getBytes();

        MultipartFile multipartFile = new MockMultipartFile("file", bytes);

        String bucket = "employee-bucket";
        when(s3Buckets.getCustomer()).thenReturn(bucket);

        // When
        underTest.uploadEmployeeProfileImage(customerId, multipartFile);

        // Then
        ArgumentCaptor<String> profileImageIdArgumentCaptor = ArgumentCaptor.forClass(String.class);

        verify(employeeDao).updateEmployeeProfileImageId(profileImageIdArgumentCaptor.capture(), eq(customerId));

        verify(s3Service).putObject(bucket, "profile-images/%s/%s".formatted(customerId, profileImageIdArgumentCaptor.getValue()), bytes);
    }

    @Test
    void cannotUploadProfileImageWhenCustomerDoesNotExists() {
        // Given
        int customerId = 10;

        when(employeeDao.existsEmployeeById(customerId)).thenReturn(false);

        // When
        assertThatThrownBy(() -> underTest.uploadEmployeeProfileImage(
                customerId, mock(MultipartFile.class))
        )
                .isInstanceOf(ResourceNotFoundException.class)
                .hasMessage("employee with id [" + customerId + "] not found");

        // Then
        verify(employeeDao).existsEmployeeById(customerId);
        verifyNoMoreInteractions(employeeDao);
        verifyNoInteractions(s3Buckets);
        verifyNoInteractions(s3Service);
    }

    @Test
    void cannotUploadProfileImageWhenExceptionIsThrown() throws IOException {
        // Given
        int customerId = 10;

        when(employeeDao.existsEmployeeById(customerId)).thenReturn(true);

        MultipartFile multipartFile = mock(MultipartFile.class);
        when(multipartFile.getBytes()).thenThrow(IOException.class);

        String bucket = "employee-bucket";
        when(s3Buckets.getCustomer()).thenReturn(bucket);

        // When
        assertThatThrownBy(() -> {
            underTest.uploadEmployeeProfileImage(customerId, multipartFile);
        }).isInstanceOf(RuntimeException.class)
                .hasMessage("failed to upload profile image")
                .hasRootCauseInstanceOf(IOException.class);

        // Then
        verify(employeeDao, never()).updateEmployeeProfileImageId(any(), any());
    }

    @Test
    void canDownloadProfileImage() {
        // Given
        int customerId = 10;
        String profileImageId = "2222";
        Employee employee = new Employee(
                customerId,
                "Alex",
                "alex@gmail.com",
                "password",
                19,
                Gender.MALE,
                profileImageId
        );
        when(employeeDao.selectEmployeeById(customerId)).thenReturn(Optional.of(employee));

        String bucket = "employee-bucket";
        when(s3Buckets.getCustomer()).thenReturn(bucket);

        byte[] expectedImage = "image".getBytes();

        when(s3Service.getObject(
                bucket,
                "profile-images/%s/%s".formatted(customerId, profileImageId))
        ).thenReturn(expectedImage);

        // When
        byte[] actualImage = underTest.getEmployeeProfileImage(customerId);

        // Then
        assertThat(actualImage).isEqualTo(expectedImage);
    }

    @Test
    void cannotDownloadWhenNoProfileImageId() {
        // Given
        int customerId = 10;
        Employee employee = new Employee(
                customerId,
                "Alex",
                "alex@gmail.com",
                "password",
                19,
                Gender.MALE
        );

        when(employeeDao.selectEmployeeById(customerId)).thenReturn(Optional.of(employee));

        // When
        // Then
        assertThatThrownBy(() -> underTest.getEmployeeProfileImage(customerId))
                .isInstanceOf(ResourceNotFoundException.class)
                .hasMessage("employee with id [%s] profile image not found".formatted(customerId));

        verifyNoInteractions(s3Buckets);
        verifyNoInteractions(s3Service);
    }

    @Test
    void cannotDownloadProfileImageWhenCustomerDoesNotExists() {
        // Given
        int customerId = 10;

        when(employeeDao.selectEmployeeById(customerId)).thenReturn(Optional.empty());

        // When
        // Then
        assertThatThrownBy(() -> underTest.getEmployeeProfileImage(customerId))
                .isInstanceOf(ResourceNotFoundException.class)
                .hasMessage("employee with id [%s] not found".formatted(customerId));

        verifyNoInteractions(s3Buckets);
        verifyNoInteractions(s3Service);
    }
}