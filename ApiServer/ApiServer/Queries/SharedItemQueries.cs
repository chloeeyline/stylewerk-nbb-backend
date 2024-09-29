using Microsoft.EntityFrameworkCore;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class SharedItemQueries(NbbContext DB, ApplicationUser CurrentUser) : BaseQueries(DB, CurrentUser)
{
    /// <summary>
    /// get all directly shared templates or entries for the current user
    /// directly shared template or entries can only be seen by the specified user that the template or entry was shard with
    /// </summary>
    /// <param name="itemType"></param>
    /// <returns></returns>
    public List<Model_ShareItem> DirectlySharedItems(ShareType itemType)
    {
        List<Model_ShareItem> list =
        [
            .. DB.Share_Item.Where(s => s.Visibility == ShareVisibility.Directly && s.ToWhom == CurrentUser.ID && s.Type == itemType)
                .Include(s => s.O_User)
                .Select(item => new Model_ShareItem(item, CurrentUser.Login.Username)),
        ];
        return list;
    }

    /// <summary>
    /// get all public shared templates or entries
    /// public shared templates or entries can be seen by everyone
    /// </summary>
    /// <param name="itemType"></param>
    /// <returns></returns>
    public List<Model_ShareItem> PublicSharedItems(ShareType itemType)
    {
        List<Model_ShareItem> list =
        [
            .. DB.Share_Item.Where(s => s.Visibility == ShareVisibility.Public && s.Type == itemType)
                .Include(s => s.O_User)
                .Select(item => new Model_ShareItem(item, CurrentUser.Login.Username)),
        ];
        return list;
    }

    /// <summary>
    /// get all templates or entries that were shared in a group the current user is part of
    /// only the group can see the template or entry that was shared
    /// </summary>
    /// <param name="itemType"></param>
    /// <returns></returns>
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
                Model_ShareItem model = new(item, groupItem.Name);
                result.Add(model);
            }
        }

        return result;
    }
}