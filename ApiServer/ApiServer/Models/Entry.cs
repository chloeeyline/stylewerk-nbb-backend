using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Database.Structure;

namespace StyleWerk.NBB.Models;

public record Model_EntryFolders(Guid? ID, string? Name, int SortOrder, Model_EntryItem[] Items);
public record Model_EntryItem(Guid ID, string Name, string? FolderName, string UserName, string TemplateName, string? Tags, long CreatedAt, long LastUpdatedAt, ShareVisibility? Visibility)
{
    public Model_EntryItem(Structure_Entry item, ShareVisibility? visibility) : this(item.ID, item.Name, item.O_Folder?.Name, item.O_User.Username, item.O_Template.Name, item.Tags, item.CreatedAt, item.LastUpdatedAt, visibility) { }
}

public record Model_FilterEntry(string? Name, string? Username, string? TemplateName, string? Tags, bool Public, bool Directly, bool Group, bool DirectUser);
public record Model_DetailedEntry(Guid? ID, Guid? FolderID, Guid TemplateID, string? Name, string? Tags, bool IsEncrypted, List<Model_EntryRow> Items);
public record Model_EntryRow(Guid? ID, Guid TemplateID, int SortOrder, Model_TemplateRow? Info, List<Model_EntryCell> Items);
public record Model_EntryCell(Guid? ID, Guid TemplateID, Model_TemplateCell? Info, string? Data);
