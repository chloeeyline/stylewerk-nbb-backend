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

    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_TemplatePaging>))]
    [HttpGet(nameof(List))]
    public IActionResult List(int page, int perPage, string? name, string? username, string? description, string? tags, bool? common, bool? directly, bool? group, bool? directUser)
    {
        Model_TemplatePaging result = Query.List(page, perPage, name, username, description, tags, common, directly, group, directUser);
        return Ok(new Model_Result<Model_TemplatePaging>(result));
    }

    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_Template>))]
    [HttpGet(nameof(Get))]
    public IActionResult Get(Guid? id)
    {
        Model_Template result = Query.Get(id);
        return Ok(new Model_Result<Model_Template>(result));
    }

    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(Update))]
    public IActionResult Update([FromBody] Model_Template model)
    {
        Query.Update(model);
        return Ok(new Model_Result<string>());
    }

    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(Remove))]
    public IActionResult Remove(Guid? id)
    {
        Query.Remove(id);
        return Ok(new Model_Result<string>());
    }

    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(Copy))]
    public IActionResult Copy(Guid? id)
    {
        Query.Copy(id);
        return Ok(new Model_Result<string>());
    }
}