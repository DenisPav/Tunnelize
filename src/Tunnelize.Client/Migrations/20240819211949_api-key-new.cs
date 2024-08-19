using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tunnelize.Client.Migrations
{
    /// <inheritdoc />
    public partial class apikeynew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ApiKey_Value",
                table: "ApiKey",
                column: "Value",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ApiKey_Value",
                table: "ApiKey");
        }
    }
}
