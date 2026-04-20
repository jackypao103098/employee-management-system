package com.jackypao.customer;

import org.springframework.stereotype.Repository;

import java.util.ArrayList;
import java.util.List;
import java.util.Optional;

@Repository("list")
public class EmployeeListDataAccessService implements EmployeeDao {

    // db
    private static final List<Employee> employees;

    static {
        employees = new ArrayList<>();

        Employee alex = new Employee(
                1,
                "Alex",
                "alex@gmail.com",
                "password",
                21,
                Gender.MALE);
        employees.add(alex);

        Employee jamila = new Employee(
                2,
                "Jamila",
                "jamila@gmail.com",
                "password",
                19,
                Gender.MALE);
        employees.add(jamila);
    }

    @Override
    public List<Employee> selectAllEmployees() {
        return employees;
    }

    @Override
    public Optional<Employee> selectEmployeeById(Integer id) {
        return employees.stream()
                .filter(c -> c.getId().equals(id))
                .findFirst();
    }

    @Override
    public void insertEmployee(Employee employee) {
        employees.add(employee);
    }

    @Override
    public boolean existsEmployeeWithEmail(String email) {
        return employees.stream()
                .anyMatch(c -> c.getEmail().equals(email));
    }

    @Override
    public boolean existsEmployeeById(Integer id) {
        return employees.stream()
                .anyMatch(c -> c.getId().equals(id));
    }

    @Override
    public void deleteEmployeeById(Integer employeeId) {
        employees.stream()
                .filter(c -> c.getId().equals(employeeId))
                .findFirst()
                .ifPresent(employees::remove);
    }

    @Override
    public void updateEmployee(Employee employee) {
        employees.add(employee);
    }

    @Override
    public Optional<Employee> selectUserByEmail(String email) {
        return employees.stream()
                .filter(c -> c.getUsername().equals(email))
                .findFirst();
    }

    @Override
    public void updateEmployeeProfileImageId(String profileImageId, Integer employeeId) {
        // TODO: Implement this
    }

}
