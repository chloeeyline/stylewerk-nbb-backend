using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;

namespace StyleWerk.NBB.Database.User;

public class User_Token : IConnectedEntity<User_Token>
{
	public Guid ID { get; set; }
	public string Agent { get; set; } = string.Empty;

	public string RefreshToken { get; set; } = string.Empty;
	public DateTime RefreshTokenExpiryTime { get; set; } = DateTime.UtcNow;

	public virtual User_Login O_User { get; set; } = new();

	public static void Build(EntityTypeBuilder<User_Token> b)
	{
		b.UseTemplates();
		b.Property(s => s.ID).IsRequired(true).HasColumnName("ID");
		b.Property(s => s.Agent).IsRequired(true);
		b.HasKey(s => new { s.ID, s.Agent });

		b.Property(s => s.RefreshToken).IsRequired(true);
		b.Property(s => s.RefreshTokenExpiryTime).IsRequired(true);
	}

	public static void Connect(EntityTypeBuilder<User_Token> b)
	{
		b.HasOne(s => s.O_User)
			.WithMany(s => s.O_Token)
			.IsRequired(true)
			.HasForeignKey(s => s.ID)
			.HasConstraintName("User");
	}
}
