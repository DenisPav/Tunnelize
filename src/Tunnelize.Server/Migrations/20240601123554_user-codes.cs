using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tunnelize.Server.Migrations
{
    /// <inheritdoc />
    public partial class usercodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("47a63e63-a677-47ac-92fa-f74712ac6a60"));

            migrationBuilder.CreateTable(
                name: "UserCode",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 6, nullable: false),
                    Expiration = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCode_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "Email", "PasswordHash" },
                values: new object[] { new Guid("49fbcf0c-e9f0-487d-9bae-e7bc135760cb"), "test@email.com", "AQAAAAIAAYagAAAAENDbkRIeCytZXQTBw4dWGgoFvlkq2yZKfx9n1OtFNQr+rHBkbMa2T5Zi2NlCuEm6zw==" });

            migrationBuilder.CreateIndex(
                name: "IX_UserCode_UserId",
                table: "UserCode",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserCode");

            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("49fbcf0c-e9f0-487d-9bae-e7bc135760cb"));

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "Email", "PasswordHash" },
                values: new object[] { new Guid("47a63e63-a677-47ac-92fa-f74712ac6a60"), "test@email.com", "AQAAAAIAAYagAAAAENDbkRIeCytZXQTBw4dWGgoFvlkq2yZKfx9n1OtFNQr+rHBkbMa2T5Zi2NlCuEm6zw==" });
        }
    }
}
