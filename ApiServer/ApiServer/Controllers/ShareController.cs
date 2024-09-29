using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace StyleWerk.NBB.Controllers;

[ApiController, Route("ShareNew"), Authorize]
public class ShareController(NbbContext db) : BaseController(db)
{
    public ShareQueries Query => new(DB, CurrentUser);

    #region Groups
    /// <summary>
    /// Load all groups which you own
    /// </summary>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Groups")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_Group>>))]
    [HttpGet(nameof(GetOwnedGroups))]
    public IActionResult GetOwnedGroups()
    {
        List<Model_Group> list = Query.GetOwnedGroups();
        return Ok(new Model_Result<List<Model_Group>>(list));
    }

    /// <summary>
    /// Load the users in a specific group with the rights they have
    /// </summary>
    /// <param name="id">The ID of the group from which zou want to load the users</param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Groups")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_GroupUser>>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid)]
    [HttpGet(nameof(GetUsersInGroup))]
    public IActionResult GetUsersInGroup(Guid? id)
    {
        List<Model_GroupUser> list = Query.GetUsersInGroup(id);
        return Ok(new Model_Result<List<Model_GroupUser>>(list));
    }

    /// <summary>
    /// Get all items with type and name that are shared to this group
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Groups")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_SharedToGroup>>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NoDataFound)]
    [HttpGet(nameof(GetSharedToGroup))]
    public IActionResult GetSharedToGroup(Guid? id)
    {
        List<Model_SharedToGroup> list = Query.GetSharedToGroup(id);
        return Ok(new Model_Result<List<Model_SharedToGroup>>(list));
    }

    /// <summary>
    /// Create a group or update the group informations of an existing group
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Groups")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_Group>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NoDataFound, ResultCodes.GroupNameAlreadyExists, ResultCodes.DontOwnGroup)]
    [HttpPost(nameof(UpdateGroup))]
    public IActionResult UpdateGroup([FromBody] Model_Group? model)
    {
        model = Query.UpdateGroup(model);
        return Ok(new Model_Result<Model_Group>(model));
    }

    /// <summary>
    /// Delete a group you own
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Groups")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NoDataFound, ResultCodes.DontOwnGroup)]
    [HttpPost(nameof(RemoveGroup))]
    public IActionResult RemoveGroup(Guid? id)
    {
        Query.RemoveGroup(id);
        return Ok(new Model_Result<string>());
    }
    #endregion

    #region Users in Group
    /// <summary>
    /// Add an user to a group or change there rights in a group where they already are a part of
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Users in Group")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NoDataFound, ResultCodes.MissingRight)]
    [HttpPost(nameof(UpdateUserInGroup))]
    public IActionResult UpdateUserInGroup([FromBody] Model_GroupUser? model)
    {
        Query.UpdateUserInGroup(model);
        return Ok(new Model_Result<string>());
    }

    /// <summary>
    /// Remove a user from a group they are part of
    /// </summary>
    /// <param name="username"></param>
    /// <param name="groupID"></param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Users in Group")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NoDataFound, ResultCodes.MissingRight)]
    [HttpPost(nameof(RemoveUserFromGroup))]
    public IActionResult RemoveUserFromGroup(string? username, Guid? groupID)
    {
        Query.RemoveUserFromGroup(username, groupID);
        return Ok(new Model_Result<string>());
    }
    #endregion

    #region Share
    /// <summary>
    /// get the rights of a group or another user on a specific shared item
    /// </summary>
    /// <param name="id"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Share")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_ShareItem>>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NoDataFound)]
    [HttpGet(nameof(GetShare))]
    public IActionResult GetShare(Guid? id, ShareType? type)
    {
        List<Model_ShareItem> list = Query.GetShare(id, type);
        return Ok(new Model_Result<List<Model_ShareItem>>(list));
    }

    /// <summary>
    /// Update the rights of a group or another user on a specific shared item 
    /// or create the item if it doesn't exists
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Share")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NoDataFound, ResultCodes.MissingRight, ResultCodes.OnlyOwnerCanChangePublicity)]
    [HttpPost(nameof(UpdateShare))]
    public IActionResult UpdateShare([FromBody] Model_Share? model)
    {
        Query.UpdateShare(model);
        return Ok(new Model_Result<string>());
    }

    /// <summary>
    /// Remove a shared template or entry based on the given Id. 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Share")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NoDataFound, ResultCodes.DontOwnGroup)]
    [HttpPost(nameof(RemoveShare))]
    public IActionResult RemoveShare(Guid? id)
    {
        Query.RemoveShare(id);
        return Ok(new Model_Result<string>());
    }
    #endregion
}
