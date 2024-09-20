using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StyleWerk.NBB.Migrations
{
	/// <inheritdoc />
	public partial class Test : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<Guid>(
				name: "UserID",
				table: "Structure_Entry_Folder",
				type: "uuid",
				nullable: false);

			migrationBuilder.CreateIndex(
				name: "IX_Structure_Entry_Folder_UserID",
				table: "Structure_Entry_Folder",
				column: "UserID");

			migrationBuilder.AddForeignKey(
				name: "FK_Structure_Entry_Folder_User_Login_UserID",
				table: "Structure_Entry_Folder",
				column: "UserID",
				principalTable: "User_Login",
				principalColumn: "ID",
				onDelete: ReferentialAction.Cascade);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_Structure_Entry_Folder_User_Login_UserID",
				table: "Structure_Entry_Folder");

			migrationBuilder.DropIndex(
				name: "IX_Structure_Entry_Folder_UserID",
				table: "Structure_Entry_Folder");

			migrationBuilder.DropColumn(
				name: "UserID",
				table: "Structure_Entry_Folder");
		}
	}
}
