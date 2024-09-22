using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StyleWerk.NBB.Database.Core;

public static class Extension
{
    /// <summary>
    /// Retrieves an entity by its globally unique identifier.
    /// </summary>
    /// <typeparam name="T">The type of the entity.</typeparam>
    /// <param name="table">The IQueryable to search within.</param>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    public static T? GetByID<T>(this IQueryable<T> table, Guid? id) where T : class, IEntity<T>, IEntity_GuidID
    {
        return table is not null && id.HasValue && id != Guid.Empty ? table.FirstOrDefault(s => s.ID == id) : null;
    }

    /// <summary>
    /// Applies basic template configuration to an entity type builder.
    /// </summary>
    /// <typeparam name="T">The entity type being configured.</typeparam>
    /// <param name="b">The builder used for entity type configuration.</param>
    public static void UseTemplates<T>(this EntityTypeBuilder<T> b) where T : class, IEntity<T>
    {
        b.ToTable(typeof(T).Name);
    }

    /// <summary>
    /// Configures the primary key and default values for entities with a GUID ID.
    /// </summary>
    /// <typeparam name="T">The entity type being configured.</typeparam>
    /// <param name="b">The builder used for entity type configuration.</param>
    public static void UseIEntity_GuidID<T>(this EntityTypeBuilder<T> b, bool defaultValue = true) where T : class, IEntity<T>, IEntity_GuidID
    {
        b.HasKey(s => s.ID);
        PropertyBuilder<Guid> prop = b.Property(s => s.ID)
            .IsRequired(true)
            .HasColumnName("ID");
        if (defaultValue) prop.HasDefaultValueSql("uuid_generate_v4()");
    }

    /// <summary>
    /// Configures the 'Name' property for entities with a name attribute.
    /// </summary>
    /// <typeparam name="T">The entity type being configured.</typeparam>
    /// <param name="b">The builder used for entity type configuration.</param>
    public static void UseIEntity_Name<T>(this EntityTypeBuilder<T> b) where T : class, IEntity<T>, IEntity_Name
    {
        b.Property(s => s.Name).IsRequired(true).HasMaxLength(250);
    }

    /// <summary>
    /// Configures the 'SortOrder' property for sortable entities.
    /// </summary>
    /// <typeparam name="T">The entity type being configured.</typeparam>
    /// <param name="b">The builder used for entity type configuration.</param>
    public static void UseIEntity_SortOrder<T>(this EntityTypeBuilder<T> b) where T : class, IEntity<T>, IEntity_SortOrder
    {
        b.Property(s => s.SortOrder).IsRequired(true);
    }

    /// <summary>
    /// Configures the creation and last update dates for entities with editable dates.
    /// </summary>
    /// <typeparam name="T">The entity type being configured.</typeparam>
    /// <param name="b">The builder used for entity type configuration.</param>
    public static void UseIEntity_EditDate<T>(this EntityTypeBuilder<T> b) where T : class, IEntity<T>, IEntity_EditDate
    {
        b.Property(s => s.CreatedAt).IsRequired(true).HasDefaultValueSql("NOW()");
        b.Property(s => s.LastUpdatedAt).IsRequired(true).HasDefaultValueSql("NOW()");
    }
}
