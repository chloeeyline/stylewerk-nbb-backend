using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace StyleWerk.NBB.Controllers;

[ApiController, Route("EntryOverview"), Authorize]
public class EntryOverviewController(NbbContext db) : BaseController(db)
{
    public EntryQueries Query => new(DB, CurrentUser);

    #region Folder
    [HttpGet(nameof(GetFolders))]
    public IActionResult GetFolders()
    {
        try
        {
            List<Model_EntryFolders> entries = Query.GetFolders();
            return Ok(new Model_Result(entries));
        }
        catch (RequestException ex)
        {
            if (!string.IsNullOrWhiteSpace(ex.Message))
                Console.WriteLine(ex.Message);
            return Ok(new Model_Result(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrWhiteSpace(ex.Message))
                Console.WriteLine(ex.Message);
            return Ok(new Model_Result(ResultType.GeneralError));
        }
    }

    [HttpGet(nameof(GetFolderContent))]
    public IActionResult GetFolderContent(Guid? folderId)
    {
        try
        {
            List<Model_EntryItem> entries = Query.GetFolderContent(folderId);
            return Ok(new Model_Result(entries));
        }
        catch (RequestException ex)
        {
            if (!string.IsNullOrWhiteSpace(ex.Message))
                Console.WriteLine(ex.Message);
            return Ok(new Model_Result(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrWhiteSpace(ex.Message))
                Console.WriteLine(ex.Message);
            return Ok(new Model_Result(ResultType.GeneralError));
        }
    }

    [HttpPost(nameof(AddFolder))]
    public IActionResult AddFolder(string? name)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new RequestException(ResultType.DataIsInvalid);

            bool isFilled = DB.Structure_Entry_Folder.Any();
            int sortOrder = isFilled ? (DB.Structure_Entry_Folder.Max(f => f.SortOrder) + 1) : 1;

            Structure_Entry_Folder newFolder = new()
            {
                ID = Guid.NewGuid(),
                Name = name,
                SortOrder = sortOrder,
                UserID = CurrentUser.ID
            };

            DB.Structure_Entry_Folder.Add(newFolder);
            DB.SaveChanges();

            return Ok(new Model_Result(newFolder.ID));
        }
        catch (RequestException ex)
        {
            if (!string.IsNullOrWhiteSpace(ex.Message))
                Console.WriteLine(ex.Message);
            return Ok(new Model_Result(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrWhiteSpace(ex.Message))
                Console.WriteLine(ex.Message);
            return Ok(new Model_Result(ResultType.GeneralError));
        }
    }

    [HttpPost(nameof(DragAndDrop))]
    public IActionResult DragAndDrop([FromBody] Model_ListFolderSortOrder listFolder)
    {

        return Ok(new Model_Result());
    }
    #endregion

    #region Entries
    [HttpPost(nameof(FilterEntries))]
    public IActionResult FilterEntries([FromBody] Model_FilterEntry? model)
    {
        try
        {
            if (model is null)
                throw new RequestException(ResultType.DataIsInvalid);

            List<Model_EntryItem> entries = Query.LoadEntryItem(model);
            return Ok(new Model_Result(entries));
        }
        catch (RequestException ex)
        {
            if (!string.IsNullOrWhiteSpace(ex.Message))
                Console.WriteLine(ex.Message);
            return Ok(new Model_Result(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrWhiteSpace(ex.Message))
                Console.WriteLine(ex.Message);
            return Ok(new Model_Result(ResultType.GeneralError));
        }
    }

    [HttpPost(nameof(AddEntry))]
    public IActionResult AddEntry([FromBody] Model_AddEntry? model)
    {
        try
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

            return Ok(new Model_Result(newEntry.ID));
        }
        catch (RequestException ex)
        {
            if (!string.IsNullOrWhiteSpace(ex.Message))
                Console.WriteLine(ex.Message);
            return Ok(new Model_Result(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrWhiteSpace(ex.Message))
                Console.WriteLine(ex.Message);
            return Ok(new Model_Result(ResultType.GeneralError));
        }
    }

    [HttpPost(nameof(ChangeEntryName))]
    public IActionResult ChangeEntryName([FromBody] Model_ChangeEntryName model)
    {
        try
        {
            if (model is null || string.IsNullOrWhiteSpace(model.Name))
                throw new RequestException(ResultType.DataIsInvalid);

            Structure_Entry? item = DB.Structure_Entry.FirstOrDefault(e => e.ID == model.EntryID)
                ?? throw new RequestException(ResultType.NoDataFound);

            item.Name = model.Name;
            DB.SaveChanges();

            return Ok(new Model_Result());
        }
        catch (RequestException ex)
        {
            if (!string.IsNullOrWhiteSpace(ex.Message))
                Console.WriteLine(ex.Message);
            return Ok(new Model_Result(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrWhiteSpace(ex.Message))
                Console.WriteLine(ex.Message);
            return Ok(new Model_Result(ResultType.GeneralError));
        }
    }

    [HttpPost(nameof(ChangeFolder))]
    public IActionResult ChangeFolder([FromBody] Model_ChangeFolder? model)
    {
        try
        {
            if (model is null)
                throw new RequestException(ResultType.DataIsInvalid);

            Structure_Entry? item = DB.Structure_Entry.FirstOrDefault(e => e.ID == model.EntryID)
                ?? throw new RequestException(ResultType.NoDataFound);

            item.FolderID = model.FolderID;
            DB.SaveChanges();

            return Ok(new Model_Result());
        }
        catch (RequestException ex)
        {
            if (!string.IsNullOrWhiteSpace(ex.Message))
                Console.WriteLine(ex.Message);
            return Ok(new Model_Result(ex.ErrorCode));
        }
        catch (Exception ex)
        {
            if (!string.IsNullOrWhiteSpace(ex.Message))
                Console.WriteLine(ex.Message);
            return Ok(new Model_Result(ResultType.GeneralError));
        }


    }
    #endregion
}
