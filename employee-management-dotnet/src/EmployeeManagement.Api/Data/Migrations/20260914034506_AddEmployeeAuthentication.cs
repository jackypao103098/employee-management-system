using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeeManagement.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeAuthentication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "role",
                table: "employee",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "EMPLOYEE");

            migrationBuilder.UpdateData(
                table: "employee",
                keyColumn: "id",
                keyValue: -3,
                column: "password",
                value: "pbkdf2-sha512$210000$9Tk38apCOflFaJ3P8v90SA==$PN0Up22LpjNC9IALuVkNUoAW7lhN6na6N9S8DJDVvuM=");

            migrationBuilder.UpdateData(
                table: "employee",
                keyColumn: "id",
                keyValue: -2,
                column: "password",
                value: "pbkdf2-sha512$210000$AliQAXJgMCXOJZ+2upmC9A==$ph1o4NERokHaYu5MMq+4gimFoKM0AEiRU94J/Ss/CW8=");

            migrationBuilder.UpdateData(
                table: "employee",
                keyColumn: "id",
                keyValue: -1,
                column: "password",
                value: "pbkdf2-sha512$210000$OZfEv+OZUJTcIIdtAVyYXg==$a8oOcVTMayfvFrgxau91wst5Gfpz+oYJSSJXKZB7/mE=");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "role",
                table: "employee");

            migrationBuilder.UpdateData(
                table: "employee",
                keyColumn: "id",
                keyValue: -3,
                column: "password",
                value: "pbkdf2-sha256$100000$ZGVtby1zYWx0LTEyMzQ1Ng==$ZGVtby1wYXNzd29yZC1oYXNoLTEyMzQ1Njc4OTAxMjM=");

            migrationBuilder.UpdateData(
                table: "employee",
                keyColumn: "id",
                keyValue: -2,
                column: "password",
                value: "pbkdf2-sha256$100000$ZGVtby1zYWx0LTEyMzQ1Ng==$ZGVtby1wYXNzd29yZC1oYXNoLTEyMzQ1Njc4OTAxMjM=");

            migrationBuilder.UpdateData(
                table: "employee",
                keyColumn: "id",
                keyValue: -1,
                column: "password",
                value: "pbkdf2-sha256$100000$ZGVtby1zYWx0LTEyMzQ1Ng==$ZGVtby1wYXNzd29yZC1oYXNoLTEyMzQ1Njc4OTAxMjM=");
        }
    }
}
