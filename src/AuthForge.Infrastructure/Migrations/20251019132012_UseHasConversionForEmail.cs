using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UseHasConversionForEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_end_users_email",
                table: "end_users");

            migrationBuilder.CreateIndex(
                name: "IX_end_users_email",
                table: "end_users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_end_users_email",
                table: "end_users");

            migrationBuilder.CreateIndex(
                name: "IX_end_users_email",
                table: "end_users",
                column: "email");
        }
    }
}
