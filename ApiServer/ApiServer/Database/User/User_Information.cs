using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;

namespace StyleWerk.NBB.Database.User;

/// <summary>
/// Represents detailed information about a user.
/// </summary>
public class User_Information : IConnectedEntity<User_Information>, IEntity_GuidID
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public required Guid ID { get; set; }

    /// <summary>
    /// The gender of the user.
    /// </summary>
    public required UserGender Gender { get; set; }

    /// <summary>
    /// The first name of the user.
    /// </summary>
    public required string FirstName { get; set; }

    /// <summary>
    /// The last name of the user.
    /// </summary>
    public required string LastName { get; set; }

    /// <summary>
    /// The birthday of the user.
    /// </summary>
    public required DateOnly Birthday { get; set; }

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
    /// <summary>
    /// Navigation property for the user login associated with this information.
    /// </summary>
    public virtual User_Login O_User { get; set; }
#pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="b"><inheritdoc/></param>
    public static void Build(EntityTypeBuilder<User_Information> b)
    {
        b.UseTemplates();
        b.UseIEntity_GuidID(false);
        b.Property(s => s.Gender).IsRequired(true);
        b.Property(s => s.FirstName).IsRequired(true).HasMaxLength(50);
        b.Property(s => s.LastName).IsRequired(true).HasMaxLength(50);
        b.Property(s => s.Birthday).IsRequired(true);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="b"><inheritdoc/></param>
    public static void Connect(EntityTypeBuilder<User_Information> b)
    {
        b.HasOne(s => s.O_User)
            .WithOne(s => s.O_Information)
            .IsRequired(true)
            .HasForeignKey<User_Information>(s => s.ID)
            .HasConstraintName("User");
    }
}

/// <summary>
/// Enumeration for user gender options.
/// </summary>
public enum UserGender : byte
{
    NotSpecified,
    Female,
    Male,
    NonBinary
}
