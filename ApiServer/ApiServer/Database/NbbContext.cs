﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

using StyleWerk.NBB.Database.Core;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Database.User;

namespace StyleWerk.NBB.Database;

public class DbContextFactory : IDesignTimeDbContextFactory<NbbContext>
{
	public NbbContext CreateDbContext(string[] args)
	{
		//TODO implement AWS Secret and DB
		//Connection string for Chloe until ported to use AWS Secrets and AWS RD (Online Database)
		string connectionString = "User ID=postgres;Password=w9MWcR@j*L9m;Host=localhost;Port=5432;Database=stylewerk;Include Error Detail=true;";

		DbContextOptionsBuilder<NbbContext> builder = new();
		builder.UseNpgsql(connectionString);

		return new NbbContext(builder.Options);
	}
}

public partial class NbbContext : DbContext
{
	public NbbContext() { }

	public NbbContext(DbContextOptions<NbbContext> options) : base(options) { }

	public virtual DbSet<User_Login> User_Login { get; set; }
	public virtual DbSet<User_Token> User_Token { get; set; }
	public virtual DbSet<User_Information> User_Information { get; set; }
	public virtual DbSet<User_Right> User_Right { get; set; }

	public virtual DbSet<Structure_Template> Structure_Template { get; set; }
	public virtual DbSet<Structure_Template_Row> Structure_Template_Row { get; set; }
	public virtual DbSet<Structure_Template_Cell> Structure_Template_Cell { get; set; }

	public virtual DbSet<Structure_Entry> Structure_Entry { get; set; }
	public virtual DbSet<Structure_Entry_Cell> Structure_Entry_Cell { get; set; }
	public virtual DbSet<Structure_Entry_Folder> Structure_Entry_Folder { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasPostgresExtension("uuid-ossp");

		_ = modelBuilder.Entity<User_Login>(User.User_Login.Build);
		_ = modelBuilder.Entity<User_Token>(User.User_Token.Build);
		_ = modelBuilder.Entity<User_Information>(User.User_Information.Build);
		_ = modelBuilder.Entity<User_Right>(User.User_Right.Build);

		_ = modelBuilder.Entity<Structure_Template>(Structure.Structure_Template.Build);
		_ = modelBuilder.Entity<Structure_Template_Row>(Structure.Structure_Template_Row.Build);
		_ = modelBuilder.Entity<Structure_Template_Cell>(Structure.Structure_Template_Cell.Build);

		_ = modelBuilder.Entity<Structure_Entry>(Structure.Structure_Entry.Build);
		_ = modelBuilder.Entity<Structure_Entry_Cell>(Structure.Structure_Entry_Cell.Build);
		_ = modelBuilder.Entity<Structure_Entry_Folder>(Structure.Structure_Entry_Folder.Build);

		_ = modelBuilder.Entity<User_Login>(User.User_Login.Connect);
		_ = modelBuilder.Entity<User_Token>(User.User_Token.Connect);
		_ = modelBuilder.Entity<User_Information>(User.User_Information.Connect);
		_ = modelBuilder.Entity<User_Right>(User.User_Right.Connect);

		_ = modelBuilder.Entity<Structure_Template>(Structure.Structure_Template.Connect);
		_ = modelBuilder.Entity<Structure_Template_Row>(Structure.Structure_Template_Row.Connect);
		_ = modelBuilder.Entity<Structure_Template_Cell>(Structure.Structure_Template_Cell.Connect);

		_ = modelBuilder.Entity<Structure_Entry>(Structure.Structure_Entry.Connect);
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
			if (entry.State == EntityState.Added) temp.CreatedAt = DateTime.UtcNow;
			if (entry.State == EntityState.Modified) temp.LastUpdatedAt = DateTime.UtcNow;
		}

		return base.SaveChanges();
	}
}
