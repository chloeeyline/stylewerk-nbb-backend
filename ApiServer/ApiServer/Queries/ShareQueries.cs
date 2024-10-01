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
    /// Gets to whom this item is shared
    /// </summary>
    /// <param name="id"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="RequestException"></exception>
    public List<Model_ShareItem> List(Guid? id, ShareType? type)
    {
        if (id is null || id == Guid.Empty || !type.HasValue)
            throw new RequestException(ResultCodes.DataIsInvalid);

        List<Share_Item> list = [.. DB.Share_Item.Where(s => s.ItemID == id && s.Type == type)];
        List<Model_ShareItem> result = [];

        foreach (Share_Item item in list)
        {
            Guid userID = type == ShareType.Entry
               ? Exist_SharedItem(DB.Structure_Entry, item.ItemID)
               : type == ShareType.Template
               ? Exist_SharedItem(DB.Structure_Template, item.ItemID)
               : throw new RequestException(ResultCodes.DataIsInvalid);

            User_Login owner = DB.User_Login.FirstOrDefault(s => s.ID == userID)
                    ?? throw new RequestException(ResultCodes.NoDataFound);

            if (item.Visibility is ShareVisibility.Public)
            {
                DB.Share_Item.RemoveRange(DB.Share_Item.Where(s => s.ItemID == item.ID));
                result.Add(new Model_ShareItem(item.ID, item.ItemID, owner.Username, item.Visibility, type.Value, ""));
            }
            else if (item.Visibility is ShareVisibility.Directly)
            {
                User_Login user = DB.User_Login.FirstOrDefault(s => s.ID == item.ToWhom)
                    ?? throw new RequestException(ResultCodes.NoDataFound);

                if (userID == CurrentUser.ID)
                    result.Add(new Model_ShareItem(item.ID, item.ItemID, owner.Username, item.Visibility, type.Value, user.Username));
            }
            else if (item.Visibility is ShareVisibility.Group)
            {
                Share_Group group = DB.Share_Group.FirstOrDefault(s => s.ID == item.ToWhom)
                    ?? throw new RequestException(ResultCodes.NoDataFound);

                if (userID == CurrentUser.ID || group.UserID == CurrentUser.ID)
                    result.Add(new Model_ShareItem(item.ID, item.ItemID, owner.Username, item.Visibility, type.Value, group.Name));
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
    public void Update(Model_ShareItem? model)
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
            if (user.ID == CurrentUser.ID)
                throw new RequestException(ResultCodes.CantShareWithYourself);
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

        Guid userID = GetOwnerID(item.ID, item.Type);

        if (userID != CurrentUser.ID)
            throw new RequestException(ResultCodes.YouDontOwnTheData);

        DB.Share_Item.Remove(item);
        DB.SaveChanges();
    }

    private Guid GetOwnerID(Guid id, ShareType type)
    {
        Guid userID = type == ShareType.Entry
                ? Exist_SharedItem(DB.Structure_Entry, id)
                : type == ShareType.Template
                ? Exist_SharedItem(DB.Structure_Template, id)
                : throw new RequestException(ResultCodes.DataIsInvalid);
        return userID;
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
