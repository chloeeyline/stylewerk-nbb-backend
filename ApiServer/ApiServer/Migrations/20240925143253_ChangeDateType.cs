using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StyleWerk.NBB.Migrations
{
    /// <inheritdoc />
    public partial class ChangeDateType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the existing columns
            migrationBuilder.DropColumn(
                name: "LastUpdatedAt",
                table: "Structure_Template");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Structure_Template");

            migrationBuilder.DropColumn(
                name: "LastUpdatedAt",
                table: "Structure_Entry");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Structure_Entry");

            // Recreate the columns with the new type and properties
            migrationBuilder.AddColumn<long>(
                name: "LastUpdatedAt",
                table: "Structure_Template",
                type: "bigint",
                nullable: false);

            migrationBuilder.AddColumn<long>(
                name: "CreatedAt",
                table: "Structure_Template",
                type: "bigint",
                nullable: false);

            migrationBuilder.AddColumn<long>(
                name: "LastUpdatedAt",
                table: "Structure_Entry",
                type: "bigint",
                nullable: false);

            migrationBuilder.AddColumn<long>(
                name: "CreatedAt",
                table: "Structure_Entry",
                type: "bigint",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "LastUpdatedAt",
                table: "Structure_Template",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Structure_Template",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "LastUpdatedAt",
                table: "Structure_Entry",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Structure_Entry",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}
