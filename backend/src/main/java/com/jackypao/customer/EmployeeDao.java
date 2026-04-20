package com.jackypao.customer;

import java.util.List;
import java.util.Optional;

public interface EmployeeDao {
    List<Employee> selectAllEmployees();
    Optional<Employee> selectEmployeeById(Integer employeeId);
    void insertEmployee(Employee employee);
    boolean existsEmployeeWithEmail(String email);
    boolean existsEmployeeById(Integer employeeId);
    void deleteEmployeeById(Integer employeeId);
    void updateEmployee(Employee update);
    Optional<Employee> selectUserByEmail(String email);
    void updateEmployeeProfileImageId(String profileImageId, Integer employeeId);
}
