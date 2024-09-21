using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;

namespace StyleWerk.NBB.Database.User;

/// <summary>
/// Represents user login credentials and related information.
/// </summary>
public class User_Login : IConnectedEntity<User_Login>, IEntity_GuidID
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public Guid ID { get; set; }

    /// <summary>
    /// The email address associated with the user account.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// A normalized version of the email for consistent lookups.
    /// </summary>
    public string EmailNormalized { get; set; }

    /// <summary>
    /// The username associated with the user account.
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// A normalized version of the username for consistent lookups.
    /// </summary>
    public string UsernameNormalized { get; set; }

    /// <summary>
    /// Hash of the user's password.
    /// </summary>
    public string PasswordHash { get; set; }

    /// <summary>
    /// Salt used along with the password hash.
    /// </summary>
    public string PasswordSalt { get; set; }

    /// <summary>
    /// Indicates whether the user has administrative privileges.
    /// </summary>
    public bool Admin { get; set; }

    /// <summary>
    /// Current status code of the user, indicating states like email verification or password reset.
    /// </summary>
    public UserStatus StatusCode { get; set; }

    /// <summary>
    /// Optional token used for operations such as password reset or email change verification.
    /// </summary>
    public Guid? StatusToken { get; set; }

    /// <summary>
    /// The timestamp when the status token was issued.
    /// </summary>
    public DateTimeOffset? StatusTokenExpireTime { get; set; }

    /// <summary>
    /// Navigation property for the user's detailed information.
    /// </summary>
    public virtual User_Information O_Information { get; set; }

    /// <summary>
    /// Navigation property for the user's rights.
    /// </summary>
    public virtual List<User_Right> O_Right { get; set; }

    /// <summary>
    /// Navigation property for the user's authentication tokens.
    /// </summary>
    public virtual List<User_Token> O_Token { get; set; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="b"><inheritdoc/></param>
    public static void Build(EntityTypeBuilder<User_Login> b)
    {
        b.UseTemplates();
        b.Property(s => s.Username).IsRequired(true).HasMaxLength(30);
        b.Property(s => s.UsernameNormalized).IsRequired(true).HasMaxLength(30);
        b.Property(s => s.Email).IsRequired(true).HasMaxLength(100);
        b.Property(s => s.EmailNormalized).IsRequired(true).HasMaxLength(100);
        b.Property(s => s.PasswordHash).IsRequired(true);
        b.Property(s => s.PasswordSalt).IsRequired(true);
        b.Property(s => s.Admin).IsRequired(true);
        b.Property(s => s.StatusCode).IsRequired(true).HasDefaultValue(UserStatus.None);
        b.Property(s => s.StatusToken).IsRequired(false);
        b.Property(s => s.StatusTokenExpireTime).IsRequired(false);

        b.HasIndex(s => s.UsernameNormalized).IsUnique(true);
        b.HasIndex(s => s.EmailNormalized).IsUnique(true);
        b.HasIndex(s => s.PasswordHash).IsUnique(true);
        b.HasIndex(s => s.PasswordSalt).IsUnique(true);
        b.HasIndex(s => s.StatusToken).IsUnique(true);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    /// <param name="b"><inheritdoc/></param>
    public static void Connect(EntityTypeBuilder<User_Login> b)
    {
        b.HasOne(s => s.O_Information)
            .WithOne(s => s.O_User)
            .IsRequired(false)
            .HasForeignKey<User_Information>(s => s.ID)
            .HasConstraintName("Information");
        b.HasMany(s => s.O_Right)
            .WithOne(s => s.O_User)
            .IsRequired(false)
            .HasForeignKey(s => s.ID)
            .HasConstraintName("Right");
        b.HasMany(s => s.O_Token)
            .WithOne(s => s.O_User)
            .IsRequired(false)
            .HasForeignKey(s => s.ID)
            .HasConstraintName("Token");
    }
}

/// <summary>
/// Enumeration for the various status codes a user account can have.
/// </summary>
public enum UserStatus
{
    /// <summary>
    /// Indicates that no special status is applied to the user account. This is the default state.
    /// </summary>
    None,

    /// <summary>
    /// Indicates that the user's email address needs to be verified. This status is typically set after a new user registration or when a user updates their email address.
    /// </summary>
    EmailVerification,

    /// <summary>
    /// Indicates that the user has requested a change of email address and this new email needs verification before it can replace the old one.
    /// </summary>
    EmailChange,

    /// <summary>
    /// Indicates that the user has requested a password reset. This status is usually set when a user has forgotten their password and initiated a process to set a new one.
    /// </summary>
    PasswordReset
}

