using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;

namespace StyleWerk.NBB.Database.User;

public class User_Information : IConnectedEntity<User_Information>, IEntity_GuidID
{
	public Guid ID { get; set; }

	public UserGender Gender { get; set; } = UserGender.NotSpecified;
	public string FirstName { get; set; } = string.Empty;
	public string LastName { get; set; } = string.Empty;
	public DateOnly Birthday { get; set; } = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day);

	public virtual User_Login O_User { get; set; } = new();

	public static void Build(EntityTypeBuilder<User_Information> b)
	{
		b.UseTemplates(); ;

		b.Property(s => s.Gender).IsRequired(true);
		b.Property(s => s.FirstName).IsRequired(true).HasMaxLength(50);
		b.Property(s => s.LastName).IsRequired(true).HasMaxLength(50);
		b.Property(s => s.Birthday).IsRequired(true);
	}

	public static void Connect(EntityTypeBuilder<User_Information> b)
	{
		b.HasOne(s => s.O_User)
			.WithOne(s => s.O_Information)
			.IsRequired(true)
			.HasForeignKey<User_Information>(s => s.ID)
			.HasConstraintName("User");
	}
}

public enum UserGender : byte
{
	NotSpecified,
	Female,
	Male,
	NonBinary
}