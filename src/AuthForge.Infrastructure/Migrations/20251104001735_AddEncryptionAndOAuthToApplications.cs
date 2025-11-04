using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEncryptionAndOAuthToApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_applications_secret_key",
                table: "applications");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "applications",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "jwt_secret",
                table: "applications",
                type: "TEXT",
                maxLength: 500,
                nullable: true,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "oauth_github_client_id",
                table: "applications",
                type: "TEXT",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "oauth_github_client_secret",
                table: "applications",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "oauth_github_enabled",
                table: "applications",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "oauth_google_client_id",
                table: "applications",
                type: "TEXT",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "oauth_google_client_secret",
                table: "applications",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "oauth_google_enabled",
                table: "applications",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "jwt_secret",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "oauth_github_client_id",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "oauth_github_client_secret",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "oauth_github_enabled",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "oauth_google_client_id",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "oauth_google_client_secret",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "oauth_google_enabled",
                table: "applications");

            migrationBuilder.CreateIndex(
                name: "IX_applications_secret_key",
                table: "applications",
                column: "secret_key",
                unique: true);
        }
    }
}
