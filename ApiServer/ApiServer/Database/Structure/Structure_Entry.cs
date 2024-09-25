using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;
using StyleWerk.NBB.Database.User;

namespace StyleWerk.NBB.Database.Structure;

public class Structure_Entry : IConnectedEntity<Structure_Entry>, IEntity_GuidID, IEntity_Name, IEntity_EditDate
{
    public Guid ID { get; set; }
    public required string Name { get; set; }
    public required Guid UserID { get; set; }
    public required Guid TemplateID { get; set; }
    public Guid? FolderID { get; set; }
    public bool IsPublic { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.MinValue;
    public DateTimeOffset LastUpdatedAt { get; set; } = DateTimeOffset.MinValue;
    public string[]? Tags { get; set; }

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
    public virtual User_Login O_User { get; set; }
    public virtual Structure_Template O_Template { get; set; }
    public virtual Structure_Entry_Folder? O_Folder { get; set; }
    public virtual List<Structure_Entry_Row> O_Rows { get; set; }
#pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.

    public static void Build(EntityTypeBuilder<Structure_Entry> b)
    {
        b.UseTemplates();

        b.UseIEntity_GuidID();
        b.UseIEntity_Name();

        b.Property(s => s.UserID).IsRequired(true);
        b.Property(s => s.TemplateID).IsRequired(true);
        b.Property(s => s.FolderID).IsRequired(false);
        b.Property(s => s.IsPublic).IsRequired(true);

        b.UseIEntity_EditDate();

        b.Property(s => s.Tags).IsRequired(false);
    }

    public static void Connect(EntityTypeBuilder<Structure_Entry> b)
    {
        b.HasOne(s => s.O_User).WithMany().HasForeignKey(s => s.UserID);
        b.HasOne(s => s.O_Template).WithMany(s => s.O_EntryList).HasForeignKey(s => s.TemplateID);
        b.HasOne(s => s.O_Folder).WithMany(s => s.O_EntryList).HasForeignKey(s => s.FolderID).IsRequired(false);

        b.HasMany(s => s.O_Rows).WithOne(s => s.O_Entry).HasForeignKey(s => s.EntryID);
    }
}