using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;

namespace StyleWerk.NBB.Database.User;

public class User_Token : IConnectedEntity<User_Token>, IEntity_GuidID
{
    public required Guid ID { get; set; }

    public required string Agent { get; set; }

    public required string RefreshToken { get; set; }

    public required long RefreshTokenExpiryTime { get; set; }

    public required bool ConsistOverSession { get; set; }

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
    public virtual User_Login O_User { get; set; }
#pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.

    public static void Build(EntityTypeBuilder<User_Token> b)
    {
        b.UseTemplates();
        b.UseIEntity_GuidID();
        b.Property(s => s.Agent).IsRequired(true);
        b.HasKey(s => new { s.ID, s.Agent });

        b.Property(s => s.RefreshToken).IsRequired(true);
        b.Property(s => s.RefreshTokenExpiryTime).IsRequired(true);
        b.Property(s => s.ConsistOverSession).IsRequired(true);

    }

    public static void Connect(EntityTypeBuilder<User_Token> b)
    {
        b.HasOne(s => s.O_User)
            .WithMany(s => s.O_Token)
            .IsRequired(true)
            .HasForeignKey(s => s.ID)
            .HasConstraintName("User")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
