using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class EntryFolderQueries(NbbContext DB, ApplicationUser CurrentUser) : BaseQueries(DB, CurrentUser)
{
    /// <summary>
    /// Get all folders and all entries which arent't in a folder, that belong to the current user
    /// </summary>
    /// <returns></returns>
    public List<Model_EntryFolders> List()
    {
        List<Model_EntryFolders> entryFolders =
        [
            .. DB.Structure_Entry_Folder
                .Where(s => s.UserID == CurrentUser.ID)
                .OrderBy(s => s.SortOrder)
                .Select(s => new Model_EntryFolders(s.ID, s.Name, new Model_EntryItem[0]))
        ];

        Model_EntryItem[] result =
        [
            .. DB.Structure_Entry
                .Where(s => s.UserID == CurrentUser.ID && s.FolderID == null)
                .Include(s => s.O_Folder)
                .Include(s => s.O_Template)
                .Include(s => s.O_User)
                .Select(s => new Model_EntryItem(s.ID, s.Name, s.O_User.Username, s.O_Template.Name, s.Tags, s.CreatedAt, s.LastUpdatedAt)),
        ];
        entryFolders.Insert(0, new Model_EntryFolders(null, null, result));
        return entryFolders;
    }

    /// <summary>
    /// Get all entries in a folder specified by the given folder id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="RequestException"></exception>
    public List<Model_EntryItem> Details(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);

        Structure_Entry_Folder folderExists = DB.Structure_Entry_Folder.FirstOrDefault(s => s.ID == id && s.UserID == CurrentUser.ID)
            ?? throw new RequestException(ResultCodes.NoDataFound);

        List<Model_EntryItem> list =
        [
            .. DB.Structure_Entry
                .Include(s => s.O_Folder)
                .Include(s => s.O_Template)
                .Include(s => s.O_User)
                .Where(s => s.FolderID == id)
                .Select(s => new Model_EntryItem(s.ID, s.Name, s.O_User.Username, s.O_Template.Name, s.Tags, s.CreatedAt, s.LastUpdatedAt)),
        ];

        return list;
    }

    /// <summary>
    /// update or add folder
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    /// <exception cref="RequestException"></exception>
    public Model_EntryFolders Update(Model_EntryFolders? model)
    {
        if (model is null || string.IsNullOrWhiteSpace(model.Name))
            throw new RequestException(ResultCodes.DataIsInvalid);

        int sortOrder = !DB.Structure_Entry_Folder.Any() ? 1 :
            (DB.Structure_Entry_Folder.Where(s => s.UserID == CurrentUser.ID)
            .Max(f => f.SortOrder) + 1);

        if (DB.Structure_Entry_Folder.Any(s => s.UserID == CurrentUser.ID && s.Name == model.Name))
            throw new RequestException(ResultCodes.DataIsInvalid);

        Structure_Entry_Folder? item = DB.Structure_Entry_Folder.FirstOrDefault(s => s.ID == model.ID);
        if (DB.Structure_Entry_Folder.Any(s => s.UserID == CurrentUser.ID && s.Name == model.Name))
            throw new RequestException(ResultCodes.FolderNameAlreadyExists);

        if (item is null)
        {
            item = new()
            {
                ID = Guid.NewGuid(),
                Name = model.Name,
                SortOrder = sortOrder,
                UserID = CurrentUser.ID
            };

            DB.Structure_Entry_Folder.Add(item);
        }
        else
        {
            if (item.UserID != CurrentUser.ID)
                throw new RequestException(ResultCodes.MissingRight);

            item.Name = model.Name;
        }

        DB.SaveChanges();

        Model_EntryFolders result = new(item.ID, item.Name, []);
        return result;
    }

    /// <summary>
    /// remove a folder based on the given folder id
    /// </summary>
    /// <param name="id"></param>
    /// <exception cref="RequestException"></exception>
    public void Remove(Guid? id)
    {
        if (id is null)
            throw new RequestException(ResultCodes.DataIsInvalid);

        Structure_Entry_Folder folder = DB.Structure_Entry_Folder.FirstOrDefault(s => s.ID == id)
           ?? throw new RequestException(ResultCodes.NoDataFound);

        List<Structure_Entry> entries = [.. DB.Structure_Entry.Where(s => s.FolderID == id)];
        foreach (Structure_Entry? entry in entries)
            entry.FolderID = null;

        DB.Structure_Entry_Folder.Remove(folder);
        DB.SaveChanges();
    }

    /// <summary>
    /// reorder folders based on the given folder ids in a list
    /// </summary>
    /// <param name="model"></param>
    /// <exception cref="RequestException"></exception>
    public void Reorder(List<Guid>? model)
    {
        if (model is null || model.Count == 0)
            throw new RequestException(ResultCodes.DataIsInvalid);

        int sortOrder = 1;
        foreach (Guid id in model)
        {
            Structure_Entry_Folder temp = DB.Structure_Entry_Folder.FirstOrDefault(s => s.ID == id)
                ?? throw new RequestException(ResultCodes.NoDataFound);
            temp.SortOrder = sortOrder++;
        }
        DB.SaveChanges();
    }
}
