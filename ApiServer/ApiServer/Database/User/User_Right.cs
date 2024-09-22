using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;

namespace StyleWerk.NBB.Database.User;

/// <summary>
/// Represents the rights or permissions assigned to a user.
/// </summary>
public class User_Right : IConnectedEntity<User_Right>, IEntity_GuidID, IEntity_Name
{
    public required Guid ID { get; set; }
    public required string Name { get; set; }


#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
    /// <summary>
    /// Navigation property to the associated user login entity.
    /// </summary>
    public virtual User_Login O_User { get; set; }
    public virtual Right O_Right { get; set; }
#pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="b"><inheritdoc/></param>
    public static void Build(EntityTypeBuilder<User_Right> b)
    {
        b.UseTemplates();
        b.UseIEntity_GuidID(false);
        b.UseIEntity_Name();
        b.HasKey(s => new { s.ID, s.Name });
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="b"><inheritdoc/></param>
    public static void Connect(EntityTypeBuilder<User_Right> b)
    {
        b.HasOne(s => s.O_User)
            .WithMany(s => s.O_Right)
            .IsRequired(true)
            .HasForeignKey(s => s.ID)
            .HasConstraintName("User");
        b.HasOne(s => s.O_Right)
             .WithMany(s => s.O_User)
             .HasForeignKey(s => s.Name);
    }
}

public class Right : IConnectedEntity<Right>
{
    public required string Name { get; set; }
#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
    /// <summary>
    /// Navigation property to the associated user login entity.
    /// </summary>
    public virtual List<User_Right> O_User { get; set; }
#pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.

    public static void Build(EntityTypeBuilder<Right> b)
    {
        b.Property(s => s.Name).IsRequired(true);
        b.HasKey(s => s.Name);
    }

    public static void Connect(EntityTypeBuilder<Right> b)
    {
        b.HasMany(s => s.O_User)
             .WithOne(s => s.O_Right)
             .HasForeignKey(s => s.Name);
    }
}