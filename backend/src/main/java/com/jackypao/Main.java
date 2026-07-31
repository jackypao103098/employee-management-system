package com.jackypao;

import com.jackypao.customer.Employee;
import com.jackypao.customer.EmployeeRepository;
import com.jackypao.customer.Gender;
import org.springframework.boot.CommandLineRunner;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.context.annotation.Bean;
import org.springframework.security.crypto.password.PasswordEncoder;

@SpringBootApplication
public class Main {

    public static void main(String[] args) {
        SpringApplication.run(Main.class, args);
    }

    @Bean
    CommandLineRunner runner(
            EmployeeRepository employeeRepository,
            PasswordEncoder passwordEncoder) {
        return args -> createRandomEmployee(employeeRepository, passwordEncoder);
    }

    private static void createRandomEmployee(EmployeeRepository employeeRepository, PasswordEncoder passwordEncoder) {
        String demoEmail = "demo@jackypao.com";
        if (employeeRepository.existsEmployeeByEmail(demoEmail)) return;
        Employee employee = new Employee(
                "Demo User",
                demoEmail,
                passwordEncoder.encode("password"),
                30,
                Gender.MALE);
        employeeRepository.save(employee);
    }

}
