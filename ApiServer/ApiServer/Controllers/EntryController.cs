using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace StyleWerk.NBB.Controllers;

[ApiController, Route("Entry"), Authorize]
public class EntryController(NbbContext db) : BaseController(db)
{
    public EntryQueries Query => new(DB, CurrentUser);

    #region Folder
    [ApiExplorerSettings(GroupName = "Folder")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_EntryFolders>>))]
    [HttpGet(nameof(GetFolders))]
    public IActionResult GetFolders()
    {
        List<Model_EntryFolders> result = Query.GetFolders();
        return Ok(new Model_Result<List<Model_EntryFolders>>(result));
    }

    [ApiExplorerSettings(GroupName = "Folder")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_EntryFolders>>))]
    [HttpGet(nameof(GetFolderContent))]
    public IActionResult GetFolderContent(Guid? id)
    {
        List<Model_EntryItem> result = Query.GetFolderContent(id);
        return Ok(new Model_Result<List<Model_EntryItem>>(result));
    }

    [ApiExplorerSettings(GroupName = "Folder")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_EntryFolders>))]
    [HttpPost(nameof(UpdateFolder))]
    public IActionResult UpdateFolder(Model_EntryFolders? model)
    {
        Model_EntryFolders result = Query.UpdateFolder(model);
        return Ok(new Model_Result<Model_EntryFolders>(result));
    }

    [ApiExplorerSettings(GroupName = "Folder")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(RemoveFolder))]
    public IActionResult RemoveFolder(Guid? id)
    {
        Query.RemoveFolder(id);
        return Ok(new Model_Result<string>());
    }

    [ApiExplorerSettings(GroupName = "Folder")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(ReorderFolders))]
    public IActionResult ReorderFolders([FromBody] List<Guid>? model)
    {
        Query.ReorderFolders(model);
        return Ok(new Model_Result<string>());
    }
    #endregion

    #region Entries
    [ApiExplorerSettings(GroupName = "Entries")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_EntryItem>>))]
    [HttpPost(nameof(FilterEntries))]
    public IActionResult FilterEntries([FromBody] Model_FilterEntry? model)
    {
        List<Model_EntryItem> result = Query.FilterEntries(model);
        return Ok(new Model_Result<List<Model_EntryItem>>(result));
    }

    [ApiExplorerSettings(GroupName = "Entries")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_DetailedEntry>))]
    [HttpGet(nameof(GetEntryFromTemplate))]
    public IActionResult GetEntryFromTemplate(Guid? id)
    {
        Model_DetailedEntry result = Query.GetEntryFromTemplate(id);
        return Ok(new Model_Result<Model_DetailedEntry>(result));
    }

    /// <summary>
    /// Not Finished
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Entries")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_DetailedEntry>))]
    [HttpGet(nameof(GetEntry))]
    public IActionResult GetEntry(Guid? id)
    {
        Model_DetailedEntry result = Query.GetEntry(id);
        return Ok(new Model_Result<Model_DetailedEntry>(result));
    }

    [ApiExplorerSettings(GroupName = "Entries")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_DetailedEntry>))]
    [HttpPost(nameof(UpdateEntry))]
    public IActionResult UpdateEntry([FromBody] Model_DetailedEntry? model)
    {
        Model_DetailedEntry result = Query.UpdateEntry(model);
        return Ok(new Model_Result<Model_DetailedEntry>(result));
    }
    #endregion
}
