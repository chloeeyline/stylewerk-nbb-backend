using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StyleWerk.NBB.Database.Core;

public interface IEntity<TSelf>
	where TSelf : class, IEntity<TSelf>
{
	public static abstract void Build(EntityTypeBuilder<TSelf> b);
}

public interface IConnectedEntity<TSelf> : IEntity<TSelf>
	where TSelf : class, IConnectedEntity<TSelf>
{
	public static abstract void Connect(EntityTypeBuilder<TSelf> b);
}

public interface IEntity_GuidID
{
	public Guid ID { get; set; }
}

public interface IEntity_Name
{
	public string Name { get; set; }
}