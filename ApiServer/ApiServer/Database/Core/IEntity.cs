using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.User;

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
    Guid ID { get; set; }
}

public interface IEntity_Name
{
    string Name { get; set; }
    string NameNormalized { get; set; }
}

public interface IEntity_EditDate
{
    long CreatedAt { get; set; }

    long LastUpdatedAt { get; set; }
}

public interface IEntity_SortOrder
{
    int SortOrder { get; set; }
}

public interface IEntity_User
{
    Guid UserID { get; set; }
    User_Login O_User { get; set; }
}