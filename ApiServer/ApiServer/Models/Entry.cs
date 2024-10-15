namespace StyleWerk.NBB.Models;

/// <summary>
/// Used to show all folders of the current user containing one or more entries 
/// </summary>
/// <param name="ID">unique identifier of the folder</param>
/// <param name="Name">name of the folder</param>
/// <param name="Items">entries</param>
/// <param name="Count">total amout of entries in folder</param>
public record Model_EntryFolders(Guid ID, string? Name, Model_EntryItem[] Items, int Count);

/// <summary>
/// Represents an entry in a folder
/// </summary>
/// <param name="ID">unique identifier of the entry</param>
/// <param name="Name">name of the entry</param>
/// <param name="IsEncrypted">boolean to show whether the entry datas encrypted</param>
/// <param name="IsPublic">boolean to show whether the entry is public visible</param>
/// <param name="Tags">tags on entry</param>
/// <param name="CreatedAt">show the creation date as unix timestamp</param>
/// <param name="LastUpdatedAt">show when the entry was last updated as unix timestamp</param>
/// <param name="TemplateName">shows the template name the entry is based on</param>
/// <param name="Username">shows the creator of the entry</param>
/// <param name="Owned">shows whether the entry belongs to current user or was copied from another user</param>
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
