namespace StyleWerk.NBB.Models;

public record Model_EntryFolders(Guid ID, string? Name, Model_EntryItem[] Items, int Count);

public record Model_EntryItem(Guid ID,
    string Name,
    bool IsEncrypted,
    bool IsPublic,
    string? Tags,
    long CreatedAt,
    long LastUpdatedAt,
    string TemplateName,
    string Username,
    bool Owned);
