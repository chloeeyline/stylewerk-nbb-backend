using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Database.User;

namespace StyleWerk.NBB.Database;


public partial class NbbContext : DbContext
{
	public NbbContext() { }

	public NbbContext(DbContextOptions<NbbContext> options) : base(options) { }

	public virtual DbSet<User_Login> User_Login { get; set; }
	public virtual DbSet<User_Token> User_Token { get; set; }
	public virtual DbSet<User_Information> User_Information { get; set; }
	public virtual DbSet<User_Right> User_Right { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasPostgresExtension("uuid-ossp");

		_ = modelBuilder.Entity<User_Login>(User.User_Login.Build);
		_ = modelBuilder.Entity<User_Token>(User.User_Token.Build);
		_ = modelBuilder.Entity<User_Information>(User.User_Information.Build);
		_ = modelBuilder.Entity<User_Right>(User.User_Right.Build);

		_ = modelBuilder.Entity<User_Login>(User.User_Login.Connect);
		_ = modelBuilder.Entity<User_Token>(User.User_Token.Connect);
		_ = modelBuilder.Entity<User_Information>(User.User_Information.Connect);
		_ = modelBuilder.Entity<User_Right>(User.User_Right.Connect);
	}
}
