using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;

namespace StyleWerk.NBB.Database.Share;

/// <summary>
/// Represents a shared item entity with access controls.
/// </summary>
public class Share_Item : IEntity<Share_Item>, IEntity_GuidID
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public required Guid ID { get; set; }
    /// <summary>
    /// User ID of the user who shared the item.
    /// </summary>
    public required Guid WhoShared { get; set; }

    /// <summary>
    /// Indicates whether the shared item is accessible to a group or individual user.
    /// </summary>
    public required ShareVisibility Visibility { get; set; }

    /// <summary>
    /// Type of the item being shared.
    /// </summary>
    public required byte ItemType { get; set; }

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

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="b"><inheritdoc/></param>
    public static void Build(EntityTypeBuilder<Share_Item> b)
    {
        b.UseTemplates();
        b.UseIEntity_GuidID();
        b.Property(s => s.WhoShared).IsRequired(true);

        b.Property(s => s.Visibility).IsRequired(true);
        b.Property(s => s.ItemType).IsRequired(true);
        b.Property(s => s.ItemID).IsRequired(true);
        b.Property(s => s.ToWhom).IsRequired(false);

        b.Property(s => s.CanShare).IsRequired(true).HasDefaultValue(false);
        b.Property(s => s.CanEdit).IsRequired(true).HasDefaultValue(false);
        b.Property(s => s.CanDelete).IsRequired(true).HasDefaultValue(false);
    }
}

public enum ShareVisibility
{
    Public,
    Group,
    Directly
}
