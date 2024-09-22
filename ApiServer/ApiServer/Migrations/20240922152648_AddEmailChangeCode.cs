using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StyleWerk.NBB.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailChangeCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailChangeCode",
                table: "User_Login",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailChangeCode",
                table: "User_Login");
        }
    }
}
