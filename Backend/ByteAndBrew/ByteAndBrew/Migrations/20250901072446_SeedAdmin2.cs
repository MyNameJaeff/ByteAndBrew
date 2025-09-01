using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Byte___Brew.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdmin2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$4sYqcN01MICgyeJNFlc93.SydTETf8gyAIMVrNd2.xckX74146Yfi");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$9q5MZ5Y3zI5pPYGfX7AiPuvQ.4y2x8TmOjG7PkP7fWtFf7Uk1s2a6");
        }
    }
}
