using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class TemplateQueries(NbbContext DB, ApplicationUser CurrentUser) : BaseQueries(DB, CurrentUser)
{
    public Model_TemplatePaging List(int page, int perPage, string? name, string? username, string? description, string? tags, bool? publicShared, bool? groupShared, bool? directlyShared, bool? directUser)
    {
        // Normalize the username for comparison
        username = username?.Normalize().ToLower();

        // Query for shared templates
        var query =
        from si in DB.Share_Item
        where si.Type == ShareType.Template
        join template in DB.Structure_Template on si.ItemID equals template.ID
        join whoShared in DB.User_Login on si.UserID equals whoShared.ID
        join owner in DB.User_Login on template.UserID equals owner.ID
        join sgu in DB.Share_GroupUser on
            new { si.ToWhom, si.Visibility } equals
            new { ToWhom = (Guid?) sgu.GroupID, Visibility = ShareVisibility.Group }
            into groupJoin
        from sharedGroup in groupJoin.DefaultIfEmpty()
        join sg in DB.Share_Group on sharedGroup.GroupID equals sg.ID into groupDataJoin
        from groupData in groupDataJoin.DefaultIfEmpty()
        where (si.ToWhom == CurrentUser.ID || sharedGroup.UserID == CurrentUser.ID)
        select new
        {
            template,
            si.Visibility,
            ownerUsername = owner.Username,
            ownerUsernameNormalized = owner.UsernameNormalized,
            si.CanShare,
            si.CanEdit,
            si.CanDelete,
            whoSharedUsername = whoShared.Username,
            whoSharedUsernameNormalized = whoShared.UsernameNormalized,
            groupName = groupData.Name
        };

        // Query to get templates owned by the current user
        var ownedQuery =
        from template in DB.Structure_Template
        join owner in DB.User_Login on template.UserID equals owner.ID
        where template.UserID == CurrentUser.ID
        select new
        {
            template,
            Visibility = ShareVisibility.None,
            ownerUsername = owner.Username,
            ownerUsernameNormalized = owner.UsernameNormalized,
            CanShare = true,
            CanEdit = true,
            CanDelete = true,
            whoSharedUsername = (string?) null,
            whoSharedUsernameNormalized = (string?) null,
            groupName = (string?) null
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
                    where s.template.Name.Contains(name)
                    select s;

        if (!string.IsNullOrWhiteSpace(description))
            query = from s in query
                    where !string.IsNullOrWhiteSpace(s.template.Description) && s.template.Description.Contains(description)
                    select s;

        if (!string.IsNullOrWhiteSpace(tags))
            query = from s in query
                    where !string.IsNullOrWhiteSpace(s.template.Tags) && tags.Contains(s.template.Tags)
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
                orderby s.Visibility, s.template.LastUpdatedAt, s.template.Name
                select s;

        int tCount = query.Count();
        if (perPage < 20)
            perPage = 20;
        int maxPages = tCount / perPage;
        if (page > maxPages)
            page = 0;

        // Final select to map data to TemplateResult
        IQueryable<Model_TemplateItem> finalQuery = query.Skip(page * perPage).Take(perPage).Select(s => new Model_TemplateItem
        (
            s.template.ID,
            s.template.Name,
            s.template.Description,
            s.template.Tags,
            s.template.CreatedAt,
            s.template.LastUpdatedAt,
            s.ownerUsername,
            s.Visibility,
            s.CanShare,
            s.CanEdit,
            s.CanDelete,
            s.whoSharedUsername,
            s.groupName
        ));

        // Execute the query and return distinct results
        Model_TemplatePaging paging = new(tCount, page, maxPages, perPage, [.. finalQuery]);
        return paging;
    }

    public Model_Template Details(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);

        Structure_Template item = DB.Structure_Template.FirstOrDefault(t => t.ID == id)
            ?? throw new RequestException(ResultCodes.NoDataFound);

        List<Model_TemplateRow> rows = [];
        List<Structure_Template_Row> rowTemplate = [.. DB.Structure_Template_Row
            .Where(s => s.TemplateID == item.ID)
            .OrderBy(s => s.SortOrder)];

        foreach (Structure_Template_Row row in rowTemplate)
        {
            List<Model_TemplateCell> cells = [];
            List<Structure_Template_Cell> cellTemplate = [.. DB.Structure_Template_Cell
                .Where(s => s.RowID == row.ID)
                .OrderBy(s => s.SortOrder)];

            foreach (Structure_Template_Cell cell in cellTemplate)
            {
                Model_TemplateCell cellModel = new(cell.ID, cell.RowID, cell.InputHelper, cell.HideOnEmpty, cell.IsRequired, cell.Text, cell.MetaData);
                cells.Add(cellModel);
            }

            Model_TemplateRow rowModel = new(row.ID, row.CanWrapCells, row.CanRepeat, row.HideOnNoInput, cells);
            rows.Add(rowModel);
        }
        Model_Template model = new(item.ID, item.Name, item.Description, item.Tags, rows);

        return model;
    }

    public void Remove(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);

        List<Structure_Entry> entries = [.. DB.Structure_Entry.Where(e => e.TemplateID == id)];
        foreach (Structure_Entry entry in entries)
        {
            List<Structure_Entry_Row> entryRows = [.. DB.Structure_Entry_Row
                .Where(t => t.EntryID == entry.ID)
                .Include(s => s.O_Cells)];
            foreach (Structure_Entry_Row row in entryRows)
                DB.Structure_Entry_Cell.RemoveRange(row.O_Cells);
            DB.Structure_Entry_Row.RemoveRange(entryRows);
        }
        DB.Structure_Entry.RemoveRange(entries);

        Structure_Template? template = DB.Structure_Template.FirstOrDefault(t => t.ID == id)
            ?? throw new RequestException(ResultCodes.NoDataFound);
        List<Structure_Template_Row> rows = [.. DB.Structure_Template_Row
            .Where(t => t.TemplateID == id)
            .Include(s => s.O_Cells)];

        foreach (Structure_Template_Row row in rows)
            DB.Structure_Template_Cell.RemoveRange(row.O_Cells);

        DB.Structure_Template_Row.RemoveRange(rows);
        DB.Structure_Template.Remove(template);

        DB.SaveChanges();
    }

    public void Update(Model_Template? model)
    {
        if (model is null || string.IsNullOrWhiteSpace(model.Name))
            throw new RequestException(ResultCodes.DataIsInvalid);

        Structure_Template? template = DB.Structure_Template.FirstOrDefault(s => s.ID == model.ID);
        if (template is null)
        {
            template = new()
            {
                ID = Guid.NewGuid(),
                UserID = CurrentUser.ID,
                Name = model.Name,
                Description = model.Description,
                Tags = model.Tags,
            };
            DB.Structure_Template.Add(template);
        }
        else
        {
            template.Name = model.Name;
            template.Description = model.Description;
            template.Tags = model.Tags;
        }

        int rowSortOrder = 0;
        foreach (Model_TemplateRow rowItem in model.Items)
        {
            Structure_Template_Row? rowTemplate = DB.Structure_Template_Row.SingleOrDefault(t => rowItem.ID == t.ID);

            if (rowTemplate is null)
            {
                rowTemplate = new()
                {
                    ID = Guid.NewGuid(),
                    TemplateID = template.ID,
                    SortOrder = rowSortOrder++,
                    CanWrapCells = rowItem.CanWrapCells,
                    CanRepeat = rowItem.CanRepeat,
                    HideOnNoInput = rowItem.HideOnNoInput
                };
                DB.Structure_Template_Row.Add(rowTemplate);
            }
            else
            {
                rowTemplate.SortOrder = rowSortOrder++;
                rowTemplate.CanWrapCells = rowItem.CanWrapCells;
                rowTemplate.CanRepeat = rowItem.CanRepeat;
                rowTemplate.HideOnNoInput = rowItem.HideOnNoInput;
            }

            int cellSortOrder = 0;
            foreach (Model_TemplateCell cell in rowItem.Items)
            {
                Structure_Template_Cell? cellTemplate = DB.Structure_Template_Cell.SingleOrDefault(c => c.ID == cell.ID);

                if (cellTemplate is null)
                {
                    cellTemplate = new()
                    {
                        ID = Guid.NewGuid(),
                        RowID = rowItem.ID,
                        SortOrder = cellSortOrder++,
                        InputHelper = cell.InputHelper,
                        HideOnEmpty = cell.HideOnEmpty,
                        IsRequired = cell.IsRequired,
                        Text = cell.Text,
                        MetaData = cell.Text
                    };
                    DB.Structure_Template_Cell.Add(cellTemplate);
                }
                else
                {
                    cellTemplate.SortOrder = cellSortOrder++;
                    cellTemplate.InputHelper = cell.InputHelper;
                    cellTemplate.HideOnEmpty = cell.HideOnEmpty;
                    cellTemplate.IsRequired = cell.IsRequired;
                    cellTemplate.Text = cell.Text;
                    cellTemplate.MetaData = cell.MetaData;
                }
            }
        }

        DB.SaveChanges();
    }

    public void Copy(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);

        Structure_Template? copyTemplate = DB.Structure_Template.FirstOrDefault(t => t.ID == id)
            ?? throw new RequestException(ResultCodes.NoDataFound);

        Structure_Template template = new()
        {
            ID = Guid.NewGuid(),
            UserID = CurrentUser.ID,
            Name = $"{copyTemplate.Name} (Kopie)",
            Description = copyTemplate.Description,
            Tags = copyTemplate.Tags,
        };
        DB.Structure_Template.Add(template);

        foreach (Structure_Template_Row row in DB.Structure_Template_Row.Where(t => t.TemplateID == copyTemplate.ID).ToList())
        {
            Structure_Template_Row newRow = new()
            {
                ID = Guid.NewGuid(),
                TemplateID = template.ID,
                SortOrder = row.SortOrder,
                CanWrapCells = row.CanWrapCells,
                CanRepeat = row.CanRepeat,
                HideOnNoInput = row.CanRepeat,
            };
            DB.Structure_Template_Row.Add(newRow);

            foreach (Structure_Template_Cell cell in DB.Structure_Template_Cell.Where(c => c.RowID == row.ID).ToList())
            {
                Structure_Template_Cell newCell = new()
                {
                    ID = Guid.NewGuid(),
                    RowID = newRow.ID,
                    SortOrder = cell.SortOrder,
                    InputHelper = cell.InputHelper,
                    HideOnEmpty = cell.HideOnEmpty,
                    IsRequired = cell.IsRequired,
                    Text = cell.Text,
                    Description = cell.Description,
                    MetaData = cell.MetaData
                };
                DB.Structure_Template_Cell.Add(newCell);
            }
        }

        DB.SaveChanges();
    }
}
