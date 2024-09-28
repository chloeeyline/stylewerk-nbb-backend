using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Authentication;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Database.User;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class ShareQueries(NbbContext DB, ApplicationUser CurrentUser) : SharedItemQueries(DB, CurrentUser)
{
    public List<Models.Model_Group> LoadUserGroups()
    {
        List<Models.Model_Group> groups = [.. DB.Share_Group
            .Include(g => g.O_GroupUser)
            .Where(g => g.UserID == CurrentUser.ID)
            .Select(g => new Models.Model_Group(g.ID, g.Name, g.IsVisible, g.CanSeeOthers,
            g.O_GroupUser.Select(u => new Model_GroupUser(u.O_User.Username, u.GroupID, new GroupUserRights(u.CanSeeUsers, u.CanAddUsers, u.CanRemoveUsers))).ToArray()))];

        return groups;
    }

    //groups that the user is in
    public List<Models.Model_Group> LoadGroups()
    {
        List<Share_GroupUser> groups = [.. DB.Share_GroupUser.Where(g => g.UserID == CurrentUser.ID)];

        List<Models.Model_Group> result = [.. DB.Share_Group
            .Where(g => groups.Any(u => u.GroupID == g.ID))
            .Select(g => new Models.Model_Group(g.ID, g.Name, g.IsVisible, g.CanSeeOthers, new Model_GroupUser[0]))];

        return result;
    }

    public List<Model_User> LoadUsers()
    {
        List<Model_User> users = [.. DB.User_Information.Select(u => new Model_User(u.ID, u.O_User.Username))];

        return users;
    }

    public Model_ShareItemRightsUser ShareItemUserRights(User_Login user, Guid shareItem)
    {
        Share_Item? rights = DB.Share_Item.FirstOrDefault(i => i.ID == shareItem && i.ToWhom == user.ID)
            ?? throw new RequestException(ResultCodes.NoDataFound);
        Model_ShareItemRightsUser itemRights = new(rights.ID, user.Username, new ShareRights(rights.CanShare, rights.CanEdit, rights.CanDelete));
        return itemRights;
    }

    public Model_ShareItemRightsGroup ShareItemGroupRights(Share_Group group, Guid shareItem)
    {
        Share_Item? rights = DB.Share_Item.FirstOrDefault(i => i.ID == shareItem && i.ToWhom == group.ID)
            ?? throw new RequestException(ResultCodes.NoDataFound);
        Model_ShareItemRightsGroup itemRights = new(rights.ID, rights.ToWhom, new ShareRights(rights.CanShare, rights.CanEdit, rights.CanDelete));
        return itemRights;
    }
}


