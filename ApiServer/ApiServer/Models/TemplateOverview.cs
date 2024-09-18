using ChaosFox.Models;

namespace StyleWerk.NBB.Models;


public record TemplateOVerview(int Count, List<TemplateItem> Items) : PagingList<TemplateItem>(Count, Items);
public record TemplateItem(Guid ID, string Name, string UserName, string Description, DateTime CreatedAt, DateTime LastUpdatedAt, ShareType Share);

public record Model_FilterTemplate(string? Name, string? Username, string? Description, ShareType? Share, string[]? Tags);