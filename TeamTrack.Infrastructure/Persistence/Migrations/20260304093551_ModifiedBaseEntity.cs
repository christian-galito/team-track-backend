using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TeamTrack.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedBaseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_user_roles",
                table: "user_roles");

            migrationBuilder.RenameIndex(
                name: "IX_users_user_name",
                table: "users",
                newName: "i_x_users_user_name");

            migrationBuilder.RenameIndex(
                name: "IX_users_email",
                table: "users",
                newName: "i_x_users_email");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_date",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_date",
                table: "user_credentials",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "user_credentials",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_date",
                table: "roles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "roles",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "p_k_user_roles",
                table: "user_roles",
                columns: new[] { "user_id", "role_id" });

            migrationBuilder.CreateIndex(
                name: "i_x_roles_name",
                table: "roles",
                column: "name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "p_k_user_roles",
                table: "user_roles");

            migrationBuilder.DropIndex(
                name: "i_x_roles_name",
                table: "roles");

            migrationBuilder.DropColumn(
                name: "deleted_date",
                table: "users");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "users");

            migrationBuilder.DropColumn(
                name: "deleted_date",
                table: "user_credentials");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "user_credentials");

            migrationBuilder.DropColumn(
                name: "deleted_date",
                table: "roles");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "roles");

            migrationBuilder.RenameIndex(
                name: "i_x_users_user_name",
                table: "users",
                newName: "IX_users_user_name");

            migrationBuilder.RenameIndex(
                name: "i_x_users_email",
                table: "users",
                newName: "IX_users_email");

            migrationBuilder.AddPrimaryKey(
                name: "PK_user_roles",
                table: "user_roles",
                columns: new[] { "user_id", "role_id" });
        }
    }
}
