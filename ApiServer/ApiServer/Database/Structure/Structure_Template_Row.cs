using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;

namespace StyleWerk.NBB.Database.Structure;

public class Structure_Template_Row : IConnectedEntity<Structure_Template_Row>, IEntity_GuidID, IEntity_SortOrder
{
	public Guid ID { get; set; } = Guid.Empty;
	public Guid TemplateID { get; set; } = Guid.Empty;
	public int SortOrder { get; set; } = 0;
	public bool CanWrapCells { get; set; } = true;

	public Structure_Template O_Template { get; set; }
	public List<Structure_Template_Cell> O_Cells { get; set; }

	public static void Build(EntityTypeBuilder<Structure_Template_Row> b)
	{
		b.UseTemplates();

		b.UseIEntity_GuidID();
		b.UseIEntity_SortOrder();

		b.Property(s => s.CanWrapCells).IsRequired(true).HasDefaultValue(true);
	}

	public static void Connect(EntityTypeBuilder<Structure_Template_Row> b)
	{
		b.HasOne(s => s.O_Template).WithMany(s => s.O_Rows).HasForeignKey(s => s.TemplateID);

		b.HasMany(s => s.O_Cells).WithOne(s => s.O_Row).HasForeignKey(s => s.RowID);
	}
}
