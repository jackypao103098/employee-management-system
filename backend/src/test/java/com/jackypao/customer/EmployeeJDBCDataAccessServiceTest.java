package com.jackypao.customer;

import com.jackypao.AbstractTestcontainers;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;

import java.util.List;
import java.util.Optional;
import java.util.UUID;

import static org.assertj.core.api.Assertions.assertThat;

class EmployeeJDBCDataAccessServiceTest extends AbstractTestcontainers {

    private EmployeeJDBCDataAccessService underTest;
    private final EmployeeRowMapper employeeRowMapper = new EmployeeRowMapper();

    @BeforeEach
    void setUp() {
        underTest = new EmployeeJDBCDataAccessService(
                getJdbcTemplate(),
                employeeRowMapper
        );
    }

    @Test
    void selectAllEmployees() {
        // Given
        Employee employee = new Employee(
                FAKER.name().fullName(),
                FAKER.internet().safeEmailAddress() + "-" + UUID.randomUUID(),
                "password", 20,
                Gender.MALE);
        underTest.insertEmployee(employee);

        // When
        List<Employee> actual = underTest.selectAllEmployees();

        // Then
        assertThat(actual).isNotEmpty();
    }

    @Test
    void selectEmployeeById() {
        // Given
        String email = FAKER.internet().safeEmailAddress() + "-" + UUID.randomUUID();
        Employee employee = new Employee(
                FAKER.name().fullName(),
                email,
                "password", 20,
                Gender.MALE);

        underTest.insertEmployee(employee);

        int id = underTest.selectAllEmployees()
                .stream()
                .filter(c -> c.getEmail().equals(email))
                .map(Employee::getId)
                .findFirst()
                .orElseThrow();

        // When
        Optional<Employee> actual = underTest.selectEmployeeById(id);

        // Then
        assertThat(actual).isPresent().hasValueSatisfying(c -> {
            assertThat(c.getId()).isEqualTo(id);
            assertThat(c.getName()).isEqualTo(employee.getName());
            assertThat(c.getEmail()).isEqualTo(employee.getEmail());
            assertThat(c.getAge()).isEqualTo(employee.getAge());
        });
    }

    @Test
    void willReturnEmptyWhenSelectCustomerById() {
        // Given
        int id = 0;

        // When
        var actual = underTest.selectEmployeeById(id);

        // Then
        assertThat(actual).isEmpty();
    }

    @Test
    void existsPersonWithEmail() {
        // Given
        String email = FAKER.internet().safeEmailAddress() + "-" + UUID.randomUUID();
        String name = FAKER.name().fullName();
        Employee employee = new Employee(
                name,
                email,
                "password", 20,
                Gender.MALE);

        underTest.insertEmployee(employee);

        // When
        boolean actual = underTest.existsEmployeeWithEmail(email);

        // Then
        assertThat(actual).isTrue();
    }

    @Test
    void existsPersonWithEmailReturnsFalseWhenDoesNotExists() {
        // Given
        String email = FAKER.internet().safeEmailAddress() + "-" + UUID.randomUUID();

        // When
        boolean actual = underTest.existsEmployeeWithEmail(email);

        // Then
        assertThat(actual).isFalse();
    }

    @Test
    void existsCustomerWithId() {
        // Given
        String email = FAKER.internet().safeEmailAddress() + "-" + UUID.randomUUID();
        Employee employee = new Employee(
                FAKER.name().fullName(),
                email,
                "password", 20,
                Gender.MALE);

        underTest.insertEmployee(employee);

        int id = underTest.selectAllEmployees()
                .stream()
                .filter(c -> c.getEmail().equals(email))
                .map(Employee::getId)
                .findFirst()
                .orElseThrow();

        // When
        var actual = underTest.existsEmployeeById(id);

        // Then
        assertThat(actual).isTrue();
    }

    @Test
    void existsPersonWithIdWillReturnFalseWhenIdNotPresent() {
        // Given
        int id = -1;

        // When
        var actual = underTest.existsEmployeeById(id);

        // Then
        assertThat(actual).isFalse();
    }

    @Test
    void deleteEmployeeById() {
        // Given
        String email = FAKER.internet().safeEmailAddress() + "-" + UUID.randomUUID();
        Employee employee = new Employee(
                FAKER.name().fullName(),
                email,
                "password", 20,
                Gender.MALE);

        underTest.insertEmployee(employee);

        int id = underTest.selectAllEmployees()
                .stream()
                .filter(c -> c.getEmail().equals(email))
                .map(Employee::getId)
                .findFirst()
                .orElseThrow();

        // When
        underTest.deleteEmployeeById(id);

        // Then
        Optional<Employee> actual = underTest.selectEmployeeById(id);
        assertThat(actual).isNotPresent();
    }

    @Test
    void updateCustomerName() {
        // Given
        String email = FAKER.internet().safeEmailAddress() + "-" + UUID.randomUUID();
        Employee employee = new Employee(
                FAKER.name().fullName(),
                email,
                "password", 20,
                Gender.MALE);

        underTest.insertEmployee(employee);

        int id = underTest.selectAllEmployees()
                .stream()
                .filter(c -> c.getEmail().equals(email))
                .map(Employee::getId)
                .findFirst()
                .orElseThrow();

        var newName = "foo";

        // When age is name
        Employee update = new Employee();
        update.setId(id);
        update.setName(newName);

        underTest.updateCustomer(update);

        // Then
        Optional<Employee> actual = underTest.selectEmployeeById(id);

        assertThat(actual).isPresent().hasValueSatisfying(c -> {
            assertThat(c.getId()).isEqualTo(id);
            assertThat(c.getName()).isEqualTo(newName); // change
            assertThat(c.getEmail()).isEqualTo(employee.getEmail());
            assertThat(c.getAge()).isEqualTo(employee.getAge());
        });
    }

