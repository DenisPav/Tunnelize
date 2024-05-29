using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tunnelize.Server.Migrations
{
    /// <inheritdoc />
    public partial class userapikeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM ApiKey");
            
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "ApiKey",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ApiKey_UserId",
                table: "ApiKey",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiKey_User_UserId",
                table: "ApiKey",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiKey_User_UserId",
                table: "ApiKey");

            migrationBuilder.DropIndex(
                name: "IX_ApiKey_UserId",
                table: "ApiKey");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ApiKey");
        }
    }
}
