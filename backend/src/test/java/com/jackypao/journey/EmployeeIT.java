package com.jackypao.journey;

import com.jackypao.customer.*;
import com.github.dockerjava.zerodep.shaded.org.apache.hc.client5.http.entity.mime.MultipartPartBuilder;
import com.github.javafaker.Faker;
import com.github.javafaker.Name;
import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.core.ParameterizedTypeReference;
import org.springframework.core.io.ClassPathResource;
import org.springframework.core.io.Resource;
import org.springframework.http.MediaType;
import org.springframework.http.client.MultipartBodyBuilder;
import org.springframework.test.web.reactive.server.WebTestClient;
import org.springframework.web.reactive.function.BodyInserters;
import org.testcontainers.shaded.com.google.common.io.Files;
import reactor.core.publisher.Mono;

import java.io.IOException;
import java.util.List;
import java.util.Random;
import java.util.UUID;

import static org.assertj.core.api.Assertions.assertThat;
import static org.springframework.boot.test.context.SpringBootTest.WebEnvironment.RANDOM_PORT;
import static org.springframework.http.HttpHeaders.AUTHORIZATION;

@SpringBootTest(webEnvironment = RANDOM_PORT)
public class EmployeeIT {

    @Autowired
    private WebTestClient webTestClient;

    private static final Random RANDOM = new Random();
    private static final String EMPLOYEE_PATH = "/api/v1/employees";

    @Test
    void canRegisterEmployee() {
        // create registration request
        Faker faker = new Faker();
        Name fakerName = faker.name();

        String name = fakerName.fullName();
        String email = fakerName.lastName() + "-" + UUID.randomUUID() + "@jackypao.com";
        int age = RANDOM.nextInt(1, 100);

        Gender gender = age % 2 == 0 ? Gender.MALE : Gender.FEMALE;

        EmployeeRegistrationRequest request = new EmployeeRegistrationRequest(
                name, email, "password", age, gender
        );
        // send a post request
        String jwtToken = webTestClient.post()
                .uri(EMPLOYEE_PATH)
                .accept(MediaType.APPLICATION_JSON)
                .contentType(MediaType.APPLICATION_JSON)
                .body(Mono.just(request), EmployeeRegistrationRequest.class)
                .exchange()
                .expectStatus()
                .isOk()
                .returnResult(Void.class)
                .getResponseHeaders()
                .get(AUTHORIZATION)
                .get(0);

        // get all employees
        List<EmployeeDTO> allEmployees = webTestClient.get()
                .uri(EMPLOYEE_PATH)
                .accept(MediaType.APPLICATION_JSON)
                .header(AUTHORIZATION, String.format("Bearer %s", jwtToken))
                .exchange()
                .expectStatus()
                .isOk()
                .expectBodyList(new ParameterizedTypeReference<EmployeeDTO>() {
                })
                .returnResult()
                .getResponseBody();

        int id = allEmployees.stream()
                .filter(employee -> employee.email().equals(email))
                .map(EmployeeDTO::id)
                .findFirst()
                .orElseThrow();

        // make sure that employee is present
        EmployeeDTO expectedEmployee = new EmployeeDTO(
                id,
                name,
                email,
                gender,
                age,
                List.of("ROLE_USER"),
                email,
                null
        );

        assertThat(allEmployees).contains(expectedEmployee);

        // get employee by id
        webTestClient.get()
                .uri(EMPLOYEE_PATH + "/{id}", id)
                .accept(MediaType.APPLICATION_JSON)
                .header(AUTHORIZATION, String.format("Bearer %s", jwtToken))
                .exchange()
                .expectStatus()
                .isOk()
                .expectBody(new ParameterizedTypeReference<EmployeeDTO>() {
                })
                .isEqualTo(expectedEmployee);
    }

