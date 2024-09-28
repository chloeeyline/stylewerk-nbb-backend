using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Admin;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Controllers;

[ApiController, AllowAnonymous, Route("Admin")]
public class AdminController(NbbContext db) : BaseController(db)
{
    #region Language
    [ApiExplorerSettings(GroupName = "Language")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_LanguageItem>>))]
    [HttpGet(nameof(GetLanguageList))]
    public IActionResult GetLanguageList()
    {
        List<Model_LanguageItem> list = [.. DB.Admin_Language.Include(s => s.O_User).Select(s => new Model_LanguageItem(s.Code, s.Name, s.O_User.Username))];
        return Ok(new Model_Result<List<Model_LanguageItem>>());
    }

    [ApiExplorerSettings(GroupName = "Language")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [HttpGet(nameof(GetLanguage))]
    public IActionResult GetLanguage(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new RequestException(ResultCodes.DataIsInvalid);
        Admin_Language item = DB.Admin_Language.FirstOrDefault(s => s.Code == code) ?? throw new RequestException(ResultCodes.NoDataFound);
        return Ok(new Model_Result<string>(item.Data));
    }

    [ApiExplorerSettings(GroupName = "Language")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_LanguageDetails>))]
    [HttpGet(nameof(GetLanguageDetails))]
    public IActionResult GetLanguageDetails(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new RequestException(ResultCodes.DataIsInvalid);
        Admin_Language item = DB.Admin_Language.FirstOrDefault(s => s.Code == code) ?? throw new RequestException(ResultCodes.NoDataFound);
        Model_LanguageDetails result = new(item.Code, item.Name, item.Data);
        return Ok(new Model_Result<Model_LanguageDetails>(result));
    }

    [ApiExplorerSettings(GroupName = "Language")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(SaveLanguage)), Authorize]
    public IActionResult SaveLanguage([FromBody] Model_LanguageDetails? model)
    {
        if (model is null)
            throw new RequestException(ResultCodes.DataIsInvalid);
        Admin_Language? item = DB.Admin_Language.FirstOrDefault(s => s.Code == model.Code);

        if (item is null)
        {
            item = new Admin_Language()
            {
                Code = model.Code,
                Name = model.Name,
                Data = model.Data,
                UserID = CurrentUser.ID
            };
            DB.Admin_Language.Add(item);
        }
        else
        {
            item.Name = model.Name;
            item.Data = model.Data;
        }
        DB.SaveChanges();

        return Ok(new Model_Result<string>());
    }
    #endregion

    #region Color Theme
    [ApiExplorerSettings(GroupName = "Color Theme")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_ColorThemeItem>>))]
    [HttpGet(nameof(GetThemeList))]
    public IActionResult GetThemeList()
    {
        List<Model_ColorThemeItem> list = [.. DB.Admin_ColorTheme.Include(s => s.O_User).Select(s => new Model_ColorThemeItem(s.ID, s.Name, s.Base, s.O_User.Username))];
        return Ok(new Model_Result<List<Model_ColorThemeItem>>());
    }

    [ApiExplorerSettings(GroupName = "Color Theme")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [HttpGet(nameof(GetTheme))]
    public IActionResult GetTheme(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);
        Admin_ColorTheme item = DB.Admin_ColorTheme.FirstOrDefault(s => s.ID == id) ?? throw new RequestException(ResultCodes.NoDataFound);
        return Ok(new Model_Result<string>(item.Data));
    }

    [ApiExplorerSettings(GroupName = "Color Theme")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_ColorThemeDetails>))]
    [HttpGet(nameof(GetThemeDetails))]
    public IActionResult GetThemeDetails(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);
        Admin_ColorTheme item = DB.Admin_ColorTheme.FirstOrDefault(s => s.ID == id) ?? throw new RequestException(ResultCodes.NoDataFound);
        Model_ColorThemeDetails result = new(item.ID, item.Name, item.Base, item.Data);
        return Ok(new Model_Result<Model_ColorThemeDetails>(result));
    }

    [ApiExplorerSettings(GroupName = "Color Theme")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(SaveTheme)), Authorize]
    public IActionResult SaveTheme([FromBody] Model_ColorThemeDetails? model)
    {
        if (model is null)
            throw new RequestException(ResultCodes.DataIsInvalid);
        Admin_ColorTheme? item = DB.Admin_ColorTheme.FirstOrDefault(s => s.ID == model.ID);

        if (item is null)
        {
            item = new Admin_ColorTheme()
            {
                ID = Guid.Empty,
                Name = model.Name,
                Data = model.Data,
                Base = model.Base,
                UserID = CurrentUser.ID
            };
            DB.Admin_ColorTheme.Add(item);
        }
        else
        {
            item.Name = model.Name;
            item.Base = model.Base;
            item.Data = model.Data;
        }
        DB.SaveChanges();

        return Ok(new Model_Result<string>());
    }
    #endregion
}
