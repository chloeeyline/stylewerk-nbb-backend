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
    public Guid ID { get; set; }

    /// <summary>
    /// The gender of the user.
    /// </summary>
    public UserGender Gender { get; set; }

    /// <summary>
    /// The first name of the user.
    /// </summary>
    public string FirstName { get; set; }

    /// <summary>
    /// The last name of the user.
    /// </summary>
    public string LastName { get; set; }

    /// <summary>
    /// The birthday of the user.
    /// </summary>
    public DateOnly Birthday { get; set; }

    /// <summary>
    /// Navigation property for the user login associated with this information.
    /// </summary>
    public virtual User_Login O_User { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="b"><inheritdoc/></param>
    public static void Build(EntityTypeBuilder<User_Information> b)
    {
        b.UseTemplates();
        b.UseIEntity_GuidID();
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
