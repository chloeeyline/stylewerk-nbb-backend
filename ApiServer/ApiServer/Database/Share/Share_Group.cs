using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;
using StyleWerk.NBB.Database.User;

namespace StyleWerk.NBB.Database.Share;

/// <summary>
/// Represents a group of users with shared access to items.
/// </summary>
public class Share_Group : IConnectedEntity<Share_Group>, IEntity_GuidID, IEntity_Name
{
	public Guid ID { get; set; }
	/// <summary>
	/// User ID of the group creator
	/// </summary>
	public Guid UserID { get; set; }

	/// <summary>
	/// Name of the group.
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Indicates if the group's presence is visible to others.
	/// </summary>
	public bool IsVisible { get; set; }

	/// <summary>
	/// Determines if members of the group can see other members within the group.
	/// </summary>
	public bool CanSeeOthers { get; set; }

	/// <summary>
	/// Navigation property for the group's user.
	/// </summary>
	public virtual User_Login O_User { get; set; } = new();

	/// <summary>
	/// Collection of users in the group.
	/// </summary>
	public virtual List<Share_GroupUser> O_GroupUser { get; set; } = new List<Share_GroupUser>();

	/// <summary>
	/// <inheritdoc/>
	/// </summary>
	/// <param name="b"><inheritdoc/></param>
	public static void Build(EntityTypeBuilder<Share_Group> b)
	{
		b.UseTemplates();
		b.UseIEntity_GuidID();
		b.Property(s => s.UserID).IsRequired(true);
		b.UseIEntity_Name();
		b.Property(s => s.IsVisible).IsRequired(true);
		b.Property(s => s.CanSeeOthers).IsRequired(true);
	}

	/// <summary>
	/// <inheritdoc/>
	/// </summary>
	/// <param name="b"><inheritdoc/></param>
	public static void Connect(EntityTypeBuilder<Share_Group> b)
	{
		b.HasOne(s => s.O_User)
			.WithMany()
			.HasForeignKey(s => s.UserID)
			.IsRequired(true);
		b.HasMany(s => s.O_GroupUser)
			.WithOne(s => s.O_Group)
			.HasForeignKey(s => s.UserID)
			.IsRequired(true);
	}
}
