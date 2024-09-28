using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace StyleWerk.NBB.Controllers;

[ApiController, AllowAnonymous, Route("Admin")]
public class AdminController(NbbContext db) : BaseController(db)
{
    public AdminQueries Query => new(DB, CurrentUser);

    #region Language
    [ApiExplorerSettings(GroupName = "Language")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_Language>>))]
    [HttpGet(nameof(GetLanguageList))]
    public IActionResult GetLanguageList()
    {
        List<Model_Language> result = Query.GetLanguageList();
        return Ok(new Model_Result<List<Model_Language>>(result));
    }

    [ApiExplorerSettings(GroupName = "Language")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [HttpGet(nameof(GetLanguage))]
    public IActionResult GetLanguage(string? code)
    {
        string result = Query.GetLanguage(code);
        return Ok(new Model_Result<string>(result));
    }

    [ApiExplorerSettings(GroupName = "Language")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_Language>))]
    [HttpGet(nameof(GetLanguageDetails))]
    public IActionResult GetLanguageDetails(string? code)
    {
        Model_Language result = Query.GetLanguageDetails(code);
        return Ok(new Model_Result<Model_Language>(result));
    }

    [ApiExplorerSettings(GroupName = "Language")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(SaveLanguage)), Authorize]
    public IActionResult SaveLanguage([FromBody] Model_Language? model)
    {
        Query.SaveLanguage(model);
        return Ok(new Model_Result<string>());
    }
    #endregion

    #region Color Theme
    [ApiExplorerSettings(GroupName = "Color Theme")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_ColorTheme>>))]
    [HttpGet(nameof(GetThemeList))]
    public IActionResult GetThemeList()
    {
        List<Model_ColorTheme> result = Query.GetThemeList();
        return Ok(new Model_Result<List<Model_ColorTheme>>(result));
    }

    [ApiExplorerSettings(GroupName = "Color Theme")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [HttpGet(nameof(GetTheme))]
    public IActionResult GetTheme(Guid? id)
    {
        string result = Query.GetTheme(id);
        return Ok(new Model_Result<string>(result));
    }

    [ApiExplorerSettings(GroupName = "Color Theme")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_ColorTheme>))]
    [HttpGet(nameof(GetThemeDetails))]
    public IActionResult GetThemeDetails(Guid? id)
    {
        Model_ColorTheme result = Query.GetThemeDetails(id);
        return Ok(new Model_Result<Model_ColorTheme>(result));
    }

    [ApiExplorerSettings(GroupName = "Color Theme")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(SaveTheme)), Authorize]
    public IActionResult SaveTheme([FromBody] Model_ColorTheme? model)
    {
        Query.SaveTheme(model);
        return Ok(new Model_Result<string>());
    }
    #endregion
}
