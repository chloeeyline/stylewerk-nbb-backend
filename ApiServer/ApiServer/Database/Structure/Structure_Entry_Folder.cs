using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;
using StyleWerk.NBB.Database.User;

namespace StyleWerk.NBB.Database.Structure;

public class Structure_Entry_Folder : IConnectedEntity<Structure_Entry_Folder>, IEntity_GuidID, IEntity_Name, IEntity_SortOrder
{
	public Guid ID { get; set; }
	public Guid UserID { get; set; }

	public string Name { get; set; }
	public int SortOrder { get; set; }

	public virtual User_Login O_User { get; set; }
	public List<Structure_Entry> O_EntryList { get; set; }

	public static void Build(EntityTypeBuilder<Structure_Entry_Folder> b)
	{
		b.UseTemplates();
		b.UseIEntity_GuidID();
		b.Property(s => s.UserID).IsRequired(true);
		b.UseIEntity_Name();
		b.UseIEntity_SortOrder();
	}

	public static void Connect(EntityTypeBuilder<Structure_Entry_Folder> b)
	{
		b.HasOne(s => s.O_User).WithMany().HasForeignKey(s => s.UserID);
		b.HasMany(s => s.O_EntryList).WithOne(s => s.O_Folder).HasForeignKey(s => s.FolderID);
	}
}
