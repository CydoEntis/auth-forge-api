using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admin_refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    admin_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    token = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    expires_at_utc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    revoked_at_utc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    used_at_utc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    replaced_by_token = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ip_address = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_refresh_tokens", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "admins",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    password_salt = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    is_email_verified = table.Column<bool>(type: "INTEGER", nullable: false),
                    failed_login_attempts = table.Column<int>(type: "INTEGER", nullable: false),
                    locked_out_until = table.Column<DateTime>(type: "TEXT", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    last_login_at_utc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    password_reset_token = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    password_reset_token_expires_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admins", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "applications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    slug = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    public_key = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    secret_key = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    deactivated_at_utc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    max_failed_login_attempts = table.Column<int>(type: "INTEGER", nullable: false),
                    lockout_duration_minutes = table.Column<int>(type: "INTEGER", nullable: false),
                    access_token_expiration_minutes = table.Column<int>(type: "INTEGER", nullable: false),
                    refresh_token_expiration_days = table.Column<int>(type: "INTEGER", nullable: false),
                    email_provider = table.Column<int>(type: "INTEGER", nullable: true),
                    email_api_key = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    email_from_email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    email_from_name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    email_password_reset_callback_url = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    email_verification_callback_url = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    allowed_origins = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_applications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    application_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    event_type = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    performed_by = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    target_user_id = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    details = table.Column<string>(type: "TEXT", nullable: false),
                    ip_address = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    timestamp = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "end_users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    application_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    password_salt = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    first_name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    is_email_verified = table.Column<bool>(type: "INTEGER", nullable: false),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false),
                    failed_login_attempts = table.Column<int>(type: "INTEGER", nullable: false),
                    locked_out_until = table.Column<DateTime>(type: "TEXT", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    last_login_at_utc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    email_verification_token = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    email_verification_token_expires_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PasswordResetToken = table.Column<string>(type: "TEXT", nullable: true),
                    PasswordResetTokenExpiresAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_end_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_end_users_applications_application_id",
                        column: x => x.application_id,
                        principalTable: "applications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "end_user_password_reset_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    token = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    expires_at_utc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    is_used = table.Column<bool>(type: "INTEGER", nullable: false),
                    used_at_utc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_end_user_password_reset_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_end_user_password_reset_tokens_end_users_user_id",
                        column: x => x.user_id,
                        principalTable: "end_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "end_user_refresh_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "TEXT", nullable: false),
                    user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    token = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    expires_at_utc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    revoked_at_utc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    used_at_utc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    replaced_by_token = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ip_address = table.Column<string>(type: "TEXT", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_end_user_refresh_tokens", x => x.id);
                    table.ForeignKey(
                        name: "FK_end_user_refresh_tokens_end_users_user_id",
                        column: x => x.user_id,
                        principalTable: "end_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_admin_refresh_tokens_token",
                table: "admin_refresh_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_admins_email",
                table: "admins",
                column: "email",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_applications_slug",
                table: "applications",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_application_id",
                table: "audit_logs",
                column: "application_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_event_type",
                table: "audit_logs",
                column: "event_type");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_target_user_id",
                table: "audit_logs",
                column: "target_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_timestamp",
                table: "audit_logs",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_end_user_password_reset_tokens_token",
                table: "end_user_password_reset_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_end_user_password_reset_tokens_user_id",
                table: "end_user_password_reset_tokens",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_end_user_refresh_tokens_token",
                table: "end_user_refresh_tokens",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_end_user_refresh_tokens_user_id",
                table: "end_user_refresh_tokens",
                column: "user_id");

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
            migrationBuilder.DropTable(
                name: "admin_refresh_tokens");

            migrationBuilder.DropTable(
                name: "admins");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "end_user_password_reset_tokens");

            migrationBuilder.DropTable(
                name: "end_user_refresh_tokens");

            migrationBuilder.DropTable(
                name: "end_users");

            migrationBuilder.DropTable(
                name: "applications");
        }
    }
}
