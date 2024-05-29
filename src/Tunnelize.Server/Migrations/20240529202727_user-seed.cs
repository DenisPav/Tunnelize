using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tunnelize.Server.Migrations
{
    /// <inheritdoc />
    public partial class userseed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "Email", "PasswordHash" },
                values: new object[] { new Guid("47a63e63-a677-47ac-92fa-f74712ac6a60"), "test@email.com", "AQAAAAIAAYagAAAAENDbkRIeCytZXQTBw4dWGgoFvlkq2yZKfx9n1OtFNQr+rHBkbMa2T5Zi2NlCuEm6zw==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("47a63e63-a677-47ac-92fa-f74712ac6a60"));
        }
    }
}
