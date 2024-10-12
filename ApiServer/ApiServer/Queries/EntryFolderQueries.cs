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
                .Select(s => new Model_EntryFolders(s.ID, s.Name, new Model_EntryItem[0], true, s.O_EntryList.Count, null))
        ];

        Model_EntryItem[] result =
        [
            .. DB.Structure_Entry
                .Where(s => s.UserID == CurrentUser.ID && s.FolderID == null)
                .Include(s => s.O_Template)
                .Include(s => s.O_User)
                .Select(s => new Model_EntryItem(s.ID, Guid.Empty, s.Name, s.IsEncrypted, s.IsPublic, s.Tags, s.CreatedAt, s.LastUpdatedAt, s.O_Template.Name, s.O_User.Username)),
        ];
        entryFolders.Insert(0, new Model_EntryFolders(Guid.Empty, null, result, true, result.Length, null));
        return entryFolders;
    }

    public List<Model_EntryFolders> List(string? name, string? username, string? templateName, string? tags, bool? includePublic)
    {
        username = username?.Normalize().ToLower();
        tags = tags?.Normalize().ToLower();

        IQueryable<Structure_Entry> query = DB.Structure_Entry
            .Include(s => s.O_User)
            .Include(s => s.O_Template)
            .Include(s => s.O_Folder);

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(s => s.Name.Contains(name));

        if (!string.IsNullOrWhiteSpace(templateName))
            query = query.Where(s => s.O_Template.Name.Contains(templateName));

        if (!string.IsNullOrWhiteSpace(tags))
            query = query.Where(s => !string.IsNullOrWhiteSpace(s.Tags) && s.Tags.Contains(tags));

        if (!string.IsNullOrWhiteSpace(username))
            query = query.Where(s => s.O_User.UsernameNormalized.Contains(username));

        if (includePublic is true)
            query = query.Where(s => s.IsPublic);

        if (string.IsNullOrWhiteSpace(username) && includePublic is not true)
            query = query.Where(s => s.O_User.UsernameNormalized == username);

        List<Structure_Entry> list = [.. query.OrderBy(s => s.O_User.UsernameNormalized)
            .ThenBy(s => s.O_Folder == null ? "" : s.O_Folder.NameNormalized)
            .ThenBy(s => s.LastUpdatedAt)];

        Guid? previousFolderID = null;
        Guid previousUserID = Guid.Empty;
        List<Model_EntryItem> itemList = [];
        List<Model_EntryFolders> folderList = [];
        foreach (Structure_Entry entry in list)
        {
            itemList.Add(new Model_EntryItem(entry.ID, entry.FolderID ?? Guid.Empty, entry.Name, entry.IsEncrypted, entry.IsPublic, entry.Tags, entry.CreatedAt, entry.LastUpdatedAt, entry.O_Template.Name, entry.O_User.Username));

            if (previousFolderID != (entry.FolderID ?? Guid.Empty) || previousUserID != entry.UserID)
            {
                int totalCount = DB.Structure_Entry_Folder.Count(s => s.UserID == previousUserID);
                folderList.Add(new Model_EntryFolders(entry.FolderID ?? Guid.Empty, entry.O_Folder?.Name ?? "", [.. itemList], CurrentUser.ID == previousUserID, totalCount, itemList.Count));
                itemList.Clear();
            }
            previousFolderID = entry.FolderID;
            previousUserID = entry.UserID;
        }

        return folderList;
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
                .Select(s => new Model_EntryItem(s.ID, s.FolderID ?? Guid.Empty, s.Name, s.IsEncrypted, s.IsPublic, s.Tags, s.CreatedAt, s.LastUpdatedAt, s.O_Template.Name, s.O_User.Username)),
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
        Model_EntryFolders result = new(item.ID, item.Name, [.. itemList], true, itemList.Count, null);
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
