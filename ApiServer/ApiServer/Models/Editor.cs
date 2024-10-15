namespace StyleWerk.NBB.Models;

/// <summary>
/// Primary model for the whole editor. It's used to add a new template or entry as well as update these, show them in the editor or display them as non editable preview.
/// </summary>
/// <param name="ID">unique entry identifier</param>
/// <param name="FolderID">unique identifier of the folder</param>
/// <param name="TemplateID">unique template identifier</param>
/// <param name="Name">name of the entry</param>
/// <param name="Tags">tags on the entry</param>
/// <param name="IsEncrypted">decides wether the entry is encrypted or not</param>
/// <param name="IsPublic">decides if the entry can be shared public</param>
/// <param name="Template">contains all template details</param>
/// <param name="Items">list of all entry rows</param>
public record Model_Editor(
    Guid ID,
    Guid? FolderID,
    Guid TemplateID,
    string? Name,
    string? Tags,
    bool IsEncrypted,
    bool IsPublic,
    Template? Template,
    List<EntryRow> Items);

/// <summary>
/// Represents one entry row
/// </summary>
/// <param name="ID">unique identifier of the row</param>
/// <param name="TemplateID">unique identifier of the template the row is based on</param>
/// <param name="Template">contains all template row details from the template the entry is based on</param>
/// <param name="Items">list of all the entry cells within this row</param>
public record EntryRow(
    Guid ID,
    Guid TemplateID,
    TemplateRow? Template,
    List<EntryCell> Items);

/// <summary>
/// Represents one entry cell within a row
/// </summary>
/// <param name="ID">unique identifier of the cell</param>
/// <param name="TemplateID">uniuqe identifier of the template the cell is based on</param>
/// <param name="Data">entry inputs from user</param>
/// <param name="Template">contains all template cell details from the template the entry is based on</param>
public record EntryCell(
    Guid ID,
    Guid TemplateID,
    string? Data,
    TemplateCell? Template);

/// <summary>
/// Illustrates the template without rows and cells
/// </summary>
/// <param name="ID">unique identifier of the template</param>
/// <param name="Name">template name</param>
/// <param name="IsPublic">boolean to show if the template is public visible</param>
/// <param name="Description">description of the template</param>
/// <param name="Tags">tags on the template</param>
public record Template(
    Guid ID,
    string? Name,
    bool IsPublic,
    string? Description,
    string? Tags);

/// <summary>
/// Symbolizes one template row without being bound to the template
/// </summary>
/// <param name="ID">unique identifier of the template row</param>
/// <param name="CanWrapCells">boolean to decide whether the row can wrap cells</param>
/// <param name="CanRepeat">boolean to decide whether the row can be repeated</param>
/// <param name="HideOnNoInput">boolean to decide whether the row should be shown if the input is null</param>
public record TemplateRow(Guid ID,
    bool CanWrapCells,
    bool CanRepeat,
    bool HideOnNoInput);

/// <summary>
/// Symbolizes one template cell without being bound to the template row
/// </summary>
/// <param name="ID">unique identifier of the cell</param>
/// <param name="InputHelper">specifies the datatype of the input</param>
/// <param name="HideOnEmpty">boolean to decide whether the cell should be displayed if it's empty</param>
/// <param name="IsRequired">boolean to decide whether the cell needs to have an input</param>
/// <param name="Text">input of user</param>
/// <param name="Description">describes the cell on mouse hover</param>
/// <param name="MetaData">shows meta data of the cell</param>
public record TemplateCell(
    Guid ID,
    int InputHelper,
    bool HideOnEmpty,
    bool IsRequired,
    string? Text,
    string? Description,
    string? MetaData);
