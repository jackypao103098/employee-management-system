package com.jackypao.customer;

import org.junit.jupiter.api.Test;

import java.sql.ResultSet;
import java.sql.SQLException;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Mockito.mock;
import static org.mockito.Mockito.when;

class EmployeeRowMapperTest {

    @Test
    void mapRow() throws SQLException {
        // Given
        EmployeeRowMapper employeeRowMapper = new EmployeeRowMapper();

        ResultSet resultSet = mock(ResultSet.class);
        when(resultSet.getInt("id")).thenReturn(1);
        when(resultSet.getInt("age")).thenReturn(19);
        when(resultSet.getString("name")).thenReturn("Jamila");
        when(resultSet.getString("email")).thenReturn("jamila@gmail.com");
        when(resultSet.getString("gender")).thenReturn("FEMALE");
        when(resultSet.getString("password")).thenReturn("password");
        when(resultSet.getString("profile_image_id")).thenReturn("22222");


        // When
        Employee actual = employeeRowMapper.mapRow(resultSet, 1);

        // Then
        Employee expected = new Employee(
                1,
                "Jamila",
                "jamila@gmail.com",
                "password",
                19,
                Gender.FEMALE,
                "22222"
        );
        assertThat(actual).isEqualTo(expected);
    }
}