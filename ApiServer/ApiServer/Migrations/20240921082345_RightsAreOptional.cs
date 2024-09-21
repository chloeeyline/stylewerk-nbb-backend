using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StyleWerk.NBB.Migrations
{
    /// <inheritdoc />
    public partial class RightsAreOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_User_Right",
                table: "User_Right");

            migrationBuilder.DropColumn(
                name: "Admin",
                table: "User_Right");

            migrationBuilder.RenameColumn(
                name: "StatusTokenTime",
                table: "User_Login",
                newName: "StatusTokenExpireTime");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "User_Right",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "User_Right",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Admin",
                table: "User_Login",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "ID",
                table: "User_Information",
                type: "uuid",
                nullable: false,
                defaultValueSql: "uuid_generate_v4()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User_Right",
                table: "User_Right",
                columns: new[] { "ID", "Name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_User_Right",
                table: "User_Right");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "User_Right");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "User_Right");

            migrationBuilder.DropColumn(
                name: "Admin",
                table: "User_Login");

            migrationBuilder.RenameColumn(
                name: "StatusTokenExpireTime",
                table: "User_Login",
                newName: "StatusTokenTime");

            migrationBuilder.AddColumn<bool>(
                name: "Admin",
                table: "User_Right",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "ID",
                table: "User_Information",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "uuid_generate_v4()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User_Right",
                table: "User_Right",
                column: "ID");
        }
    }
}
