using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StyleWerk.NBB.Migrations
{
    /// <inheritdoc />
    public partial class AddedRelationShips : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Share_GroupUser_User_Login_O_WhoSharedID",
                table: "Share_GroupUser");

            migrationBuilder.DropIndex(
                name: "IX_Share_GroupUser_O_WhoSharedID",
                table: "Share_GroupUser");

            migrationBuilder.DropColumn(
                name: "O_WhoSharedID",
                table: "Share_GroupUser");

            migrationBuilder.RenameColumn(
                name: "WhoShared",
                table: "Share_Item",
                newName: "UserID");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.RenameColumn(
                name: "UserID",
                table: "Share_Item",
                newName: "WhoShared");

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
    }
}
