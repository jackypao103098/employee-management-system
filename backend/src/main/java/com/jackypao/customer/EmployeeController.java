package com.jackypao.customer;

import com.jackypao.jwt.JWTUtil;
import org.springframework.http.HttpHeaders;
import org.springframework.http.MediaType;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.multipart.MultipartFile;

import java.util.List;

@RestController
@RequestMapping("api/v1/employees")
public class EmployeeController {

    private final EmployeeService employeeService;
    private final JWTUtil jwtUtil;

    public EmployeeController(EmployeeService employeeService,
                              JWTUtil jwtUtil) {
        this.employeeService = employeeService;
        this.jwtUtil = jwtUtil;
    }

    @GetMapping
    public List<EmployeeDTO> getEmployees() {
        return employeeService.getAllEmployees();
    }

    @GetMapping("{employeeId}")
    public EmployeeDTO getEmployee(
            @PathVariable("employeeId") Integer employeeId) {
        return employeeService.getEmployee(employeeId);
    }

    @PostMapping
    public ResponseEntity<?> registerEmployee(
            @RequestBody EmployeeRegistrationRequest request) {
        employeeService.addEmployee(request);
        String jwtToken = jwtUtil.issueToken(request.email(), "ROLE_USER");
        return ResponseEntity.ok()
                .header(HttpHeaders.AUTHORIZATION, jwtToken)
                .build();
    }

    @DeleteMapping("{employeeId}")
    public void deleteEmployee(
            @PathVariable("employeeId") Integer employeeId) {
        employeeService.deleteEmployeeById(employeeId);
    }

    @PutMapping("{employeeId}")
    public void updateEmployee(
            @PathVariable("employeeId") Integer employeeId,
            @RequestBody EmployeeUpdateRequest updateRequest) {
        employeeService.updateEmployee(employeeId, updateRequest);
    }

    @PostMapping(
            value = "{employeeId}/profile-image",
            consumes = MediaType.MULTIPART_FORM_DATA_VALUE
    )
    public void uploadEmployeeProfileImage(
            @PathVariable("employeeId") Integer employeeId,
            @RequestParam("file") MultipartFile file) {
        employeeService.uploadEmployeeProfileImage(employeeId, file);
    }

    @GetMapping(
            value = "{employeeId}/profile-image",
            produces = MediaType.IMAGE_JPEG_VALUE
    )
    public byte[] getEmployeeProfileImage(
            @PathVariable("employeeId") Integer employeeId) {
        return employeeService.getEmployeeProfileImage(employeeId);
    }

}
