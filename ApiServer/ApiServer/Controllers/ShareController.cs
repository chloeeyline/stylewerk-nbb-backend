using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace StyleWerk.NBB.Controllers;

[ApiController, Route("Share"), Authorize]
public class ShareController(NbbContext db) : BaseController(db)
{
    public ShareQueries Query => new(DB, CurrentUser);

    /// <summary>
    /// Gets shared entries or templates with visibility
    /// </summary>
    /// <param name="id">shared item ID</param>
    /// <param name="type">entry or template</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_ShareItem>>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NoDataFound)]
    [HttpGet(nameof(List))]
    public IActionResult List(Guid? id, ShareType? type)
    {
        List<Model_ShareItem> list = Query.List(id, type);
        return Ok(new Model_Result<List<Model_ShareItem>>(list));
    }

    /// <summary>
    /// Sets the rights of a group or user on a specific shared item and create it if it doen't exist
    /// </summary>
    /// <param name="model">contains the information about the shared item</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.YouDontOwnTheData, ResultCodes.NoDataFound, ResultCodes.CantShareWithYourself)]
    [HttpPost(nameof(Update))]
    public IActionResult Update([FromBody] Model_ShareItem? model)
    {
        Query.Update(model);
        return Ok(new Model_Result<string>());
    }

    /// <summary>
    /// Removes a shared template or entry 
    /// </summary>
    /// <param name="id">item ID that should be removed</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NoDataFound, ResultCodes.YouDontOwnTheData)]
    [HttpPost(nameof(Remove))]
    public IActionResult Remove(Guid? id)
    {
        Query.Remove(id);
        return Ok(new Model_Result<string>());
    }
}