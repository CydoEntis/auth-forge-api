using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminIdToAdminRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Token",
                table: "admin_refresh_tokens",
                newName: "token");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "admin_refresh_tokens",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UserAgent",
                table: "admin_refresh_tokens",
                newName: "user_agent");

            migrationBuilder.RenameColumn(
                name: "UsedAtUtc",
                table: "admin_refresh_tokens",
                newName: "used_at_utc");

            migrationBuilder.RenameColumn(
                name: "RevokedAtUtc",
                table: "admin_refresh_tokens",
                newName: "revoked_at_utc");

            migrationBuilder.RenameColumn(
                name: "ReplacedByToken",
                table: "admin_refresh_tokens",
                newName: "replaced_by_token");

            migrationBuilder.RenameColumn(
                name: "IpAddress",
                table: "admin_refresh_tokens",
                newName: "ip_address");

            migrationBuilder.RenameColumn(
                name: "ExpiresAtUtc",
                table: "admin_refresh_tokens",
                newName: "expires_at_utc");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "admin_refresh_tokens",
                newName: "created_at_utc");

            migrationBuilder.RenameIndex(
                name: "IX_admin_refresh_tokens_Token",
                table: "admin_refresh_tokens",
                newName: "IX_admin_refresh_tokens_token");

            migrationBuilder.AddColumn<Guid>(
                name: "admin_id",
                table: "admin_refresh_tokens",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "admin_id",
                table: "admin_refresh_tokens");

            migrationBuilder.RenameColumn(
                name: "token",
                table: "admin_refresh_tokens",
                newName: "Token");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "admin_refresh_tokens",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "user_agent",
                table: "admin_refresh_tokens",
                newName: "UserAgent");

            migrationBuilder.RenameColumn(
                name: "used_at_utc",
                table: "admin_refresh_tokens",
                newName: "UsedAtUtc");

            migrationBuilder.RenameColumn(
                name: "revoked_at_utc",
                table: "admin_refresh_tokens",
                newName: "RevokedAtUtc");

            migrationBuilder.RenameColumn(
                name: "replaced_by_token",
                table: "admin_refresh_tokens",
                newName: "ReplacedByToken");

            migrationBuilder.RenameColumn(
                name: "ip_address",
                table: "admin_refresh_tokens",
                newName: "IpAddress");

            migrationBuilder.RenameColumn(
                name: "expires_at_utc",
                table: "admin_refresh_tokens",
                newName: "ExpiresAtUtc");

            migrationBuilder.RenameColumn(
                name: "created_at_utc",
                table: "admin_refresh_tokens",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameIndex(
                name: "IX_admin_refresh_tokens_token",
                table: "admin_refresh_tokens",
                newName: "IX_admin_refresh_tokens_Token");
        }
    }
}