    @Test
    void canDeleteEmployee() {
        // create registration request
        Faker faker = new Faker();
        Name fakerName = faker.name();

        String name = fakerName.fullName();
        String email = fakerName.lastName() + "-" + UUID.randomUUID() + "@jackypao.com";
        int age = RANDOM.nextInt(1, 100);

        Gender gender = age % 2 == 0 ? Gender.MALE : Gender.FEMALE;

        EmployeeRegistrationRequest request = new EmployeeRegistrationRequest(
                name, email, "password", age, gender
        );

        EmployeeRegistrationRequest request2 = new EmployeeRegistrationRequest(
                name, email + ".uk", "password", age, gender
        );

        // send a post request to create employee 1
        webTestClient.post()
                .uri(EMPLOYEE_PATH)
                .accept(MediaType.APPLICATION_JSON)
                .contentType(MediaType.APPLICATION_JSON)
                .body(Mono.just(request), EmployeeRegistrationRequest.class)
                .exchange()
                .expectStatus()
                .isOk();

        // send a post request to create employee 2
        String jwtToken = webTestClient.post()
                .uri(EMPLOYEE_PATH)
                .accept(MediaType.APPLICATION_JSON)
                .contentType(MediaType.APPLICATION_JSON)
                .body(Mono.just(request2), EmployeeRegistrationRequest.class)
                .exchange()
                .expectStatus()
                .isOk()
                .returnResult(Void.class)
                .getResponseHeaders()
                .get(AUTHORIZATION)
                .get(0);

        // get all employees
        List<EmployeeDTO> allEmployees = webTestClient.get()
                .uri(EMPLOYEE_PATH)
                .accept(MediaType.APPLICATION_JSON)
                .header(AUTHORIZATION, String.format("Bearer %s", jwtToken))
                .exchange()
                .expectStatus()
                .isOk()
                .expectBodyList(new ParameterizedTypeReference<EmployeeDTO>() {
                })
                .returnResult()
                .getResponseBody();


        int id = allEmployees.stream()
                .filter(employee -> employee.email().equals(email))
                .map(EmployeeDTO::id)
                .findFirst()
                .orElseThrow();

        // employee 2 deletes employee 1
        webTestClient.delete()
                .uri(EMPLOYEE_PATH + "/{id}", id)
                .header(AUTHORIZATION, String.format("Bearer %s", jwtToken))
                .accept(MediaType.APPLICATION_JSON)
                .exchange()
                .expectStatus()
                .isOk();

        // employee 2 gets employee 1 by id
        webTestClient.get()
                .uri(EMPLOYEE_PATH + "/{id}", id)
                .accept(MediaType.APPLICATION_JSON)
                .header(AUTHORIZATION, String.format("Bearer %s", jwtToken))
                .exchange()
                .expectStatus()
                .isNotFound();
    }

    @Test
    void canUpdateEmployee() {
        // create registration request
        Faker faker = new Faker();
        Name fakerName = faker.name();

        String name = fakerName.fullName();
        String email = fakerName.lastName() + "-" + UUID.randomUUID() + "@jackypao.com";
        int age = RANDOM.nextInt(1, 100);

        Gender gender = age % 2 == 0 ? Gender.MALE : Gender.FEMALE;

        EmployeeRegistrationRequest request = new EmployeeRegistrationRequest(
                name, email, "password", age, gender
        );

        // send a post request
        String jwtToken = webTestClient.post()
                .uri(EMPLOYEE_PATH)
                .accept(MediaType.APPLICATION_JSON)
                .contentType(MediaType.APPLICATION_JSON)
                .body(Mono.just(request), EmployeeRegistrationRequest.class)
                .exchange()
                .expectStatus()
                .isOk()
                .returnResult(Void.class)
                .getResponseHeaders()
                .get(AUTHORIZATION)
                .get(0);

        // get all employees
        List<EmployeeDTO> allEmployees = webTestClient.get()
                .uri(EMPLOYEE_PATH)
                .accept(MediaType.APPLICATION_JSON)
                .header(AUTHORIZATION, String.format("Bearer %s", jwtToken))
                .exchange()
                .expectStatus()
                .isOk()
                .expectBodyList(new ParameterizedTypeReference<EmployeeDTO>() {
                })
                .returnResult()
                .getResponseBody();


        int id = allEmployees.stream()
                .filter(employee -> employee.email().equals(email))
                .map(EmployeeDTO::id)
                .findFirst()
                .orElseThrow();

        // update employee

        String newName = "Ali";

        EmployeeUpdateRequest updateRequest = new EmployeeUpdateRequest(
                newName, null, null
        );

        webTestClient.put()
                .uri(EMPLOYEE_PATH + "/{id}", id)
                .accept(MediaType.APPLICATION_JSON)
                .header(AUTHORIZATION, String.format("Bearer %s", jwtToken))
                .contentType(MediaType.APPLICATION_JSON)
                .body(Mono.just(updateRequest), EmployeeUpdateRequest.class)
                .exchange()
                .expectStatus()
                .isOk();

        // get employee by id
        EmployeeDTO updatedEmployee = webTestClient.get()
                .uri(EMPLOYEE_PATH + "/{id}", id)
                .accept(MediaType.APPLICATION_JSON)
                .header(AUTHORIZATION, String.format("Bearer %s", jwtToken))
                .exchange()
                .expectStatus()
                .isOk()
                .expectBody(EmployeeDTO.class)
                .returnResult()
                .getResponseBody();

        EmployeeDTO expected = new EmployeeDTO(
                id,
                newName,
                email,
                gender,
                age,
                List.of("ROLE_USER"),
                email,
                null
        );

        assertThat(updatedEmployee).isEqualTo(expected);
    }

