using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Authentication;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Database.User;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class ShareQueries(NbbContext DB, ApplicationUser CurrentUser) : BaseQueries(DB, CurrentUser)
{
    public List<Model_SharedItem> DirectlySharedItems(int itemType)
    {
        List<Share_Item> sharedItems = [.. DB.Share_Item.Where(s => !s.Group && s.ToWhom == CurrentUser.ID && s.ItemType == itemType)];
        List<Model_SharedItem> result = [];
        foreach (Share_Item item in sharedItems)
        {
            User_Login? userWhoShared = DB.User_Login.FirstOrDefault(s => s.ID == item.WhoShared);
            if (userWhoShared is not null)
            {
                Model_SharedItem model = new(
                    item.ItemID,
                    userWhoShared.Username,
                    false,
                    null,
                    null,
                    item.CanShare,
                    item.CanEdit,
                    item.CanDelete);
                result.Add(model);
            }
        }
        return result;
    }

    public List<Model_SharedItem> SharedViaGroupItems(int itemType)
    {
        List<Share_Group> groupPartof =
        [
            .. DB.Share_GroupUser
                .Include(u => u.O_Group)
                .Where(u => u.UserID == CurrentUser.ID)
                .Select(g => g.O_Group),
        ];
        List<Model_SharedItem> result = [];

        foreach (Share_Group? groupItem in groupPartof)
        {
            List<Share_Item> shareItem = [.. DB.Share_Item.Where(s => s.Group && s.ToWhom == groupItem.ID && s.ItemType == itemType)];

            foreach (Share_Item? item in shareItem)
            {
                User_Login? userWhoShared = DB.User_Login.FirstOrDefault(s => s.ID == item.WhoShared);
                if (userWhoShared is not null)
                {
                    Guid? groupID = groupItem.IsVisible ? groupItem.ID : null;
                    string? groupName = groupItem.IsVisible ? groupItem.Name : null;
                    Model_SharedItem model = new(
                        item.ItemID,
                        userWhoShared.Username,
                        true,
                        groupID,
                        groupName,
                        item.CanShare,
                        item.CanEdit,
                        item.CanDelete);
                    result.Add(model);
                }
            }
        }

        return result;
    }

    public List<Model_Group> LoadUserGroups()
    {
        List<Model_Group> groups = [.. DB.Share_Group
            .Include(g => g.O_GroupUser)
            .Where(g => g.UserID == CurrentUser.ID)
            .Select(g => new Model_Group(g.ID, g.Name, g.IsVisible, g.CanSeeOthers,
            g.O_GroupUser.Select(u => new Model_GroupUser(u.O_User.Username, u.GroupID, new GroupUserRights(u.CanSeeUsers, u.CanAddUsers, u.CanRemoveUsers))).ToArray()))];

        return groups;
    }

    //groups that the user is in
    public List<Model_Group> LoadGroups()
    {
        List<Share_GroupUser> groups = [.. DB.Share_GroupUser.Where(g => g.UserID == CurrentUser.ID)];

        List<Model_Group> result = [ .. DB.Share_Group
            .Where(g => groups.Any(u => u.GroupID == g.ID))
            .Select(g => new Model_Group(g.ID, g.Name, g.IsVisible, g.CanSeeOthers, new Model_GroupUser[0]))];

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


