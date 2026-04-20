package com.jackypao.customer;

import java.util.List;
import org.junit.jupiter.api.AfterEach;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.mockito.ArgumentCaptor;
import org.mockito.Mock;
import org.mockito.MockitoAnnotations;
import org.springframework.data.domain.Page;
import org.springframework.data.domain.Pageable;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.verify;
import static org.mockito.Mockito.when;

class EmployeeJPADataAccessServiceTest {

    private EmployeeJPADataAccessService underTest;
    private AutoCloseable autoCloseable;
    @Mock private EmployeeRepository employeeRepository;

    @BeforeEach
    void setUp() {
        autoCloseable = MockitoAnnotations.openMocks(this);
        underTest = new EmployeeJPADataAccessService(employeeRepository);
    }

    @AfterEach
    void tearDown() throws Exception {
        autoCloseable.close();
    }

    @Test
    void selectAllEmployees() {
        Page<Employee> page = mock(Page.class);
        List<Employee> employees = List.of(new Employee());
        when(page.getContent()).thenReturn(employees);
        when(employeeRepository.findAll(any(Pageable.class))).thenReturn(page);
        // When
        List<Employee> expected = underTest.selectAllEmployees();

        // Then
        assertThat(expected).isEqualTo(employees);
        ArgumentCaptor<Pageable> pageArgumentCaptor = ArgumentCaptor.forClass(Pageable.class);
        verify(employeeRepository).findAll(pageArgumentCaptor.capture());
        assertThat(pageArgumentCaptor.getValue()).isEqualTo(Pageable.ofSize(1000));
    }

    @Test
    void selectEmployeeById() {
        // Given
        int id = 1;

        // When
        underTest.selectEmployeeById(id);

        // Then
        verify(employeeRepository).findById(id);
    }

    @Test
    void insertEmployee() {
        // Given
        Employee employee = new Employee(
                1, "Ali", "ali@gmail.com", "password", 2,
                Gender.MALE);

        // When
        underTest.insertEmployee(employee);

        // Then
        verify(employeeRepository).save(employee);
    }

    @Test
    void existsEmployeeWithEmail() {
        // Given
        String email = "foo@gmail.com";

        // When
        underTest.existsEmployeeWithEmail(email);

        // Then
        verify(employeeRepository).existsEmployeeByEmail(email);
    }

    @Test
    void existsEmployeeById() {
        // Given
        int id = 1;

        // When
        underTest.existsEmployeeById(id);

        // Then
        verify(employeeRepository).existsEmployeeById(id);
    }

    @Test
    void deleteEmployeeById() {
        // Given
        int id = 1;

        // When
        underTest.deleteEmployeeById(id);

        // Then
        verify(employeeRepository).deleteById(id);
    }

    @Test
    void updateCustomer() {
        // Given
        Employee employee = new Employee(
                1, "Ali", "ali@gmail.com", "password", 2,
                Gender.MALE);

        // When
        underTest.updateEmployee(employee);

        // Then
        verify(employeeRepository).save(employee);
    }

    @Test
    void canUpdateProfileImageId() {
        // Given
        String profileImageId = "2222";
        Integer customerId = 1;

        // When
        underTest.updateEmployeeProfileImageId(profileImageId, customerId);

        // Then
        verify(employeeRepository).updateProfileImageId(profileImageId, customerId);
    }
}