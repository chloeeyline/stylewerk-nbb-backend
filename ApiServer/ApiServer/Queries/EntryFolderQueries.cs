using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Core;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class EntryFolderQueries(NbbContext DB, ApplicationUser CurrentUser) : BaseQueries(DB, CurrentUser)
{
    public List<Model_EntryFolders> List()
    {
        List<Model_EntryFolders> entryFolders =
        [
            .. DB.Structure_Entry_Folder
                .Include(s => s.O_EntryList)
                .Where(s => s.UserID == CurrentUser.ID)
                .OrderBy(s => s.SortOrder)
                .Select(s => new Model_EntryFolders(s.ID, s.Name, new Model_EntryItem[0], s.O_EntryList.Count))
        ];

        Model_EntryItem[] result =
        [
            .. DB.Structure_Entry
                .Where(s => s.UserID == CurrentUser.ID && s.FolderID == null)
                .Include(s => s.O_Template)
                .Include(s => s.O_User)
                .Select(s => new Model_EntryItem(s.ID, s.Name, s.IsPublic, s.Tags, s.CreatedAt, s.LastUpdatedAt, s.O_Template.Name, s.O_User.Username, true)),
        ];
        entryFolders.Insert(0, new Model_EntryFolders(Guid.Empty, null, result, result.Length));
        return entryFolders;
    }

    public List<Model_EntryItem> Details(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);

        Structure_Entry_Folder folderExists = DB.Structure_Entry_Folder.FirstOrDefault(s => s.ID == id && s.UserID == CurrentUser.ID)
            ?? throw new RequestException(ResultCodes.NoDataFound);

        List<Model_EntryItem> list =
        [
            .. DB.Structure_Entry
                .Include(s => s.O_Template)
                .Include(s => s.O_Folder)
                .Include(s => s.O_User)
                .Where(s => s.FolderID == id)
                .Select(s => new Model_EntryItem(s.ID, s.Name, s.IsPublic, s.Tags, s.CreatedAt, s.LastUpdatedAt, s.O_Template.Name, s.O_User.Username, true)),
        ];

        return list;
    }

    public Model_EntryFolders Update(Model_EntryFolders? model)
    {
        if (model is null || string.IsNullOrWhiteSpace(model.Name))
            throw new RequestException(ResultCodes.DataIsInvalid);

        int sortOrder = !DB.Structure_Entry_Folder.Any(s => s.UserID == CurrentUser.ID) ? 1 :
            (DB.Structure_Entry_Folder.Where(s => s.UserID == CurrentUser.ID)
            .Max(f => f.SortOrder) + 1);

        string name = model.Name.NormalizeName();
        Structure_Entry_Folder? item = DB.Structure_Entry_Folder.FirstOrDefault(s => s.ID == model.ID);

        if (item is null)
        {
            if (DB.Structure_Entry_Folder.Any(s => s.UserID == CurrentUser.ID && s.NameNormalized == name))
                throw new RequestException(ResultCodes.NameMustBeUnique);
            item = new()
            {
                ID = Guid.NewGuid(),
                Name = model.Name,
                NameNormalized = model.Name.NormalizeName(),
                SortOrder = sortOrder,
                UserID = CurrentUser.ID
            };

            DB.Structure_Entry_Folder.Add(item);
        }
        else
        {
            if (item.UserID != CurrentUser.ID)
                throw new RequestException(ResultCodes.MissingRight);
            if (item.NameNormalized != name && DB.Structure_Entry_Folder.Any(s => s.UserID == CurrentUser.ID && s.NameNormalized == name))
                throw new RequestException(ResultCodes.NameMustBeUnique);
            item.Name = model.Name;
        }

        DB.SaveChanges();

        List<Model_EntryItem> itemList = Details(item.ID);
        Model_EntryFolders result = new(item.ID, item.Name, [.. itemList], itemList.Count);
        return result;
    }

    public void Remove(Guid? id)
    {
        if (id is null)
            throw new RequestException(ResultCodes.DataIsInvalid);

        Structure_Entry_Folder folder = DB.Structure_Entry_Folder.FirstOrDefault(s => s.ID == id)
           ?? throw new RequestException(ResultCodes.NoDataFound);

        if (folder.UserID != CurrentUser.ID)
            throw new RequestException(ResultCodes.YouDontOwnTheData);

        DB.Structure_Entry_Folder.Remove(folder);
        DB.SaveChanges();
    }

    public void Reorder(List<Guid>? model)
    {
        if (model is null || model.Count == 0)
            throw new RequestException(ResultCodes.DataIsInvalid);

        int sortOrder = 1;
        foreach (Guid id in model)
        {
            if (id == Guid.Empty) continue;
            Structure_Entry_Folder folder = DB.Structure_Entry_Folder.FirstOrDefault(s => s.ID == id)
                ?? throw new RequestException(ResultCodes.NoDataFound);
            if (folder.UserID != CurrentUser.ID)
                throw new RequestException(ResultCodes.YouDontOwnTheData);
            folder.SortOrder = sortOrder++;
        }
        DB.SaveChanges();
    }
}
