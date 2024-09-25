using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace StyleWerk.NBB.Controllers;

[ApiController, Route("Entry"), Authorize]
public class EntryController(NbbContext db) : BaseController(db)
{
    public EntryQueries Query => new(DB, CurrentUser);

    #region Folder
    [ApiExplorerSettings(GroupName = "Folder")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_EntryFolders>>))]
    [HttpGet(nameof(GetFolders))]
    public IActionResult GetFolders()
    {
        List<Model_EntryFolders> entries = Query.GetFolders();
        return Ok(new Model_Result<List<Model_EntryFolders>>(entries));
    }

    [ApiExplorerSettings(GroupName = "Folder")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_EntryFolders>>))]
    [HttpGet(nameof(GetFolderContent))]
    public IActionResult GetFolderContent(Guid? folderId)
    {
        List<Model_EntryItem> entries = Query.GetFolderContent(folderId);
        return Ok(new Model_Result<List<Model_EntryItem>>(entries));
    }

    [ApiExplorerSettings(GroupName = "Folder")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Guid>))]
    [HttpPost(nameof(AddFolder))]
    public IActionResult AddFolder(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new RequestException(ResultType.DataIsInvalid);

        bool isEmpty = !DB.Structure_Entry_Folder.Any();
        int sortOrder = isEmpty ? 1 :
            (DB.Structure_Entry_Folder.Where(s => s.UserID == CurrentUser.ID).Max(f => f.SortOrder) + 1);
        if (DB.Structure_Entry_Folder.Any(s => s.UserID == CurrentUser.ID && s.Name == name))
            throw new RequestException(ResultType.DataIsInvalid, "You already have a Folder with the same Name");

        Structure_Entry_Folder newFolder = new()
        {
            ID = Guid.NewGuid(),
            Name = name,
            SortOrder = sortOrder,
            UserID = CurrentUser.ID
        };

        DB.Structure_Entry_Folder.Add(newFolder);
        DB.SaveChanges();

        return Ok(new Model_Result<Guid>(newFolder.ID));
    }

    [ApiExplorerSettings(GroupName = "Folder")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(DeleteFolder))]
    public IActionResult DeleteFolder(Guid? folderId)
    {
        if (folderId is null)
            throw new RequestException(ResultType.DataIsInvalid);

        Structure_Entry_Folder? folder = DB.Structure_Entry_Folder.FirstOrDefault(f => f.ID == folderId)
           ?? throw new RequestException(ResultType.NoDataFound);

        IQueryable<Structure_Entry> entries = DB.Structure_Entry.Where(e => e.FolderID == folderId);
        if (entries.Any())
        {
            foreach (Structure_Entry? entry in entries)
            {
                entry.FolderID = null;
            }
        }

        DB.Structure_Entry_Folder.Remove(folder);
        DB.SaveChanges();

        return Ok(new Model_Result<string>());
    }

    [ApiExplorerSettings(GroupName = "Folder")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(DeleteEntryFromFolder))]
    public IActionResult DeleteEntryFromFolder(Model_DeleteFromFolder model)
    {
        if (model is null)
            throw new RequestException(ResultType.DataIsInvalid);

        Structure_Entry entry = DB.Structure_Entry.FirstOrDefault(e => e.FolderID == model.FolderId && e.ID == model.EntryId)
            ?? throw new RequestException(ResultType.NoDataFound);

        entry.FolderID = null;
        DB.SaveChanges();

        return Ok(new Model_Result<string>());
    }

    [ApiExplorerSettings(GroupName = "Folder")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(DragAndDrop))]
    public IActionResult DragAndDrop([FromBody] Model_ListFolderSortOrder model)
    {
        foreach (Model_FolderSortOrder item in model.FolderSortOrders)
        {
            Structure_Entry_Folder? temp = DB.Structure_Entry_Folder.FirstOrDefault(s => s.ID == item.FolderID);
            if (temp is not null)
                temp.SortOrder = item.SortOrder;
        }
        DB.SaveChanges();

        return Ok(new Model_Result<string>());
    }
    #endregion

    #region Entries
    [ApiExplorerSettings(GroupName = "Entries")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_EntryItem>>))]
    [HttpPost(nameof(FilterEntries))]
    public IActionResult FilterEntries([FromBody] Model_FilterEntry? model)
    {
        if (model is null)
            throw new RequestException(ResultType.DataIsInvalid);

        List<Model_EntryItem> entries = Query.LoadEntryItem(model);
        return Ok(new Model_Result<List<Model_EntryItem>>(entries));
    }

    [ApiExplorerSettings(GroupName = "Entries")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Guid>))]
    [HttpPost(nameof(AddEntry))]
    public IActionResult AddEntry([FromBody] Model_AddEntry? model)
    {
        if (model is null || string.IsNullOrWhiteSpace(model.Name))
            throw new RequestException(ResultType.DataIsInvalid);

        Structure_Entry newEntry = new()
        {
            ID = Guid.NewGuid(),
            Name = model.Name,
            UserID = CurrentUser.ID,
            TemplateID = model.TemplateId,
            FolderID = model.FolderId
        };

        DB.Structure_Entry.Add(newEntry);
        DB.SaveChanges();

        return Ok(new Model_Result<Guid>(newEntry.ID));
    }

    [ApiExplorerSettings(GroupName = "Entries")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(ChangeEntryName))]
    public IActionResult ChangeEntryName([FromBody] Model_ChangeEntryName model)
    {
        if (model is null || string.IsNullOrWhiteSpace(model.Name))
            throw new RequestException(ResultType.DataIsInvalid);

        Structure_Entry? item = DB.Structure_Entry.FirstOrDefault(e => e.ID == model.EntryID)
            ?? throw new RequestException(ResultType.NoDataFound);

        item.Name = model.Name;
        DB.SaveChanges();

        return Ok(new Model_Result<string>());
    }

    [ApiExplorerSettings(GroupName = "Entries")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(ChangeFolder))]
    public IActionResult ChangeFolder([FromBody] Model_ChangeFolder? model)
    {
        if (model is null)
            throw new RequestException(ResultType.DataIsInvalid);

        Structure_Entry? item = DB.Structure_Entry.FirstOrDefault(e => e.ID == model.EntryID)
            ?? throw new RequestException(ResultType.NoDataFound);

        item.FolderID = model.FolderID;
        DB.SaveChanges();

        return Ok(new Model_Result<string>());
    }

    [ApiExplorerSettings(GroupName = "Entries")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_DetailedEntry>))]
    [HttpGet(nameof(GetEntry))]
    public IActionResult GetEntry(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultType.DataIsInvalid);

        Structure_Entry? item = DB.Structure_Entry.FirstOrDefault(e => e.ID == id) ?? throw new RequestException(ResultType.NoDataFound);
        List<Structure_Entry_Row> itemRows = [.. DB.Structure_Entry_Row.Where(s => s.EntryID == item.ID).Include(s => s.TemplateID).OrderBy(s => s.O_Template.SortOrder).ThenBy(s => s.SortOrder)];

        List<Model_EntryRow> rows = [];
        foreach (Structure_Entry_Row row in item.O_Rows)
        {
            List<Model_EntryCell> cells = [];
            List<Structure_Entry_Cell> itemCells = [.. DB.Structure_Entry_Cell.Where(s => s.RowID == row.ID).Include(s => s.O_Template).OrderBy(s => s.O_Template.SortOrder)];
            foreach (Structure_Entry_Cell cell in row.O_Cells)
            {
                Model_EntryCell cellModel = new(new Model_TemplateCell(cell.O_Template), cell.ID, cell.Data);
                cells.Add(cellModel);
            }
            Model_EntryRow rowModel = new(new Model_TemplateRow(row.O_Template), row.SortOrder, cells);
            rows.Add(rowModel);
        }
        Model_DetailedEntry entryModel = new(item.ID, item.Name, item.Tags is null ? [] : item.Tags, rows);

        return Ok(new Model_Result<Model_DetailedEntry>());
    }
    #endregion
}

public record Model_DetailedEntry(Guid ID, string Name, string[] Tags, List<Model_EntryRow> Rows);
public record Model_EntryRow(Model_TemplateRow Info, int SortOrder, List<Model_EntryCell> Cells);
public record Model_EntryCell(Model_TemplateCell Info, Guid ID, string Data);
