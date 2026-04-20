package com.jackypao.customer;

import com.jackypao.AbstractTestcontainers;
import com.jackypao.TestConfig;
import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.autoconfigure.jdbc.AutoConfigureTestDatabase;
import org.springframework.boot.test.autoconfigure.orm.jpa.DataJpaTest;
import org.springframework.context.ApplicationContext;
import org.springframework.context.annotation.Import;

import java.util.Optional;
import java.util.UUID;

import static org.assertj.core.api.Assertions.assertThat;

@DataJpaTest
@AutoConfigureTestDatabase(replace = AutoConfigureTestDatabase.Replace.NONE)
@Import({TestConfig.class})
class EmployeeRepositoryTest extends AbstractTestcontainers {

    @Autowired
    private EmployeeRepository underTest;

    @Autowired
    private ApplicationContext applicationContext;

    @BeforeEach
    void setUp() {
        underTest.deleteAll();
        System.out.println(applicationContext.getBeanDefinitionCount());
    }

    @Test
    void existsEmployeeByEmail() {
        // Given
        String email = FAKER.internet().safeEmailAddress() + "-" + UUID.randomUUID();
        Employee employee = new Employee(
                FAKER.name().fullName(),
                email,
                "password",
                20,
                Gender.MALE);

        underTest.save(employee);

        // When
        var actual = underTest.existsEmployeeByEmail(email);

        // Then
        assertThat(actual).isTrue();
    }

    @Test
    void existsEmployeeByEmailFailsWhenEmailNotPresent() {
        // Given
        String email = FAKER.internet().safeEmailAddress() + "-" + UUID.randomUUID();

        // When
        var actual = underTest.existsEmployeeByEmail(email);

        // Then
        assertThat(actual).isFalse();
    }

    @Test
    void existsEmployeeById() {
        // Given
        String email = FAKER.internet().safeEmailAddress() + "-" + UUID.randomUUID();
        Employee employee = new Employee(
                FAKER.name().fullName(),
                email,
                "password", 20,
                Gender.MALE);

        underTest.save(employee);

        int id = underTest.findAll()
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
    void existsEmployeeByIdFailsWhenIdNotPresent() {
        // Given
        int id = -1;

        // When
        var actual = underTest.existsEmployeeById(id);

        // Then
        assertThat(actual).isFalse();
    }

    @Test
    void canUpdateProfileImageId() {
        // Given
        String email = "email";

        Employee employee = new Employee(
                FAKER.name().fullName(),
                email,
                "password", 20,
                Gender.MALE);

        underTest.save(employee);

        int id = underTest.findAll()
                .stream()
                .filter(c -> c.getEmail().equals(email))
                .map(Employee::getId)
                .findFirst()
                .orElseThrow();

        // When
        underTest.updateProfileImageId("2222", id);

        // Then
        Optional<Employee> customerOptional = underTest.findById(id);
        assertThat(customerOptional)
                .isPresent()
                .hasValueSatisfying(
                        c -> assertThat(c.getProfileImageId()).isEqualTo("2222")
                );
    }
}