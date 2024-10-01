using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StyleWerk.NBB.Migrations
{
    /// <inheritdoc />
    public partial class SpecifiedDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Structure_Entry_Structure_Entry_Folder_FolderID",
                table: "Structure_Entry");

            migrationBuilder.AddForeignKey(
                name: "FK_Structure_Entry_Structure_Entry_Folder_FolderID",
                table: "Structure_Entry",
                column: "FolderID",
                principalTable: "Structure_Entry_Folder",
                principalColumn: "ID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Structure_Entry_Structure_Entry_Folder_FolderID",
                table: "Structure_Entry");

            migrationBuilder.AddForeignKey(
                name: "FK_Structure_Entry_Structure_Entry_Folder_FolderID",
                table: "Structure_Entry",
                column: "FolderID",
                principalTable: "Structure_Entry_Folder",
                principalColumn: "ID");
        }
    }
}
