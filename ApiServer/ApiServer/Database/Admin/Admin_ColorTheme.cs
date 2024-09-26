using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;
using StyleWerk.NBB.Database.User;

namespace StyleWerk.NBB.Database.Admin;

public class Admin_ColorTheme : IEntity<Admin_ColorTheme>, IEntity_GuidID, IEntity_User, IEntity_Name
{
    public required Guid ID { get; set; }
    public required Guid UserID { get; set; }
    public required string Name { get; set; }
    public required string Base { get; set; }
    public required string Data { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public User_Login O_User { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public static void Build(EntityTypeBuilder<Admin_ColorTheme> b)
    {
        b.UseTemplates();
        b.UseIEntity_GuidID();
        b.UseIEntity_User();
        b.UseIEntity_Name();
        b.Property(s => s.Base).IsRequired(true).HasMaxLength(50);
        b.Property(s => s.Data).HasColumnType("JSONB").IsRequired(true);
    }
}
