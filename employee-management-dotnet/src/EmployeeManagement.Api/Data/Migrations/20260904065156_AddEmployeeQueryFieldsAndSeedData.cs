using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EmployeeManagement.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeQueryFieldsAndSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "department",
                table: "employee",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "Unassigned");

            migrationBuilder.AddColumn<DateOnly>(
                name: "hire_date",
                table: "employee",
                type: "date",
                nullable: false,
                defaultValueSql: "CURRENT_DATE");

            migrationBuilder.InsertData(
                table: "employee",
                columns: new[] { "id", "age", "department", "email", "gender", "hire_date", "name", "password" },
                values: new object[,]
                {
                    { -3, 45, "Human Resources", "peter.drucker@example.com", "MALE", new DateOnly(2020, 3, 20), "Peter Drucker", "pbkdf2-sha256$100000$ZGVtby1zYWx0LTEyMzQ1Ng==$ZGVtby1wYXNzd29yZC1oYXNoLTEyMzQ1Njc4OTAxMjM=" },
                    { -2, 40, "Engineering", "grace.hopper@example.com", "FEMALE", new DateOnly(2021, 6, 15), "Grace Hopper", "pbkdf2-sha256$100000$ZGVtby1zYWx0LTEyMzQ1Ng==$ZGVtby1wYXNzd29yZC1oYXNoLTEyMzQ1Njc4OTAxMjM=" },
                    { -1, 36, "Engineering", "ada.lovelace@example.com", "FEMALE", new DateOnly(2022, 1, 10), "Ada Lovelace", "pbkdf2-sha256$100000$ZGVtby1zYWx0LTEyMzQ1Ng==$ZGVtby1wYXNzd29yZC1oYXNoLTEyMzQ1Njc4OTAxMjM=" }
                });

            migrationBuilder.CreateIndex(
                name: "employee_department_index",
                table: "employee",
                column: "department");

            migrationBuilder.CreateIndex(
                name: "employee_hire_date_id_index",
                table: "employee",
                columns: new[] { "hire_date", "id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "employee_department_index",
                table: "employee");

            migrationBuilder.DropIndex(
                name: "employee_hire_date_id_index",
                table: "employee");

            migrationBuilder.DeleteData(
                table: "employee",
                keyColumn: "id",
                keyValue: -3);

            migrationBuilder.DeleteData(
                table: "employee",
                keyColumn: "id",
                keyValue: -2);

            migrationBuilder.DeleteData(
                table: "employee",
                keyColumn: "id",
                keyValue: -1);

            migrationBuilder.DropColumn(
                name: "department",
                table: "employee");

            migrationBuilder.DropColumn(
                name: "hire_date",
                table: "employee");
        }
    }
}
