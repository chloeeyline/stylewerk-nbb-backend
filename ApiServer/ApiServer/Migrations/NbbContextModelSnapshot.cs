﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using StyleWerk.NBB.Database;

#nullable disable

namespace StyleWerk.NBB.Migrations
{
    [DbContext(typeof(NbbContext))]
    partial class NbbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresExtension(modelBuilder, "uuid-ossp");
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("StyleWerk.NBB.Database.Share.Share_Group", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("ID")
                        .HasDefaultValueSql("uuid_generate_v4()");

                    b.Property<bool>("CanSeeOthers")
                        .HasColumnType("boolean");

                    b.Property<bool>("IsVisible")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("character varying(250)");

                    b.Property<Guid>("UserID")
                        .HasColumnType("uuid");

                    b.HasKey("ID");

                    b.HasIndex("UserID");

                    b.ToTable("Share_Group", (string)null);
                });

            modelBuilder.Entity("StyleWerk.NBB.Database.Share.Share_GroupUser", b =>
                {
                    b.Property<Guid>("GroupID")
                        .HasColumnType("uuid");

                    b.Property<Guid>("UserID")
                        .HasColumnType("uuid");

                    b.Property<bool>("CanAddUsers")
                        .HasColumnType("boolean");

                    b.Property<bool>("CanRemoveUsers")
                        .HasColumnType("boolean");

                    b.Property<bool>("CanSeeUsers")
                        .HasColumnType("boolean");

                    b.Property<Guid>("WhoAdded")
                        .HasColumnType("uuid");

                    b.HasKey("GroupID", "UserID");

                    b.HasIndex("UserID");

                    b.ToTable("Share_GroupUser", (string)null);
                });

            modelBuilder.Entity("StyleWerk.NBB.Database.Share.Share_Item", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("ID")
                        .HasDefaultValueSql("uuid_generate_v4()");

                    b.Property<bool>("CanDelete")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.Property<bool>("CanEdit")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.Property<bool>("CanShare")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.Property<bool>("Group")
                        .HasColumnType("boolean");

                    b.Property<Guid>("ItemID")
                        .HasColumnType("uuid");

                    b.Property<byte>("ItemType")
                        .HasColumnType("smallint");

                    b.Property<Guid>("ToWhom")
                        .HasColumnType("uuid");

                    b.Property<Guid>("WhoShared")
                        .HasColumnType("uuid");

                    b.HasKey("ID");

                    b.ToTable("Share_Item", (string)null);
                });

            modelBuilder.Entity("StyleWerk.NBB.Database.Structure.Structure_Entry", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("ID")
                        .HasDefaultValueSql("uuid_generate_v4()");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("NOW()");

                    b.Property<Guid?>("FolderID")
                        .HasColumnType("uuid");

                    b.Property<bool>("IsPublic")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("LastUpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("NOW()");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("character varying(250)");

                    b.Property<string[]>("Tags")
                        .HasColumnType("text[]");

                    b.Property<Guid>("TemplateID")
                        .HasColumnType("uuid");

                    b.Property<Guid>("UserID")
                        .HasColumnType("uuid");

                    b.HasKey("ID");

                    b.HasIndex("FolderID");

                    b.HasIndex("TemplateID");

                    b.HasIndex("UserID");

                    b.ToTable("Structure_Entry", (string)null);
                });

            modelBuilder.Entity("StyleWerk.NBB.Database.Structure.Structure_Entry_Cell", b =>
                {
                    b.Property<Guid>("EntryID")
                        .HasColumnType("uuid");

                    b.Property<Guid>("CellID")
                        .HasColumnType("uuid");

                    b.Property<string>("Data")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("EntryID", "CellID");

                    b.HasIndex("CellID");

                    b.ToTable("Structure_Entry_Cell", (string)null);
                });

            modelBuilder.Entity("StyleWerk.NBB.Database.Structure.Structure_Entry_Folder", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("ID")
                        .HasDefaultValueSql("uuid_generate_v4()");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("character varying(250)");

                    b.Property<int>("SortOrder")
                        .HasColumnType("integer");

                    b.Property<Guid>("UserID")
                        .HasColumnType("uuid");

                    b.HasKey("ID");

                    b.HasIndex("UserID");

                    b.ToTable("Structure_Entry_Folder", (string)null);
                });

            modelBuilder.Entity("StyleWerk.NBB.Database.Structure.Structure_Template", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("ID")
                        .HasDefaultValueSql("uuid_generate_v4()");

                    b.Property<DateTime>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("NOW()");

                    b.Property<string>("Description")
                        .HasColumnType("text");

                    b.Property<bool>("IsPublic")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.Property<DateTime>("LastUpdatedAt")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("timestamp with time zone")
                        .HasDefaultValueSql("NOW()");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("character varying(250)");

                    b.Property<string[]>("Tags")
                        .HasColumnType("text[]");

                    b.Property<Guid>("UserID")
                        .HasColumnType("uuid");

                    b.HasKey("ID");

                    b.HasIndex("UserID");

                    b.ToTable("Structure_Template", (string)null);
                });

            modelBuilder.Entity("StyleWerk.NBB.Database.Structure.Structure_Template_Cell", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("ID")
                        .HasDefaultValueSql("uuid_generate_v4()");

                    b.Property<bool>("HideOnEmpty")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.Property<int>("InputHelper")
                        .HasColumnType("integer");

                    b.Property<bool>("IsRequiered")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(false);

                    b.Property<string>("MetaData")
                        .HasColumnType("text");

                    b.Property<Guid>("RowID")
                        .HasColumnType("uuid");

                    b.Property<int>("SortOrder")
                        .HasColumnType("integer");

                    b.Property<string>("Text")
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.HasIndex("RowID");

                    b.ToTable("Structure_Template_Cell", (string)null);
                });

            modelBuilder.Entity("StyleWerk.NBB.Database.Structure.Structure_Template_Row", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("ID")
                        .HasDefaultValueSql("uuid_generate_v4()");

                    b.Property<bool>("CanWrapCells")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("boolean")
                        .HasDefaultValue(true);

                    b.Property<int>("SortOrder")
                        .HasColumnType("integer");

                    b.Property<Guid>("TemplateID")
                        .HasColumnType("uuid");

                    b.HasKey("ID");

                    b.HasIndex("TemplateID");

                    b.ToTable("Structure_Template_Row", (string)null);
                });

            modelBuilder.Entity("StyleWerk.NBB.Database.User.Right", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.HasKey("Name");

                    b.ToTable("Right", (string)null);
                });

            modelBuilder.Entity("StyleWerk.NBB.Database.User.User_Information", b =>
                {
                    b.Property<Guid>("ID")
                        .HasColumnType("uuid")
                        .HasColumnName("ID");

                    b.Property<DateOnly>("Birthday")
                        .HasColumnType("date");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<byte>("Gender")
                        .HasColumnType("smallint");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.HasKey("ID");

                    b.ToTable("User_Information", (string)null);
                });

            modelBuilder.Entity("StyleWerk.NBB.Database.User.User_Login", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid")
                        .HasColumnName("ID")
                        .HasDefaultValueSql("uuid_generate_v4()");

                    b.Property<bool>("Admin")
                        .HasColumnType("boolean");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("EmailNormalized")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PasswordSalt")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("StatusCode")
                        .HasColumnType("integer");

                    b.Property<Guid?>("StatusToken")
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset?>("StatusTokenExpireTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.Property<string>("UsernameNormalized")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)");

                    b.HasKey("ID");

                    b.HasIndex("EmailNormalized")
                        .IsUnique();

                    b.HasIndex("PasswordHash")
                        .IsUnique();

                    b.HasIndex("PasswordSalt")
                        .IsUnique();

                    b.HasIndex("StatusToken")
                        .IsUnique();

                    b.HasIndex("UsernameNormalized")
                        .IsUnique();

                    b.ToTable("User_Login", (string)null);
                });

            modelBuilder.Entity("StyleWerk.NBB.Database.User.User_Right", b =>
                {
                    b.Property<Guid>("ID")
                        .HasColumnType("uuid")
                        .HasColumnName("ID");

                    b.Property<string>("Name")
                        .HasMaxLength(250)
                        .HasColumnType("character varying(250)");

                    b.HasKey("ID", "Name");

                    b.HasIndex("Name");

                    b.ToTable("User_Right", (string)null);
                });

            modelBuilder.Entity("StyleWerk.NBB.Database.User.User_Token", b =>
                {
                    b.Property<Guid>("ID")
                        .HasColumnType("uuid")
                        .HasColumnName("ID");

                    b.Property<string>("Agent")
                        .HasColumnType("text");

                    b.Property<bool>("ConsistOverSession")
                        .HasColumnType("boolean");

                    b.Property<string>("RefreshToken")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTimeOffset>("RefreshTokenExpiryTime")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("ID", "Agent");

                    b.ToTable("User_Token", (string)null);
                });

            modelBuilder.Entity("StyleWerk.NBB.Database.Share.Share_Group", b =>
                {
                    b.HasOne("StyleWerk.NBB.Database.User.User_Login", "O_User")
                        .WithMany()
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("O_User");
                });

            modelBuilder.Entity("StyleWerk.NBB.Database.Share.Share_GroupUser", b =>
                {
                    b.HasOne("StyleWerk.NBB.Database.Share.Share_Group", "O_Group")
                        .WithMany("O_GroupUser")
                        .HasForeignKey("GroupID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("StyleWerk.NBB.Database.User.User_Login", "O_User")
                        .WithMany()
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("O_Group");

                    b.Navigation("O_User");
                });

            modelBuilder.Entity("StyleWerk.NBB.Database.Structure.Structure_Entry", b =>
                {
                    b.HasOne("StyleWerk.NBB.Database.Structure.Structure_Entry_Folder", "O_Folder")
                        .WithMany("O_EntryList")
                        .HasForeignKey("FolderID");

                    b.HasOne("StyleWerk.NBB.Database.Structure.Structure_Template", "OTemplate")
                        .WithMany("O_EntryList")
                        .HasForeignKey("TemplateID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("StyleWerk.NBB.Database.User.User_Login", "O_User")
                        .WithMany()
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("OTemplate");

                    b.Navigation("O_Folder");

                    b.Navigation("O_User");
                });

            modelBuilder.Entity("StyleWerk.NBB.Database.Structure.Structure_Entry_Cell", b =>
                {
                    b.HasOne("StyleWerk.NBB.Database.Structure.Structure_Template_Cell", "O_Cell")
                        .WithMany("O_EntryCells")
                        .HasForeignKey("CellID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("StyleWerk.NBB.Database.Structure.Structure_Entry", "O_Entry")
                        .WithMany("O_Cells")
                        .HasForeignKey("EntryID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("O_Cell");

                    b.Navigation("O_Entry");
                });

            modelBuilder.Entity("StyleWerk.NBB.Database.Structure.Structure_Entry_Folder", b =>
                {
                    b.HasOne("StyleWerk.NBB.Database.User.User_Login", "O_User")
                        .WithMany()
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("O_User");
                });

            modelBuilder.Entity("StyleWerk.NBB.Database.Structure.Structure_Template", b =>
                {
                    b.HasOne("StyleWerk.NBB.Database.User.User_Login", "O_User")
                        .WithMany()
                        .HasForeignKey("UserID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("O_User");
                });

            modelBuilder.Entity("StyleWerk.NBB.Database.Structure.Structure_Template_Cell", b =>
                {
                    b.HasOne("StyleWerk.NBB.Database.Structure.Structure_Template_Row", "O_Row")
                        .WithMany("O_Cells")
                        .HasForeignKey("RowID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("O_Row");
                });

            modelBuilder.Entity("StyleWerk.NBB.Database.Structure.Structure_Template_Row", b =>
                {
                    b.HasOne("StyleWerk.NBB.Database.Structure.Structure_Template", "O_Template")
                        .WithMany("O_Rows")
                        .HasForeignKey("TemplateID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("O_Template");
                });

            modelBuilder.Entity("StyleWerk.NBB.Database.User.User_Information", b =>
                {
                    b.HasOne("StyleWerk.NBB.Database.User.User_Login", "O_User")
                        .WithOne("O_Information")
                        .HasForeignKey("StyleWerk.NBB.Database.User.User_Information", "ID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("User");

                    b.Navigation("O_User");
                });

            modelBuilder.Entity("StyleWerk.NBB.Database.User.User_Right", b =>
                {
                    b.HasOne("StyleWerk.NBB.Database.User.User_Login", "O_User")
                        .WithMany("O_Right")
                        .HasForeignKey("ID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("User");

                    b.HasOne("StyleWerk.NBB.Database.User.Right", "O_Right")
                        .WithMany("O_User")
                        .HasForeignKey("Name")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("O_Right");

                    b.Navigation("O_User");
                });

            modelBuilder.Entity("StyleWerk.NBB.Database.User.User_Token", b =>
                {
                    b.HasOne("StyleWerk.NBB.Database.User.User_Login", "O_User")
                        .WithMany("O_Token")
                        .HasForeignKey("ID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("User");

                    b.Navigation("O_User");
                });

            modelBuilder.Entity("StyleWerk.NBB.Database.Share.Share_Group", b =>
                {
                    b.Navigation("O_GroupUser");
                });

            modelBuilder.Entity("StyleWerk.NBB.Database.Structure.Structure_Entry", b =>
                {
                    b.Navigation("O_Cells");
                });

            modelBuilder.Entity("StyleWerk.NBB.Database.Structure.Structure_Entry_Folder", b =>
                {
                    b.Navigation("O_EntryList");
                });

            modelBuilder.Entity("StyleWerk.NBB.Database.Structure.Structure_Template", b =>
                {
                    b.Navigation("O_EntryList");

                    b.Navigation("O_Rows");
                });

            modelBuilder.Entity("StyleWerk.NBB.Database.Structure.Structure_Template_Cell", b =>
                {
                    b.Navigation("O_EntryCells");
                });

            modelBuilder.Entity("StyleWerk.NBB.Database.Structure.Structure_Template_Row", b =>
                {
                    b.Navigation("O_Cells");
                });

            modelBuilder.Entity("StyleWerk.NBB.Database.User.Right", b =>
                {
                    b.Navigation("O_User");
                });

            modelBuilder.Entity("StyleWerk.NBB.Database.User.User_Login", b =>
                {
                    b.Navigation("O_Information")
                        .IsRequired();

                    b.Navigation("O_Right");

                    b.Navigation("O_Token");
                });
#pragma warning restore 612, 618
        }
    }
}
