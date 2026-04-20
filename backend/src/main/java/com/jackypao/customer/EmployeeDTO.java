package com.jackypao.customer;

import java.util.List;

public record EmployeeDTO(
        Integer id,
        String name,
        String email,
        Gender gender,
        Integer age,
        List<String> roles,
        String username,
        String profileImageId
) {

}
