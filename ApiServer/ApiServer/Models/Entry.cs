namespace StyleWerk.NBB.Models;

public record Model_EntryFolders(Guid ID, string? Name, Model_EntryItem[] Items, bool Owned, int TotalCount, int? FilteredCount);

public record Model_EntryItem(Guid ID,
    Guid FolderID,
    string Name,
    bool IsEncrypted,
    bool IsPublic,
    string? Tags,
    long CreatedAt,
    long LastUpdatedAt,
    string TemplateName,
    string Username);
