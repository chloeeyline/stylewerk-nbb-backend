using StyleWerk.NBB.Database.Share;

namespace StyleWerk.NBB.Models;

public record Model_EntryFolders(Guid? ID, string? Name, Model_EntryItem[] Items);
public record Model_EntryItem(Guid ID, string Name, string UserName, string TemplateName, string? Tags, long CreatedAt, long LastUpdatedAt);

public record Model_EntryFilterItem(Guid ID,
    string Name,
    bool IsEncrypted,
    string? Tags,
    long CreatedAt,
    long LastUpdatedAt,
    string TemplateName,
    string OwnerUsername,
    ShareVisibility Visibility,
    string? FolderName,
    string? SharedByUsername,
    string? GroupName);

public record Model_Entry(Guid? ID, Guid? FolderID, Guid TemplateID, string? Name, string? Tags, bool IsEncrypted, List<Model_EntryRow> Items);
public record Model_EntryRow(Guid? ID, Guid TemplateID, int SortOrder, Model_TemplateRow? Info, List<Model_EntryCell> Items);
public record Model_EntryCell(Guid? ID, Guid TemplateID, Model_TemplateCell? Info, string? Data);
