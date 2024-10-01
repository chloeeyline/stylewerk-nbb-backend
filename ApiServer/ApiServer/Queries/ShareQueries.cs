using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Core;
using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Database.User;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class ShareQueries(NbbContext DB, ApplicationUser CurrentUser) : BaseQueries(DB, CurrentUser)
{
    /// <summary>
    /// Get a template or entry based on the given id or 
    /// get a list of templates and entries based on the given share type
    /// </summary>
    /// <param name="id"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="RequestException"></exception>
    public List<Model_ShareItem> List(Guid? id, ShareType? type)
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
    public void Update(Model_Share? model)
    {
        if (model is null)
            throw new RequestException(ResultCodes.DataIsInvalid);

        Guid userID = model.Type == ShareType.Entry
            ? Exist_SharedItem(DB.Structure_Entry, model.ID)
            : model.Type == ShareType.Template
            ? Exist_SharedItem(DB.Structure_Template, model.ID)
            : throw new RequestException(ResultCodes.DataIsInvalid);

        if (userID != CurrentUser.ID)
            throw new RequestException(ResultCodes.YouDontOwnTheData);

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
                ToWhom = toWhom
            };

            DB.Share_Item.Add(item);
        }
        DB.SaveChanges();
    }

    /// <summary>
    /// Remove a shared template or entry from a direct or group share. 
    /// Only the creator of the template or entry can change the publicity of it 
    /// </summary>
    /// <param name="id"></param>
    /// <exception cref="RequestException"></exception>
    public void Remove(Guid? id)
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
                throw new RequestException(ResultCodes.YouDontOwnTheData);
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
}
