using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StyleWerk.NBB.Migrations
{
    /// <inheritdoc />
    public partial class ReworkedRightSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "User_Right");

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

            migrationBuilder.CreateIndex(
                name: "IX_User_Right_Name",
                table: "User_Right",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Right_Right_Name",
                table: "User_Right",
                column: "Name",
                principalTable: "Right",
                principalColumn: "Name",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Right_Right_Name",
                table: "User_Right");

            migrationBuilder.DropTable(
                name: "Right");

            migrationBuilder.DropIndex(
                name: "IX_User_Right_Name",
                table: "User_Right");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "User_Right",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
