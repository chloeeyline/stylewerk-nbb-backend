using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;
using StyleWerk.NBB.Database.User;

namespace StyleWerk.NBB.Database.Share;

/// <summary>
/// Represents a group of users with shared access to items.
/// </summary>
public class Share_Group : IConnectedEntity<Share_Group>, IEntity_GuidID, IEntity_Name, IEntity_User
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public required Guid ID { get; set; }
    /// <summary>
    /// User ID of the group creator
    /// </summary>
    public required Guid UserID { get; set; }

    /// <summary>
    /// Name of the group.
    /// </summary>
    public required string Name { get; set; }

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
    /// <summary>
    /// Navigation property for the group's user.
    /// </summary>
    public virtual User_Login O_User { get; set; }

    /// <summary>
    /// Collection of users in the group.
    /// </summary>
    public virtual List<Share_GroupUser> O_GroupUser { get; set; }
#pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="b"><inheritdoc/></param>
    public static void Build(EntityTypeBuilder<Share_Group> b)
    {
        b.UseTemplates();
        b.UseIEntity_GuidID();
        b.UseIEntity_User();
        b.UseIEntity_Name();
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="b"><inheritdoc/></param>
    public static void Connect(EntityTypeBuilder<Share_Group> b)
    {
        b.HasMany(s => s.O_GroupUser)
            .WithOne(s => s.O_Group)
            .HasForeignKey(s => s.UserID)
            .IsRequired(true);
    }
}
