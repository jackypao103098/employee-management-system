package com.jackypao;

import com.jackypao.customer.Employee;
import com.jackypao.customer.EmployeeRepository;
import com.jackypao.customer.Gender;
import com.jackypao.s3.S3Buckets;
import com.jackypao.s3.S3Service;
import com.github.javafaker.Faker;
import com.github.javafaker.Name;
import org.springframework.boot.CommandLineRunner;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.context.annotation.Bean;
import org.springframework.security.crypto.password.PasswordEncoder;

import java.util.Random;
import java.util.UUID;

@SpringBootApplication
public class Main {

    public static void main(String[] args) {
        SpringApplication.run(Main.class, args);
    }

    @Bean
    CommandLineRunner runner(
            EmployeeRepository employeeRepository,
            PasswordEncoder passwordEncoder) {
        return args -> {
            createRandomEmployee(employeeRepository, passwordEncoder);
            // testBucketUploadAndDownload(s3Service, s3Buckets);
        };
    }

    private static void testBucketUploadAndDownload(S3Service s3Service,
                                                    S3Buckets s3Buckets) {
        s3Service.putObject(
                s3Buckets.getCustomer(),
                "foo/bar/jamila",
                "Hello World".getBytes()
        );

        byte[] obj = s3Service.getObject(
                s3Buckets.getCustomer(),
                "foo/bar/jamila"
        );

        System.out.println("Hooray: " + new String(obj));
    }

    private static void createRandomEmployee(EmployeeRepository employeeRepository, PasswordEncoder passwordEncoder) {
        var faker = new Faker();
        Random random = new Random();
        Name name = faker.name();
        String firstName = name.firstName();
        String lastName = name.lastName();
        int age = random.nextInt(16, 99);
        Gender gender = age % 2 == 0 ? Gender.MALE : Gender.FEMALE;
        String email = firstName.toLowerCase() + "." + lastName.toLowerCase() + "@jackypao.com";
        Employee employee = new Employee(
                firstName +  " " + lastName,
                email,
                passwordEncoder.encode("password"),
                age,
                gender);
        employeeRepository.save(employee);
        System.out.println(email);
    }

}
