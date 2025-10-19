using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailSettingsToApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "email_api_key",
                table: "applications",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email_from_email",
                table: "applications",
                type: "TEXT",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email_from_name",
                table: "applications",
                type: "TEXT",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "email_provider",
                table: "applications",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "email_api_key",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "email_from_email",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "email_from_name",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "email_provider",
                table: "applications");
        }
    }
}
