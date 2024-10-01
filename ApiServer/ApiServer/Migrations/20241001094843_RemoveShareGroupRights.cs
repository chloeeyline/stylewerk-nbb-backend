using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StyleWerk.NBB.Migrations
{
    /// <inheritdoc />
    public partial class RemoveShareGroupRights : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Share_GroupUser_User_Login_WhoAdded",
                table: "Share_GroupUser");

            migrationBuilder.DropForeignKey(
                name: "FK_Share_Item_User_Login_UserID",
                table: "Share_Item");

            migrationBuilder.DropIndex(
                name: "IX_Share_Item_UserID",
                table: "Share_Item");

            migrationBuilder.DropIndex(
                name: "IX_Share_GroupUser_WhoAdded",
                table: "Share_GroupUser");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Share_Item");

            migrationBuilder.DropColumn(
                name: "CanAddUsers",
                table: "Share_GroupUser");

            migrationBuilder.DropColumn(
                name: "CanRemoveUsers",
                table: "Share_GroupUser");

            migrationBuilder.DropColumn(
                name: "WhoAdded",
                table: "Share_GroupUser");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserID",
                table: "Share_Item",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "CanAddUsers",
                table: "Share_GroupUser",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanRemoveUsers",
                table: "Share_GroupUser",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "WhoAdded",
                table: "Share_GroupUser",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Share_Item_UserID",
                table: "Share_Item",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Share_GroupUser_WhoAdded",
                table: "Share_GroupUser",
                column: "WhoAdded");

            migrationBuilder.AddForeignKey(
                name: "FK_Share_GroupUser_User_Login_WhoAdded",
                table: "Share_GroupUser",
                column: "WhoAdded",
                principalTable: "User_Login",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Share_Item_User_Login_UserID",
                table: "Share_Item",
                column: "UserID",
                principalTable: "User_Login",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
