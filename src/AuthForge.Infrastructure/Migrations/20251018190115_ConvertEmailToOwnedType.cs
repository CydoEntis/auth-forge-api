using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConvertEmailToOwnedType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_end_users_application_id_email",
                table: "end_users");

            migrationBuilder.CreateIndex(
                name: "IX_end_users_application_id",
                table: "end_users",
                column: "application_id");

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
                name: "IX_end_users_application_id",
                table: "end_users");

            migrationBuilder.DropIndex(
                name: "IX_end_users_email",
                table: "end_users");

            migrationBuilder.CreateIndex(
                name: "IX_end_users_application_id_email",
                table: "end_users",
                columns: new[] { "application_id", "email" },
                unique: true);
        }
    }
}
