using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthForge.Api.Data.Migrations.ConfigDb
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "configurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IsSetupComplete = table.Column<bool>(type: "INTEGER", nullable: false),
                    AuthForgeDomain = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DatabaseProvider = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DatabaseConnectionString = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    EmailProvider = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    EmailFromAddress = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    EmailFromName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    SmtpHost = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    SmtpPort = table.Column<int>(type: "INTEGER", nullable: true),
                    SmtpUsername = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    SmtpPasswordEncrypted = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    SmtpUseSsl = table.Column<bool>(type: "INTEGER", nullable: false),
                    ResendApiKeyEncrypted = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    JwtSecretEncrypted = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configurations", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "configurations");
        }
    }
}
