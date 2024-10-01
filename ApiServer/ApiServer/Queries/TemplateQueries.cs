using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class TemplateQueries(NbbContext DB, ApplicationUser CurrentUser) : BaseQueries(DB, CurrentUser)
{
    public Model_TemplatePaging List(int page, int perPage, string? name, string? username, string? description, string? tags, bool? publicShared, bool? shared, bool? includeOwned, bool? directUser)
    {
        // Normalize the username for comparison
        username = username?.Normalize().ToLower();

        // Query for shared templates
        var query =
        from si in DB.Share_Item
        where si.Type == ShareType.Template
        join template in DB.Structure_Template on si.ItemID equals template.ID
        join owner in DB.User_Login on template.UserID equals owner.ID
        join sgu in DB.Share_GroupUser on
            new { si.ToWhom, si.Visibility } equals
            new { ToWhom = (Guid?)sgu.GroupID, Visibility = ShareVisibility.Group }
            into groupJoin
        from sharedGroup in groupJoin.DefaultIfEmpty()
        join sg in DB.Share_Group on sharedGroup.GroupID equals sg.ID into groupDataJoin
        from groupData in groupDataJoin.DefaultIfEmpty()
        where (si.ToWhom == CurrentUser.ID || sharedGroup.UserID == CurrentUser.ID)
        select new
        {
            template.ID,
            template.Name,
            template.Description,
            template.Tags,
            template.CreatedAt,
            template.LastUpdatedAt,
            ownerUsername = owner.Username,
            si.Visibility
        };

        if (includeOwned is true)
        {
            // Query for owned templates
            var ownedQuery =
            from template in DB.Structure_Template
            join owner in DB.User_Login on template.UserID equals owner.ID
            where template.UserID == CurrentUser.ID
            select new
            {
                template.ID,
                template.Name,
                template.Description,
                template.Tags,
                template.CreatedAt,
                template.LastUpdatedAt,
                ownerUsername = owner.Username,
                Visibility = ShareVisibility.None // Mark as owned
            };

            // Combine the two queries (shared + owned) using Union
            query = query.Union(ownedQuery);
        }

        // Apply visibility filters based on the model
        query = from s in query
                where
                    (includeOwned == true && s.Visibility == ShareVisibility.None) || // Include owned templates if filter is true
                    (publicShared == true && s.Visibility == ShareVisibility.Public) ||
                    (shared == true && (s.Visibility == ShareVisibility.Directly || s.Visibility == ShareVisibility.Group))
                select s;

        // Apply filters prior to grouping
        if (!string.IsNullOrWhiteSpace(name))
            query = from s in query
                    where s.Name.Contains(name)
                    select s;

        if (!string.IsNullOrWhiteSpace(description))
            query = from s in query
                    where !string.IsNullOrWhiteSpace(s.Description) && s.Description.Contains(description)
                    select s;

        if (!string.IsNullOrWhiteSpace(tags))
            query = from s in query
                    where !string.IsNullOrWhiteSpace(s.Tags) && tags.Contains(s.Tags)
                    select s;

        if (!string.IsNullOrWhiteSpace(username) && directUser is false)
            query = from s in query
                    where s.ownerUsername.Contains(username)
                    select s;

        if (!string.IsNullOrWhiteSpace(username) && directUser is true)
            query = from s in query
                    where s.ownerUsername == username
                    select s;

        // Group templates by ID, giving priority to owned templates over shared and public templates
        var groupedQuery =
        from s in query
        group s by s.ID into g
        select new
        {
            Template = g.FirstOrDefault(x => x.Visibility == ShareVisibility.None) ?? // Highest priority: owned
                       g.FirstOrDefault(x => x.Visibility == ShareVisibility.Directly) ?? // Second priority: directly shared
                       g.FirstOrDefault(x => x.Visibility == ShareVisibility.Group) ??    // Third priority: group shared
                       g.FirstOrDefault(x => x.Visibility == ShareVisibility.Public)      // Lowest priority: public
        };

        // Apply ordering before final selection
        IQueryable<Model_TemplateItem> orderedQuery =
        from g in groupedQuery
        orderby g.Template.Visibility, g.Template.LastUpdatedAt, g.Template.Name
        select new Model_TemplateItem
        (
            g.Template.ID,
            g.Template.Name,
            g.Template.Description,
            g.Template.Tags,
            g.Template.CreatedAt,
            g.Template.LastUpdatedAt,
            g.Template.ownerUsername,
            g.Template.Visibility
        );

        // Calculate pagination
        int tCount = orderedQuery.Count();
        if (perPage < 20)
            perPage = 20;
        int maxPages = (int)Math.Ceiling(tCount / (double)perPage);
        if (page > maxPages)
            page = 0;

        // Apply pagination
        List<Model_TemplateItem> pagedQuery = [.. orderedQuery.Skip(page * perPage).Take(perPage)];

        // Return the final paginated result
        Model_TemplatePaging paging = new(tCount, page, maxPages, perPage, pagedQuery);
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

    /// <summary>
    /// Remove a Template, all its rows, cells and entries where the template was used
    /// </summary>
    /// <param name="id"></param>
    /// <exception cref="RequestException"></exception>
    public void Remove(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);

        Structure_Template? template = DB.Structure_Template.FirstOrDefault(t => t.ID == id)
            ?? throw new RequestException(ResultCodes.NoDataFound);
        if (template.UserID != CurrentUser.ID)
            throw new RequestException(ResultCodes.YouDontOwnTheData);
        DB.Structure_Template.Remove(template);

        DB.SaveChanges();
    }
    /// <summary>
    /// update or add template
    /// </summary>
    /// <param name="model"></param>
    /// <exception cref="RequestException"></exception>
    public Model_Template Update(Model_Template? model)
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
            if (template.UserID != CurrentUser.ID)
                throw new RequestException(ResultCodes.YouDontOwnTheData);
            template.Name = model.Name;
            template.Description = model.Description;
            template.Tags = model.Tags;
        }

        List<Guid> rowIDs = [];
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
            rowIDs.Add(rowTemplate.ID);
            int cellSortOrder = 0;
            List<Guid> cellIDs = [];
            foreach (Model_TemplateCell cell in rowItem.Items)
            {
                Structure_Template_Cell? cellTemplate = DB.Structure_Template_Cell.SingleOrDefault(c => c.ID == cell.ID);

                if (cellTemplate is null)
                {
                    cellTemplate = new()
                    {
                        ID = Guid.NewGuid(),
                        RowID = rowTemplate.ID,
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
                cellIDs.Add(cellTemplate.ID);
            }
            DB.Structure_Template_Cell.RemoveRange(DB.Structure_Template_Cell.Where(s => !cellIDs.Contains(s.ID) && s.RowID == rowTemplate.ID));
        }
        DB.Structure_Template_Row.RemoveRange(DB.Structure_Template_Row.Where(s => !rowIDs.Contains(s.ID) && s.TemplateID == template.ID));

        DB.SaveChanges();
        return Details(template.ID);
    }

    public Model_Template Copy(Guid? id)
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
        return Details(template.ID);
    }
}
