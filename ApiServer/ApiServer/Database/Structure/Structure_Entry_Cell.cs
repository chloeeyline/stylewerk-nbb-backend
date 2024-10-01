using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;

namespace StyleWerk.NBB.Database.Structure;

public class Structure_Entry_Cell : IConnectedEntity<Structure_Entry_Cell>, IEntity_GuidID
{
    public required Guid ID { get; set; }
    public required Guid RowID { get; set; }
    public required Guid TemplateID { get; set; }
    public required string Data { get; set; }

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
    public virtual Structure_Entry_Row O_Row { get; set; }
    public virtual Structure_Template_Cell O_Template { get; set; }
#pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.

    public static void Build(EntityTypeBuilder<Structure_Entry_Cell> b)
    {
        b.UseTemplates();
        b.UseIEntity_GuidID();
        b.Property(s => s.RowID).IsRequired(true);
        b.Property(s => s.TemplateID).IsRequired(true);
        b.Property(s => s.Data).IsRequired(true);
    }

    public static void Connect(EntityTypeBuilder<Structure_Entry_Cell> b)
    {
        b.HasOne(s => s.O_Row)
            .WithMany(s => s.O_Cells)
            .HasForeignKey(e => e.RowID)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasOne(s => s.O_Template)
            .WithMany()
            .HasForeignKey(e => e.TemplateID)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
