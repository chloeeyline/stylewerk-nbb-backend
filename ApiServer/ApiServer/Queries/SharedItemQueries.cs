using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Authentication;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class SharedItemQueries(NbbContext DB, ApplicationUser CurrentUser) : BaseQueries(DB, CurrentUser)
{
    public List<Model_ShareItem> DirectlySharedItems(ShareType itemType)
    {
        List<Model_ShareItem> list =
        [
            .. DB.Share_Item.Where(s => s.Visibility == ShareVisibility.Directly && s.ToWhom == CurrentUser.ID && s.Type == itemType)
                .Include(s => s.O_User)
                .Select(item => new Model_ShareItem(
                    item.ID,
                    item.ItemID,
                    item.O_User.Username,
                    CurrentUser.Login.Username,
                    item.Visibility,
                    item.CanShare,
                    item.CanEdit,
                    item.CanDelete)),
        ];
        return list;
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
                Model_ShareItem model = new(
                    item.ID,
                    item.ItemID,
                    item.O_User.Username,
                    groupItem.Name,
                    item.Visibility,
                    item.CanShare,
                    item.CanEdit,
                    item.CanDelete);
                result.Add(model);
            }
        }

        return result;
    }
}