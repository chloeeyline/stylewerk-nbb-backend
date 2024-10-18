using System.Text.Json;

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

    /// <summary>
    /// Gets color theme data as JSON
    /// </summary>
    /// <param name="id">color theme ID</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NoDataFound)]
    [HttpGet]
    public IActionResult Get(Guid? id)
    {
        string result = Query.Get(id);
        return Ok(JsonSerializer.Deserialize(result, typeof(object)));
    }

    /// <summary>
    /// Lists all available color themes
    /// </summary>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_ColorTheme>>))]
    [HttpGet(nameof(List))]
    public IActionResult List()
    {
        List<Model_ColorTheme> result = Query.List();
        return Ok(new Model_Result<List<Model_ColorTheme>>(result));
    }

    /// <summary>
    /// Loads color theme name, base and data for for editor list
    /// </summary>
    /// <param name="id">color theme ID</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_ColorTheme>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NoDataFound)]
    [HttpGet(nameof(Details))]
    public IActionResult Details(Guid? id)
    {
        Model_ColorTheme result = Query.Details(id);
        return Ok(new Model_Result<Model_ColorTheme>(result));
    }

    /// <summary>
    /// Removes a color theme 
    /// </summary>
    /// <param name="id">color theme ID</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NoDataFound, ResultCodes.UserMustBeAdmin)]
    [HttpPost(nameof(Remove)), Authorize]
    public IActionResult Remove(Guid? id)
    {
        Query.Remove(id);
        return Ok(new Model_Result<string>());
    }

    /// <summary>
    /// Adds or updates a color theme
    /// </summary>
    /// <param name="model">contains name, base and data as JSON <br/> If updating an existing one also includes it's ID</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.UserMustBeAdmin, ResultCodes.NameMustBeUnique)]
    [HttpPost(nameof(Update)), Authorize]
    public IActionResult Update([FromBody] Model_ColorTheme? model)
    {
        Query.Update(model);
        return Ok(new Model_Result<string>());
    }
}
