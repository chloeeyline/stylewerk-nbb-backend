using StyleWerk.NBB.Database.Structure;

namespace StyleWerk.NBB.Models;

public record Model_Templates(Guid Id, string TemplateTitle, string? Description, string UserName, long CreatedAt, long LastUpdated, string[] Tags, ShareTypes Share)
{
    public Model_Templates(Structure_Template item, ShareTypes share) : this(item.ID, item.Name, item.Description, item.O_User.Username, item.CreatedAt.ToUnixTimeMilliseconds(), item.LastUpdatedAt.ToUnixTimeMilliseconds(), item.Tags is null ? [] : item.Tags, share) { }
}
public record Model_TemplateRow(Guid RowId, Guid TemplateId, int SortOrder, bool CanWrapCells, Model_TemplateCell[] Cells);
public record Model_TemplateCell(Guid CellId, Guid RowId, int SortOrder, bool HideOnEmpty, bool IsRequired, string? Text, string? MetaData);
public record Model_TemplatePreviewItems(Guid Id, string TemplateTitle, Model_TemplateRow[] TemplateRows);
public record Model_FilterTemplate(string? Name, string? Username, string[] Tags, ShareTypes Share, bool DirectUser);
public record Model_ChangeTemplateDescription(string Description, Guid TemplateId);
public record Model_ChangeTemplateName(string Name, Guid TemplateId);
public record Model_AddTemplate(string Name, string? Description, string[]? Tags);
public record Model_ShareTemplate(Guid TemplateId, ShareTypes Share, Guid? ShareId, ShareRights Rights);