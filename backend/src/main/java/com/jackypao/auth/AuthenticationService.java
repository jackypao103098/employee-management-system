package com.jackypao.auth;

import com.jackypao.customer.Employee;
import com.jackypao.customer.EmployeeDTO;
import com.jackypao.customer.EmployeeDTOMapper;
import com.jackypao.jwt.JWTUtil;
import org.springframework.security.authentication.AuthenticationManager;
import org.springframework.security.authentication.UsernamePasswordAuthenticationToken;
import org.springframework.security.core.Authentication;
import org.springframework.stereotype.Service;

@Service
public class AuthenticationService {

    private final AuthenticationManager authenticationManager;
    private final EmployeeDTOMapper employeeDTOMapper;
    private final JWTUtil jwtUtil;

    public AuthenticationService(AuthenticationManager authenticationManager,
                                 EmployeeDTOMapper employeeDTOMapper,
                                 JWTUtil jwtUtil) {
        this.authenticationManager = authenticationManager;
        this.employeeDTOMapper = employeeDTOMapper;
        this.jwtUtil = jwtUtil;
    }

    public AuthenticationResponse login(AuthenticationRequest request) {
        Authentication authentication = authenticationManager.authenticate(
                new UsernamePasswordAuthenticationToken(
                        request.username(),
                        request.password()
                )
        );
        Employee principal = (Employee) authentication.getPrincipal();
        EmployeeDTO employeeDTO = employeeDTOMapper.apply(principal);
        String token = jwtUtil.issueToken(employeeDTO.username(), employeeDTO.roles());
        return new AuthenticationResponse(token, employeeDTO);
    }

}
