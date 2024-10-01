﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

using StyleWerk.NBB.AWS;
using StyleWerk.NBB.Database.Admin;
using StyleWerk.NBB.Database.Core;
using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Database.User;

namespace StyleWerk.NBB.Database;

public class DbContextFactory : IDesignTimeDbContextFactory<NbbContext>
{
    public NbbContext CreateDbContext(string[] args) => NbbContext.Create();
}

public record ApplicationUser
{
    public ApplicationUser()
    {
        Instantiated = false;
        Login = new()
        {
            ID = Guid.Empty,
            Email = string.Empty,
            EmailNormalized = string.Empty,
            Username = string.Empty,
            UsernameNormalized = string.Empty,
            PasswordSalt = string.Empty,
            PasswordHash = string.Empty,
            Admin = false,
        };
        Information = new()
        {
            ID = Guid.Empty,
            FirstName = string.Empty,
            LastName = string.Empty,
            Gender = UserGender.NotSpecified,
            Birthday = new DateTimeOffset(new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day)).ToUnixTimeMilliseconds(),
        };
    }

    public ApplicationUser(User_Login login, User_Information information)
    {
        Login = login ?? throw new ArgumentNullException(nameof(login));
        Information = information ?? throw new ArgumentNullException(nameof(information));
        ID = login.ID;
        Instantiated = true;
    }

    public bool Instantiated { get; init; }
    public Guid ID { get; init; }
    public User_Login Login { get; init; }
    public User_Information Information { get; init; }
}

public partial class NbbContext : DbContext
{
    public NbbContext() { }

    public NbbContext(DbContextOptions<NbbContext> options) : base(options) { }

    public virtual DbSet<User_Login> User_Login { get; set; }
    public virtual DbSet<User_Token> User_Token { get; set; }
    public virtual DbSet<User_Information> User_Information { get; set; }

    public virtual DbSet<Share_Group> Share_Group { get; set; }
    public virtual DbSet<Share_GroupUser> Share_GroupUser { get; set; }
    public virtual DbSet<Share_Item> Share_Item { get; set; }

    public virtual DbSet<Admin_ColorTheme> Admin_ColorTheme { get; set; }
    public virtual DbSet<Admin_Language> Admin_Language { get; set; }

    public virtual DbSet<Structure_Template> Structure_Template { get; set; }
    public virtual DbSet<Structure_Template_Row> Structure_Template_Row { get; set; }
    public virtual DbSet<Structure_Template_Cell> Structure_Template_Cell { get; set; }

    public virtual DbSet<Structure_Entry> Structure_Entry { get; set; }
    public virtual DbSet<Structure_Entry_Row> Structure_Entry_Row { get; set; }
    public virtual DbSet<Structure_Entry_Cell> Structure_Entry_Cell { get; set; }
    public virtual DbSet<Structure_Entry_Folder> Structure_Entry_Folder { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("uuid-ossp");

        _ = modelBuilder.Entity<User_Login>(User.User_Login.Build);
        _ = modelBuilder.Entity<User_Token>(User.User_Token.Build);
        _ = modelBuilder.Entity<User_Information>(User.User_Information.Build);

        _ = modelBuilder.Entity<Share_Group>(Share.Share_Group.Build);
        _ = modelBuilder.Entity<Share_GroupUser>(Share.Share_GroupUser.Build);
        _ = modelBuilder.Entity<Share_Item>(Share.Share_Item.Build);

        _ = modelBuilder.Entity<Admin_ColorTheme>(Admin.Admin_ColorTheme.Build);
        _ = modelBuilder.Entity<Admin_Language>(Admin.Admin_Language.Build);

        _ = modelBuilder.Entity<Structure_Template>(Structure.Structure_Template.Build);
        _ = modelBuilder.Entity<Structure_Template_Row>(Structure.Structure_Template_Row.Build);
        _ = modelBuilder.Entity<Structure_Template_Cell>(Structure.Structure_Template_Cell.Build);

        _ = modelBuilder.Entity<Structure_Entry>(Structure.Structure_Entry.Build);
        _ = modelBuilder.Entity<Structure_Entry_Row>(Structure.Structure_Entry_Row.Build);
        _ = modelBuilder.Entity<Structure_Entry_Cell>(Structure.Structure_Entry_Cell.Build);
        _ = modelBuilder.Entity<Structure_Entry_Folder>(Structure.Structure_Entry_Folder.Build);

        _ = modelBuilder.Entity<User_Login>(User.User_Login.Connect);
        _ = modelBuilder.Entity<User_Token>(User.User_Token.Connect);
        _ = modelBuilder.Entity<User_Information>(User.User_Information.Connect);

        _ = modelBuilder.Entity<Share_Group>(Share.Share_Group.Connect);
        _ = modelBuilder.Entity<Share_GroupUser>(Share.Share_GroupUser.Connect);

        _ = modelBuilder.Entity<Structure_Template>(Structure.Structure_Template.Connect);
        _ = modelBuilder.Entity<Structure_Template_Row>(Structure.Structure_Template_Row.Connect);
        _ = modelBuilder.Entity<Structure_Template_Cell>(Structure.Structure_Template_Cell.Connect);

        _ = modelBuilder.Entity<Structure_Entry>(Structure.Structure_Entry.Connect);
        _ = modelBuilder.Entity<Structure_Entry_Row>(Structure.Structure_Entry_Row.Connect);
        _ = modelBuilder.Entity<Structure_Entry_Cell>(Structure.Structure_Entry_Cell.Connect);
        _ = modelBuilder.Entity<Structure_Entry_Folder>(Structure.Structure_Entry_Folder.Connect);
    }

    public override int SaveChanges()
    {
        IEnumerable<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry> entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is IEntity_EditDate &&
                        (e.State == EntityState.Modified || e.State == EntityState.Added));

        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry? entry in entries)
        {
            IEntity_EditDate temp = ((IEntity_EditDate) entry.Entity);
            if (entry.State == EntityState.Added) temp.CreatedAt = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
            if (entry.State == EntityState.Modified) temp.LastUpdatedAt = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
        }

        return base.SaveChanges();
    }

    public ApplicationUser GetUser(Guid? id)
    {
        User_Login? login = User_Login.FirstOrDefault(s => s.ID == id);
        User_Information? information = User_Information.FirstOrDefault(s => s.ID == id);
        return id is null || login is null || information is null ?
            new ApplicationUser() :
            new ApplicationUser(login, information);
    }

    public static NbbContext Create()
    {
        SecretData secretData = SecretData.GetData();
        DbContextOptionsBuilder<NbbContext> builder = new();
        builder.UseNpgsql(secretData.ConnectionString);
        return new NbbContext(builder.Options);
    }
}
