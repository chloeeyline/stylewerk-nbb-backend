using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace StyleWerk.NBB.Controllers;

[ApiController, Route("Template"), Authorize]
public class TemplateController(NbbContext db) : BaseController(db)
{
    public TemplateQueries Query => new(DB, CurrentUser);

    /// <summary>
    /// Lists all Templates based on the filters
    /// </summary>
    /// <param name="page"></param>
    /// <param name="perPage"></param>
    /// <param name="name"></param>
    /// <param name="username"></param>
    /// <param name="description"></param>
    /// <param name="tags"></param>
    /// <param name="publicShared"></param>
    /// <param name="shared"></param>
    /// <param name="includeOwned"></param>
    /// <param name="directUser"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_TemplatePaging>))]
    [HttpGet(nameof(List))]
    public IActionResult List(int page, int perPage, string? name, string? username, string? description, string? tags, bool? publicShared, bool? shared, bool? includeOwned, bool? directUser)
    {
        Model_TemplatePaging result = Query.List(page, perPage, name, username, description, tags, publicShared, shared, includeOwned, directUser);
        return Ok(new Model_Result<Model_TemplatePaging>(result));
    }

    /// <summary>
    /// Removes a template and all its rows, cells and all entries that are based on the template
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

    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_Editor>))]
    [HttpPost(nameof(Copy))]
    public IActionResult Copy(Guid? id)
    {
        Model_Editor result = Query.Copy(id);
        return Ok(new Model_Result<Model_Editor>(result));
    }
}