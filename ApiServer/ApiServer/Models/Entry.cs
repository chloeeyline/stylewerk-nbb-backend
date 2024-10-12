namespace StyleWerk.NBB.Models;

public record Model_EntryFolders(Guid ID, string? Name, Model_EntryItem[] Items);

public record Model_EntryItem(Guid ID,
    Guid FolderID,
    string Name,
    bool IsEncrypted,
    string? Tags,
    long CreatedAt,
    long LastUpdatedAt,
    string TemplateName,
    string Username);
