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
    /// Load all entries and filter them
    /// </summary>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_EntryItem>>))]
    [HttpGet(nameof(List))]
    public IActionResult List(string? name, string? username, string? templateName, string? tags, bool? publicShared, bool? groupShared, bool? directlyShared, bool? directUser)
    {
        List<Model_EntryItem> result = Query.List(name, username, templateName, tags, publicShared, groupShared, directlyShared, directUser);
        return base.Ok(new Model_Result<List<Model_EntryItem>>(result));
    }

    /// <summary>
    /// Not Finished
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_Entry>))]
    [HttpGet(nameof(Details))]
    public IActionResult Details(Guid? id)
    {
        Model_Entry result = Query.Details(id);
        return Ok(new Model_Result<Model_Entry>(result));
    }

    /// <summary>
    /// Get the structure of the entry based on the selected template
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_Entry>))]
    [HttpGet(nameof(GetFromTemplate))]
    public IActionResult GetFromTemplate(Guid? id)
    {
        Model_Entry result = Query.GetFromTemplate(id);
        return Ok(new Model_Result<Model_Entry>(result));
    }

    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(Remove))]
    public IActionResult Remove(Guid? id)
    {
        Query.Remove(id);
        return Ok(new Model_Result<string>());
    }

    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_Entry>))]
    [HttpPost(nameof(Update))]
    public IActionResult Update([FromBody] Model_Entry? model)
    {
        Model_Entry result = Query.Update(model);
        return Ok(new Model_Result<Model_Entry>(result));
    }
}
