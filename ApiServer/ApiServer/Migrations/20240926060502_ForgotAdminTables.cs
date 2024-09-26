using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StyleWerk.NBB.Migrations
{
    /// <inheritdoc />
    public partial class ForgotAdminTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Admin_ColorTheme",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    UserID = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Base = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Data = table.Column<string>(type: "JSONB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admin_ColorTheme", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Admin_ColorTheme_User_Login_UserID",
                        column: x => x.UserID,
                        principalTable: "User_Login",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Admin_Language",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    UserID = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Data = table.Column<string>(type: "JSONB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admin_Language", x => x.Code);
                    table.ForeignKey(
                        name: "FK_Admin_Language_User_Login_UserID",
                        column: x => x.UserID,
                        principalTable: "User_Login",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Admin_ColorTheme_UserID",
                table: "Admin_ColorTheme",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Admin_Language_UserID",
                table: "Admin_Language",
                column: "UserID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admin_ColorTheme");

            migrationBuilder.DropTable(
                name: "Admin_Language");
        }
    }
}
