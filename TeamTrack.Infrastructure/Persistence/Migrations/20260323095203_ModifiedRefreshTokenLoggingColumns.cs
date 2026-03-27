using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamTrack.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedRefreshTokenLoggingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "replaced_by_token",
                table: "refresh_tokens",
                type: "character varying(44)",
                unicode: false,
                maxLength: 44,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ip_address",
                table: "refresh_tokens",
                type: "character varying(45)",
                unicode: false,
                maxLength: 45,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "revoked_at",
                table: "refresh_tokens",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "revoked_by",
                table: "refresh_tokens",
                type: "character varying(50)",
                unicode: false,
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "user_agent",
                table: "refresh_tokens",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ip_address",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "revoked_at",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "revoked_by",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "user_agent",
                table: "refresh_tokens");

            migrationBuilder.AlterColumn<string>(
                name: "replaced_by_token",
                table: "refresh_tokens",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(44)",
                oldUnicode: false,
                oldMaxLength: 44,
                oldNullable: true);
        }
    }
}
