package com.jackypao.customer;

public record EmployeeUpdateRequest(
        String name,
        String email,
        Integer age
) {
}
