using Microsoft.EntityFrameworkCore;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Core;
using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Database.User;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class ShareQueries(NbbContext DB, ApplicationUser CurrentUser) : SharedItemQueries(DB, CurrentUser)
{
    #region Groups
    /// <summary>
    /// get all groups that the current user has created
    /// </summary>
    /// <returns></returns>
    public List<Model_Group> GetOwnedGroups()
    {
        List<Model_Group> list = [..
            DB.Share_Group
            .Where(s => s.UserID == CurrentUser.ID)
            .Include(s => s.O_GroupUser)
            .Select(s => new Model_Group(s.ID, s.Name, s.O_GroupUser.Count))];
        return list;
    }

    /// <summary>
    /// show all users and their rights in a group based on the given group id 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="RequestException"></exception>
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

    /// <summary>
    /// Get all templates and entries that were shared in a group based on the given group id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="RequestException"></exception>
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

    /// <summary>
    /// updates or adds a group if it doesn't already exists based on the given model
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    /// <exception cref="RequestException"></exception>
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

    /// <summary>
    /// removes a group, all users in the group and all items shared within the group based on the given id 
    /// only the creator of the group can remove the group
    /// </summary>
    /// <param name="id"></param>
    /// <exception cref="RequestException"></exception>
    public void RemoveGroup(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);

        Share_Group item = DB.Share_Group.FirstOrDefault(s => s.ID == id) ??
            throw new RequestException(ResultCodes.NoDataFound);

        if (item.UserID != CurrentUser.ID)
            throw new RequestException(ResultCodes.DontOwnGroup);

        DB.Share_Group.Remove(item);
        DB.Share_GroupUser.RemoveRange(DB.Share_GroupUser.Where(s => s.GroupID == id));
        DB.Share_Item.RemoveRange(DB.Share_Item.Where(s => s.Visibility == ShareVisibility.Group && s.ToWhom == item.ID));
        DB.SaveChanges();
    }
    #endregion

    #region Users in Group
    /// <summary>
    /// update userrights in group or create a group if the group doesn't exists
    /// only the creator of the group can change userrights within the group
    /// </summary>
    /// <param name="model"></param>
    /// <exception cref="RequestException"></exception>
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

            DB.Share_GroupUser.Add(item);
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

    /// <summary>
    /// remove user from a group based on the given username and group id 
    /// only users that have the right to remove users and only the creator of the group can remove users
    /// </summary>
    /// <param name="username"></param>
    /// <param name="groupID"></param>
    /// <exception cref="RequestException"></exception>
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

    /// <summary>
    /// Get a template or entry based on the given id or 
    /// get a list of templates and entries based on the given share type
    /// </summary>
    /// <param name="id"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="RequestException"></exception>
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

    /// <summary>
    /// Change rights on the template or entry that was shared in a group or directly shared.
    /// Only the creator of the template or entry can set it to public.
    /// If the item doesn't exists, create it
    /// </summary>
    /// <param name="model"></param>
    /// <exception cref="RequestException"></exception>
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

        //wieso? das wird doch nie der fall sein, dass in der tabelle ein item drin ist, dass mit niemanden geteilt ist...
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

    /// <summary>
    /// Remove a shared template or entry from a direct or group share. 
    /// Only the creator of the template or entry can change the publicity of it 
    /// </summary>
    /// <param name="id"></param>
    /// <exception cref="RequestException"></exception>
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

    /// <summary>
    /// Return the user Id from the item creator if it exists
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="set"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="RequestException"></exception>
    private static Guid Exist_SharedItem<T>(DbSet<T> set, Guid id) where T : class, IEntity_GuidID, IEntity_User
    {
        T item = set.FirstOrDefault(s => s.ID == id)
            ?? throw new RequestException(ResultCodes.NoDataFound);
        return item.UserID;
    }
    #endregion
}
