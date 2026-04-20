package com.jackypao.customer;

public record EmployeeRegistrationRequest(
        String name,
        String email,
        String password,
        Integer age,
        Gender gender
) {
}
