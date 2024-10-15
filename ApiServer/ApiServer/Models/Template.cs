namespace StyleWerk.NBB.Models;

/// <summary>
/// Base model for template overview
/// </summary>
/// <param name="Paging">used to determine how many templates should be shown on one page</param>
/// <param name="Items">all available templates for the current user as list</param>
public record Model_TemplatePaging(Paging Paging, List<Model_TemplateItem> Items);

/// <summary>
/// Used to display noteable template information
/// </summary>
/// <param name="ID">unique identifier of the template</param>
/// <param name="Name">name of the template</param>
/// <param name="IsPublic">boolean to decide whether the template is public visible</param>
/// <param name="Description">description of the template</param>
/// <param name="Tags">tags on the template</param>
/// <param name="CreatedAt">creation date as unix timestamp</param>
/// <param name="LastUpdatedAt">shows the date when the template was last updated</param>
/// <param name="Username">shows the username of the creator</param>
/// <param name="Owned">boolean to decide whether the template belongs to the current user or was copied</param>
public record Model_TemplateItem(
    Guid ID,
    string Name,
    bool IsPublic,
    string? Description,
    string? Tags,
    long CreatedAt,
    long LastUpdatedAt,
    string Username,
    bool Owned);