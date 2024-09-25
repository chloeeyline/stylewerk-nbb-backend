using StyleWerk.NBB.Database.Structure;

namespace StyleWerk.NBB.Models;

public record Model_Template(Guid Id, string TemplateTitle, string? Description, string UserName, long CreatedAt, long LastUpdated, string[] Tags, ShareTypes Share)
{
    public Model_Template(Structure_Template item, ShareTypes share) : this(item.ID, item.Name, item.Description, item.O_User.Username, item.CreatedAt.ToUnixTimeMilliseconds(), item.LastUpdatedAt.ToUnixTimeMilliseconds(), item.Tags is null ? [] : item.Tags, share) { }
}

public record Model_TemplatePaging(int Count, int Page, int MaxPage, int PerPage, List<Model_Template> Items);
public record Model_TemplateRow(Guid RowId, Guid TemplateId, int SortOrder, bool CanWrapCells, Model_TemplateCell[] Cells)
{
    public Model_TemplateRow(Structure_Template_Row item) : this(item.ID, item.TemplateID, item.SortOrder, item.CanWrapCells, [.. item.O_Cells.Select(s => new Model_TemplateCell(s))]) { }
}
public record Model_TemplateCell(Guid CellId, Guid RowId, int SortOrder, bool HideOnEmpty, int InputHelper, bool IsRequired, string? Text, string? MetaData)
{
    public Model_TemplateCell(Structure_Template_Cell item) : this(item.ID, item.RowID, item.SortOrder, item.HideOnEmpty, item.IsRequiered, item.Text, item.MetaData) { }
}
public record Model_DetailedTemplate(Guid Id, string TemplateTitle, string? Description, Model_TemplateRow[] TemplateRows);
public record Model_FilterTemplate(string? Name, string? Username, string[] Tags, ShareTypes Share, bool DirectUser, int Page, int PerPage);
public record Model_ChangeTemplateDescription(string Description, Guid TemplateId);
public record Model_ChangeTemplateName(string Name, Guid TemplateId);
public record Model_AddTemplate(string Name, string? Description, string[]? Tags);