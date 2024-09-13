using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;

namespace StyleWerk.NBB.Database.User;

public class User_Right : IConnectedEntity<User_Right>, IEntity_GuidID
{
	public Guid ID { get; set; }

	public bool Admin { get; set; } = false;

	public virtual User_Login O_User { get; set; } = new();

	public static void Build(EntityTypeBuilder<User_Right> b)
	{
		b.UseTemplates(); ;

		b.Property(s => s.Admin).IsRequired(true).HasDefaultValue(false);
	}

	public static void Connect(EntityTypeBuilder<User_Right> b)
	{
		b.HasOne(s => s.O_User)
			.WithOne(s => s.O_Right)
			.IsRequired(true)
			.HasForeignKey<User_Right>(s => s.ID)
			.HasConstraintName("User");
	}
}

[Flags]
public enum UserRight
{
	Restricted = 0,
	Access = 1,
	Create = 2,
	Edit = 4,
	Delete = 8,
	Admin = 16,

	DefaultUsage = Access | Create,
	ManipulateOthers = Edit | Delete,
	SuperAdmin = Admin | ManipulateOthers | DefaultUsage,
}
