using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;

namespace StyleWerk.NBB.Database.Structure;

public class Structure_Entry_Folder : IConnectedEntity<Structure_Entry_Folder>, IEntity_GuidID, IEntity_Name, IEntity_SortOrder
{
	public Guid ID { get; set; } = Guid.Empty;

	public string Name { get; set; } = string.Empty;
	public int SortOrder { get; set; } = 0;

	public List<Structure_Entry> O_EntryList { get; set; }

	public static void Build(EntityTypeBuilder<Structure_Entry_Folder> b)
	{
		b.UseTemplates();
		b.UseIEntity_GuidID();
		b.UseIEntity_Name();
		b.UseIEntity_SortOrder();
	}

	public static void Connect(EntityTypeBuilder<Structure_Entry_Folder> b)
	{
		b.HasMany(s => s.O_EntryList).WithOne(s => s.O_Folder).HasForeignKey(s => s.FolderID);
	}
}
