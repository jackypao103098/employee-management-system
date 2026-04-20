package com.jackypao.customer;

import jakarta.transaction.Transactional;
import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Modifying;
import org.springframework.data.jpa.repository.Query;

import java.util.Optional;

@Transactional
public interface EmployeeRepository
        extends JpaRepository<Employee, Integer> {

    boolean existsEmployeeByEmail(String email);
    boolean existsEmployeeById(Integer id);
    Optional<Employee> findEmployeeByEmail(String email);
    @Modifying(clearAutomatically = true)
    @Query("UPDATE Employee e SET e.profileImageId = ?1 WHERE e.id = ?2")
    int updateProfileImageId(String profileImageId, Integer employeeId);
}
