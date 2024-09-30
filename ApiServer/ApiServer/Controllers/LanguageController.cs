using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace StyleWerk.NBB.Controllers;

[ApiController, AllowAnonymous, Route("Language")]
public class LanguageController(NbbContext db) : BaseController(db)
{
    public LanguageQueries Query => new(DB, CurrentUser);

    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [HttpGet()]
    public IActionResult Get(string? code)
    {
        string result = Query.Get(code);
        return Ok(new Model_Result<string>(result));
    }

    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_Language>>))]
    [HttpGet(nameof(List))]
    public IActionResult List()
    {
        List<Model_Language> result = Query.List();
        return Ok(new Model_Result<List<Model_Language>>(result));
    }

    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_Language>))]
    [HttpGet(nameof(Details))]
    public IActionResult Details(string? code)
    {
        Model_Language result = Query.Details(code);
        return Ok(new Model_Result<Model_Language>(result));
    }

    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(Update)), Authorize]
    public IActionResult Update([FromBody] Model_Language? model)
    {
        Query.Update(model);
        return Ok(new Model_Result<string>());
    }
}
