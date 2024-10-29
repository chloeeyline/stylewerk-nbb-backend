using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StyleWerk.NBB.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEncrypted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEncrypted",
                table: "Structure_Entry");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEncrypted",
                table: "Structure_Entry",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
