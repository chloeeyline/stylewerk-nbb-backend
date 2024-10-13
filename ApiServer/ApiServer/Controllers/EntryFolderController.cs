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
    /// Gets all folders with entries from current user
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
    /// Gets all items in a folder
    /// </summary>
    /// <param name="id">folder ID</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_EntryItem>>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NoDataFound)]
    [HttpGet(nameof(Details))]
    public IActionResult Details(Guid? id)
    {
        List<Model_EntryItem> result = Query.Details(id);
        return base.Ok(new Model_Result<List<Model_EntryItem>>(result));
    }

    /// <summary>
    /// Changes folder name or add a folder
    /// </summary>
    /// <param name="model">contains folder ID, name and entries</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_EntryFolders>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NameMustBeUnique, ResultCodes.MissingRight)]
    [HttpPost(nameof(Update))]
    public IActionResult Update([FromBody] Model_EntryFolders? model)
    {
        Model_EntryFolders result = Query.Update(model);
        return Ok(new Model_Result<Model_EntryFolders>(result));
    }

    /// <summary>
    /// Removes a folder but preserve all the items that were in it
    /// </summary>
    /// <param name="id">folder ID</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NoDataFound, ResultCodes.YouDontOwnTheData)]
    [HttpPost(nameof(Remove))]
    public IActionResult Remove(Guid? id)
    {
        Query.Remove(id);
        return Ok(new Model_Result<string>());
    }

    /// <summary>
    /// Reorders folders for drap and drop 
    /// </summary>
    /// <param name="model">list of all folders for the current user</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NoDataFound, ResultCodes.YouDontOwnTheData)]
    [HttpPost(nameof(Reorder))]
    public IActionResult Reorder([FromBody] List<Guid>? model)
    {
        Query.Reorder(model);
        return Ok(new Model_Result<string>());
    }
}
