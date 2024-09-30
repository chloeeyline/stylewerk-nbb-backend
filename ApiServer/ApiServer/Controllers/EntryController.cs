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
    /// <summary>
    /// Get all folders and entries
    /// </summary>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Folder")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_EntryFolders>>))]
    [HttpGet(nameof(GetFolders))]
    public IActionResult GetFolders()
    {
        List<Model_EntryFolders> result = Query.GetFolders();
        return Ok(new Model_Result<List<Model_EntryFolders>>(result));
    }

    /// <summary>
    /// Get all items in a folder
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Folder")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_EntryFolders>>))]
    [HttpGet(nameof(GetFolderContent))]
    public IActionResult GetFolderContent(Guid? id)
    {
        List<Model_EntryItem> result = Query.GetFolderContent(id);
        return Ok(new Model_Result<List<Model_EntryItem>>(result));
    }

    /// <summary>
    /// Change folder name or add a folder
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Folder")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_EntryFolders>))]
    [HttpPost(nameof(UpdateFolder))]
    public IActionResult UpdateFolder(Model_EntryFolders? model)
    {
        Model_EntryFolders result = Query.UpdateFolder(model);
        return Ok(new Model_Result<Model_EntryFolders>(result));
    }

    /// <summary>
    /// Remove a folder but preserve all the items that were in it
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Folder")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(RemoveFolder))]
    public IActionResult RemoveFolder(Guid? id)
    {
        Query.RemoveFolder(id);
        return Ok(new Model_Result<string>());
    }

    /// <summary>
    /// Change the order of a folder
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
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
    /// <summary>
    /// Load all entries and filter them
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Entries")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_EntryItem>>))]
    [HttpPost(nameof(FilterEntries))]
    public IActionResult FilterEntries([FromBody] Model_FilterEntry? model)
    {
        List<Model_EntryItem> result = Query.FilterEntries(model);
        return Ok(new Model_Result<List<Model_EntryItem>>(result));
    }

    /// <summary>
    /// Get the structure of the entry based on the selected template
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Entries")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_Entry>))]
    [HttpGet(nameof(GetEntryFromTemplate))]
    public IActionResult GetEntryFromTemplate(Guid? id)
    {
        Model_Entry result = Query.GetEntryFromTemplate(id);
        return Ok(new Model_Result<Model_Entry>(result));
    }

    /// <summary>
    /// Not Finished
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Entries")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_Entry>))]
    [HttpGet(nameof(GetEntry))]
    public IActionResult GetEntry(Guid? id)
    {
        Model_Entry result = Query.GetEntry(id);
        return Ok(new Model_Result<Model_Entry>(result));
    }

    [ApiExplorerSettings(GroupName = "Entries")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_Entry>))]
    [HttpPost(nameof(UpdateEntry))]
    public IActionResult UpdateEntry([FromBody] Model_Entry? model)
    {
        Model_Entry result = Query.UpdateEntry(model);
        return Ok(new Model_Result<Model_Entry>(result));
    }

    /// <summary>
    /// This method should be more performant but needss testing as the syntax is not often  used by me
    /// </summary>
    [ApiExplorerSettings(GroupName = "Entries")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<ShareEntryResult>>))]
    [HttpGet(nameof(FilterEntriesNew))]
    public IActionResult FilterEntriesNew(string? name, string? username, string? templateName, string? tags, bool? publicShared, bool? groupShared, bool? directlyShared, bool? directUser)
    {
        List<ShareEntryResult> result = Query.GetUserSharedEntries(name, username, templateName, tags, publicShared, groupShared, directlyShared, directUser);
        return Ok(new Model_Result<List<ShareEntryResult>>(result));
    }
    #endregion
}
