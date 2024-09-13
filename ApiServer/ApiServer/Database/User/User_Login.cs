using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;

namespace StyleWerk.NBB.Database.User;

public class User_Login : IConnectedEntity<User_Login>, IEntity_GuidID
{
	public Guid ID { get; set; }

	public string Email { get; set; } = string.Empty;
	public string EmailNormalized { get; set; } = string.Empty;

	public string Username { get; set; } = string.Empty;
	public string UsernameNormalized { get; set; } = string.Empty;

	public string PasswordHash { get; set; } = string.Empty;
	public string PasswordSalt { get; set; } = string.Empty;

	public UserStatus StatusCode { get; set; } = UserStatus.None;
	public Guid? StatusToken { get; set; } = null;
	public DateTimeOffset? StatusTokenTime { get; set; } = null;

	public virtual User_Information O_Information { get; set; } = new();
	public virtual User_Right O_Right { get; set; } = new();
	public virtual List<User_Token> O_Token { get; set; } = [];

	public void SetEmail(string email)
	{
		Email = email;
		EmailNormalized = email.ToLower().Normalize();
	}

	public void SetUsername(string username)
	{
		Username = username;
		UsernameNormalized = username.ToLower().Normalize();
	}

	public static void Build(EntityTypeBuilder<User_Login> b)
	{
		b.UseTemplates();


		b.Property(s => s.Username).IsRequired(true).HasMaxLength(30);
		b.Property(s => s.UsernameNormalized).IsRequired(true).HasMaxLength(30);
		b.HasIndex(s => s.UsernameNormalized).IsUnique(true);

		b.Property(s => s.Email).IsRequired(true).HasMaxLength(100);
		b.Property(s => s.EmailNormalized).IsRequired(true).HasMaxLength(100);
		b.HasIndex(s => s.EmailNormalized).IsUnique(true);

		b.Property(s => s.PasswordHash).IsRequired(true);
		b.Property(s => s.PasswordSalt).IsRequired(true);
		b.HasIndex(s => s.PasswordHash).IsUnique(true);
		b.HasIndex(s => s.PasswordSalt).IsUnique(true);

		b.Property(s => s.StatusCode).IsRequired(true).HasDefaultValue(UserStatus.None);
		b.Property(s => s.StatusToken).IsRequired(false);
		b.Property(s => s.StatusTokenTime).IsRequired(false);
		b.HasIndex(s => s.StatusToken).IsUnique(true);
	}

	public static void Connect(EntityTypeBuilder<User_Login> b)
	{
		b.HasOne(s => s.O_Information)
			.WithOne(s => s.O_User)
			.IsRequired(false)
			.HasForeignKey<User_Information>(s => s.ID)
			.HasConstraintName("Information");
		b.HasOne(s => s.O_Right)
			.WithOne(s => s.O_User)
			.IsRequired(false)
			.HasForeignKey<User_Right>(s => s.ID)
			.HasConstraintName("Right");
		b.HasMany(s => s.O_Token)
			.WithOne(s => s.O_User)
			.IsRequired(false)
			.HasForeignKey(s => s.ID)
			.HasConstraintName("Token");
	}
}

public enum UserStatus
{
	None,
	EmailVerification,
	EmailChange,
	PasswordReset
}