using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StyleWerk.NBB.Migrations
{
    /// <inheritdoc />
    public partial class RemoveShareItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Share_GroupUser");

            migrationBuilder.DropTable(
                name: "Share_Item");

            migrationBuilder.DropTable(
                name: "Share_Group");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Share_Group",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    UserID = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NameNormalized = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Share_Group", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Share_Group_User_Login_UserID",
                        column: x => x.UserID,
                        principalTable: "User_Login",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Share_Item",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemID = table.Column<Guid>(type: "uuid", nullable: false),
                    ToWhom = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<byte>(type: "smallint", nullable: false),
                    Visibility = table.Column<byte>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Share_Item", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Share_GroupUser",
                columns: table => new
                {
                    GroupID = table.Column<Guid>(type: "uuid", nullable: false),
                    UserID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Share_GroupUser", x => new { x.GroupID, x.UserID });
                    table.ForeignKey(
                        name: "FK_Share_GroupUser_Share_Group_GroupID",
                        column: x => x.GroupID,
                        principalTable: "Share_Group",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Share_GroupUser_User_Login_UserID",
                        column: x => x.UserID,
                        principalTable: "User_Login",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Share_Group_UserID_NameNormalized",
                table: "Share_Group",
                columns: new[] { "UserID", "NameNormalized" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Share_GroupUser_UserID",
                table: "Share_GroupUser",
                column: "UserID");
        }
    }
}
