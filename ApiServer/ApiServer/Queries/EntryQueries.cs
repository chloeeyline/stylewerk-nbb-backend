using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class EntryQueries(NbbContext DB, ApplicationUser CurrentUser) : SharedItemQueries(DB, CurrentUser)
{
    #region Folder
    public List<Model_EntryFolders> GetFolders()
    {
        List<Model_EntryFolders> entryFolders =
        [
            .. DB.Structure_Entry_Folder
                .Where(s => s.UserID == CurrentUser.ID)
                .OrderBy(s => s.SortOrder)
                .Select(s => new Model_EntryFolders(s.ID, s.Name, s.SortOrder, new Model_EntryItem[0]))
        ];

        Model_EntryItem[] result =
        [
            .. DB.Structure_Entry
                .Where(s => s.UserID == CurrentUser.ID && s.FolderID == null)
                .Include(s => s.O_Folder)
                .Include(s => s.O_Template)
                .Include(s => s.O_User)
                .Select(s => new Model_EntryItem(s, null)),
        ];
        entryFolders.Insert(0, new Model_EntryFolders(null, null, 0, result));
        return entryFolders;
    }

    public List<Model_EntryItem> GetFolderContent(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);

        List<Model_EntryItem> list =
        [
            .. DB.Structure_Entry
                .Include(s => s.O_Folder)
                .Include(s => s.O_Template)
                .Include(s => s.O_User)
                .Where(s => s.FolderID == id)
                .Select(s => new Model_EntryItem(s, null)),
        ];

        return list;
    }

    public Model_EntryFolders UpdateFolder(Model_EntryFolders? model)
    {
        if (model is null || string.IsNullOrWhiteSpace(model.Name))
            throw new RequestException(ResultCodes.DataIsInvalid);

        int sortOrder = !DB.Structure_Entry_Folder.Any() ? 1 :
            (DB.Structure_Entry_Folder.Where(s => s.UserID == CurrentUser.ID)
            .Max(f => f.SortOrder) + 1);

        if (DB.Structure_Entry_Folder.Any(s => s.UserID == CurrentUser.ID && s.Name == model.Name))
            throw new RequestException(ResultCodes.DataIsInvalid);

        Structure_Entry_Folder? item = DB.Structure_Entry_Folder.FirstOrDefault(s => s.ID == model.ID);
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

        Model_EntryFolders result = new(item.ID, item.Name, item.SortOrder, []);
        return result;
    }

    public void RemoveFolder(Guid? id)
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

    public void ReorderFolders(List<Guid>? model)
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
    #endregion

    #region Entries
    public List<Model_EntryItem> FilterEntries(Model_FilterEntry? model)
    {
        IEnumerable<Structure_Entry> Filter(IEnumerable<Structure_Entry> list)
        {
            if (!string.IsNullOrWhiteSpace(model.Name))
                list = list.Where(s => s.Name.Contains(model.Name));
            if (!string.IsNullOrWhiteSpace(model.TemplateName))
                list = list.Where(s => s.O_Template.Name.Contains(model.TemplateName));
            if (!string.IsNullOrWhiteSpace(model.Username) && !model.DirectUser)
                list = list.Where(s => s.O_User.UsernameNormalized.Contains(model.Username));
            if (!string.IsNullOrWhiteSpace(model.Username) && model.DirectUser)
                list = list.Where(s => s.O_User.UsernameNormalized == model.Username);
            if (!string.IsNullOrWhiteSpace(model.Tags))
                list = list.Where(s => !string.IsNullOrWhiteSpace(s.Tags) && model.Tags.Contains(s.Tags));
            return list.Distinct().OrderBy(s => s.LastUpdatedAt).ThenBy(s => s.Name);
        }

        List<Model_EntryItem> LoadShared(List<Model_ShareItem> shareList, ShareVisibility visibility)
        {
            List<Model_EntryItem> result = [];
            foreach (Model_ShareItem item in shareList)
            {
                IEnumerable<Structure_Entry> list = DB.Structure_Entry
                .Where(s => s.ID == item.ItemID)
                .Include(s => s.O_Folder)
                .Include(s => s.O_Template)
                .Include(s => s.O_User);

                list = Filter(list);
                result.AddRange(list.Select(s => new Model_EntryItem(s, visibility)));
            }
            return result;
        }

        if (model is null)
            throw new RequestException(ResultCodes.DataIsInvalid);
        List<Model_EntryItem> result = [];
        model = model with { Username = model.Username?.Normalize().ToLower() };

        if (string.IsNullOrWhiteSpace(model.Username))
        {
            IEnumerable<Structure_Entry> list = DB.Structure_Entry
                .Where(s => s.UserID == CurrentUser.ID)
                .Include(s => s.O_Folder)
                .Include(s => s.O_Template)
                .Include(s => s.O_User);

            list = Filter(list);
            result.AddRange(list.Select(s => new Model_EntryItem(s, null)));
        }
        if (model.Public || !string.IsNullOrWhiteSpace(model.Username))
            result.AddRange(LoadShared(PublicSharedItems(ShareType.Entry), ShareVisibility.Public));
        if (model.Group || !string.IsNullOrWhiteSpace(model.Username))
            result.AddRange(LoadShared(SharedViaGroupItems(ShareType.Entry), ShareVisibility.Group));
        if (model.Directly || !string.IsNullOrWhiteSpace(model.Username))
            result.AddRange(LoadShared(DirectlySharedItems(ShareType.Entry), ShareVisibility.Directly));

        List<Model_EntryItem> entries = [.. result
            .DistinctBy(s => s.ID)
            .OrderBy(s => s.LastUpdatedAt)];

        return entries;
    }

    public Model_DetailedEntry GetEntryFromTemplate(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);

        Structure_Template item = DB.Structure_Template.FirstOrDefault(e => e.ID == id)
            ?? throw new RequestException(ResultCodes.NoDataFound);
        List<Structure_Template_Row> itemRows = [.. DB.Structure_Template_Row
            .Where(s => s.TemplateID == item.ID)
            .OrderBy(s => s.SortOrder)];

        List<Model_EntryRow> rows = [];
        foreach (Structure_Template_Row row in itemRows)
        {
            List<Model_EntryCell> cells = [];
            List<Structure_Template_Cell> itemCells = [.. DB.Structure_Template_Cell
                .Where(s => s.RowID == row.ID)
                .OrderBy(s => s.SortOrder)];

            foreach (Structure_Template_Cell cell in itemCells)
            {
                Model_EntryCell cellModel = new(null, cell.ID, new Model_TemplateCell(cell), null);
                cells.Add(cellModel);
            }

            Model_EntryRow rowModel = new(null, row.ID, 0, new Model_TemplateRow(row), cells);
            rows.Add(rowModel);
        }

        Model_DetailedEntry result = new(null, null, item.ID, null, null, false, rows);
        return result;
    }

    public Model_DetailedEntry GetEntry(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);

        Structure_Entry item = DB.Structure_Entry.FirstOrDefault(e => e.ID == id)
            ?? throw new RequestException(ResultCodes.NoDataFound);

        List<Structure_Template_Row> itemRows = [.. DB.Structure_Template_Row
            .Where(s => s.TemplateID == item.TemplateID)
            .OrderBy(s => s.SortOrder)];

        List<Model_EntryRow> rows = [];
        foreach (Structure_Template_Row row in itemRows)
        {
            List<Model_EntryCell> cells = [];
            List<Structure_Template_Cell> itemCells = [.. DB.Structure_Template_Cell
                .Where(s => s.RowID == row.ID)
                .OrderBy(s => s.SortOrder)];

            foreach (Structure_Template_Cell cell in itemCells)
            {
                Model_EntryCell cellModel = new(null, cell.ID, new Model_TemplateCell(cell), null);
                cells.Add(cellModel);
            }

            Model_EntryRow rowModel = new(null, row.ID, 0, new Model_TemplateRow(row), cells);
            rows.Add(rowModel);
        }
        Model_DetailedEntry entryModel = new(item.ID, item.FolderID, item.TemplateID, item.Name, item.Tags, item.IsEncrypted, rows);

        return entryModel;
    }

    public Model_DetailedEntry UpdateEntry(Model_DetailedEntry? model)
    {
        if (model is null || string.IsNullOrWhiteSpace(model.Name))
            throw new RequestException(ResultCodes.DataIsInvalid);

        Structure_Entry? entry = DB.Structure_Entry.FirstOrDefault(s => s.ID == model.ID);
        if (entry is null)
        {
            if (!DB.Structure_Template.Any(s => s.ID == model.TemplateID && s.UserID == CurrentUser.ID))
                throw new RequestException(ResultCodes.DataIsInvalid);

            entry = new()
            {
                ID = Guid.NewGuid(),
                UserID = CurrentUser.ID,
                FolderID = model.FolderID,
                TemplateID = model.TemplateID,
                Name = model.Name,
                IsEncrypted = model.IsEncrypted,
            };
            DB.Structure_Entry.Add(entry);
        }
        else
        {
            if (entry.TemplateID != model.TemplateID)
                throw new RequestException(ResultCodes.DataIsInvalid);

            entry.ID = Guid.NewGuid();
            entry.UserID = CurrentUser.ID;
            entry.FolderID = model.FolderID;
            entry.Name = model.Name;
            entry.IsEncrypted = model.IsEncrypted;
        }

        Guid rowTemplateID = Guid.Empty;
        int rowSortOrder = 0;
        foreach (Model_EntryRow row in model.Items)
        {
            Structure_Entry_Row? entryRow = DB.Structure_Entry_Row.FirstOrDefault(s => s.ID == row.ID);
            if (rowTemplateID == row.TemplateID)
                rowSortOrder++;

            bool isNewRow = false;
            if (entryRow is null)
            {
                isNewRow = true;
                entryRow = new()
                {
                    ID = Guid.NewGuid(),
                    EntryID = entry.ID,
                    TemplateID = row.TemplateID,
                    SortOrder = rowSortOrder
                };
            }
            else
            {
                entryRow.SortOrder = rowSortOrder;
            }

            bool hasData = false;
            foreach (Model_EntryCell cell in row.Items)
            {
                Structure_Entry_Cell? entryCell = DB.Structure_Entry_Cell.FirstOrDefault(s => s.ID == cell.ID);
                if (entryCell is null)
                {
                    if (string.IsNullOrWhiteSpace(cell.Data))
                        continue;

                    hasData = true;
                    entryCell = new()
                    {
                        ID = Guid.NewGuid(),
                        RowID = entryRow.ID,
                        TemplateID = cell.TemplateID,
                        Data = cell.Data
                    };
                    DB.Structure_Entry_Cell.Add(entryCell);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(cell.Data))
                    {
                        DB.Structure_Entry_Cell.Remove(entryCell);
                        continue;
                    }
                    else
                    {
                        entryCell.Data = cell.Data;
                        hasData = true;
                    }
                }
            }

            if (!isNewRow && !hasData)
                DB.Structure_Entry_Row.Remove(entryRow);
            if (isNewRow && hasData)
                DB.Structure_Entry_Row.Add(entryRow);
            rowSortOrder = rowTemplateID == entryRow.TemplateID ? rowSortOrder + 1 : 0;
            rowTemplateID = entryRow.TemplateID;
        }

        Model_DetailedEntry result = GetEntry(model.ID);
        return result;
    }
    #endregion
}