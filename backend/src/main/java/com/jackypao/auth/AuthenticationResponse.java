package com.jackypao.auth;

import com.jackypao.customer.EmployeeDTO;

public record AuthenticationResponse (
        String token,
        EmployeeDTO employeeDTO){
}
