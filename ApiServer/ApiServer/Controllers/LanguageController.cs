using System.Text.Json;

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

    /// <summary>
    /// Returns language data as JSON
    /// </summary>
    /// <param name="code">language code</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NoDataFound)]
    [HttpGet()]
    public IActionResult Get(string? code)
    {
        string result = Query.Get(code);
        return Ok(JsonSerializer.Deserialize(result, typeof(object)));
    }

    /// <summary>
    /// Loads all available languages
    /// </summary>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_Language>>))]
    [HttpGet(nameof(List))]
    public IActionResult List()
    {
        List<Model_Language> result = Query.List();
        return Ok(new Model_Result<List<Model_Language>>(result));
    }

    /// <summary>
    /// Loads language title, code and details for the download 
    /// </summary>
    /// <param name="code">language code</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_Language>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NoDataFound)]
    [HttpGet(nameof(Details))]
    public IActionResult Details(string? code)
    {
        Model_Language result = Query.Details(code);
        return Ok(new Model_Result<Model_Language>(result));
    }

    /// <summary>
    /// Removes a language 
    /// </summary>
    /// <param name="code">language code</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NoDataFound, ResultCodes.UserMustBeAdmin)]
    [HttpPost(nameof(Remove)), Authorize]
    public IActionResult Remove(string? code)
    {
        Query.Remove(code);
        return Ok(new Model_Result<string>());
    }

    /// <summary>
    /// Adds or updates a language with its details
    /// </summary>
    /// <param name="model">contains language code, title and data as JSON</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.UserMustBeAdmin, ResultCodes.NameMustBeUnique)]
    [HttpPost(nameof(Update)), Authorize]
    public IActionResult Update([FromBody] Model_Language? model)
    {
        Query.Update(model);
        return Ok(new Model_Result<string>());
    }
}
