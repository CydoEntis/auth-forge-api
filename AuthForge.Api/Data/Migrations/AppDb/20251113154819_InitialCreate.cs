using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthForge.Api.Data.Migrations.AppDb
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admins", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "applications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    PublicKey = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SecretKey = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    JwtSecret = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    AllowedOrigins = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    MaxFailedLoginAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                    LockoutDurationMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    AccessTokenExpirationMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    RefreshTokenExpirationDays = table.Column<int>(type: "INTEGER", nullable: false),
                    EmailProvider = table.Column<string>(type: "TEXT", nullable: true),
                    EmailApiKey = table.Column<string>(type: "TEXT", nullable: true),
                    FromEmail = table.Column<string>(type: "TEXT", nullable: true),
                    FromName = table.Column<string>(type: "TEXT", nullable: true),
                    PasswordResetCallbackUrl = table.Column<string>(type: "TEXT", nullable: true),
                    EmailVerificationCallbackUrl = table.Column<string>(type: "TEXT", nullable: true),
                    GoogleEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    GoogleClientId = table.Column<string>(type: "TEXT", nullable: true),
                    GoogleClientSecret = table.Column<string>(type: "TEXT", nullable: true),
                    GithubEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    GithubClientId = table.Column<string>(type: "TEXT", nullable: true),
                    GithubClientSecret = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_applications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AdminId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Action = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Details = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "admin_password_reset_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AdminId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Token = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsUsed = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_password_reset_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_admin_password_reset_tokens_admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "admin_refresh_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AdminId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Token = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsRevoked = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_refresh_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_admin_refresh_tokens_admins_AdminId",
                        column: x => x.AdminId,
                        principalTable: "admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "end_users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    EmailVerified = table.Column<bool>(type: "INTEGER", nullable: false),
                    EmailVerificationToken = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    EmailVerificationTokenExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FailedLoginAttempts = table.Column<int>(type: "INTEGER", nullable: false),
                    LockedOutUntil = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_end_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_end_users_applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "end_user_password_reset_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EndUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Token = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsUsed = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_end_user_password_reset_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_end_user_password_reset_tokens_end_users_EndUserId",
                        column: x => x.EndUserId,
                        principalTable: "end_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "end_user_refresh_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EndUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Token = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsRevoked = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_end_user_refresh_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_end_user_refresh_tokens_end_users_EndUserId",
                        column: x => x.EndUserId,
                        principalTable: "end_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_admin_password_reset_tokens_AdminId",
                table: "admin_password_reset_tokens",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_admin_password_reset_tokens_Token",
                table: "admin_password_reset_tokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_admin_refresh_tokens_AdminId",
                table: "admin_refresh_tokens",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_admin_refresh_tokens_Token",
                table: "admin_refresh_tokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_admins_Email",
                table: "admins",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_applications_PublicKey",
                table: "applications",
                column: "PublicKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_applications_Slug",
                table: "applications",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_AdminId",
                table: "audit_logs",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_ApplicationId",
                table: "audit_logs",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_ApplicationId_CreatedAtUtc",
                table: "audit_logs",
                columns: new[] { "ApplicationId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_CreatedAtUtc",
                table: "audit_logs",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_UserId",
                table: "audit_logs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_end_user_password_reset_tokens_EndUserId",
                table: "end_user_password_reset_tokens",
                column: "EndUserId");

            migrationBuilder.CreateIndex(
                name: "IX_end_user_password_reset_tokens_Token",
                table: "end_user_password_reset_tokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_end_user_refresh_tokens_EndUserId",
                table: "end_user_refresh_tokens",
                column: "EndUserId");

            migrationBuilder.CreateIndex(
                name: "IX_end_user_refresh_tokens_Token",
                table: "end_user_refresh_tokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_end_users_ApplicationId_Email",
                table: "end_users",
                columns: new[] { "ApplicationId", "Email" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_password_reset_tokens");

            migrationBuilder.DropTable(
                name: "admin_refresh_tokens");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "end_user_password_reset_tokens");

            migrationBuilder.DropTable(
                name: "end_user_refresh_tokens");

            migrationBuilder.DropTable(
                name: "admins");

            migrationBuilder.DropTable(
                name: "end_users");

            migrationBuilder.DropTable(
                name: "applications");
        }
    }
}
