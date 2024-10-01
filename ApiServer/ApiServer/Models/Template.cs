using StyleWerk.NBB.Database.Share;

namespace StyleWerk.NBB.Models;

public record Model_TemplatePaging(
    int Count,
    int Page,
    int MaxPage,
    int PerPage,
    List<Model_TemplateItem> Items);
public record Model_TemplateItem(
    Guid ID,
    string Name,
    string? Description,
    string? Tags,
    long CreatedAt,
    long LastUpdatedAt,
    string OwnerUsername,
    ShareVisibility Visibility,
    string? GroupName);


public record Model_Template(
    Guid? ID,
    string Name,
    string? Description,
    string? Tags,
    List<Model_TemplateRow> Items);
public record Model_TemplateRow(Guid ID,
    bool CanWrapCells,
    bool CanRepeat,
    bool HideOnNoInput,
    List<Model_TemplateCell> Items);
public record Model_TemplateCell(
    Guid ID,
    Guid RowID,
    int InputHelper,
    bool HideOnEmpty,
    bool IsRequired,
    string? Text,
    string? MetaData);