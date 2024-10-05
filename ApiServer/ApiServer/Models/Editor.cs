namespace StyleWerk.NBB.Models;

public record Model_Editor(
    Guid? ID,
    Guid? FolderID,
    Guid? TemplateID,
    string? Name,
    string? Tags,
    bool IsEncrypted,
    Template? Template,
    List<EntryRow> Items);
public record EntryRow(
    Guid? ID,
    Guid? TemplateID,
    TemplateRow? Template,
    List<EntryCell> Items);
public record EntryCell(
    Guid? ID,
    Guid? TemplateID,
    string? Data,
    TemplateCell? Template);

public record Template(
    Guid? ID,
    string? Name,
    string? Description,
    string? Tags);
public record TemplateRow(Guid? ID,
    bool CanWrapCells,
    bool CanRepeat,
    bool HideOnNoInput);
public record TemplateCell(
    Guid? ID,
    int InputHelper,
    bool HideOnEmpty,
    bool IsRequired,
    string? Text,
    string? Description,
    string? MetaData);
