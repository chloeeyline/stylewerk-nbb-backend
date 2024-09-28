using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Authentication;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Database.User;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class SharedItemQueries(NbbContext DB, ApplicationUser CurrentUser) : BaseQueries(DB, CurrentUser)
{
    public List<Model_ShareItem> DirectlySharedItems(ShareType itemType)
    {
        List<Share_Item> sharedItems = [.. DB.Share_Item.Where(s => s.Visibility == ShareVisibility.Directly && s.ToWhom == CurrentUser.ID && s.Type == itemType).Include(s => s.O_User)];
        List<Model_ShareItem> result = [];
        foreach (Share_Item item in sharedItems)
        {
            User_Login? userWhoShared = DB.User_Login.FirstOrDefault(s => s.ID == item.UserID);
            if (userWhoShared is not null)
            {
                Model_ShareItem model = new(
                    item.ID,
                    item.ItemID,
                    item.O_User.Username,
                    CurrentUser.Login.Username,
                    item.Visibility,
                    new ShareRight(
                    item.CanShare,
                    item.CanEdit,
                    item.CanDelete));
                result.Add(model);
            }
        }
        return result;
    }

    public List<Model_ShareItem> SharedViaGroupItems(ShareType itemType)
    {
        List<Share_Group> groupPartof =
        [
            .. DB.Share_GroupUser
                .Include(u => u.O_Group)
                .Where(u => u.UserID == CurrentUser.ID)
                .Select(g => g.O_Group),
        ];
        List<Model_ShareItem> result = [];

        foreach (Share_Group? groupItem in groupPartof)
        {
            List<Share_Item> shareItem = [.. DB.Share_Item.Where(s => s.Visibility == ShareVisibility.Group && s.ToWhom == groupItem.ID && s.Type == itemType)];

            foreach (Share_Item? item in shareItem)
            {
                User_Login? userWhoShared = DB.User_Login.FirstOrDefault(s => s.ID == item.UserID);
                if (userWhoShared is not null)
                {
                    Model_ShareItem model = new(
                        item.ID,
                        item.ItemID,
                        item.O_User.Username,
                        groupItem.Name,
                        item.Visibility,
                        new ShareRight(
                        item.CanShare,
                        item.CanEdit,
                        item.CanDelete));
                    result.Add(model);
                }
            }
        }

        return result;
    }
}