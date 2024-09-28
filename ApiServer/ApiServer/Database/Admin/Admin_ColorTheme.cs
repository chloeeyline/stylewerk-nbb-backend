using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;

namespace StyleWerk.NBB.Database.Admin;

public class Admin_ColorTheme : IEntity<Admin_ColorTheme>, IEntity_GuidID, IEntity_Name
{
    public required Guid ID { get; set; }
    public required string Name { get; set; }
    public required string Base { get; set; }
    public required string Data { get; set; }

    public static void Build(EntityTypeBuilder<Admin_ColorTheme> b)
    {
        b.UseTemplates();
        b.UseIEntity_GuidID();
        b.UseIEntity_Name();

        b.Property(s => s.Base).IsRequired(true).HasMaxLength(50);
        b.Property(s => s.Data).HasColumnType("JSONB").IsRequired(true);
    }
}
