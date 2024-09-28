using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;

namespace StyleWerk.NBB.Database.Admin;

public class Admin_Language : IEntity<Admin_Language>, IEntity_Name
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public required string Data { get; set; }

    public static void Build(EntityTypeBuilder<Admin_Language> b)
    {
        b.UseTemplates();
        b.Property(s => s.Code).IsRequired().HasMaxLength(5);
        b.UseIEntity_Name();

        b.Property(s => s.Data).HasColumnType("JSONB").IsRequired(true);

        b.HasKey(s => s.Code);
    }
}