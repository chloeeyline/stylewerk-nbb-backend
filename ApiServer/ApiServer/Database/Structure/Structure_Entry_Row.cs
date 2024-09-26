using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;

namespace StyleWerk.NBB.Database.Structure;

public class Structure_Entry_Row : IConnectedEntity<Structure_Entry_Row>, IEntity_GuidID, IEntity_SortOrder
{
    public required Guid ID { get; set; }
    public required Guid EntryID { get; set; }
    public required Guid TemplateID { get; set; }
    public required int SortOrder { get; set; }

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
    public virtual Structure_Entry O_Entry { get; set; }
    public virtual List<Structure_Entry_Cell> O_Cells { get; set; }
    public virtual Structure_Template_Row O_Template { get; set; }
#pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.

    public static void Build(EntityTypeBuilder<Structure_Entry_Row> b)
    {
        b.UseTemplates();
        b.UseIEntity_GuidID();
        b.Property(s => s.EntryID).IsRequired(true);
        b.Property(s => s.TemplateID).IsRequired(true);
        b.UseIEntity_SortOrder();
    }
    public static void Connect(EntityTypeBuilder<Structure_Entry_Row> b)
    {
        b.HasOne(s => s.O_Entry).WithMany(s => s.O_Rows).HasForeignKey(e => e.EntryID);
        b.HasOne(s => s.O_Template).WithMany().HasForeignKey(e => e.TemplateID);
        b.HasMany(s => s.O_Cells).WithOne(s => s.O_Row).HasForeignKey(s => s.RowID);
    }
}
