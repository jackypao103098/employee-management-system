package com.jackypao.journey;

import com.jackypao.auth.AuthenticationRequest;
import com.jackypao.auth.AuthenticationResponse;
import com.jackypao.customer.EmployeeDTO;
import com.jackypao.customer.EmployeeRegistrationRequest;
import com.jackypao.customer.Gender;
import com.jackypao.jwt.JWTUtil;
import com.github.javafaker.Faker;
import com.github.javafaker.Name;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.core.ParameterizedTypeReference;
import org.springframework.http.HttpHeaders;
import org.springframework.http.MediaType;
import org.springframework.test.web.reactive.server.EntityExchangeResult;
import org.springframework.test.web.reactive.server.WebTestClient;
import reactor.core.publisher.Mono;

import java.util.List;
import java.util.Random;
import java.util.UUID;

import static org.springframework.boot.test.context.SpringBootTest.WebEnvironment.RANDOM_PORT;
import static org.assertj.core.api.Assertions.assertThat;

@SpringBootTest(webEnvironment = RANDOM_PORT)
public class AuthenticationIT {
    @Autowired
    private WebTestClient webTestClient;

    @Autowired
    private JWTUtil jwtUtil;

    private static final Random RANDOM = new Random();
    private static final String AUTHENTICATION_PATH = "/api/v1/auth";
    private static final String EMPLOYEE_PATH = "/api/v1/employees";

    @Test
    void canLogin() {
        // Given
// create registration employeeRegistrationRequest
        Faker faker = new Faker();
        Name fakerName = faker.name();

        String name = fakerName.fullName();
        String email = fakerName.lastName() + "-" + UUID.randomUUID() + "@jackypao.com";
        int age = RANDOM.nextInt(1, 100);

        Gender gender = age % 2 == 0 ? Gender.MALE : Gender.FEMALE;

        String password = "password";

        EmployeeRegistrationRequest employeeRegistrationRequest = new EmployeeRegistrationRequest(
                name, email, password, age, gender
        );

        AuthenticationRequest authenticationRequest = new AuthenticationRequest(
                email,
                password
        );

        webTestClient.post()
                .uri(AUTHENTICATION_PATH + "/login")
                .accept(MediaType.APPLICATION_JSON)
                .contentType(MediaType.APPLICATION_JSON)
                .body(Mono.just(authenticationRequest), AuthenticationRequest.class)
                .exchange()
                .expectStatus()
                .isUnauthorized();

        // send a post employeeRegistrationRequest
        webTestClient.post()
                .uri(EMPLOYEE_PATH)
                .accept(MediaType.APPLICATION_JSON)
                .contentType(MediaType.APPLICATION_JSON)
                .body(
                        Mono.just(employeeRegistrationRequest),
                        EmployeeRegistrationRequest.class
                )
                .exchange()
                .expectStatus()
                .isOk();

        EntityExchangeResult<AuthenticationResponse> result = webTestClient.post()
                .uri(AUTHENTICATION_PATH + "/login")
                .accept(MediaType.APPLICATION_JSON)
                .contentType(MediaType.APPLICATION_JSON)
                .body(Mono.just(authenticationRequest), AuthenticationRequest.class)
                .exchange()
                .expectStatus()
                .isOk()
                .expectBody(new ParameterizedTypeReference<AuthenticationResponse>() {
                })
                .returnResult();

        String jwtToken = result.getResponseHeaders()
                .get(HttpHeaders.AUTHORIZATION)
                .get(0);

        AuthenticationResponse authenticationResponse = result.getResponseBody();

        EmployeeDTO employeeDTO = authenticationResponse.employeeDTO();

        assertThat(jwtUtil.isTokenValid(
                jwtToken,
                employeeDTO.username())).isTrue();

        assertThat(employeeDTO.email()).isEqualTo(email);
        assertThat(employeeDTO.age()).isEqualTo(age);
        assertThat(employeeDTO.name()).isEqualTo(name);
        assertThat(employeeDTO.username()).isEqualTo(email);
        assertThat(employeeDTO.gender()).isEqualTo(gender);
        assertThat(employeeDTO.roles()).isEqualTo(List.of("ROLE_USER"));

    }
}
