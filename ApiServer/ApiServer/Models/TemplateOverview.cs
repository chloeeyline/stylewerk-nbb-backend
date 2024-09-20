using StyleWerk.NBB.Database.Structure;

namespace StyleWerk.NBB.Models
{
    public record Model_Templates(Guid Id, string TemplateTitle, string? Description, string UserName, DateTime CreatedAt, DateTime LastUpdated, ShareTypes Share)
    {
        public Model_Templates(Structure_Template item, ShareTypes share) : this(item.ID, item.Name, item.Description, item.O_User.Username, item.CreatedAt, item.LastUpdatedAt, share) { }
    }

    public record Model_TemplateRow(Guid RowId, Guid TemplateId, int SortOrder, bool CanWrapCells);
    public record Model_TemplateCell(Guid CellId, Guid RowId, int SortOrder, bool HideOnEmpty, bool IsRequired, string Text, string MetaData);
    //not sure yet what i should give back to FE
    public record Model_TemplatePreviewItems(Guid Id, string TemplateTitle, Model_TemplateRow[] TemplateRows, Model_TemplateCell[] TemplateCells);


    public record Model_FilterTemplate(string? Name, string? Username, string? TemplateName, string[]? Tags, ShareTypes Share, bool directUser);
    public record Model_ChangeTemplateName(string Name, Guid TemplateId);
    public record Model_AddTemplate(string Name, string Description, bool IsPublic);
    //Not quite sure if that function should be in a separate Controller
    //ShareId the id that the template gets shared. We can derive that info from the object sharetypes
    public record Model_ShareTemplate(Guid TemplateId, ShareTypes Share, Guid ShareId);
    public record Model_CopyTemplate(Guid TemplateId);
}
