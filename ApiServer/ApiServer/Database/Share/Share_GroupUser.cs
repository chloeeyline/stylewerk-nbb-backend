using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;
using StyleWerk.NBB.Database.User;

namespace StyleWerk.NBB.Database.Share;

/// <summary>
/// Represents a user within a shared group, detailing their permissions within the group.
/// </summary>
public class Share_GroupUser : IConnectedEntity<Share_GroupUser>, IEntity_User
{
    /// <summary>
    /// Group ID for which the user belongs.
    /// </summary>
    public required Guid GroupID { get; set; }

    /// <summary>
    /// User ID of the group member.
    /// </summary>
    public required Guid UserID { get; set; }

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
    /// <summary>
    /// Navigation property for the group.
    /// </summary>
    public virtual Share_Group O_Group { get; set; }

    /// <summary>
    /// Navigation property for the user.
    /// </summary>
    public virtual User_Login O_User { get; set; }
#pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="b"><inheritdoc/></param>
    public static void Build(EntityTypeBuilder<Share_GroupUser> b)
    {
        b.UseTemplates();
        b.UseIEntity_User();
        b.Property(s => s.GroupID).IsRequired(true);

        b.HasKey(s => new { s.GroupID, s.UserID });
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
    }
}
