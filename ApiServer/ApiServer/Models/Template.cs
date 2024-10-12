namespace StyleWerk.NBB.Models;

public record Model_TemplatePaging(Paging Paging, List<Model_TemplateItem> Items);
public record Model_TemplateItem(
    Guid ID,
    string Name,
    string? Description,
    string? Tags,
    long CreatedAt,
    long LastUpdatedAt,
    string Username);