    @Test
    void canUploadAndDownloadProfilePictures() throws IOException {
        // create registration request
        Faker faker = new Faker();
        Name fakerName = faker.name();

        String name = fakerName.fullName();
        String email = fakerName.lastName() + "-" + UUID.randomUUID() + "@jackypao.com";
        int age = RANDOM.nextInt(1, 100);

        Gender gender = age % 2 == 0 ? Gender.MALE : Gender.FEMALE;

        EmployeeRegistrationRequest request = new EmployeeRegistrationRequest(
                name, email, "password", age, gender
        );

        // send a post request
        String jwtToken = webTestClient.post()
                .uri(EMPLOYEE_PATH)
                .accept(MediaType.APPLICATION_JSON)
                .contentType(MediaType.APPLICATION_JSON)
                .body(Mono.just(request), EmployeeRegistrationRequest.class)
                .exchange()
                .expectStatus()
                .isOk()
                .returnResult(Void.class)
                .getResponseHeaders()
                .get(AUTHORIZATION)
                .get(0);

        // get all employees
        List<EmployeeDTO> allEmployees = webTestClient.get()
                .uri(EMPLOYEE_PATH)
                .accept(MediaType.APPLICATION_JSON)
                .header(AUTHORIZATION, String.format("Bearer %s", jwtToken))
                .exchange()
                .expectStatus()
                .isOk()
                .expectBodyList(new ParameterizedTypeReference<EmployeeDTO>() {
                })
                .returnResult()
                .getResponseBody();

        EmployeeDTO employeeDTO = allEmployees.stream()
                .filter(employee -> employee.email().equals(email))
                .findFirst()
                .orElseThrow();

        assertThat(employeeDTO.profileImageId()).isNullOrEmpty();

        Resource image = new ClassPathResource(
                "%s.jpeg".formatted(gender.name().toLowerCase())
        );

        MultipartBodyBuilder bodyBuilder = new MultipartBodyBuilder();
        bodyBuilder.part("file", image);

        // When

        // send a post request
        webTestClient.post()
                .uri(EMPLOYEE_PATH + "/{employeeId}/profile-image", employeeDTO.id())
                .body(BodyInserters.fromMultipartData(bodyBuilder.build()))
                .header(AUTHORIZATION, String.format("Bearer %s", jwtToken))
                .exchange()
                .expectStatus()
                .isOk();

        // Then the profile image id should be populated

        // get employee by id
        String profileImageId = webTestClient.get()
                .uri(EMPLOYEE_PATH + "/{id}", employeeDTO.id())
                .accept(MediaType.APPLICATION_JSON)
                .header(AUTHORIZATION, String.format("Bearer %s", jwtToken))
                .exchange()
                .expectStatus()
                .isOk()
                .expectBody(EmployeeDTO.class)
                .returnResult()
                .getResponseBody()
                .profileImageId();

        assertThat(profileImageId).isNotBlank();

        // download image for employee
        byte[] downloadedImage = webTestClient.get()
                .uri(EMPLOYEE_PATH + "/{employeeId}/profile-image", employeeDTO.id())
                .accept(MediaType.IMAGE_JPEG)
                .exchange()
                .expectStatus()
                .isOk()
                .expectBody(byte[].class)
                .returnResult()
                .getResponseBody();

        byte[] actual = Files.toByteArray(image.getFile());

        assertThat(actual).isEqualTo(downloadedImage);

    }
}
