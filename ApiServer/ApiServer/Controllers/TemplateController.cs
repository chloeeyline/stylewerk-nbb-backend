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
    /// Lists all templates based on given filters
    /// </summary>
    /// <param name="page">pages</param>
    /// <param name="perPage">how many templates per page should be shown</param>
    /// <param name="name">template name</param>
    /// <param name="username">username</param>
    /// <param name="description">template description</param>
    /// <param name="tags">tags on the template</param>
    /// <param name="publicShared">tempate is public visible</param>
    /// <param name="shared">template is shared</param>
    /// <param name="includeOwned">template belongs to current user</param>
    /// <param name="directUser">username has to be exactly the given username</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_TemplatePaging>))]
    [HttpGet(nameof(List))]
    public IActionResult List(int page, int perPage, string? name, string? username, string? description, string? tags, bool? publicShared, bool? shared, bool? includeOwned, bool? directUser)
    {
        Model_TemplatePaging result = Query.List(page, perPage, name, username, description, tags, publicShared, shared, includeOwned, directUser);
        return Ok(new Model_Result<Model_TemplatePaging>(result));
    }

    /// <summary>
    /// Removes a template and all its rows, cells and all entries that are based on it
    /// </summary>
    /// <param name="id">template ID</param>
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
    /// Copies a public template for the current user
    /// </summary>
    /// <param name="id">template ID</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_Editor>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NoDataFound)]
    [HttpPost(nameof(Copy))]
    public IActionResult Copy(Guid? id)
    {
        Model_Editor result = Query.Copy(id);
        return Ok(new Model_Result<Model_Editor>(result));
    }
}