using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Database.User;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class ShareGroupQueries(NbbContext DB, ApplicationUser CurrentUser) : BaseQueries(DB, CurrentUser)
{
    /// <summary>
    /// get all groups that the current user has created
    /// </summary>
    /// <returns></returns>
    public List<Model_Group> List()
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
    public List<string> Details(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);

        List<string> list = [..
            DB.Share_GroupUser
            .Where(s => s.GroupID == id)
            .Include(s => s.O_User)
            .Select(s => s.O_User.Username)];
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
    public Model_Group Update(Model_Group? model)
    {
        if (model is null)
            throw new RequestException(ResultCodes.DataIsInvalid);

        Share_Group? item = DB.Share_Group.FirstOrDefault(s => s.ID == model.ID);

        if (item is null)
        {
            if (DB.Share_Group.Any(s => s.Name.Equals(model.Name, StringComparison.CurrentCultureIgnoreCase)))
                throw new RequestException(ResultCodes.NameMustBeUnique);

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
                throw new RequestException(ResultCodes.YouDontOwnTheData);

            if (!item.Name.Equals(model.Name, StringComparison.OrdinalIgnoreCase) &&
                DB.Share_Group.Any(s => s.Name.Equals(model.Name, StringComparison.CurrentCultureIgnoreCase)))
                throw new RequestException(ResultCodes.NameMustBeUnique);

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
    public void Remove(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);

        Share_Group item = DB.Share_Group.FirstOrDefault(s => s.ID == id) ??
            throw new RequestException(ResultCodes.NoDataFound);

        if (item.UserID != CurrentUser.ID)
            throw new RequestException(ResultCodes.YouDontOwnTheData);

        DB.Share_Group.Remove(item);
        DB.Share_Item.RemoveRange(DB.Share_Item.Where(s => s.Visibility == ShareVisibility.Group && s.ToWhom == item.ID));
        DB.SaveChanges();
    }

    /// <summary>
    /// update userrights in group or create a group if the group doesn't exists
    /// only the creator of the group can change userrights within the group
    /// </summary>
    /// <param name="model"></param>
    /// <exception cref="RequestException"></exception>
    public void UpdateUser(Model_GroupUser? model)
    {
        if (model is null ||
            model.GroupID == Guid.Empty ||
            string.IsNullOrWhiteSpace(model.Username))
            throw new RequestException(ResultCodes.DataIsInvalid);

        Share_Group group = DB.Share_Group.FirstOrDefault(s => s.ID == model.GroupID) ??
            throw new RequestException(ResultCodes.NoDataFound);

        model = model with { Username = model.Username.ToLower().Normalize() };
        User_Login? user = DB.User_Login.FirstOrDefault(s => s.UsernameNormalized == model.Username) ??
            throw new RequestException(ResultCodes.NoUserFound);

        if (user.ID == CurrentUser.ID)
            throw new RequestException(ResultCodes.CantShareWithYourself);

        Share_GroupUser? item = DB.Share_GroupUser.Include(s => s.O_User)
            .FirstOrDefault(s => s.GroupID == group.ID && s.O_User.UsernameNormalized == model.Username);

        if (item is null)
        {
            item = new Share_GroupUser()
            {
                GroupID = group.ID,
                UserID = user.ID,
            };

            DB.Share_GroupUser.Add(item);
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
    public void RemoveUser(string? username, Guid? groupID)
    {
        if (groupID is null ||
            groupID == Guid.Empty ||
            string.IsNullOrWhiteSpace(username))
            throw new RequestException(ResultCodes.DataIsInvalid);

        Share_Group group = DB.Share_Group.FirstOrDefault(s => s.ID == groupID) ??
            throw new RequestException(ResultCodes.NoDataFound);

        username = username.ToLower().Normalize();
        User_Login? user = DB.User_Login.FirstOrDefault(s => s.UsernameNormalized == username) ??
            throw new RequestException(ResultCodes.NoUserFound);

        Share_GroupUser item = DB.Share_GroupUser.Include(s => s.O_User)
            .FirstOrDefault(s => s.GroupID == group.ID && s.UserID == user.ID) ??
            throw new RequestException(ResultCodes.NoDataFound);

        if (group.UserID != CurrentUser.ID)
            throw new RequestException(ResultCodes.YouDontOwnTheData);

        DB.Share_GroupUser.Remove(item);
        DB.SaveChanges();
    }
}
