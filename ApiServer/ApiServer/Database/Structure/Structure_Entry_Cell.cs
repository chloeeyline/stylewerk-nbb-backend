using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;

namespace StyleWerk.NBB.Database.Structure;

public class Structure_Entry_Cell : IConnectedEntity<Structure_Entry_Cell>
{
	public Guid EntryID { get; set; }
	public Guid CellID { get; set; }

	public string Metadata { get; set; }

	public Structure_Entry O_Entry { get; set; }
	public Structure_Template_Cell O_Cell { get; set; }

	public static void Build(EntityTypeBuilder<Structure_Entry_Cell> b)
	{
		b.UseTemplates();

		b.HasKey(s => new { s.EntryID, s.CellID });

		b.Property(s => s.EntryID).IsRequired(true);
		b.Property(s => s.CellID).IsRequired(true);
		b.Property(s => s.Metadata).IsRequired(true);
	}

	public static void Connect(EntityTypeBuilder<Structure_Entry_Cell> b)
	{
		b.HasOne(s => s.O_Entry).WithMany(s => s.O_Cells).HasForeignKey(e => e.EntryID);
		b.HasOne(s => s.O_Cell).WithMany(s => s.O_EntryCells).HasForeignKey(e => e.CellID);
	}
}
