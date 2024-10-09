using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace StyleWerk.NBB.Controllers;

[ApiController, AllowAnonymous, Route("ColorTheme")]
public class ColorThemeController(NbbContext db) : BaseController(db)
{
    public ColorThemeQueries Query => new(DB, CurrentUser);

    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [HttpGet]
    public IActionResult Get(Guid? id)
    {
        string result = Query.Get(id);
        return Ok(result);
    }

    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_ColorTheme>>))]
    [HttpGet(nameof(List))]
    public IActionResult List()
    {
        List<Model_ColorTheme> result = Query.List();
        return Ok(new Model_Result<List<Model_ColorTheme>>(result));
    }

    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_ColorTheme>))]
    [HttpGet(nameof(Details))]
    public IActionResult Details(Guid? id)
    {
        Model_ColorTheme result = Query.Details(id);
        return Ok(new Model_Result<Model_ColorTheme>(result));
    }

    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpGet(nameof(Remove))]
    public IActionResult Remove(Guid? id)
    {
        Query.Remove(id);
        return Ok(new Model_Result<string>());
    }

    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(Update)), Authorize]
    public IActionResult Update([FromBody] Model_ColorTheme? model)
    {
        Query.Update(model);
        return Ok(new Model_Result<string>());
    }
}
