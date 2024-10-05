using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StyleWerk.NBB.Migrations
{
    /// <inheritdoc />
    public partial class NameNormalize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Structure_Template_UserID",
                table: "Structure_Template");

            migrationBuilder.DropIndex(
                name: "IX_Structure_Entry_Folder_UserID",
                table: "Structure_Entry_Folder");

            migrationBuilder.DropIndex(
                name: "IX_Structure_Entry_UserID",
                table: "Structure_Entry");

            migrationBuilder.DropIndex(
                name: "IX_Share_Group_UserID",
                table: "Share_Group");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:CollationDefinition:und-ci", "und,und,icu,False")
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.AlterColumn<string>(
                name: "UsernameNormalized",
                table: "User_Login",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                collation: "und-ci",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "User_Login",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                collation: "und-ci",
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "EmailNormalized",
                table: "User_Login",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                collation: "und-ci",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "User_Login",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                collation: "und-ci",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Structure_Template",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                collation: "und-ci",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "NameNormalized",
                table: "Structure_Template",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                collation: "und-ci");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Structure_Entry_Folder",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                collation: "und-ci",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "NameNormalized",
                table: "Structure_Entry_Folder",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                collation: "und-ci");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Structure_Entry",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                collation: "und-ci",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "NameNormalized",
                table: "Structure_Entry",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                collation: "und-ci");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Share_Group",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                collation: "und-ci",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "NameNormalized",
                table: "Share_Group",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                collation: "und-ci");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Admin_Language",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                collation: "und-ci",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "NameNormalized",
                table: "Admin_Language",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                collation: "und-ci");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Admin_ColorTheme",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                collation: "und-ci",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "NameNormalized",
                table: "Admin_ColorTheme",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                collation: "und-ci");

            migrationBuilder.CreateIndex(
                name: "IX_Structure_Template_UserID_NameNormalized",
                table: "Structure_Template",
                columns: new[] { "UserID", "NameNormalized" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Structure_Entry_Folder_UserID_NameNormalized",
                table: "Structure_Entry_Folder",
                columns: new[] { "UserID", "NameNormalized" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Structure_Entry_UserID_NameNormalized",
                table: "Structure_Entry",
                columns: new[] { "UserID", "NameNormalized" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Share_Group_UserID_NameNormalized",
                table: "Share_Group",
                columns: new[] { "UserID", "NameNormalized" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Structure_Template_UserID_NameNormalized",
                table: "Structure_Template");

            migrationBuilder.DropIndex(
                name: "IX_Structure_Entry_Folder_UserID_NameNormalized",
                table: "Structure_Entry_Folder");

            migrationBuilder.DropIndex(
                name: "IX_Structure_Entry_UserID_NameNormalized",
                table: "Structure_Entry");

            migrationBuilder.DropIndex(
                name: "IX_Share_Group_UserID_NameNormalized",
                table: "Share_Group");

            migrationBuilder.DropColumn(
                name: "NameNormalized",
                table: "Structure_Template");

            migrationBuilder.DropColumn(
                name: "NameNormalized",
                table: "Structure_Entry_Folder");

            migrationBuilder.DropColumn(
                name: "NameNormalized",
                table: "Structure_Entry");

            migrationBuilder.DropColumn(
                name: "NameNormalized",
                table: "Share_Group");

            migrationBuilder.DropColumn(
                name: "NameNormalized",
                table: "Admin_Language");

            migrationBuilder.DropColumn(
                name: "NameNormalized",
                table: "Admin_ColorTheme");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,")
                .OldAnnotation("Npgsql:CollationDefinition:und-ci", "und,und,icu,False")
                .OldAnnotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.AlterColumn<string>(
                name: "UsernameNormalized",
                table: "User_Login",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldCollation: "und-ci");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "User_Login",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldCollation: "und-ci");

            migrationBuilder.AlterColumn<string>(
                name: "EmailNormalized",
                table: "User_Login",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldCollation: "und-ci");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "User_Login",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldCollation: "und-ci");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Structure_Template",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldCollation: "und-ci");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Structure_Entry_Folder",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldCollation: "und-ci");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Structure_Entry",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldCollation: "und-ci");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Share_Group",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldCollation: "und-ci");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Admin_Language",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldCollation: "und-ci");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Admin_ColorTheme",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldCollation: "und-ci");

            migrationBuilder.CreateIndex(
                name: "IX_Structure_Template_UserID",
                table: "Structure_Template",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Structure_Entry_Folder_UserID",
                table: "Structure_Entry_Folder",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Structure_Entry_UserID",
                table: "Structure_Entry",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Share_Group_UserID",
                table: "Share_Group",
                column: "UserID");
        }
    }
}
