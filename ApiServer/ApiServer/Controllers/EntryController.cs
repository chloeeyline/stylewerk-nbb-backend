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

    /// <summary>
    /// Gets all entries based on the given filter
    /// </summary>
    /// <param name="name">entry name</param>
    /// <param name="username">username</param>
    /// <param name="templateName">template name the entry is based on</param>
    /// <param name="tags">tags on entry</param>
    /// <param name="includePublic">entry is public visible</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_EntryItem>>))]
    [HttpGet(nameof(List))]
    public IActionResult List(string? name, string? username, string? templateName, string? tags, bool? includePublic)
    {
        List<Model_EntryItem> result = Query.List(name, username, templateName, tags, includePublic);
        return base.Ok(new Model_Result<List<Model_EntryItem>>(result));
    }

    /// <summary>
    /// Removes a entry and all its rows and cells
    /// </summary>
    /// <param name="id">entry ID</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NoDataFound, ResultCodes.YouDontOwnTheData)]
    [HttpPost(nameof(Remove))]
    public IActionResult Remove(Guid? id)
    {
        Query.Remove(id);
        return Ok(new Model_Result<string>());
    }
}
