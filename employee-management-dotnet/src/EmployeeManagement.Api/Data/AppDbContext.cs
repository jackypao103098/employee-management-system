using EmployeeManagement.Api.Entities;
using EmployeeManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<Employee> Employees => Set<Employee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var employee = modelBuilder.Entity<Employee>();

        employee.ToTable("employee");
        employee.HasKey(entity => entity.Id);
        employee.Property(entity => entity.Id).HasColumnName("id");
        employee.Property(entity => entity.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();
        employee.Property(entity => entity.Email)
            .HasColumnName("email")
            .HasMaxLength(254)
            .IsRequired();
        employee.HasIndex(entity => entity.Email)
            .IsUnique()
            .HasDatabaseName("employee_email_unique");
        employee.Property(entity => entity.Age)
            .HasColumnName("age")
            .IsRequired();
        employee.Property(entity => entity.Gender)
            .HasColumnName("gender")
            .HasConversion(
                gender => gender == Gender.Male ? "MALE" : "FEMALE",
                value => value == "MALE" ? Gender.Male : Gender.Female)
            .HasMaxLength(20)
            .IsRequired();
        employee.Property(entity => entity.PasswordHash)
            .HasColumnName("password")
            .HasMaxLength(512)
            .IsRequired();
        employee.Property(entity => entity.Department)
            .HasColumnName("department")
            .HasMaxLength(100)
            .HasDefaultValue("Unassigned")
            .IsRequired();
        employee.Property(entity => entity.HireDate)
            .HasColumnName("hire_date")
            .HasDefaultValueSql("CURRENT_DATE")
            .IsRequired();
        employee.Property(entity => entity.Role)
            .HasColumnName("role")
            .HasConversion(
                role => EmployeeRoleNames.From(role),
                value => value == EmployeeRoleNames.Admin
                    ? EmployeeRole.Admin
                    : EmployeeRole.Employee)
            .HasMaxLength(20)
            .HasDefaultValue(EmployeeRole.Employee)
            .IsRequired();
        employee.HasIndex(entity => entity.Department)
            .HasDatabaseName("employee_department_index");
        employee.HasIndex(entity => new { entity.HireDate, entity.Id })
            .HasDatabaseName("employee_hire_date_id_index");

        employee.HasData(
            new Employee
            {
                Id = -1,
                Name = "Ada Lovelace",
                Email = "ada.lovelace@example.com",
                Age = 36,
                Gender = Gender.Female,
                PasswordHash = "pbkdf2-sha512$210000$OZfEv+OZUJTcIIdtAVyYXg==$a8oOcVTMayfvFrgxau91wst5Gfpz+oYJSSJXKZB7/mE=",
                Department = "Engineering",
                HireDate = new DateOnly(2022, 1, 10),
                Role = EmployeeRole.Employee
            },
            new Employee
            {
                Id = -2,
                Name = "Grace Hopper",
                Email = "grace.hopper@example.com",
                Age = 40,
                Gender = Gender.Female,
                PasswordHash = "pbkdf2-sha512$210000$AliQAXJgMCXOJZ+2upmC9A==$ph1o4NERokHaYu5MMq+4gimFoKM0AEiRU94J/Ss/CW8=",
                Department = "Engineering",
                HireDate = new DateOnly(2021, 6, 15),
                Role = EmployeeRole.Employee
            },
            new Employee
            {
                Id = -3,
                Name = "Peter Drucker",
                Email = "peter.drucker@example.com",
                Age = 45,
                Gender = Gender.Male,
                PasswordHash = "pbkdf2-sha512$210000$9Tk38apCOflFaJ3P8v90SA==$PN0Up22LpjNC9IALuVkNUoAW7lhN6na6N9S8DJDVvuM=",
                Department = "Human Resources",
                HireDate = new DateOnly(2020, 3, 20),
                Role = EmployeeRole.Employee
            });
    }
}
