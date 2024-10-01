using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StyleWerk.NBB.Migrations
{
    /// <inheritdoc />
    public partial class RemovedUserRights : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "User",
                table: "User_Information");

            migrationBuilder.DropTable(
                name: "User_Right");

            migrationBuilder.DropTable(
                name: "Right");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Right",
                columns: table => new
                {
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Right", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "User_Right",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User_Right", x => new { x.ID, x.Name });
                    table.ForeignKey(
                        name: "FK_User_Right_Right_Name",
                        column: x => x.Name,
                        principalTable: "Right",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "User",
                        column: x => x.ID,
                        principalTable: "User_Login",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_User_Right_Name",
                table: "User_Right",
                column: "Name");
        }
    }
}
