using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;

namespace StyleWerk.NBB.Database.User;

/// <summary>
/// Represents the rights or permissions assigned to a user.
/// </summary>
public class User_Right : IConnectedEntity<User_Right>, IEntity_Name
{
    public Guid ID { get; set; }

    public string Name { get; set; }
    public UserRight Type { get; set; }


    /// <summary>
    /// Navigation property to the associated user login entity.
    /// </summary>
    public virtual User_Login O_User { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="b"><inheritdoc/></param>
    public static void Build(EntityTypeBuilder<User_Right> b)
    {
        b.UseTemplates();
        b.Property(s => s.ID).IsRequired(true).HasColumnName("ID");
        b.UseIEntity_Name();
        b.HasKey(s => new { s.ID, s.Name });
        b.Property(s => s.Type).IsRequired();
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
    }


}

/// <summary>
/// Flags enumeration for detailed user rights, allowing combination of multiple rights.
/// This supports setting and checking multiple permissions easily using bitwise operations.
/// </summary>
[Flags]
public enum UserRight
{
    /// <summary>
    /// No rights assigned. This value is used to represent a state where the user has no permissions.
    /// </summary>
    Restricted = 0,

    /// <summary>
    /// Basic access right, allowing the user to access general areas or features within the system.
    /// </summary>
    Access = 1,

    /// <summary>
    /// Permission to create new entities or documents within the system.
    /// </summary>
    Create = 2,

    /// <summary>
    /// Permission to edit existing entities or documents.
    /// </summary>
    Edit = 4,

    /// <summary>
    /// Permission to delete entities or documents.
    /// </summary>
    Delete = 8,

    /// <summary>
    /// Administrative rights, granting the ability to perform administrative functions beyond typical user capabilities.
    /// </summary>
    Admin = 16,

    /// <summary>
    /// Default usage combination, granting both access and create rights.
    /// This combination is typically assigned to users who need to contribute to the system by adding and accessing content.
    /// </summary>
    DefaultUsage = Access | Create,

    /// <summary>
    /// Combination of edit and delete rights, enabling users to modify and remove existing content.
    /// This set is usually assigned to users who manage areas of the system where content needs to be regularly updated or pruned.
    /// </summary>
    ManipulateOthers = Edit | Delete,

    /// <summary>
    /// The highest level of rights, combining administrative, editing, deleting, and creating capabilities.
    /// This level is reserved for users who need full control over all aspects of the system.
    /// </summary>
    SuperAdmin = Admin | ManipulateOthers | DefaultUsage,
}