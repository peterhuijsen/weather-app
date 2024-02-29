using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace App.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OneTimeCredentials",
                columns: table => new
                {
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OneTimeCredentials", x => x.Uuid);
                });

            migrationBuilder.CreateTable(
                name: "Credentials",
                columns: table => new
                {
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    MFA = table.Column<bool>(type: "boolean", nullable: false),
                    Hash = table.Column<string>(type: "text", nullable: true),
                    OTPUuid = table.Column<Guid>(type: "uuid", nullable: false),
                    Google = table.Column<string>(type: "text", nullable: true),
                    Microsoft = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Credentials", x => x.Uuid);
                    table.ForeignKey(
                        name: "FK_Credentials_OneTimeCredentials_OTPUuid",
                        column: x => x.OTPUuid,
                        principalTable: "OneTimeCredentials",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HashOneTimeCredentials",
                columns: table => new
                {
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    Secret = table.Column<string>(type: "text", nullable: false),
                    Counter = table.Column<long>(type: "bigint", nullable: false),
                    OneTimeCredentialUuid = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HashOneTimeCredentials", x => x.Uuid);
                    table.ForeignKey(
                        name: "FK_HashOneTimeCredentials_OneTimeCredentials_OneTimeCredential~",
                        column: x => x.OneTimeCredentialUuid,
                        principalTable: "OneTimeCredentials",
                        principalColumn: "Uuid");
                });

            migrationBuilder.CreateTable(
                name: "TimeOneTimeCredentials",
                columns: table => new
                {
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    Secret = table.Column<string>(type: "text", nullable: false),
                    OneTimeCredentialUuid = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeOneTimeCredentials", x => x.Uuid);
                    table.ForeignKey(
                        name: "FK_TimeOneTimeCredentials_OneTimeCredentials_OneTimeCredential~",
                        column: x => x.OneTimeCredentialUuid,
                        principalTable: "OneTimeCredentials",
                        principalColumn: "Uuid");
                });

            migrationBuilder.CreateTable(
                name: "PasskeyCredentials",
                columns: table => new
                {
                    Id = table.Column<byte[]>(type: "bytea", nullable: false),
                    PublicKey = table.Column<byte[]>(type: "bytea", nullable: true),
                    CredentialsUuid = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasskeyCredentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PasskeyCredentials_Credentials_CredentialsUuid",
                        column: x => x.CredentialsUuid,
                        principalTable: "Credentials",
                        principalColumn: "Uuid");
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Verified = table.Column<bool>(type: "boolean", nullable: false),
                    CredentialsUuid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Uuid);
                    table.ForeignKey(
                        name: "FK_Users_Credentials_CredentialsUuid",
                        column: x => x.CredentialsUuid,
                        principalTable: "Credentials",
                        principalColumn: "Uuid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Credentials_OTPUuid",
                table: "Credentials",
                column: "OTPUuid");

            migrationBuilder.CreateIndex(
                name: "IX_HashOneTimeCredentials_OneTimeCredentialUuid",
                table: "HashOneTimeCredentials",
                column: "OneTimeCredentialUuid");

            migrationBuilder.CreateIndex(
                name: "IX_PasskeyCredentials_CredentialsUuid",
                table: "PasskeyCredentials",
                column: "CredentialsUuid");

            migrationBuilder.CreateIndex(
                name: "IX_TimeOneTimeCredentials_OneTimeCredentialUuid",
                table: "TimeOneTimeCredentials",
                column: "OneTimeCredentialUuid");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CredentialsUuid",
                table: "Users",
                column: "CredentialsUuid");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HashOneTimeCredentials");

            migrationBuilder.DropTable(
                name: "PasskeyCredentials");

            migrationBuilder.DropTable(
                name: "TimeOneTimeCredentials");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Credentials");

            migrationBuilder.DropTable(
                name: "OneTimeCredentials");
        }
    }
}
