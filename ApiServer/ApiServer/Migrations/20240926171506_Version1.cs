using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StyleWerk.NBB.Migrations
{
    /// <inheritdoc />
    public partial class Version1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

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

            migrationBuilder.CreateTable(
                name: "Share_Item",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    WhoShared = table.Column<Guid>(type: "uuid", nullable: false),
                    Group = table.Column<bool>(type: "boolean", nullable: false),
                    ItemType = table.Column<byte>(type: "smallint", nullable: false),
                    ItemID = table.Column<Guid>(type: "uuid", nullable: false),
                    ToWhom = table.Column<Guid>(type: "uuid", nullable: false),
                    CanShare = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CanEdit = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CanDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Share_Item", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "User_Login",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EmailNormalized = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Username = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    UsernameNormalized = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    PasswordSalt = table.Column<string>(type: "text", nullable: false),
                    Admin = table.Column<bool>(type: "boolean", nullable: false),
                    StatusCode = table.Column<int>(type: "integer", nullable: true),
                    StatusToken = table.Column<string>(type: "text", nullable: true),
                    StatusTokenExpireTime = table.Column<long>(type: "bigint", nullable: true),
                    NewEmail = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User_Login", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Admin_ColorTheme",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "Share_Group",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    UserID = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsVisible = table.Column<bool>(type: "boolean", nullable: false),
                    CanSeeOthers = table.Column<bool>(type: "boolean", nullable: false)
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
                name: "Structure_Entry_Folder",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    UserID = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Structure_Entry_Folder", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Structure_Entry_Folder_User_Login_UserID",
                        column: x => x.UserID,
                        principalTable: "User_Login",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Structure_Template",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    UserID = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Tags = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdatedAt = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Structure_Template", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Structure_Template_User_Login_UserID",
                        column: x => x.UserID,
                        principalTable: "User_Login",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "User_Information",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Gender = table.Column<byte>(type: "smallint", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Birthday = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User_Information", x => x.ID);
                    table.ForeignKey(
                        name: "User",
                        column: x => x.ID,
                        principalTable: "User_Login",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "User_Right",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User_Right", x => new { x.ID, x.Name });
                    table.ForeignKey(
                        name: "FK_User_Right_Right_Name",
                        column: x => x.Name,
                        principalTable: "Right",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "User",
                        column: x => x.ID,
                        principalTable: "User_Login",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "User_Token",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Agent = table.Column<string>(type: "text", nullable: false),
                    RefreshToken = table.Column<string>(type: "text", nullable: false),
                    RefreshTokenExpiryTime = table.Column<long>(type: "bigint", nullable: false),
                    ConsistOverSession = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User_Token", x => new { x.ID, x.Agent });
                    table.ForeignKey(
                        name: "User",
                        column: x => x.ID,
                        principalTable: "User_Login",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Share_GroupUser",
                columns: table => new
                {
                    GroupID = table.Column<Guid>(type: "uuid", nullable: false),
                    UserID = table.Column<Guid>(type: "uuid", nullable: false),
                    WhoAdded = table.Column<Guid>(type: "uuid", nullable: false),
                    CanSeeUsers = table.Column<bool>(type: "boolean", nullable: false),
                    CanAddUsers = table.Column<bool>(type: "boolean", nullable: false),
                    CanRemoveUsers = table.Column<bool>(type: "boolean", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Structure_Entry",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UserID = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateID = table.Column<Guid>(type: "uuid", nullable: false),
                    FolderID = table.Column<Guid>(type: "uuid", nullable: true),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    IsEncrypted = table.Column<bool>(type: "boolean", nullable: false),
                    Tags = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<long>(type: "bigint", nullable: false),
                    LastUpdatedAt = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Structure_Entry", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Structure_Entry_Structure_Entry_Folder_FolderID",
                        column: x => x.FolderID,
                        principalTable: "Structure_Entry_Folder",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_Structure_Entry_Structure_Template_TemplateID",
                        column: x => x.TemplateID,
                        principalTable: "Structure_Template",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Structure_Entry_User_Login_UserID",
                        column: x => x.UserID,
                        principalTable: "User_Login",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Structure_Template_Row",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateID = table.Column<Guid>(type: "uuid", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CanWrapCells = table.Column<bool>(type: "boolean", nullable: false),
                    CanRepeat = table.Column<bool>(type: "boolean", nullable: false),
                    HideOnNoInput = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Structure_Template_Row", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Structure_Template_Row_Structure_Template_TemplateID",
                        column: x => x.TemplateID,
                        principalTable: "Structure_Template",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Structure_Entry_Row",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "Structure_Template_Cell",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    RowID = table.Column<Guid>(type: "uuid", nullable: false),
                    InputHelper = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    HideOnEmpty = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Text = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    MetaData = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Structure_Template_Cell", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Structure_Template_Cell_Structure_Template_Row_RowID",
                        column: x => x.RowID,
                        principalTable: "Structure_Template_Row",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Structure_Entry_Cell",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    RowID = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateID = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Structure_Entry_Cell", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Structure_Entry_Cell_Structure_Entry_Row_RowID",
                        column: x => x.RowID,
                        principalTable: "Structure_Entry_Row",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Structure_Entry_Cell_Structure_Template_Cell_TemplateID",
                        column: x => x.TemplateID,
                        principalTable: "Structure_Template_Cell",
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

            migrationBuilder.CreateIndex(
                name: "IX_Share_Group_UserID",
                table: "Share_Group",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Share_GroupUser_UserID",
                table: "Share_GroupUser",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Structure_Entry_FolderID",
                table: "Structure_Entry",
                column: "FolderID");

            migrationBuilder.CreateIndex(
                name: "IX_Structure_Entry_TemplateID",
                table: "Structure_Entry",
                column: "TemplateID");

            migrationBuilder.CreateIndex(
                name: "IX_Structure_Entry_UserID",
                table: "Structure_Entry",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Structure_Entry_Cell_RowID",
                table: "Structure_Entry_Cell",
                column: "RowID");

            migrationBuilder.CreateIndex(
                name: "IX_Structure_Entry_Cell_TemplateID",
                table: "Structure_Entry_Cell",
                column: "TemplateID");

            migrationBuilder.CreateIndex(
                name: "IX_Structure_Entry_Folder_UserID",
                table: "Structure_Entry_Folder",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Structure_Entry_Row_EntryID",
                table: "Structure_Entry_Row",
                column: "EntryID");

            migrationBuilder.CreateIndex(
                name: "IX_Structure_Entry_Row_TemplateID",
                table: "Structure_Entry_Row",
                column: "TemplateID");

            migrationBuilder.CreateIndex(
                name: "IX_Structure_Template_UserID",
                table: "Structure_Template",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Structure_Template_Cell_RowID",
                table: "Structure_Template_Cell",
                column: "RowID");

            migrationBuilder.CreateIndex(
                name: "IX_Structure_Template_Row_TemplateID",
                table: "Structure_Template_Row",
                column: "TemplateID");

            migrationBuilder.CreateIndex(
                name: "IX_User_Login_EmailNormalized",
                table: "User_Login",
                column: "EmailNormalized",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Login_PasswordHash",
                table: "User_Login",
                column: "PasswordHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Login_PasswordSalt",
                table: "User_Login",
                column: "PasswordSalt",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Login_StatusToken",
                table: "User_Login",
                column: "StatusToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Login_UsernameNormalized",
                table: "User_Login",
                column: "UsernameNormalized",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_Right_Name",
                table: "User_Right",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admin_ColorTheme");

            migrationBuilder.DropTable(
                name: "Admin_Language");

            migrationBuilder.DropTable(
                name: "Share_GroupUser");

            migrationBuilder.DropTable(
                name: "Share_Item");

            migrationBuilder.DropTable(
                name: "Structure_Entry_Cell");

            migrationBuilder.DropTable(
                name: "User_Information");

            migrationBuilder.DropTable(
                name: "User_Right");

            migrationBuilder.DropTable(
                name: "User_Token");

            migrationBuilder.DropTable(
                name: "Share_Group");

            migrationBuilder.DropTable(
                name: "Structure_Entry_Row");

            migrationBuilder.DropTable(
                name: "Structure_Template_Cell");

            migrationBuilder.DropTable(
                name: "Right");

            migrationBuilder.DropTable(
                name: "Structure_Entry");

            migrationBuilder.DropTable(
                name: "Structure_Template_Row");

            migrationBuilder.DropTable(
                name: "Structure_Entry_Folder");

            migrationBuilder.DropTable(
                name: "Structure_Template");

            migrationBuilder.DropTable(
                name: "User_Login");
        }
    }
}
