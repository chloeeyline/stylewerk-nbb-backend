using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StyleWerk.NBB.Migrations
{
    /// <inheritdoc />
    public partial class ReworkedPublicSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Structure_Template");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Structure_Entry");

            migrationBuilder.DropColumn(
                name: "Group",
                table: "Share_Item");

            migrationBuilder.AlterColumn<Guid>(
                name: "ToWhom",
                table: "Share_Item",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<int>(
                name: "Visibility",
                table: "Share_Item",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "O_WhoSharedID",
                table: "Share_GroupUser",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Share_GroupUser_O_WhoSharedID",
                table: "Share_GroupUser",
                column: "O_WhoSharedID");

            migrationBuilder.AddForeignKey(
                name: "FK_Share_GroupUser_User_Login_O_WhoSharedID",
                table: "Share_GroupUser",
                column: "O_WhoSharedID",
                principalTable: "User_Login",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Share_GroupUser_User_Login_O_WhoSharedID",
                table: "Share_GroupUser");

            migrationBuilder.DropIndex(
                name: "IX_Share_GroupUser_O_WhoSharedID",
                table: "Share_GroupUser");

            migrationBuilder.DropColumn(
                name: "Visibility",
                table: "Share_Item");

            migrationBuilder.DropColumn(
                name: "O_WhoSharedID",
                table: "Share_GroupUser");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Structure_Template",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Structure_Entry",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "ToWhom",
                table: "Share_Item",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Group",
                table: "Share_Item",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
