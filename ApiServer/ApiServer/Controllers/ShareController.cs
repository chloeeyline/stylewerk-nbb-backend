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
    /// get the rights of a group or another user on a specific shared item
    /// </summary>
    /// <param name="id"></param>
    /// <param name="type"></param>
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
    /// Update the rights of a group or another user on a specific shared item 
    /// or create the item if it doesn't exists
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NoDataFound, ResultCodes.MissingRight, ResultCodes.OnlyOwnerCanChangePublicity)]
    [HttpPost(nameof(Update))]
    public IActionResult Update([FromBody] Model_ShareItem? model)
    {
        Query.Update(model);
        return Ok(new Model_Result<string>());
    }

    /// <summary>
    /// Remove a shared template or entry based on the given Id. 
    /// </summary>
    /// <param name="id"></param>
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