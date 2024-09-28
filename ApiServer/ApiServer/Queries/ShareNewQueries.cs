using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Authentication;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Core;
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
        DB.Share_Item.RemoveRange(DB.Share_Item.Where(s => s.Visibility == ShareVisibility.Group && s.ToWhom == item.ID));
        DB.SaveChanges();
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
        DB.SaveChanges();
    }
    #endregion

    #region Share
    public void GetShare()
    {
    }

    public void UpdateShare(Model_Share? model)
    {
        if (model is null)
            throw new RequestException(ResultCodes.DataIsInvalid);

        Guid userID = Guid.Empty;
        userID = model.Type == 1
            ? Exist_SharedItem(DB.Structure_Entry, model.ItemID)
            : model.Type == 2
            ? Exist_SharedItem(DB.Structure_Template, model.ItemID)
            : throw new RequestException(ResultCodes.DataIsInvalid);

        bool canShare = userID == CurrentUser.ID;
        if (canShare)
        {
            canShare = DB.Share_Item.FirstOrDefault(s => s.ToWhom == CurrentUser.ID &&
                        s.Visibility == ShareVisibility.Directly &&
                        s.ItemID == model.ItemID &&
                        s.ItemType == model.Type)
                        is not null;

            if (!canShare)
            {
                List<Share_Group> groupsImPartOf = [..
                    DB.Share_GroupUser.Where(s => s.UserID == CurrentUser.ID)
                    .Include(s => s.O_Group)
                    .Select(s => s.O_Group)];

                foreach (Share_Group group in groupsImPartOf)
                {
                    canShare = DB.Share_Item.FirstOrDefault(s => s.ToWhom == group.ID &&
                        s.Visibility == ShareVisibility.Group &&
                        s.ItemID == model.ItemID &&
                        s.ItemType == model.Type)?.CanShare is true;
                    if (canShare)
                        break;
                }
            }
        }

        if (!canShare)
            throw new RequestException(ResultCodes.MissingRight);

        Guid? toWhom = null;
        Share_Item? item = null;

        if (string.IsNullOrWhiteSpace(model.ToWhom) && model.Visibility is ShareVisibility.Public)
        {
            item = DB.Share_Item.FirstOrDefault(s => s.ToWhom == null &&
                s.Visibility == ShareVisibility.Public &&
                s.ItemID == model.ItemID &&
                s.ItemType == model.Type);

            if (userID != CurrentUser.ID)
                throw new RequestException(ResultCodes.OnlyOwnerCanChangePublicity);
        }
        else if (Guid.TryParse(model.ToWhom, out Guid groupID) && model.Visibility is ShareVisibility.Group)
        {
            Share_Group group = DB.Share_Group.FirstOrDefault(s => s.ID == groupID)
                ?? throw new RequestException(ResultCodes.NoDataFound);

            item = DB.Share_Item.FirstOrDefault(s => s.ToWhom == groupID &&
                s.Visibility == ShareVisibility.Group &&
                s.ItemID == model.ItemID &&
                s.ItemType == model.Type);

            toWhom = group.ID;
        }
        else if (!string.IsNullOrWhiteSpace(model.ToWhom) && model.Visibility is ShareVisibility.Directly)
        {
            string username = model.ToWhom.ToLower().Normalize();
            User_Login user = DB.User_Login.FirstOrDefault(s => s.UsernameNormalized == username)
                ?? throw new RequestException(ResultCodes.NoDataFound);

            item = DB.Share_Item.FirstOrDefault(s => s.ToWhom == user.ID &&
                s.Visibility == ShareVisibility.Directly &&
                s.ItemID == model.ItemID &&
                s.ItemType == model.Type);

            toWhom = user.ID;
        }
        else
        {
            throw new RequestException(ResultCodes.DataIsInvalid);
        }

        if (item is null)
        {
            item = new()
            {
                ID = Guid.NewGuid(),
                WhoShared = CurrentUser.ID,
                Visibility = model.Visibility,
                ItemType = model.Type,
                ItemID = model.ItemID,
                ToWhom = toWhom,
                CanShare = model.Rights.CanShare,
                CanDelete = model.Rights.CanDelete,
                CanEdit = model.Rights.CanEdit
            };

            DB.Share_Item.Add(item);
        }
        else
        {
            item.CanShare = model.Rights.CanShare;
            item.CanEdit = model.Rights.CanEdit;
            item.CanDelete = model.Rights.CanDelete;
        }
        DB.SaveChanges();
    }

    public void RemoveShare(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);

        Share_Item item = DB.Share_Item.FirstOrDefault(s => s.ID == id) ??
            throw new RequestException(ResultCodes.NoDataFound);

        if (item.Visibility is ShareVisibility.Public && item.WhoShared != CurrentUser.ID)
            throw new RequestException(ResultCodes.OnlyOwnerCanChangePublicity);
        else if (item.WhoShared != CurrentUser.ID)
        {
            Guid userID = item.ItemType == 1
                ? Exist_SharedItem(DB.Structure_Entry, item.ItemID)
                : item.ItemType == 2
                ? Exist_SharedItem(DB.Structure_Template, item.ItemID)
                : throw new RequestException(ResultCodes.DataIsInvalid);
            if (userID != CurrentUser.ID)
                throw new RequestException(ResultCodes.DontOwnGroup);
        }

        DB.Share_Item.Remove(item);
        DB.SaveChanges();
    }

    private static Guid Exist_SharedItem<T>(DbSet<T> set, Guid id) where T : class, IEntity_GuidID, IEntity_User
    {
        T item = set.FirstOrDefault(s => s.ID == id) ?? throw new RequestException(ResultCodes.NoDataFound);
        return item.UserID;
    }
    #endregion
}

public record Model_Share(Guid ItemID, string? ToWhom, ShareVisibility Visibility, byte Type, ShareRight Rights);
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

public record ShareRight(bool CanShare, bool CanEdit, bool CanDelete);
