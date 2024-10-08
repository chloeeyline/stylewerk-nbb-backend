using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace StyleWerk.NBB.Controllers;

[ApiController, Route("Share/Group"), Authorize]
public class ShareGroupController(NbbContext db) : BaseController(db)
{
    public ShareGroupQueries Query => new(DB, CurrentUser);

    /// <summary>
    /// load all groups that the current user owns
    /// </summary>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_Group>>))]
    [HttpGet(nameof(List))]
    public IActionResult List()
    {
        List<Model_Group> list = Query.List();
        return Ok(new Model_Result<List<Model_Group>>(list));
    }

    /// <summary>
    /// Load the users in a specific group with the rights they have
    /// </summary>
    /// <param name="id">The ID of the group that the users should be loaded</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<string>>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid)]
    [HttpGet(nameof(Details))]
    public IActionResult Details(Guid? id)
    {
        List<string> list = Query.Details(id);
        return Ok(new Model_Result<List<string>>(list));
    }

    /// <summary>
    /// Get all items with type and name that are shared to this group
    /// </summary>
    /// <param name="id">groupId</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_SharedToGroup>>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NoDataFound)]
    [HttpGet(nameof(GetSharedToGroup))]
    public IActionResult GetSharedToGroup(Guid? id)
    {
        List<Model_SharedToGroup> list = Query.GetSharedToGroup(id);
        return Ok(new Model_Result<List<Model_SharedToGroup>>(list));
    }

    /// <summary>
    /// Creates or updates a group
    /// </summary>
    /// <param name="model">contains groupId, group name and a total amout of the users in it</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_Group>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NoDataFound, ResultCodes.NameMustBeUnique, ResultCodes.YouDontOwnTheData)]
    [HttpPost(nameof(Update))]
    public IActionResult Update([FromBody] Model_Group? model)
    {
        model = Query.Update(model);
        return Ok(new Model_Result<Model_Group>(model));
    }

    /// <summary>
    /// Deletes a group and removes all users from it
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

    /// <summary>
    /// Add a user to a group or change there rights in a group where they already are a part of
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NoDataFound, ResultCodes.MissingRight)]
    [HttpPost(nameof(UpdateUser))]
    public IActionResult UpdateUser([FromBody] Model_GroupUser? model)
    {
        Query.UpdateUser(model);
        return Ok(new Model_Result<string>());
    }

    /// <summary>
    /// Remove a user from a group they are part of
    /// </summary>
    /// <param name="username"></param>
    /// <param name="groupID"></param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NoDataFound, ResultCodes.MissingRight)]
    [HttpPost(nameof(RemoveUser))]
    public IActionResult RemoveUser(string? username, Guid? groupID)
    {
        Query.RemoveUser(username, groupID);
        return Ok(new Model_Result<string>());
    }
}
