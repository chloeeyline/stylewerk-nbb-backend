using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Authentication;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Database.User;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class SharedItemQueries(NbbContext DB, ApplicationUser CurrentUser) : BaseQueries(DB, CurrentUser)
{
    public List<Model_SharedItem> DirectlySharedItems(int itemType)
    {
        List<Share_Item> sharedItems = [.. DB.Share_Item.Where(s => s.Visibility == ShareVisibility.Directly && s.ToWhom == CurrentUser.ID && s.ItemType == itemType)];
        List<Model_SharedItem> result = [];
        foreach (Share_Item item in sharedItems)
        {
            User_Login? userWhoShared = DB.User_Login.FirstOrDefault(s => s.ID == item.UserID);
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
            List<Share_Item> shareItem = [.. DB.Share_Item.Where(s => s.Visibility == ShareVisibility.Group && s.ToWhom == groupItem.ID && s.ItemType == itemType)];

            foreach (Share_Item? item in shareItem)
            {
                User_Login? userWhoShared = DB.User_Login.FirstOrDefault(s => s.ID == item.UserID);
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
}