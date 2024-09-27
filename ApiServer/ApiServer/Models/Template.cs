using StyleWerk.NBB.Database.Structure;

namespace StyleWerk.NBB.Models;

public record Model_Template(Guid ID, string Name, string? Description, string UserName, long CreatedAt, long LastUpdated, string? Tags, ShareTypes Share)
{
    public Model_Template(Structure_Template item, ShareTypes share) : this(item.ID, item.Name, item.Description, item.O_User.Username, item.CreatedAt, item.LastUpdatedAt, item.Tags, share) { }
}

public record Model_TemplatePaging(int Count, int Page, int MaxPage, int PerPage, List<Model_Template> Items);
public record Model_TemplateRow(Guid ID, Guid TemplateID, int SortOrder, bool CanWrapCells, bool CanRepeat, bool HideOnNoInput, Model_TemplateCell[] Items)
{
    public Model_TemplateRow(Structure_Template_Row item) : this(item.ID, item.TemplateID, item.SortOrder, item.CanWrapCells, item.CanRepeat, item.HideOnNoInput, [.. item.O_Cells.Select(s => new Model_TemplateCell(s))]) { }
}
public record Model_TemplateCell(Guid ID, Guid RowID, int SortOrder, bool HideOnEmpty, int InputHelper, bool IsRequired, string? Text, string? MetaData)
{
    public Model_TemplateCell(Structure_Template_Cell item) : this(item.ID, item.RowID, item.SortOrder, item.HideOnEmpty, item.InputHelper, item.IsRequired, item.Text, item.MetaData) { }
}
public record Model_DetailedTemplate(Guid ID, string Name, string? Description, Model_TemplateRow[] Items);
public record Model_FilterTemplate(string? Name, string? Username, string? Tags, ShareTypes Share, bool DirectUser, int Page, int PerPage);
public record Model_ChangeTemplateDescription(Guid ID, string? Description);
public record Model_ChangeTemplateName(Guid ID, string Name);
public record Model_AddTemplate(string Name, string? Description, string? Tags);