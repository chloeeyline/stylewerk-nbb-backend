using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StyleWerk.NBB.Database.Core;

public static class Extension
{
	public static T? GetByID<T>(this IQueryable<T> table, Guid? id) where T : class, IEntity<T>, IEntity_GuidID
	{
		return table is not null && id.HasValue && id != Guid.Empty ? table.FirstOrDefault(s => s.ID == id) : null;
	}
	public static void UseTemplates<T>(this EntityTypeBuilder<T> b) where T : class, IEntity<T>
	{
		b.ToTable(typeof(T).Name);
	}
	public static void UseIEntity_GuidID<T>(this EntityTypeBuilder<T> b) where T : class, IEntity<T>, IEntity_GuidID
	{
		b.HasKey(s => s.ID);
		b.Property(s => s.ID)
			.IsRequired(true)
			.HasColumnName("ID")
			.HasDefaultValueSql("uuid_generate_v4()");
	}
	public static void UseIEntity_Name<T>(this EntityTypeBuilder<T> b) where T : class, IEntity<T>, IEntity_Name
	{
		b.Property(s => s.Name).IsRequired(true).HasMaxLength(250);
	}

	public static void UseIEntity_SortOrder<T>(this EntityTypeBuilder<T> b) where T : class, IEntity<T>, IEntity_SortOrder
	{
		b.Property(s => s.SortOrder).IsRequired(true);
	}

	public static void UseIEntity_EditDate<T>(this EntityTypeBuilder<T> b) where T : class, IEntity<T>, IEntity_EditDate
	{
		b.Property(s => s.CreatedAt).IsRequired(true).HasDefaultValueSql("NOW()");
		b.Property(s => s.LastUpdatedAt).IsRequired(true).HasDefaultValueSql("NOW()");
	}
}