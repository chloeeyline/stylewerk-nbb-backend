using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;

namespace StyleWerk.NBB.Database.Structure;

public class Structure_Template_Cell : IConnectedEntity<Structure_Template_Cell>, IEntity_GuidID, IEntity_SortOrder
{
	public Guid ID { get; set; } = Guid.Empty;
	public Guid RowID { get; set; } = Guid.Empty;

	public int SortOrder { get; set; } = 0;
	public int InputHelper { get; set; } = -1;
	public bool HideOnEmpty { get; set; } = false;
	public bool IsRequiered { get; set; } = false;
	public string? Text { get; set; } = null;
	public string? MetaData { get; set; } = null;

	public Structure_Template_Row O_Row { get; set; } = new();
	public List<Structure_Entry_Cell> O_EntryCells { get; set; } = [];

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
