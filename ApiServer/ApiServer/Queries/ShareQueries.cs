using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Authentication;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Core;
using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Database.User;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class ShareQueries(NbbContext DB, ApplicationUser CurrentUser) : SharedItemQueries(DB, CurrentUser)
{
    #region Groups
    public List<Model_Group> GetOwnedGroups()
    {
        List<Model_Group> list = [..
            DB.Share_Group
            .Where(s => s.UserID == CurrentUser.ID)
            .Include(s => s.O_GroupUser)
            .Select(s => new Model_Group(s.ID, s.Name, s.O_GroupUser.Count))];
        return list;
    }

    public List<Model_GroupUser> GetUsersInGroup(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);

        List<Model_GroupUser> list = [..
            DB.Share_GroupUser
            .Where(s => s.GroupID == id)
            .Include(s => s.O_User)
            .Include(s => s.O_WhoAdded)
            .Select(s => new Model_GroupUser(s.O_User.Username, s.GroupID, s.CanAddUsers, s.CanRemoveUsers, s.O_WhoAdded.Username))];
        return list;
    }

    public List<Model_SharedToGroup> GetSharedToGroup(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);

        Share_Group group = DB.Share_Group.FirstOrDefault(s => s.ID == id)
            ?? throw new RequestException(ResultCodes.NoDataFound);

        List<Share_Item> list = [.. DB.Share_Item.Where(s => s.Visibility == ShareVisibility.Group && s.ToWhom == group.ID)];
        List<Model_SharedToGroup> result = [];
        foreach (Share_Item item in list)
        {
            string? name = item.Type == ShareType.Entry
                ? DB.Structure_Entry.FirstOrDefault(s => s.ID == item.ItemID)?.Name
                : item.Type == ShareType.Template
                ? DB.Structure_Template.FirstOrDefault(s => s.ID == item.ItemID)?.Name
                : null;

            if (string.IsNullOrWhiteSpace(name))
                throw new RequestException(ResultCodes.DataIsInvalid);
            result.Add(new Model_SharedToGroup(item.ItemID, item.Type, name));
        }

        return result;
    }

    public Model_Group UpdateGroup(Model_Group? model)
    {
        if (model is null)
            throw new RequestException(ResultCodes.DataIsInvalid);

        Share_Group? item = DB.Share_Group.FirstOrDefault(s => s.ID == model.ID);

        if (item is null)
        {
            if (DB.Share_Group.Any(s => s.Name.Equals(model.Name, StringComparison.CurrentCultureIgnoreCase)))
                throw new RequestException(ResultCodes.GroupNameAlreadyExists);

            item = new Share_Group()
            {
                ID = Guid.NewGuid(),
                UserID = CurrentUser.ID,
                Name = model.Name,
            };
            DB.Share_Group.Add(item);
            model = model with { ID = model.ID };
        }
        else
        {
            if (item.UserID != CurrentUser.ID)
                throw new RequestException(ResultCodes.DontOwnGroup);

            if (!item.Name.Equals(model.Name, StringComparison.OrdinalIgnoreCase) &&
                DB.Share_Group.Any(s => s.Name.Equals(model.Name, StringComparison.CurrentCultureIgnoreCase)))
                throw new RequestException(ResultCodes.GroupNameAlreadyExists);

            item.Name = model.Name;
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
    public void UpdateUserInGroup(Model_GroupUser? model)
    {
        if (model is null ||
            model.GroupID == Guid.Empty ||
            string.IsNullOrWhiteSpace(model.Username))
            throw new RequestException(ResultCodes.DataIsInvalid);

        Share_Group group = DB.Share_Group.FirstOrDefault(s => s.ID == model.GroupID) ??
            throw new RequestException(ResultCodes.NoDataFound);

        model = model with { Username = model.Username.ToLower().Normalize() };
        User_Login? user = DB.User_Login.FirstOrDefault(s => s.UsernameNormalized == model.Username) ??
            throw new RequestException(ResultCodes.NoDataFound);

        Share_GroupUser? item = DB.Share_GroupUser.Include(s => s.O_User)
            .FirstOrDefault(s => s.GroupID == group.ID && s.O_User.UsernameNormalized == model.Username);

        if (item is null)
        {
            item = new Share_GroupUser()
            {
                GroupID = group.ID,
                UserID = user.ID,
                WhoAdded = CurrentUser.ID,
                CanAddUsers = model.CanAddUsers,
                CanRemoveUsers = model.CanRemoveUsers,
            };
            DB.Share_Group.Add(group);
        }
        else
        {
            if (group.UserID != CurrentUser.ID || item.CanAddUsers)
                throw new RequestException(ResultCodes.MissingRight);

            item.CanAddUsers = model.CanAddUsers;
            item.CanRemoveUsers = model.CanRemoveUsers;
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
    public List<Model_ShareItem> GetShare(Guid? id, ShareType? type)
    {
        if (id is null || id == Guid.Empty || type is null)
            throw new RequestException(ResultCodes.DataIsInvalid);

        List<Share_Item> list = [.. DB.Share_Item.Where(s => s.ItemID == id && s.Type == type).Include(s => s.O_User)];
        List<Model_ShareItem> result = [];

        foreach (Share_Item item in list)
        {
            Guid userID = type == ShareType.Entry
               ? Exist_SharedItem(DB.Structure_Entry, item.ItemID)
               : type == ShareType.Template
               ? Exist_SharedItem(DB.Structure_Template, item.ItemID)
               : throw new RequestException(ResultCodes.DataIsInvalid);

            if (item.Visibility is ShareVisibility.Public)
            {
                result.Add(new Model_ShareItem(item, ""));
            }
            else if (item.Visibility is ShareVisibility.Directly)
            {
                User_Login user = DB.User_Login.FirstOrDefault(s => s.ID == item.ToWhom)
                    ?? throw new RequestException(ResultCodes.NoDataFound);

                if (userID == CurrentUser.ID || item.UserID == CurrentUser.ID)
                    result.Add(new Model_ShareItem(item, user.Username));
            }
            else if (item.Visibility is ShareVisibility.Group)
            {
                Share_Group group = DB.Share_Group.FirstOrDefault(s => s.ID == item.ToWhom)
                    ?? throw new RequestException(ResultCodes.NoDataFound);

                if (userID == CurrentUser.ID || group.UserID == CurrentUser.ID)
                    result.Add(new Model_ShareItem(item, group.Name));
            }
        }
        return result;
    }

    public void UpdateShare(Model_Share? model)
    {
        if (model is null)
            throw new RequestException(ResultCodes.DataIsInvalid);

        Guid userID = model.Type == ShareType.Entry
            ? Exist_SharedItem(DB.Structure_Entry, model.ID)
            : model.Type == ShareType.Template
            ? Exist_SharedItem(DB.Structure_Template, model.ID)
            : throw new RequestException(ResultCodes.DataIsInvalid);

        bool canShare = userID == CurrentUser.ID;
        if (canShare)
        {
            canShare = DB.Share_Item.FirstOrDefault(s => s.ToWhom == CurrentUser.ID &&
                        s.Visibility == ShareVisibility.Directly &&
                        s.ItemID == model.ID &&
                        s.Type == model.Type)
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
                        s.ItemID == model.ID &&
                        s.Type == model.Type)?.CanShare is true;
                    if (canShare)
                        break;
                }
            }
        }

        if (!canShare)
            throw new RequestException(ResultCodes.MissingRight);

        Guid? toWhom = null;

        if (model.Visibility is ShareVisibility.Public && string.IsNullOrWhiteSpace(model.ToWhom))
        {
            if (userID != CurrentUser.ID)
                throw new RequestException(ResultCodes.OnlyOwnerCanChangePublicity);
        }
        else if (model.Visibility is ShareVisibility.Group && !string.IsNullOrWhiteSpace(model.ToWhom) && Guid.TryParse(model.ToWhom, out Guid groupID))
        {
            Share_Group group = DB.Share_Group.FirstOrDefault(s => s.ID == groupID)
                ?? throw new RequestException(ResultCodes.NoDataFound);
            toWhom = group.ID;
        }
        else if (model.Visibility is ShareVisibility.Directly && !string.IsNullOrWhiteSpace(model.ToWhom))
        {
            string username = model.ToWhom.ToLower().Normalize();
            User_Login user = DB.User_Login.FirstOrDefault(s => s.UsernameNormalized == username)
                ?? throw new RequestException(ResultCodes.NoDataFound);
            toWhom = user.ID;
        }
        else
            throw new RequestException(ResultCodes.DataIsInvalid);

        Share_Item? item = DB.Share_Item.FirstOrDefault(s => s.ToWhom == null &&
            s.Visibility == model.Visibility &&
            s.ItemID == model.ID &&
            s.Type == model.Type);

        if (item is null)
        {
            item = new()
            {
                ID = Guid.NewGuid(),
                UserID = CurrentUser.ID,
                Visibility = model.Visibility,
                Type = model.Type,
                ItemID = model.ID,
                ToWhom = toWhom,
                CanShare = model.CanShare,
                CanDelete = model.CanDelete,
                CanEdit = model.CanEdit
            };

            DB.Share_Item.Add(item);
        }
        else
        {
            item.CanShare = model.CanShare;
            item.CanEdit = model.CanEdit;
            item.CanDelete = model.CanDelete;
        }
        DB.SaveChanges();
    }

    public void RemoveShare(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);

        Share_Item item = DB.Share_Item.FirstOrDefault(s => s.ID == id) ??
            throw new RequestException(ResultCodes.NoDataFound);

        if (item.Visibility is ShareVisibility.Public && item.UserID != CurrentUser.ID)
            throw new RequestException(ResultCodes.OnlyOwnerCanChangePublicity);
        else if (item.UserID != CurrentUser.ID)
        {
            Guid userID = item.Type == ShareType.Entry
                ? Exist_SharedItem(DB.Structure_Entry, item.ItemID)
                : item.Type == ShareType.Template
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
