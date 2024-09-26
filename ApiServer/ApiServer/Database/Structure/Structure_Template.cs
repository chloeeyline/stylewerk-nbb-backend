using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;
using StyleWerk.NBB.Database.User;

namespace StyleWerk.NBB.Database.Structure;

public class Structure_Template : IConnectedEntity<Structure_Template>, IEntity_GuidID, IEntity_Name, IEntity_EditDate, IEntity_User
{
    public required Guid ID { get; set; }
    public required Guid UserID { get; set; }
    public required string Name { get; set; }
    public required bool IsPublic { get; set; }
    public string? Description { get; set; }
    public string? Tags { get; set; }
    public long CreatedAt { get; set; }
    public long LastUpdatedAt { get; set; }

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
    public virtual User_Login O_User { get; set; }
    public virtual List<Structure_Entry> O_EntryList { get; set; }
    public virtual List<Structure_Template_Row> O_Rows { get; set; }
#pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.

    public static void Build(EntityTypeBuilder<Structure_Template> b)
    {
        b.UseTemplates();
        b.UseIEntity_GuidID();
        b.UseIEntity_User();
        b.UseIEntity_Name();
        b.Property(s => s.Description).IsRequired(false).HasMaxLength(500);
        b.Property(s => s.IsPublic).IsRequired(true).HasDefaultValue(false);

        b.UseIEntity_EditDate();
    }

    public static void Connect(EntityTypeBuilder<Structure_Template> b)
    {
        b.HasMany(s => s.O_EntryList).WithOne(s => s.O_Template).HasForeignKey(s => s.TemplateID);
        b.HasMany(s => s.O_Rows).WithOne(s => s.O_Template).HasForeignKey(s => s.TemplateID);
    }
}