package com.jackypao.customer;

import org.springframework.jdbc.core.JdbcTemplate;
import org.springframework.stereotype.Repository;

import java.util.List;
import java.util.Optional;

@Repository("jdbc")
public class EmployeeJDBCDataAccessService implements EmployeeDao {

    private final JdbcTemplate jdbcTemplate;
    private final EmployeeRowMapper employeeRowMapper;

    public EmployeeJDBCDataAccessService(JdbcTemplate jdbcTemplate,
                                         EmployeeRowMapper employeeRowMapper) {
        this.jdbcTemplate = jdbcTemplate;
        this.employeeRowMapper = employeeRowMapper;
    }

    @Override
    public List<Employee> selectAllEmployees() {
        var sql = """
                SELECT id, name, email, password, age, gender, profile_image_id
                FROM employee
                LIMIT 1000
                """;

        return jdbcTemplate.query(sql, employeeRowMapper);
    }

    @Override
    public Optional<Employee> selectEmployeeById(Integer id) {
        var sql = """
                SELECT id, name, email, password, age, gender, profile_image_id
                FROM employee
                WHERE id = ?
                """;
        return jdbcTemplate.query(sql, employeeRowMapper, id)
                .stream()
                .findFirst();
    }

    @Override
    public void insertEmployee(Employee employee) {
        var sql = """
                INSERT INTO employee(name, email, password, age, gender)
                VALUES (?, ?, ?, ?, ?)
                """;
        jdbcTemplate.update(
                sql,
                employee.getName(),
                employee.getEmail(),
                employee.getPassword(),
                employee.getAge(),
                employee.getGender().name()
        );
    }

    @Override
    public boolean existsEmployeeWithEmail(String email) {
        var sql = """
                SELECT count(id)
                FROM employee
                WHERE email = ?
                """;
        Integer count = jdbcTemplate.queryForObject(sql, Integer.class, email);
        return count != null && count > 0;
    }

    @Override
    public boolean existsEmployeeById(Integer id) {
        var sql = """
                SELECT count(id)
                FROM employee
                WHERE id = ?
                """;
        Integer count = jdbcTemplate.queryForObject(sql, Integer.class, id);
        return count != null && count > 0;
    }

    @Override
    public void deleteEmployeeById(Integer employeeId) {
        var sql = """
                DELETE
                FROM employee
                WHERE id = ?
                """;
        jdbcTemplate.update(sql, employeeId);
    }

    @Override
    public void updateEmployee(Employee update) {
        if (update.getName() != null) {
            String sql = "UPDATE employee SET name = ? WHERE id = ?";
            jdbcTemplate.update(
                    sql,
                    update.getName(),
                    update.getId()
            );
        }
        if (update.getAge() != null) {
            String sql = "UPDATE employee SET age = ? WHERE id = ?";
            jdbcTemplate.update(
                    sql,
                    update.getAge(),
                    update.getId()
            );
        }
        if (update.getEmail() != null) {
            String sql = "UPDATE employee SET email = ? WHERE id = ?";
            jdbcTemplate.update(
                    sql,
                    update.getEmail(),
                    update.getId());
        }
    }

    @Override
    public Optional<Employee> selectUserByEmail(String email) {
        var sql = """
                SELECT id, name, email, password, age, gender, profile_image_id
                FROM employee
                WHERE email = ?
                """;
        return jdbcTemplate.query(sql, employeeRowMapper, email)
                .stream()
                .findFirst();
    }

    @Override
    public void updateEmployeeProfileImageId(String profileImageId,
                                             Integer employeeId) {
        var sql = """
                UPDATE employee
                SET profile_image_id = ?
                WHERE id = ?
                """;
        jdbcTemplate.update(sql, profileImageId, employeeId);
    }
}
