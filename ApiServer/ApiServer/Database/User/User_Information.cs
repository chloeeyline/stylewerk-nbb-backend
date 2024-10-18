using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;

namespace StyleWerk.NBB.Database.User;

public class User_Information : IConnectedEntity<User_Information>, IEntity_GuidID
{
    public required Guid ID { get; set; }

    public required UserGender Gender { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public required long Birthday { get; set; }

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
    public virtual User_Login O_User { get; set; }
#pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.

    public static void Build(EntityTypeBuilder<User_Information> b)
    {
        b.UseTemplates();
        b.UseIEntity_GuidID();

        b.Property(s => s.Gender).IsRequired(true);
        b.Property(s => s.FirstName).IsRequired(true).HasMaxLength(50);
        b.Property(s => s.LastName).IsRequired(true).HasMaxLength(50);
        b.Property(s => s.Birthday).IsRequired(true);
    }

    public static void Connect(EntityTypeBuilder<User_Information> b)
    {
        b.HasOne(s => s.O_User)
            .WithOne(s => s.O_Information)
            .IsRequired(true)
            .HasForeignKey<User_Information>(s => s.ID)
            .HasConstraintName("User")
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public enum UserGender : byte
{
    NotSpecified,
    Female,
    Male,
    NonBinary
}
