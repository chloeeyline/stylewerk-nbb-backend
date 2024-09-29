using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StyleWerk.NBB.Migrations
{
    /// <inheritdoc />
    public partial class SmallChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Admin_ColorTheme_User_Login_UserID",
                table: "Admin_ColorTheme");

            migrationBuilder.DropForeignKey(
                name: "FK_Admin_Language_User_Login_UserID",
                table: "Admin_Language");

            migrationBuilder.DropIndex(
                name: "IX_Admin_Language_UserID",
                table: "Admin_Language");

            migrationBuilder.DropIndex(
                name: "IX_Admin_ColorTheme_UserID",
                table: "Admin_ColorTheme");

            migrationBuilder.DropColumn(
                name: "CanSeeUsers",
                table: "Share_GroupUser");

            migrationBuilder.DropColumn(
                name: "CanSeeOthers",
                table: "Share_Group");

            migrationBuilder.DropColumn(
                name: "IsVisible",
                table: "Share_Group");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Admin_Language");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Admin_ColorTheme");

            migrationBuilder.RenameColumn(
                name: "ItemType",
                table: "Share_Item",
                newName: "Type");

            migrationBuilder.AlterColumn<byte>(
                name: "Visibility",
                table: "Share_Item",
                type: "smallint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Share_Item",
                newName: "ItemType");

            migrationBuilder.AlterColumn<int>(
                name: "Visibility",
                table: "Share_Item",
                type: "integer",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "smallint");

            migrationBuilder.AddColumn<bool>(
                name: "CanSeeUsers",
                table: "Share_GroupUser",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanSeeOthers",
                table: "Share_Group",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsVisible",
                table: "Share_Group",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "UserID",
                table: "Admin_Language",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserID",
                table: "Admin_ColorTheme",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Admin_Language_UserID",
                table: "Admin_Language",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Admin_ColorTheme_UserID",
                table: "Admin_ColorTheme",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Admin_ColorTheme_User_Login_UserID",
                table: "Admin_ColorTheme",
                column: "UserID",
                principalTable: "User_Login",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Admin_Language_User_Login_UserID",
                table: "Admin_Language",
                column: "UserID",
                principalTable: "User_Login",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
