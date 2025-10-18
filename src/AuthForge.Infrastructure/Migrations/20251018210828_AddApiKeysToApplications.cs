using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApiKeysToApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "settings",
                table: "applications");

            migrationBuilder.AddColumn<int>(
                name: "access_token_expiration_minutes",
                table: "applications",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "lockout_duration_minutes",
                table: "applications",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "max_failed_login_attempts",
                table: "applications",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "public_key",
                table: "applications",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "refresh_token_expiration_days",
                table: "applications",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "secret_key",
                table: "applications",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_applications_public_key",
                table: "applications",
                column: "public_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_applications_secret_key",
                table: "applications",
                column: "secret_key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_applications_public_key",
                table: "applications");

            migrationBuilder.DropIndex(
                name: "IX_applications_secret_key",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "access_token_expiration_minutes",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "lockout_duration_minutes",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "max_failed_login_attempts",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "public_key",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "refresh_token_expiration_days",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "secret_key",
                table: "applications");

            migrationBuilder.AddColumn<string>(
                name: "settings",
                table: "applications",
                type: "TEXT",
                nullable: false,
                defaultValue: "{}");
        }
    }
}
