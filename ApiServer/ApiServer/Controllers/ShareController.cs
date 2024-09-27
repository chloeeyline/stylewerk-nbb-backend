﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Database.User;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace StyleWerk.NBB.Controllers
{
    [ApiController, Route("Share"), Authorize]
    public class ShareController(NbbContext db) : BaseController(db)
    {
        public TemplateQueries Query => new(DB, CurrentUser);

        #region Group
        [ApiExplorerSettings(GroupName = "Load Group")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_Group>>))]
        [HttpGet(nameof(GetUsersInGroup))]
        public IActionResult GetUsersInGroup()
        {
            List<Model_Group> groups = Query.LoadUserGroups();
            return Ok(new Model_Result<List<Model_Group>>(groups));
        }

        [ApiExplorerSettings(GroupName = "Load Group")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_ShareItemRightsUser>))]
        [HttpGet(nameof(LoadGroupDetailsForShareItem))]
        public IActionResult LoadGroupDetailsForShareItem(Model_GroupDetails model)
        {
            if (model is null)
                throw new RequestException(ResultType.DataIsInvalid);

            Share_Group group = GroupExists(model.GroupId);
            Model_ShareItemRightsGroup groupRights = Query.ShareItemGroupRights(group, model.ShareItem);

            return Ok(new Model_Result<Model_ShareItemRightsUser>());
        }

        [ApiExplorerSettings(GroupName = "Load Group")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_Group>>))]
        [HttpGet(nameof(GetGroups))]
        public IActionResult GetGroups()
        {
            List<Model_Group> groups = Query.LoadGroups();
            return Ok(new Model_Result<List<Model_Group>>());
        }

        [ApiExplorerSettings(GroupName = "Group Actions")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Guid>))]
        [HttpPost(nameof(CreateGroup))]
        public IActionResult CreateGroup(Model_CreateGroup model)
        {
            if (model is null)
                throw new RequestException(ResultType.DataIsInvalid);

            Share_Group groupExistsForUser = DB.Share_Group.FirstOrDefault(g => g.UserID == CurrentUser.ID && g.Name == model.Name)
                ?? throw new RequestException(ResultType.DataIsInvalid, "Es existiert bereits eine Gruppe mit diesem Namen");

            Share_Group newGroup = new()
            {
                ID = Guid.NewGuid(),
                UserID = CurrentUser.ID,
                Name = model.Name,
                IsVisible = model.IsVisible,
                CanSeeOthers = model.CanSeeOthers
            };

            DB.Share_Group.Add(newGroup);
            DB.SaveChanges();

            return Ok(new Model_Result<Guid>(newGroup.ID));
        }

        [ApiExplorerSettings(GroupName = "Group Actions")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
        [HttpPost(nameof(DeleteGroup))]
        public IActionResult DeleteGroup(Guid? groupID)
        {
            if (groupID is null)
                throw new RequestException(ResultType.DataIsInvalid);

            Share_Group group = DB.Share_Group.FirstOrDefault(g => g.ID == groupID && g.UserID == CurrentUser.ID)
                ?? throw new RequestException(ResultType.NoDataFound);

            List<Share_GroupUser> groupUsers = DB.Share_GroupUser.Where(g => g.GroupID == groupID).ToList();
            if (groupUsers.Any())
            {
                DB.Share_GroupUser.RemoveRange(groupUsers);
            }

            DB.Share_Group.Remove(group);
            DB.SaveChanges();

            return Ok(new Model_Result<string>());
        }

        [ApiExplorerSettings(GroupName = "Group Actions")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Guid>))]
        [HttpPost(nameof(ChangeGroupRights))]
        public IActionResult ChangeGroupRights(Model_GroupRight model)
        {
            if (model is null)
                throw new RequestException(ResultType.DataIsInvalid);

            //Check if User is creator of group
            Share_Group group = DB.Share_Group.FirstOrDefault(g => g.UserID == CurrentUser.ID && g.ID == model.GroupId)
                ?? throw new RequestException(ResultType.NoDataFound);

            group.CanSeeOthers = model.CanSeeOthers;
            group.IsVisible = model.IsVisible;

            DB.SaveChanges();

            return Ok(new Model_Result<Guid>(model.GroupId));
        }

        [ApiExplorerSettings(GroupName = "Share Rights")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Guid>))]
        [HttpPost(nameof(ChangeGroupName))]
        public IActionResult ChangeGroupName(Model_GroupName model)
        {
            if (model is null)
                throw new RequestException(ResultType.DataIsInvalid);

            //check if User is creator of group 
            Share_Group isCreator = DB.Share_Group.FirstOrDefault(g => g.ID == model.GroupId && g.UserID == CurrentUser.ID)
                ?? throw new RequestException(ResultType.NoDataFound);

            isCreator.Name = model.GroupName;

            DB.SaveChanges();

            return Ok(new Model_Result<Guid>(model.GroupId));
        }

        [ApiExplorerSettings(GroupName = "Group Usermanagement")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Guid>))]
        public IActionResult AddUser(Model_GroupUser model)
        {
            if (model is null)
                throw new RequestException(ResultType.DataIsInvalid);

            User_Login user = UserExists(model.Username);

            //check if group exists
            Share_Group? group = DB.Share_Group.FirstOrDefault(g => g.ID == model.GroupId)
                ?? throw new RequestException(ResultType.NoDataFound);

            //check if currentUser has the right to add User
            Share_GroupUser hasRight = DB.Share_GroupUser.FirstOrDefault(g => g.GroupID == group.ID && g.UserID == CurrentUser.ID && g.CanAddUsers == true)
                ?? throw new RequestException(ResultType.MissingRight);

            //check if user already in group
            bool inGroup = DB.Share_GroupUser.Any(g => g.UserID == user.ID && g.GroupID == group.ID);
            if (!inGroup)
            {
                Share_GroupUser newUser = new()
                {
                    GroupID = model.GroupId,
                    UserID = user.ID,
                    WhoAdded = CurrentUser.ID,
                    CanSeeUsers = model.userRights.CanSeeUsers,
                    CanAddUsers = model.userRights.CanAddUsers,
                    CanRemoveUsers = model.userRights.CanRemoveUsers
                };

                DB.Share_GroupUser.Add(newUser);
            }

            DB.SaveChanges();

            return Ok(new Model_Result<Guid>());
        }

        [ApiExplorerSettings(GroupName = "Group Usermanagement")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
        [HttpPost(nameof(RemoveUser))]
        public IActionResult RemoveUser(Model_RemoveUser model)
        {
            if (model is null)
                throw new RequestException(ResultType.DataIsInvalid);

            User_Login user = UserExists(model.Username);
            Share_Group group = GroupExists(model.GroupId);

            Share_GroupUser hasRight = DB.Share_GroupUser.FirstOrDefault(g => g.GroupID == group.ID && g.UserID == CurrentUser.ID && g.CanRemoveUsers == true)
                ?? throw new RequestException(ResultType.MissingRight);

            Share_GroupUser? userInGroup = DB.Share_GroupUser.FirstOrDefault(g => g.UserID == user.ID && g.GroupID == group.ID);
            if (userInGroup is not null)
            {
                DB.Share_GroupUser.Remove(userInGroup);
            }

            DB.SaveChanges();

            return Ok(new Model_Result<string>());
        }

        [ApiExplorerSettings(GroupName = "Group Usermanagement")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Guid>))]
        [HttpPost(nameof(ChangeUserRightsInGroup))]
        public IActionResult ChangeUserRightsInGroup(Model_GroupUser model)
        {
            if (model is null)
                throw new RequestException(ResultType.DataIsInvalid);

            User_Login user = UserExists(model.Username);
            Share_Group group = GroupExists(model.GroupId);

            //why -> if you can add an user to the group, you might want to change his rights he might have
            Share_GroupUser hasRight = DB.Share_GroupUser.FirstOrDefault(g => g.GroupID == group.ID && g.CanAddUsers)
                ?? throw new RequestException(ResultType.MissingRight);

            Share_GroupUser userInGroup = DB.Share_GroupUser.FirstOrDefault(u => u.GroupID == group.ID && u.UserID == user.ID)
                ?? throw new RequestException(ResultType.NoDataFound);

            userInGroup.CanSeeUsers = model.userRights.CanSeeUsers;
            userInGroup.CanRemoveUsers = model.userRights.CanRemoveUsers;
            userInGroup.CanAddUsers = model.userRights.CanAddUsers;

            DB.SaveChanges();

            return Ok(new Model_Result<Guid>(user.ID));
        }

        #endregion

        #region User
        [ApiExplorerSettings(GroupName = "Load User")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<List<Model_User>>))]
        [HttpGet(nameof(LoadUsers))]
        public IActionResult LoadUsers()
        {
            List<Model_User> users = Query.LoadUsers();
            return Ok(new Model_Result<List<Model_User>>(users));
        }

        //Load User Details and rights
        [ApiExplorerSettings(GroupName = "Load User")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_ShareItemRightsUser>))]
        [HttpPost(nameof(LoadUserDetailsForShareItem))]
        public IActionResult LoadUserDetailsForShareItem(Model_UserDetails model)
        {
            if (model is null)
                throw new RequestException(ResultType.DataIsInvalid);

            User_Login user = UserExists(model.Username);
            Model_ShareItemRightsUser userRights = Query.ShareItemUserRights(user, model.ShareItem);

            return Ok(new Model_Result<Model_ShareItemRightsUser>(userRights));
        }

        [ApiExplorerSettings(GroupName = "Share Rights")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Guid>))]
        [HttpPost(nameof(ChangeShareItemRights))]
        public IActionResult ChangeShareItemRights(Model_ShareItemRightsUser model)
        {
            if (model is null)
                throw new RequestException(ResultType.DataIsInvalid);

            User_Login user = UserExists(model.Username);

            Share_Item exists = DB.Share_Item.FirstOrDefault(i => i.ID == model.ShareItem && i.ToWhom == user.ID && i.WhoShared == CurrentUser.ID)
                ?? throw new RequestException(ResultType.NoDataFound);

            exists.CanDelete = model.Rights.CanDelete;
            exists.CanShare = model.Rights.CanShare;
            exists.CanEdit = model.Rights.CanEdit;

            DB.SaveChanges();

            return Ok(new Model_Result<Guid>(exists.ID));
        }

        #endregion

        #region Actions

        [ApiExplorerSettings(GroupName = "Share")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
        [HttpPost(nameof(ShareGroup))]
        public IActionResult ShareGroup(Model_ShareGroup model)
        {
            if (model is null)
                throw new RequestException(ResultType.DataIsInvalid);

            //check if group exists and if User is in group
            Share_Group groupExists = DB.Share_Group.FirstOrDefault(g => g.ID == model.GroupId && g.UserID == CurrentUser.ID)
                ?? throw new RequestException(ResultType.NoDataFound);

            //Entry
            if (model.ItemType == 1)
            {
                //Check if Entry exists
                Structure_Entry? entry = DB.Structure_Entry.FirstOrDefault(e => e.ID == model.ShareItem)
                    ?? throw new RequestException(ResultType.NoDataFound);

                //check if entry has already been shared with group
                bool isShared = DB.Share_Item.Any(i => i.ItemID == model.ShareItem && i.ToWhom == model.GroupId && i.Group);
                if (isShared)
                    throw new RequestException(ResultType.GeneralError, "Der Eintrag wurde bereits in dieser Gruppe geteilt");

                //check if entry belongs to user
                bool belongsUser = entry.UserID == CurrentUser.ID ? true : false;

                if (!belongsUser)
                {
                    IEnumerable<Model_Group> groups = Query.LoadGroups().Where(g => g.GroupId != model.GroupId);
                    bool shareRight = DB.Share_Item.Any(i => i.ItemID == entry.ID && groups.Any(g => g.GroupId == i.ToWhom) && i.CanShare);
                    if (!shareRight)
                        throw new RequestException(ResultType.MissingRight);

                    //check if entry was once shared directly
                    bool shareDirectly = DB.Share_Item.Any(i => i.ItemID == entry.ID && i.ToWhom == CurrentUser.ID && i.CanShare);
                    if (!shareDirectly)
                        throw new RequestException(ResultType.MissingRight);
                }

                ShareEntry(entry, model.GroupId, true, model.Rights);
            }

            //Template
            if (model.ItemType == 2)
            {
                //Check if Template exists
                Structure_Template template = DB.Structure_Template.FirstOrDefault(t => t.ID == model.ShareItem)
                    ?? throw new RequestException(ResultType.NoDataFound);

                //check if template has already been shared with group
                bool isShared = DB.Share_Item.Any(i => i.ItemID == template.ID && i.ToWhom == model.GroupId);
                if (isShared)
                    throw new RequestException(ResultType.GeneralError, "Die Vorlage wurde bereits in dieser Gruppe geteilt");

                //check if template belongs to user
                bool belongsUser = template.UserID == CurrentUser.ID ? true : false;

                if (!belongsUser)
                {
                    //check if template was once shared in another group
                    IEnumerable<Model_Group> groups = Query.LoadGroups().Where(g => g.GroupId != model.GroupId);
                    bool shareRight = DB.Share_Item.Any(i => i.ItemID == template.ID && groups.Any(g => g.GroupId == i.ToWhom) && i.CanShare);
                    if (!shareRight)
                        throw new RequestException(ResultType.MissingRight);

                    //check if entry was once shared directly
                    bool shareDirectly = DB.Share_Item.Any(i => i.ItemID == template.ID && i.ToWhom == CurrentUser.ID && i.CanShare);
                    if (!shareDirectly)
                        throw new RequestException(ResultType.MissingRight);
                }

                ShareTemplate(template, model.GroupId, true, model.Rights);
            }
            return Ok(new Model_Result<Guid>(model.ShareItem));
        }

        [ApiExplorerSettings(GroupName = "Share")]
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
        [HttpPost(nameof(ShareDirectly))]
        public IActionResult ShareDirectly(Model_ShareDirectly model)
        {
            if (model is null)
                throw new RequestException(ResultType.DataIsInvalid);

            User_Login user = UserExists(model.Username);

            //Entry
            if (model.ItemType == 1)
            {
                //Check if Entry exists
                Structure_Entry? entry = DB.Structure_Entry.FirstOrDefault(e => e.ID == model.ShareItem)
                    ?? throw new RequestException(ResultType.NoDataFound);

                //check if entry has already been shared with user
                bool isShared = DB.Share_Item.Any(i => i.ItemID == entry.ID && i.ToWhom == user.ID);
                if (isShared)
                    throw new RequestException(ResultType.GeneralError, "Der Eintrag wurde bereits mit diesem User geteilt");

                //check if entry belongs to user
                bool belongsUser = entry.UserID == CurrentUser.ID ? true : false;

                if (!belongsUser)
                {
                    //Check if entry was once shared within a group 
                    List<Model_Group> groups = Query.LoadGroups();
                    bool shareRight = DB.Share_Item.Any(i => i.ItemID == entry.ID && groups.Any(g => g.GroupId == i.ToWhom) && i.CanShare);
                    if (!shareRight)
                        throw new RequestException(ResultType.MissingRight);

                    //check if entry was once shared directly
                    bool shareDirectly = DB.Share_Item.Any(i => i.ItemID == entry.ID && i.ToWhom == CurrentUser.ID && i.CanShare);
                    if (!shareDirectly)
                        throw new RequestException(ResultType.MissingRight);
                }

                ShareEntry(entry, user.ID, false, model.Rights);
            }

            //Template
            if (model.ItemType == 2)
            {
                //Check if Template exists
                Structure_Template template = DB.Structure_Template.FirstOrDefault(t => t.ID == model.ShareItem)
                    ?? throw new RequestException(ResultType.NoDataFound);

                //check if entry has already been shared with user
                bool isShared = DB.Share_Item.Any(i => i.ItemID == template.ID && i.ToWhom == user.ID);
                if (isShared)
                    throw new RequestException(ResultType.GeneralError, "Der Eintrag wurde bereits mit diesem User geteilt");

                //check if template belongs to user
                bool belongsUser = template.UserID == CurrentUser.ID ? true : false;

                if (!belongsUser)
                {
                    //Check if template was once shared within a group 
                    List<Model_Group> groups = Query.LoadGroups();
                    bool shareRight = DB.Share_Item.Any(i => i.ItemID == template.ID && groups.Any(g => g.GroupId == i.ToWhom) && i.CanShare);
                    if (!shareRight)
                        throw new RequestException(ResultType.MissingRight);

                    //check if template was once shared directly
                    bool shareDirectly = DB.Share_Item.Any(i => i.ItemID == template.ID && i.ToWhom == CurrentUser.ID && i.CanShare);
                    if (!shareDirectly)
                        throw new RequestException(ResultType.MissingRight);
                }

                ShareTemplate(template, user.ID, true, model.Rights);
            }

            return Ok(new Model_Result<Guid>());
        }

        #endregion

        #region Private Functions
        private void ShareEntry(Structure_Entry entry, Guid toWhom, bool isGroup, ShareRights rights)
        {
            Share_Item newSharedEntry = new()
            {
                ID = Guid.NewGuid(),
                WhoShared = CurrentUser.ID,
                Group = isGroup,
                ItemType = 1,
                ItemID = entry.ID,
                ToWhom = toWhom,
                CanShare = rights.CanShare,
                CanDelete = rights.CanDelete,
                CanEdit = rights.CanEdit
            };

            DB.Share_Item.Add(newSharedEntry);
            DB.SaveChanges();
        }
        private void ShareTemplate(Structure_Template template, Guid toWhom, bool isGroup, ShareRights rights)
        {
            Share_Item newSharedEntry = new()
            {
                ID = Guid.NewGuid(),
                WhoShared = CurrentUser.ID,
                Group = isGroup,
                ItemType = 2,
                ItemID = template.ID,
                ToWhom = toWhom,
                CanShare = rights.CanShare,
                CanDelete = rights.CanDelete,
                CanEdit = rights.CanEdit
            };

            DB.Share_Item.Add(newSharedEntry);
            DB.SaveChanges();
        }
        private User_Login UserExists(string username)
        {
            User_Login user = DB.User_Login.FirstOrDefault(u => u.Username == username)
                ?? throw new RequestException(ResultType.NoDataFound);

            return user;
        }
        private Share_Group GroupExists(Guid groupId)
        {
            Share_Group exists = DB.Share_Group.FirstOrDefault(g => g.ID == groupId)
                ?? throw new RequestException(ResultType.NoDataFound);

            return exists;
        }
        #endregion
    }
}
