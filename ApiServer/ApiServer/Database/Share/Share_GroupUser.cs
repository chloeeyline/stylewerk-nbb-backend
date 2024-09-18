using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;
using StyleWerk.NBB.Database.User;

namespace StyleWerk.NBB.Database.Share;

/// <summary>
/// Represents a user within a shared group, detailing their permissions within the group.
/// </summary>
public class Share_GroupUser : IConnectedEntity<Share_GroupUser>
{
	/// <summary>
	/// Group ID for which the user belongs.
	/// </summary>
	public Guid GroupID { get; set; }

	/// <summary>
	/// User ID of the group member.
	/// </summary>
	public Guid UserID { get; set; }

	/// <summary>
	/// User ID of the person who added the user to the group.
	/// </summary>
	public Guid WhoAdded { get; set; }

	/// <summary>
	/// Determines if the user can view other group members.
	/// </summary>
	public bool CanSeeUsers { get; set; }

	/// <summary>
	/// Determines if the user can add new users to the group.
	/// </summary
	public bool CanAddUsers { get; set; }

	/// <summary>
	/// Determines if the user can remove users from the group.
	/// </summary
	public bool CanRemoveUsers { get; set; }

	/// <summary>
	/// Navigation property for the group.
	/// </summary>
	public virtual Share_Group O_Group { get; set; }

	/// <summary>
	/// Navigation property for the user.
	/// </summary>
	public virtual User_Login O_User { get; set; }

	/// <summary>
	/// <inheritdoc/>
	/// </summary>
	/// <param name="b"><inheritdoc/></param>
	public static void Build(EntityTypeBuilder<Share_GroupUser> b)
	{
		b.UseTemplates();
		b.Property(s => s.GroupID).IsRequired(true);
		b.Property(s => s.UserID).IsRequired(true);
		b.HasKey(s => new { s.GroupID, s.UserID });
		b.Property(s => s.WhoAdded).IsRequired(true);
		b.Property(s => s.CanSeeUsers).IsRequired(true);
		b.Property(s => s.CanAddUsers).IsRequired(true);
		b.Property(s => s.CanRemoveUsers).IsRequired(true);
	}

	/// <summary>
	/// <inheritdoc/>
	/// </summary>
	/// <param name="b"><inheritdoc/></param>
	public static void Connect(EntityTypeBuilder<Share_GroupUser> b)
	{
		b.HasOne(s => s.O_Group)
			.WithMany(s => s.O_GroupUser)
			.HasForeignKey(s => s.GroupID)
			.IsRequired(true);
		b.HasOne(s => s.O_User)
			.WithMany()
			.HasForeignKey(s => s.UserID)
			.IsRequired(true);
	}
}