    @Test
    void updateCustomerEmail() {
        // Given
        String email = FAKER.internet().safeEmailAddress() + "-" + UUID.randomUUID();
        Employee employee = new Employee(
                FAKER.name().fullName(),
                email,
                "password", 20,
                Gender.MALE);

        underTest.insertEmployee(employee);

        int id = underTest.selectAllEmployees()
                .stream()
                .filter(c -> c.getEmail().equals(email))
                .map(Employee::getId)
                .findFirst()
                .orElseThrow();

        var newEmail = FAKER.internet().safeEmailAddress() + "-" + UUID.randomUUID();;

        // When email is changed
        Employee update = new Employee();
        update.setId(id);
        update.setEmail(newEmail);

        underTest.updateCustomer(update);

        // Then
        Optional<Employee> actual = underTest.selectEmployeeById(id);

        assertThat(actual).isPresent().hasValueSatisfying(c -> {
            assertThat(c.getId()).isEqualTo(id);
            assertThat(c.getEmail()).isEqualTo(newEmail); // change
            assertThat(c.getName()).isEqualTo(employee.getName());
            assertThat(c.getAge()).isEqualTo(employee.getAge());
        });
    }

    @Test
    void updateCustomerAge() {
        // Given
        String email = FAKER.internet().safeEmailAddress() + "-" + UUID.randomUUID();
        Employee employee = new Employee(
                FAKER.name().fullName(),
                email,
                "password", 20,
                Gender.MALE);

        underTest.insertEmployee(employee);

        int id = underTest.selectAllEmployees()
                .stream()
                .filter(c -> c.getEmail().equals(email))
                .map(Employee::getId)
                .findFirst()
                .orElseThrow();

        var newAge = 100;

        // When age is changed
        Employee update = new Employee();
        update.setId(id);
        update.setAge(newAge);

        underTest.updateCustomer(update);

        // Then
        Optional<Employee> actual = underTest.selectEmployeeById(id);

        assertThat(actual).isPresent().hasValueSatisfying(c -> {
            assertThat(c.getId()).isEqualTo(id);
            assertThat(c.getAge()).isEqualTo(newAge); // change
            assertThat(c.getName()).isEqualTo(employee.getName());
            assertThat(c.getEmail()).isEqualTo(employee.getEmail());
        });
    }

    @Test
    void willUpdateAllPropertiesCustomer() {
        // Given
        String email = FAKER.internet().safeEmailAddress() + "-" + UUID.randomUUID();
        Employee employee = new Employee(
                FAKER.name().fullName(),
                email,
                "password", 20,
                Gender.MALE);

        underTest.insertEmployee(employee);

        int id = underTest.selectAllEmployees()
                .stream()
                .filter(c -> c.getEmail().equals(email))
                .map(Employee::getId)
                .findFirst()
                .orElseThrow();

        // When update with new name, age and email
        Employee update = new Employee();
        update.setId(id);
        update.setName("foo");
        String newEmail = UUID.randomUUID().toString();
        update.setEmail(newEmail);
        update.setAge(22);

        underTest.updateCustomer(update);

        // Then
        Optional<Employee> actual = underTest.selectEmployeeById(id);

        assertThat(actual).isPresent().hasValueSatisfying(updated -> {
            assertThat(updated.getId()).isEqualTo(id);
            assertThat(updated.getGender()).isEqualTo(Gender.MALE);
            assertThat(updated.getName()).isEqualTo("foo");
            assertThat(updated.getEmail()).isEqualTo(newEmail);
            assertThat(updated.getAge()).isEqualTo(22);
        });
    }

    @Test
    void willNotUpdateWhenNothingToUpdate() {
        // Given
        String email = FAKER.internet().safeEmailAddress() + "-" + UUID.randomUUID();
        Employee employee = new Employee(
                FAKER.name().fullName(),
                email,
                "password", 20,
                Gender.MALE);

        underTest.insertEmployee(employee);

        int id = underTest.selectAllEmployees()
                .stream()
                .filter(c -> c.getEmail().equals(email))
                .map(Employee::getId)
                .findFirst()
                .orElseThrow();

        // When update without no changes
        Employee update = new Employee();
        update.setId(id);

        underTest.updateCustomer(update);

        // Then
        Optional<Employee> actual = underTest.selectEmployeeById(id);

        assertThat(actual).isPresent().hasValueSatisfying(c -> {
            assertThat(c.getId()).isEqualTo(id);
            assertThat(c.getAge()).isEqualTo(employee.getAge());
            assertThat(c.getName()).isEqualTo(employee.getName());
            assertThat(c.getEmail()).isEqualTo(employee.getEmail());
        });
    }

    @Test
    void canUpdateProfileImageId() {
        // Given
        String email = FAKER.internet().safeEmailAddress() + "-" + UUID.randomUUID();
        Employee employee = new Employee(
                FAKER.name().fullName(),
                email,
                "password", 20,
                Gender.MALE);

        underTest.insertEmployee(employee);

        int id = underTest.selectAllEmployees()
                .stream()
                .filter(c -> c.getEmail().equals(email))
                .map(Employee::getId)
                .findFirst()
                .orElseThrow();

        // When
        underTest.updateEmployeeProfileImageId("2222", id);

        // Then
        Optional<Employee> customerOptional = underTest.selectEmployeeById(id);
        assertThat(customerOptional)
                .isPresent()
                .hasValueSatisfying(
                        c -> assertThat(c.getProfileImageId()).isEqualTo("2222")
                );
    }
}