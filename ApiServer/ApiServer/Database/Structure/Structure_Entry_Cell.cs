using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;

namespace StyleWerk.NBB.Database.Structure;

public class Structure_Entry_Cell : IConnectedEntity<Structure_Entry_Cell>
{
    public required Guid EntryID { get; set; }
    public required Guid CellID { get; set; }
    public required string Data { get; set; }

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
    public virtual Structure_Entry O_Entry { get; set; }
    public virtual Structure_Template_Cell O_Cell { get; set; }
#pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.

    public static void Build(EntityTypeBuilder<Structure_Entry_Cell> b)
    {
        b.UseTemplates();

        b.HasKey(s => new { s.EntryID, s.CellID });

        b.Property(s => s.EntryID).IsRequired(true);
        b.Property(s => s.CellID).IsRequired(true);
        b.Property(s => s.Data).IsRequired(true);
    }

    public static void Connect(EntityTypeBuilder<Structure_Entry_Cell> b)
    {
        b.HasOne(s => s.O_Entry).WithMany(s => s.O_Cells).HasForeignKey(e => e.EntryID);
        b.HasOne(s => s.O_Cell).WithMany(s => s.O_EntryCells).HasForeignKey(e => e.CellID);
    }
}
