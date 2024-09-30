﻿using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class EntryQueries(NbbContext DB, ApplicationUser CurrentUser) : BaseQueries(DB, CurrentUser)
{
    /// <summary>
    /// Load all Entries that are available for User and filter them by the specified filters
    /// </summary>
    public List<Model_EntryFilterItem> List(string? name, string? username, string? templateName, string? tags, bool? publicShared, bool? groupShared, bool? directlyShared, bool? directUser)
    {
        // Normalize the username for comparison
        username = username?.Normalize().ToLower();

        // Define the query with filters applied before the select
        var query =
        from si in DB.Share_Item
        where si.Type == ShareType.Entry
        join entry in DB.Structure_Entry on
            si.ItemID equals entry.ID
        join whoShared in DB.User_Login on
            si.UserID equals whoShared.ID
        join owner in DB.User_Login on
            entry.UserID equals owner.ID
        join folder in DB.Structure_Entry_Folder on
            entry.FolderID equals folder.ID into folderJoin
        from folderData in folderJoin.DefaultIfEmpty()
        join template in DB.Structure_Template on
            entry.TemplateID equals template.ID
        join sgu in DB.Share_GroupUser on
            new { si.ToWhom, si.Visibility } equals
            new { ToWhom = (Guid?) sgu.GroupID, Visibility = ShareVisibility.Group }
            into groupJoin
        from sharedGroup in groupJoin.DefaultIfEmpty()
        join sg in DB.Share_Group on
            sharedGroup.GroupID equals sg.ID into groupDataJoin
        from groupData in groupDataJoin.DefaultIfEmpty()
        where (si.ToWhom == CurrentUser.ID || sharedGroup.UserID == CurrentUser.ID)
        select new
        {
            entry,
            si.Visibility,
            ownerUsername = owner.Username,
            ownerUsernameNormalized = owner.UsernameNormalized,
            templateName = template.Name,
            si.CanShare,
            si.CanEdit,
            si.CanDelete,
            folderName = folderData.Name,
            whoSharedUsername = whoShared.Username,
            whoSharedUsernameNormalized = whoShared.UsernameNormalized,
            groupName = groupData.Name
        };

        // Query to get entries owned by the current user
        var ownedQuery =
        from entry in DB.Structure_Entry
        join owner in DB.User_Login on
            entry.UserID equals owner.ID
        join folder in DB.Structure_Entry_Folder on
            entry.FolderID equals folder.ID into folderJoin
        from folderData in folderJoin.DefaultIfEmpty()
        join template in DB.Structure_Template on
            entry.TemplateID equals template.ID
        where entry.UserID == CurrentUser.ID
        select new
        {
            entry,
            Visibility = ShareVisibility.None,
            ownerUsername = owner.Username,
            ownerUsernameNormalized = owner.UsernameNormalized,
            templateName = template.Name,
            CanShare = true,
            CanEdit = true,
            CanDelete = true,
            folderName = folderData.Name,
            whoSharedUsername = (string?) null,
            whoSharedUsernameNormalized = (string?) null,
            groupName = (string?) null,
        };

        // Combine the two queries (shared + owned) using Union
        query = query.Union(ownedQuery);

        // Apply visibility filters based on the model
        query = from s in query
                where
                (publicShared == true && s.Visibility == ShareVisibility.Public) ||
                (groupShared == true && s.Visibility == ShareVisibility.Group) ||
                (directlyShared == true && s.Visibility == ShareVisibility.Directly) ||
                s.Visibility == ShareVisibility.None
                select s;

        // Apply filters prior to select
        if (!string.IsNullOrWhiteSpace(name))
            query = from s in query
                    where s.entry.Name.Contains(name)
                    select s;

        if (!string.IsNullOrWhiteSpace(templateName))
            query = from s in query
                    where s.templateName.Contains(templateName)
                    select s;

        if (!string.IsNullOrWhiteSpace(tags))
            query = from s in query
                    where !string.IsNullOrWhiteSpace(s.entry.Tags) && tags.Contains(s.entry.Tags)
                    select s;

        if (!string.IsNullOrWhiteSpace(username) && directUser is false)
            query = from s in query
                    where s.ownerUsernameNormalized.Contains(username)
                    select s;

        if (!string.IsNullOrWhiteSpace(username) && directUser is true)
            query = from s in query
                    where s.ownerUsernameNormalized == username
                    select s;

        // Apply ordering before final select
        query = from s in query
                orderby s.Visibility, s.entry.LastUpdatedAt, s.entry.Name
                select s;

        // Final select to map data to ShareEntryResult
        IQueryable<Model_EntryFilterItem> finalQuery = query.Select(s => new Model_EntryFilterItem
        (
            s.entry.ID,
            s.entry.Name,
            s.entry.IsEncrypted,
            s.entry.Tags,
            s.entry.CreatedAt,
            s.entry.LastUpdatedAt,
            s.templateName,
            s.ownerUsername,
            s.Visibility,
            s.CanShare,
            s.CanEdit,
            s.CanDelete,
            s.folderName,
            s.whoSharedUsername,
            s.groupName
        ));

        // Execute the query and return distinct results
        return [.. finalQuery];
    }

    /// <summary>
    /// Get entry details based on entry id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="RequestException"></exception>
    public Model_Entry Details(Guid? id)
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
                Model_TemplateCell cellModelTemplate = new(cell.ID, cell.RowID, cell.InputHelper, cell.HideOnEmpty, cell.IsRequired, cell.Text, cell.MetaData);
                Model_EntryCell cellModel = new(null, cell.ID, cellModelTemplate, null);
                cells.Add(cellModel);
            }

            Model_TemplateRow rowModeltempalte = new(row.ID, row.CanWrapCells, row.CanRepeat, row.HideOnNoInput, []);
            Model_EntryRow rowModel = new(null, row.ID, 0, rowModeltempalte, cells);
            rows.Add(rowModel);
        }
        Model_Entry entryModel = new(item.ID, item.FolderID, item.TemplateID, item.Name, item.Tags, item.IsEncrypted, rows);

        return entryModel;
    }

    /// <summary>
    /// Get template rows and cells to use for an entry based on the given tempate Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="RequestException"></exception>
    public Model_Entry GetFromTemplate(Guid? id)
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
                Model_TemplateCell cellModelTemplate = new(cell.ID, cell.RowID, cell.InputHelper, cell.HideOnEmpty, cell.IsRequired, cell.Text, cell.MetaData);
                Model_EntryCell cellModel = new(null, cell.ID, cellModelTemplate, null);
                cells.Add(cellModel);
            }

            Model_EntryRow rowModel = new(null, row.ID, 0, new Model_TemplateRow(row.ID, row.CanWrapCells, row.CanRepeat, row.HideOnNoInput, []), cells);
            rows.Add(rowModel);
        }

        Model_Entry result = new(null, null, item.ID, null, null, false, rows);
        return result;
    }

    /// <summary>
    /// update or add entry and entry details
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    /// <exception cref="RequestException"></exception>
    public Model_Entry Update(Model_Entry? model)
    {
        if (model is null || string.IsNullOrWhiteSpace(model.Name))
            throw new RequestException(ResultCodes.DataIsInvalid);

        Structure_Entry? entry = DB.Structure_Entry.FirstOrDefault(s => s.ID == model.ID);

        if (!DB.Structure_Template.Any(s => s.ID == model.TemplateID && s.UserID == CurrentUser.ID))
            throw new RequestException(ResultCodes.NoDataFound);

        if (DB.Structure_Entry.Any(s => s.Name == model.Name && s.UserID == CurrentUser.ID))
            throw new RequestException(ResultCodes.EntryNameAlreadyExists);

        if (model.FolderID is not null)
        {
            if (!DB.Structure_Entry_Folder.Any(s => s.ID == model.FolderID && s.UserID == CurrentUser.ID))
                throw new RequestException(ResultCodes.NoDataFound);
        }

        if (entry is null)
        {
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
        DB.SaveChanges();

        Model_Entry result = Details(entry.ID);
        return result;
    }
}
