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
    /// <inheritdoc/>
    /// </summary>
    /// <param name="b"><inheritdoc/></param>
    public static void Build(EntityTypeBuilder<Share_Item> b)
    {
        b.UseTemplates();
        b.UseIEntity_GuidID();

        b.Property(s => s.Visibility).IsRequired(true);
        b.Property(s => s.Type).IsRequired(true);
        b.Property(s => s.ItemID).IsRequired(true);
        b.Property(s => s.ToWhom).IsRequired(false);
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
