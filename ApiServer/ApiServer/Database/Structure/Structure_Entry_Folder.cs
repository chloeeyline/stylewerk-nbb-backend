using Microsoft.EntityFrameworkCore.Metadata.Builders;

using StyleWerk.NBB.Database.Core;
using StyleWerk.NBB.Database.User;

namespace StyleWerk.NBB.Database.Structure;

public class Structure_Entry_Folder : IConnectedEntity<Structure_Entry_Folder>, IEntity_GuidID, IEntity_Name, IEntity_SortOrder, IEntity_User
{
    public required Guid ID { get; set; }
    public required Guid UserID { get; set; }
    public required string Name { get; set; }
    public required int SortOrder { get; set; }

#pragma warning disable CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.
    public virtual User_Login O_User { get; set; }
    public virtual List<Structure_Entry> O_EntryList { get; set; }
#pragma warning restore CS8618 // Ein Non-Nullable-Feld muss beim Beenden des Konstruktors einen Wert ungleich NULL enthalten. Erwägen Sie die Deklaration als Nullable.

    public static void Build(EntityTypeBuilder<Structure_Entry_Folder> b)
    {
        b.UseTemplates();
        b.UseIEntity_GuidID();
        b.UseIEntity_User(); ;
        b.UseIEntity_Name();

        b.UseIEntity_SortOrder();
    }

    public static void Connect(EntityTypeBuilder<Structure_Entry_Folder> b)
    {
        b.HasMany(s => s.O_EntryList).WithOne(s => s.O_Folder).HasForeignKey(s => s.FolderID);
    }
}
