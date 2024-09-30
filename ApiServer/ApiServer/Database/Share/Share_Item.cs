using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;
using StyleWerk.NBB.Database.User;

namespace StyleWerk.NBB.Database.Share;

/// <summary>
/// Represents a shared item entity with access controls.
/// </summary>
public class Share_Item : IEntity<Share_Item>, IEntity_GuidID, IEntity_User
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public required Guid ID { get; set; }
    /// <summary>
    /// User ID of the user who shared the item.
    /// </summary>
    public required Guid UserID { get; set; }

    /// <summary>
    /// Indicates whether the shared item is accessible to a group or individual user.
    /// </summary>
    public required ShareVisibility Visibility { get; set; }

    /// <summary>
    /// Type of the item being shared.
    /// </summary>
    public required ShareType Type { get; set; }

    /// <summary>
    /// Identifier of the item being shared.
    /// </summary>
    public required Guid ItemID { get; set; }

    /// <summary>
    /// User or group ID to whom the item is shared.
    /// </summary>
    public Guid? ToWhom { get; set; }

    /// <summary>
    /// Determines if the recipient can share this item further.
    /// </summary>
    public required bool CanShare { get; set; }

    /// <summary>
    /// Determines if the recipient can edit this item.
    /// </summary>
    public required bool CanEdit { get; set; }

    /// <summary>
    /// Determines if the recipient can delete this item.
    /// </summary>
    public required bool CanDelete { get; set; }

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
    public virtual User_Login O_User { get; set; }
#pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="b"><inheritdoc/></param>
    public static void Build(EntityTypeBuilder<Share_Item> b)
    {
        b.UseTemplates();
        b.UseIEntity_GuidID();
        b.UseIEntity_User();

        b.Property(s => s.Visibility).IsRequired(true);
        b.Property(s => s.Type).IsRequired(true);
        b.Property(s => s.ItemID).IsRequired(true);
        b.Property(s => s.ToWhom).IsRequired(false);

        b.Property(s => s.CanShare).IsRequired(true).HasDefaultValue(false);
        b.Property(s => s.CanEdit).IsRequired(true).HasDefaultValue(false);
        b.Property(s => s.CanDelete).IsRequired(true).HasDefaultValue(false);
    }
}

public enum ShareVisibility : byte
{
    None = 0,
    Directly = 1,
    Group = 2,
    Public = 3,
}

public enum ShareType : byte
{
    Entry,
    Template
}
