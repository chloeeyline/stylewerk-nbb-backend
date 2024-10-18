using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;

namespace StyleWerk.NBB.Database.User;

public class User_Login : IConnectedEntity<User_Login>, IEntity_GuidID
{
    public required Guid ID { get; set; }

    public required string Email { get; set; }

    public required string EmailNormalized { get; set; }

    public required string Username { get; set; }

    public required string UsernameNormalized { get; set; }

    public required string PasswordHash { get; set; }

    public required string PasswordSalt { get; set; }

    public required bool Admin { get; set; }

    public UserStatus? StatusCode { get; set; }

    public string? StatusToken { get; set; }

    public long? StatusTokenExpireTime { get; set; }

    public string? NewEmail { get; set; }

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
    public virtual User_Information O_Information { get; set; }

    public virtual List<User_Token> O_Token { get; set; }
#pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.

    public static void Build(EntityTypeBuilder<User_Login> b)
    {
        b.UseTemplates();
        b.UseIEntity_GuidID();

        b.Property(s => s.Username).IsRequired(true).HasMaxLength(30);
        b.Property(s => s.UsernameNormalized).IsRequired(true).HasMaxLength(30);
        b.Property(s => s.Email).IsRequired(true).HasMaxLength(100);
        b.Property(s => s.EmailNormalized).IsRequired(true).HasMaxLength(100);

        b.Property(s => s.PasswordHash).IsRequired(true);
        b.Property(s => s.PasswordSalt).IsRequired(true);

        b.Property(s => s.Admin).IsRequired(true);
        b.Property(s => s.StatusCode).IsRequired(false);
        b.Property(s => s.StatusToken).IsRequired(false);
        b.Property(s => s.StatusTokenExpireTime).IsRequired(false);
        b.Property(s => s.NewEmail).IsRequired(false);

        b.HasIndex(s => s.UsernameNormalized).IsUnique(true);
        b.HasIndex(s => s.EmailNormalized).IsUnique(true);
        b.HasIndex(s => s.PasswordHash).IsUnique(true);
        b.HasIndex(s => s.PasswordSalt).IsUnique(true);
        b.HasIndex(s => s.StatusToken).IsUnique(true);
    }

    public static void Connect(EntityTypeBuilder<User_Login> b)
    {
        b.HasOne(s => s.O_Information)
            .WithOne(s => s.O_User)
            .IsRequired(false)
            .HasForeignKey<User_Information>(s => s.ID)
            .HasConstraintName("Information")
            .OnDelete(DeleteBehavior.Cascade);
        b.HasMany(s => s.O_Token)
            .WithOne(s => s.O_User)
            .IsRequired(false)
            .HasForeignKey(s => s.ID)
            .HasConstraintName("Token")
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public enum UserStatus
{
    EmailVerification = 1,

    EmailChange = 2,

    PasswordReset = 3
}

