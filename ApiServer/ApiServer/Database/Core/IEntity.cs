using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StyleWerk.NBB.Database.Core;

/// <summary>
/// Provides a contract for entities to include custom configuration logic for entity properties and relationships.
/// </summary>
/// <typeparam name="TSelf">The type of the entity being configured.</typeparam>
public interface IEntity<TSelf>
	where TSelf : class, IEntity<TSelf>
{
	/// <summary>
	/// Configures entity properties and relationships in a model builder.
	/// </summary>
	/// <param name="b">Entity type builder to configure the entity.</param>
	public static abstract void Build(EntityTypeBuilder<TSelf> b);
}

/// <summary>
/// Extends IEntity with functionality to connect related entities through navigation properties.
/// </summary>
/// <typeparam name="TSelf">The type of the connected entity.</typeparam>
public interface IConnectedEntity<TSelf> : IEntity<TSelf>
	where TSelf : class, IConnectedEntity<TSelf>
{
	/// <summary>
	/// Establishes connections between entities via navigation properties.
	/// </summary>
	/// <param name="b">Entity type builder to configure the relationships.</param>
	public static abstract void Connect(EntityTypeBuilder<TSelf> b);
}

/// <summary>
/// Represents an entity that has a globally unique identifier as its primary key.
/// </summary>
public interface IEntity_GuidID
{
	public Guid ID { get; set; }
}

/// <summary>
/// Represents an entity that has a name.
/// </summary>
public interface IEntity_Name
{
	public string Name { get; set; }
}

/// <summary>
/// Represents an entity that tracks creation and last updated dates.
/// </summary>
public interface IEntity_EditDate
{
	DateTime CreatedAt { get; set; }
	DateTime LastUpdatedAt { get; set; }
}

/// <summary>
/// Represents an entity that can be ordered.
/// </summary>
public interface IEntity_SortOrder
{
	int SortOrder { get; set; }
}