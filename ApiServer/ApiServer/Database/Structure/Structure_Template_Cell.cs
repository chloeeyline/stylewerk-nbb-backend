using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;

namespace StyleWerk.NBB.Database.Structure;

public class Structure_Template_Cell : IConnectedEntity<Structure_Template_Cell>, IEntity_GuidID, IEntity_SortOrder
{
    public Guid ID { get; set; }
    public required Guid RowID { get; set; }
    public required int SortOrder { get; set; }
    public required int InputHelper { get; set; }
    public required bool HideOnEmpty { get; set; }
    public required bool IsRequiered { get; set; }
    public string? Text { get; set; }
    public string? MetaData { get; set; }

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
    public virtual Structure_Template_Row O_Row { get; set; }
    public virtual List<Structure_Entry_Cell> O_EntryCells { get; set; }
#pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.

    public static void Build(EntityTypeBuilder<Structure_Template_Cell> b)
    {
        b.UseTemplates();

        b.UseIEntity_GuidID();

        b.Property(s => s.RowID).IsRequired(true);

        b.UseIEntity_SortOrder();

        b.Property(s => s.InputHelper).IsRequired(true);
        b.Property(s => s.HideOnEmpty).IsRequired(true).HasDefaultValue(false);
        b.Property(s => s.IsRequiered).IsRequired(true).HasDefaultValue(false);
        b.Property(s => s.Text).IsRequired(false);
        b.Property(s => s.MetaData).IsRequired(false);
    }

    public static void Connect(EntityTypeBuilder<Structure_Template_Cell> b)
    {
        b.HasOne(s => s.O_Row).WithMany(s => s.O_Cells).HasForeignKey(s => s.RowID);

        b.HasMany(s => s.O_EntryCells).WithOne(s => s.O_Cell).HasForeignKey(s => s.CellID);
    }
}
