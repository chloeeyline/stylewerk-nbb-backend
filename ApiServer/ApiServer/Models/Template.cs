using StyleWerk.NBB.Database.Share;

namespace StyleWerk.NBB.Models;

public record Model_TemplatePaging(Paging Paging, List<Model_TemplateItem> Items);
public record Model_TemplateItem(
    Guid ID,
    string Name,
    string? Description,
    string? Tags,
    long CreatedAt,
    long LastUpdatedAt,
    string Username,
    ShareVisibility Visibility);


public record Model_Template(
    Guid? ID,
    string Name,
    string? Description,
    string? Tags,
    List<Model_TemplateRow> Items);
public record Model_TemplateRow(Guid? ID,
    bool CanWrapCells,
    bool CanRepeat,
    bool HideOnNoInput,
    List<Model_TemplateCell> Items);
public record Model_TemplateCell(
    Guid? ID,
    int InputHelper,
    bool HideOnEmpty,
    bool IsRequired,
    string? Text,
    string? MetaData);