using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace StyleWerk.NBB.Controllers;

[ApiController, Route("ShareNew"), Authorize]
public class ShareNewController(NbbContext db) : BaseController(db)
{
    public ShareNewQueries Query => new(DB, CurrentUser);

    #region Groups
    /// <summary>
    /// Load all groups which you own
    /// </summary>
    /// <returns></returns>
    [Produces("application/json"), ApiExplorerSettings(GroupName = "Groups")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_Group2>>))]
    [HttpGet(nameof(GetOwnedGroups))]
    public IActionResult GetOwnedGroups()
    {
        List<Model_Group2> list = Query.GetOwnedGroups();
        return Ok(new Model_Result<List<Model_Group2>>(list));
    }

    /// <summary>
    /// Load the users in a specific group with the rights they have
    /// </summary>
    /// <param name="id">The ID of the group from which zou want to load the users</param>
    /// <returns></returns>
    [Produces("application/json"), ApiExplorerSettings(GroupName = "Groups")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_GroupUser2>>))]
    [HttpGet(nameof(GetUsersInGroup))]
    public IActionResult GetUsersInGroup(Guid? id)
    {
        List<Model_GroupUser2> list = Query.GetUsersInGroup(id);
        return Ok(new Model_Result<List<Model_GroupUser2>>(list));
    }

    /// <summary>
    /// Create a group or update the group informations of an existing group
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [Produces("application/json"), ApiExplorerSettings(GroupName = "Groups")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_UpdateGroup2>))]
    [HttpPost(nameof(UpdateGroup))]
    public IActionResult UpdateGroup([FromBody] Model_UpdateGroup2? model)
    {
        model = Query.UpdateGroup(model);
        return Ok(new Model_Result<Model_UpdateGroup2>(model));
    }

    /// <summary>
    /// Delete a group you own
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Produces("application/json"), ApiExplorerSettings(GroupName = "Groups")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
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
    [Produces("application/json"), ApiExplorerSettings(GroupName = "Users in Group")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(UpdateUserInGroup))]
    public IActionResult UpdateUserInGroup([FromBody] Model_UserFromGroup2? model)
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
    [Produces("application/json"), ApiExplorerSettings(GroupName = "Users in Group")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(RemoveUserFromGroup))]
    public IActionResult RemoveUserFromGroup(string? username, Guid? groupID)
    {
        Query.RemoveUserFromGroup(username, groupID);
        return Ok(new Model_Result<string>());
    }
    #endregion

    #region Share
    [Produces("application/json"), ApiExplorerSettings(GroupName = "Share")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(UpdateShare))]
    public IActionResult UpdateShare([FromBody] Model_Share? model)
    {
        Query.UpdateShare(model);
        return Ok(new Model_Result<string>());
    }

    [Produces("application/json"), ApiExplorerSettings(GroupName = "Share")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(RemoveShare))]
    public IActionResult RemoveShare(Guid? id)
    {
        Query.RemoveShare(id);
        return Ok(new Model_Result<string>());
    }
    #endregion
}
