using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace StyleWerk.NBB.Controllers;

[ApiController, Route("Entry/Folder"), Authorize]
public class EntryFolderController(NbbContext db) : BaseController(db)
{
    public EntryFolderQueries Query => new(DB, CurrentUser);

    /// <summary>
    /// Get all folders and entries
    /// </summary>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_EntryFolders>>))]
    [HttpGet(nameof(List))]
    public IActionResult List()
    {
        List<Model_EntryFolders> result = Query.List();
        return Ok(new Model_Result<List<Model_EntryFolders>>(result));
    }

    /// <summary>
    /// Get all items in a folder
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_EntryFolders>>))]
    [HttpGet(nameof(Details))]
    public IActionResult Details(Guid? id)
    {
        List<Model_EntryItem> result = Query.Details(id);
        return base.Ok(new Model_Result<List<Model_EntryItem>>(result));
    }

    /// <summary>
    /// Change folder name or add a folder
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_EntryFolders>))]
    [HttpPost(nameof(Update))]
    public IActionResult Update(Model_EntryFolders? model)
    {
        Model_EntryFolders result = Query.Update(model);
        return Ok(new Model_Result<Model_EntryFolders>(result));
    }

    /// <summary>
    /// Remove a folder but preserve all the items that were in it
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(Remove))]
    public IActionResult Remove(Guid? id)
    {
        Query.Remove(id);
        return Ok(new Model_Result<string>());
    }

    /// <summary>
    /// Change the order of a folder
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(Reorder))]
    public IActionResult Reorder([FromBody] List<Guid>? model)
    {
        Query.Reorder(model);
        return Ok(new Model_Result<string>());
    }
}
