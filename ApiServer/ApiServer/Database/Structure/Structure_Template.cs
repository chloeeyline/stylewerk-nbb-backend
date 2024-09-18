using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;
using StyleWerk.NBB.Database.User;

namespace StyleWerk.NBB.Database.Structure;

public class Structure_Template : IConnectedEntity<Structure_Template>, IEntity_GuidID, IEntity_Name, IEntity_EditDate
{
	public Guid ID { get; set; }
	public Guid UserID { get; set; }

	public string Name { get; set; }
	public string? Description { get; set; }
	public bool IsPublic { get; set; }
	public DateTime CreatedAt { get; set; } = DateTime.MinValue;
	public DateTime LastUpdatedAt { get; set; } = DateTime.MinValue;

	public User_Login O_User { get; set; }
	public List<Structure_Entry> O_EntryList { get; set; }
	public List<Structure_Template_Row> O_Rows { get; set; }

	public static void Build(EntityTypeBuilder<Structure_Template> b)
	{
		b.UseTemplates();

		b.UseIEntity_GuidID();
		b.UseIEntity_Name();

		b.Property(s => s.UserID).IsRequired(true);
		b.Property(s => s.Description).IsRequired(false);
		b.Property(s => s.IsPublic).IsRequired(true).HasDefaultValue(false);

		b.UseIEntity_EditDate();
	}

	public static void Connect(EntityTypeBuilder<Structure_Template> b)
	{
		b.HasOne(s => s.O_User).WithMany().HasForeignKey(o => o.UserID);

		b.HasMany(s => s.O_EntryList).WithOne(s => s.O_Template).HasForeignKey(s => s.TemplateID);
		b.HasMany(s => s.O_Rows).WithOne(s => s.O_Template).HasForeignKey(s => s.TemplateID);
	}
}