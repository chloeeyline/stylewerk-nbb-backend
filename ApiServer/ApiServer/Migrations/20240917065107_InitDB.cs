using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StyleWerk.NBB.Migrations
{
    /// <inheritdoc />
    public partial class InitDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "Share_Item",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
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
                name: "Structure_Entry_Folder",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Structure_Entry_Folder", x => x.ID);
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
                    StatusCode = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    StatusToken = table.Column<Guid>(type: "uuid", nullable: true),
                    StatusTokenTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User_Login", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Share_Group",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    UserID = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
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
                name: "Structure_Template",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    UserID = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
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
                    Admin = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User_Right", x => x.ID);
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
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                    ID = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    Name = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    UserID = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateID = table.Column<Guid>(type: "uuid", nullable: false),
                    FolderID = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
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
                    ID = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    TemplateID = table.Column<Guid>(type: "uuid", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CanWrapCells = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
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
                name: "Structure_Template_Cell",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    RowID = table.Column<Guid>(type: "uuid", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    InputHelper = table.Column<int>(type: "integer", nullable: false),
                    HideOnEmpty = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsRequiered = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Text = table.Column<string>(type: "text", nullable: true),
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
                    EntryID = table.Column<Guid>(type: "uuid", nullable: false),
                    CellID = table.Column<Guid>(type: "uuid", nullable: false),
                    Metadata = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Structure_Entry_Cell", x => new { x.EntryID, x.CellID });
                    table.ForeignKey(
                        name: "FK_Structure_Entry_Cell_Structure_Entry_EntryID",
                        column: x => x.EntryID,
                        principalTable: "Structure_Entry",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Structure_Entry_Cell_Structure_Template_Cell_CellID",
                        column: x => x.CellID,
                        principalTable: "Structure_Template_Cell",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_Structure_Entry_Cell_CellID",
                table: "Structure_Entry_Cell",
                column: "CellID");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "Structure_Entry");

            migrationBuilder.DropTable(
                name: "Structure_Template_Cell");

            migrationBuilder.DropTable(
                name: "Structure_Entry_Folder");

            migrationBuilder.DropTable(
                name: "Structure_Template_Row");

            migrationBuilder.DropTable(
                name: "Structure_Template");

            migrationBuilder.DropTable(
                name: "User_Login");
        }
    }
}
