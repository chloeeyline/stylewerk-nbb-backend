using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Authentication;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Database.User;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class ShareNewQueries(NbbContext DB, ApplicationUser CurrentUser) : SharedItemQueries(DB, CurrentUser)
{
    #region Groups
    public List<Model_Group2> GetOwnedGroups()
    {
        List<Model_Group2> list = [..
            DB.Share_Group
            .Where(s => s.UserID == CurrentUser.ID)
            .Include(s => s.O_User)
            .Include(s => s.O_GroupUser)
            .Select(s => new Model_Group2(s))];
        return list;
    }

    public List<Model_GroupUser2> GetUsersInGroup(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);

        List<Model_GroupUser2> list = [..
            DB.Share_GroupUser
            .Where(s => s.GroupID == id)
            .Include(s => s.O_User)
            .Include(s => s.O_WhoShared)
            .Select(s => new Model_GroupUser2(s))];
        return list;
    }

    public Model_UpdateGroup2 UpdateGroup(Model_UpdateGroup2? model)
    {
        if (model is null)
            throw new RequestException(ResultCodes.DataIsInvalid);

        Share_Group? item = DB.Share_Group.FirstOrDefault(s => s.ID == model.ID);

        if (item is null)
        {
            if (DB.Share_Group.Any(s => s.Name.Equals(model.Settings.Name, StringComparison.CurrentCultureIgnoreCase)))
                throw new RequestException(ResultCodes.GroupNameAlreadyExists);

            item = new Share_Group()
            {
                ID = Guid.NewGuid(),
                UserID = CurrentUser.ID,
                Name = model.Settings.Name,
                IsVisible = model.Settings.IsVisible,
                CanSeeOthers = model.Settings.CanSeeOthers
            };
            DB.Share_Group.Add(item);
            model = model with { ID = model.ID };
        }
        else
        {
            if (item.UserID != CurrentUser.ID)
                throw new RequestException(ResultCodes.DontOwnGroup);

            if (!item.Name.Equals(model.Settings.Name, StringComparison.OrdinalIgnoreCase) &&
                DB.Share_Group.Any(s => s.Name.Equals(model.Settings.Name, StringComparison.CurrentCultureIgnoreCase)))
                throw new RequestException(ResultCodes.GroupNameAlreadyExists);

            item.Name = model.Settings.Name;
            item.IsVisible = model.Settings.IsVisible;
            item.CanSeeOthers = model.Settings.CanSeeOthers;
        }

        DB.SaveChanges();
        return model;
    }

    public void RemoveGroup(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);

        Share_Group item = DB.Share_Group.FirstOrDefault(s => s.ID == id) ??
            throw new RequestException(ResultCodes.NoDataFound);

        if (item.UserID != CurrentUser.ID)
            throw new RequestException(ResultCodes.DontOwnGroup);

        DB.Share_Group.Remove(item);
        DB.Share_Item.RemoveRange(DB.Share_Item.Where(s => s.Group && s.ToWhom == item.ID));
    }
    #endregion

    #region Users in Group
    public void UpdateUserInGroup(Model_UserFromGroup2? model)
    {
        if (model is null ||
            model.GroupID == Guid.Empty ||
            string.IsNullOrWhiteSpace(model.UserName))
            throw new RequestException(ResultCodes.DataIsInvalid);

        Share_Group group = DB.Share_Group.FirstOrDefault(s => s.ID == model.GroupID) ??
            throw new RequestException(ResultCodes.NoDataFound);

        model = model with { UserName = model.UserName.ToLower().Normalize() };
        User_Login? user = DB.User_Login.FirstOrDefault(s => s.UsernameNormalized == model.UserName) ??
            throw new RequestException(ResultCodes.NoDataFound);

        Share_GroupUser? item = DB.Share_GroupUser.Include(s => s.O_User)
            .FirstOrDefault(s => s.GroupID == group.ID && s.O_User.UsernameNormalized == model.UserName);

        if (item is null)
        {
            item = new Share_GroupUser()
            {
                GroupID = group.ID,
                UserID = user.ID,
                WhoAdded = CurrentUser.ID,
                CanSeeUsers = model.Rights.CanSeeUsers,
                CanAddUsers = model.Rights.CanAddUsers,
                CanRemoveUsers = model.Rights.CanRemoveUsers,
            };
            DB.Share_Group.Add(group);
        }
        else
        {
            if (group.UserID != CurrentUser.ID || item.CanAddUsers)
                throw new RequestException(ResultCodes.MissingRight);

            item.CanSeeUsers = model.Rights.CanSeeUsers;
            item.CanAddUsers = model.Rights.CanAddUsers;
            item.CanRemoveUsers = model.Rights.CanRemoveUsers;
        }

        DB.SaveChanges();
    }

    public void RemoveUserFromGroup(string? username, Guid? groupID)
    {
        if (groupID is null ||
            groupID == Guid.Empty ||
            string.IsNullOrWhiteSpace(username))
            throw new RequestException(ResultCodes.DataIsInvalid);

        Share_Group group = DB.Share_Group.FirstOrDefault(s => s.ID == groupID) ??
            throw new RequestException(ResultCodes.NoDataFound);

        username = username.ToLower().Normalize();
        User_Login? user = DB.User_Login.FirstOrDefault(s => s.UsernameNormalized == username) ??
            throw new RequestException(ResultCodes.NoDataFound);

        Share_GroupUser item = DB.Share_GroupUser.Include(s => s.O_User)
            .FirstOrDefault(s => s.GroupID == group.ID && s.O_User.UsernameNormalized == username) ??
            throw new RequestException(ResultCodes.NoDataFound);

        if (group.UserID != CurrentUser.ID || item.CanRemoveUsers)
            throw new RequestException(ResultCodes.MissingRight);

        DB.Share_GroupUser.Remove(item);
    }
    #endregion

    #region Directly Share
    public void GetShare() { }

    public void UpdateShare() { }

    public void RemoveShare() { }
    #endregion
}

public record Model_UserFromGroup2(string UserName, Guid GroupID, GroupRights2 Rights);
public record Model_UpdateGroup2(Guid ID, GroupSettings Settings);

public record Model_Group2(Guid ID, GroupSettings Settings, string Username, int UserCount)
{
    public Model_Group2(Share_Group item) :
        this(item.ID, new GroupSettings(item), item.O_User.Username, item.O_GroupUser.Count)
    { }
}

public record Model_GroupUser2(string Username, string WhoAdded, GroupRights2 Rights)
{
    public Model_GroupUser2(Share_GroupUser item) :
        this(item.O_User.Username, item.O_WhoShared.Username, new GroupRights2(item))
    { }
}

public record GroupSettings(string Name, bool IsVisible, bool CanSeeOthers)
{
    public GroupSettings(Share_Group item) :
        this(item.Name, item.IsVisible, item.CanSeeOthers)
    { }
}
public record GroupRights2(bool CanSeeUsers, bool CanAddUsers, bool CanRemoveUsers)
{
    public GroupRights2(Share_GroupUser item) :
        this(item.CanSeeUsers, item.CanAddUsers, item.CanRemoveUsers)
    { }
}
