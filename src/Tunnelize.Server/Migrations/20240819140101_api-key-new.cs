using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tunnelize.Server.Migrations
{
    /// <inheritdoc />
    public partial class apikeynew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("49fbcf0c-e9f0-487d-9bae-e7bc135760cb"));

            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "ApiKey",
                type: "TEXT",
                fixedLength: true,
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "Email", "PasswordHash" },
                values: new object[] { new Guid("6f085383-169c-432a-850b-fe7573ed16e6"), "test@email.com", "AQAAAAIAAYagAAAAENDbkRIeCytZXQTBw4dWGgoFvlkq2yZKfx9n1OtFNQr+rHBkbMa2T5Zi2NlCuEm6zw==" });

            migrationBuilder.CreateIndex(
                name: "IX_ApiKey_Key",
                table: "ApiKey",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApiKey_Key",
                table: "ApiKey");

            migrationBuilder.DeleteData(
                table: "User",
                keyColumn: "Id",
                keyValue: new Guid("6f085383-169c-432a-850b-fe7573ed16e6"));

            migrationBuilder.DropColumn(
                name: "Key",
                table: "ApiKey");

            migrationBuilder.InsertData(
                table: "User",
                columns: new[] { "Id", "Email", "PasswordHash" },
                values: new object[] { new Guid("49fbcf0c-e9f0-487d-9bae-e7bc135760cb"), "test@email.com", "AQAAAAIAAYagAAAAENDbkRIeCytZXQTBw4dWGgoFvlkq2yZKfx9n1OtFNQr+rHBkbMa2T5Zi2NlCuEm6zw==" });
        }
    }
}
