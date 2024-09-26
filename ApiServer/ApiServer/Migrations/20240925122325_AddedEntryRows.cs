using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StyleWerk.NBB.Migrations
{
    /// <inheritdoc />
    public partial class AddedEntryRows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Structure_Entry_Cell_Structure_Entry_EntryID",
                table: "Structure_Entry_Cell");

            migrationBuilder.DropForeignKey(
                name: "FK_Structure_Entry_Cell_Structure_Template_Cell_CellID",
                table: "Structure_Entry_Cell");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Structure_Entry_Cell",
                table: "Structure_Entry_Cell");

            migrationBuilder.RenameColumn(
                name: "CellID",
                table: "Structure_Entry_Cell",
                newName: "TemplateID");

            migrationBuilder.RenameColumn(
                name: "EntryID",
                table: "Structure_Entry_Cell",
                newName: "RowID");

            migrationBuilder.RenameIndex(
                name: "IX_Structure_Entry_Cell_CellID",
                table: "Structure_Entry_Cell",
                newName: "IX_Structure_Entry_Cell_TemplateID");

            migrationBuilder.AddColumn<bool>(
                name: "CanRepeat",
                table: "Structure_Template_Row",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HideOnNoInput",
                table: "Structure_Template_Row",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ID",
                table: "Structure_Entry_Cell",
                type: "uuid",
                nullable: false,
                defaultValueSql: "uuid_generate_v4()");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Structure_Entry_Cell",
                table: "Structure_Entry_Cell",
                column: "ID");

            migrationBuilder.CreateTable(
                name: "Structure_Entry_Row",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    EntryID = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateID = table.Column<Guid>(type: "uuid", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Structure_Entry_Row", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Structure_Entry_Row_Structure_Entry_EntryID",
                        column: x => x.EntryID,
                        principalTable: "Structure_Entry",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Structure_Entry_Row_Structure_Template_Row_TemplateID",
                        column: x => x.TemplateID,
                        principalTable: "Structure_Template_Row",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Structure_Entry_Cell_RowID",
                table: "Structure_Entry_Cell",
                column: "RowID");

            migrationBuilder.CreateIndex(
                name: "IX_Structure_Entry_Row_EntryID",
                table: "Structure_Entry_Row",
                column: "EntryID");

            migrationBuilder.CreateIndex(
                name: "IX_Structure_Entry_Row_TemplateID",
                table: "Structure_Entry_Row",
                column: "TemplateID");

            migrationBuilder.AddForeignKey(
                name: "FK_Structure_Entry_Cell_Structure_Entry_Row_RowID",
                table: "Structure_Entry_Cell",
                column: "RowID",
                principalTable: "Structure_Entry_Row",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Structure_Entry_Cell_Structure_Template_Cell_TemplateID",
                table: "Structure_Entry_Cell",
                column: "TemplateID",
                principalTable: "Structure_Template_Cell",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Structure_Entry_Cell_Structure_Entry_Row_RowID",
                table: "Structure_Entry_Cell");

            migrationBuilder.DropForeignKey(
                name: "FK_Structure_Entry_Cell_Structure_Template_Cell_TemplateID",
                table: "Structure_Entry_Cell");

            migrationBuilder.DropTable(
                name: "Structure_Entry_Row");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Structure_Entry_Cell",
                table: "Structure_Entry_Cell");

            migrationBuilder.DropIndex(
                name: "IX_Structure_Entry_Cell_RowID",
                table: "Structure_Entry_Cell");

            migrationBuilder.DropColumn(
                name: "CanRepeat",
                table: "Structure_Template_Row");

            migrationBuilder.DropColumn(
                name: "HideOnNoInput",
                table: "Structure_Template_Row");

            migrationBuilder.DropColumn(
                name: "ID",
                table: "Structure_Entry_Cell");

            migrationBuilder.RenameColumn(
                name: "TemplateID",
                table: "Structure_Entry_Cell",
                newName: "CellID");

            migrationBuilder.RenameColumn(
                name: "RowID",
                table: "Structure_Entry_Cell",
                newName: "EntryID");

            migrationBuilder.RenameIndex(
                name: "IX_Structure_Entry_Cell_TemplateID",
                table: "Structure_Entry_Cell",
                newName: "IX_Structure_Entry_Cell_CellID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Structure_Entry_Cell",
                table: "Structure_Entry_Cell",
                columns: ["EntryID", "CellID"]);

            migrationBuilder.AddForeignKey(
                name: "FK_Structure_Entry_Cell_Structure_Entry_EntryID",
                table: "Structure_Entry_Cell",
                column: "EntryID",
                principalTable: "Structure_Entry",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Structure_Entry_Cell_Structure_Template_Cell_CellID",
                table: "Structure_Entry_Cell",
                column: "CellID",
                principalTable: "Structure_Template_Cell",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
