using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Authentication;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class ShareNewQueries(NbbContext DB, ApplicationUser CurrentUser) : SharedItemQueries(DB, CurrentUser)
{
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
            if (DB.Share_Group.Any(s => s.Name.Equals(model.Name, StringComparison.CurrentCultureIgnoreCase)))
                throw new RequestException(ResultCodes.GroupNameAlreadyExists);

            item = new Share_Group()
            {
                ID = Guid.NewGuid(),
                UserID = CurrentUser.ID,
                Name = model.Name,
                IsVisible = model.IsVisible,
                CanSeeOthers = model.CanSeeOthers
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
            item.IsVisible = model.IsVisible;
            item.CanSeeOthers = model.CanSeeOthers;
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
        DB.Share_Item.RemoveRange(DB.Share_Item.Where(s => s.Group && s.ToWhom == item.ID));
    }
}

public record Model_UpdateGroup2(Guid ID, string Name, bool IsVisible, bool CanSeeOthers);

public record Model_Group2(Guid ID, string Name, string Username, bool IsVisible, bool CanSeeOthers, int UserCount)
{
    public Model_Group2(Share_Group item) :
        this(item.ID, item.Name, item.O_User.Username, item.IsVisible, item.CanSeeOthers, item.O_GroupUser.Count)
    { }
}

public record Model_GroupUser2(string Username, string WhoAdded, bool CanSeeUsers, bool CanAddUsers, bool CanRemoveUsers)
{
    public Model_GroupUser2(Share_GroupUser item) :
        this(item.O_User.Username, item.O_WhoShared.Username, item.CanSeeUsers, item.CanAddUsers, item.CanRemoveUsers)
    { }
}
