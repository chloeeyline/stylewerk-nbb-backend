using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Core;
using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class TemplateQueries(NbbContext DB, ApplicationUser CurrentUser) : BaseQueries(DB, CurrentUser)
{
    /// <summary>
    /// Filtering all templates based on the given Filters
    /// </summary>
    /// <param name="page"></param>
    /// <param name="perPage"></param>
    /// <param name="name"></param>
    /// <param name="username"></param>
    /// <param name="description"></param>
    /// <param name="tags"></param>
    /// <param name="publicShared"></param>
    /// <param name="shared"></param>
    /// <param name="includeOwned"></param>
    /// <param name="directUser"></param>
    /// <returns></returns>
    public Model_TemplatePaging List(int? page, int? perPage, string? name, string? username, string? description, string? tags, bool? publicShared, bool? shared, bool? includeOwned, bool? directUser)
    {
        if (publicShared is not true && shared is not true)
            includeOwned = true;
        // Normalize the username for comparison
        username = username?.Normalize().ToLower();
        tags = tags?.Normalize().ToLower();

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
            ownerUsernameNormalized = owner.UsernameNormalized,
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
                ownerUsernameNormalized = owner.UsernameNormalized,
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
                    where !string.IsNullOrWhiteSpace(s.Tags) && s.Tags.Contains(tags)
                    select s;

        if (!string.IsNullOrWhiteSpace(username) && directUser is false)
            query = from s in query
                    where s.ownerUsernameNormalized.Contains(username)
                    select s;

        if (!string.IsNullOrWhiteSpace(username) && directUser is true)
            query = from s in query
                    where s.ownerUsernameNormalized == username
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
        from g in query
        orderby g.Visibility, g.LastUpdatedAt, g.Name
        select new Model_TemplateItem
        (
            g.ID,
            g.Name,
            g.Description,
            g.Tags,
            g.CreatedAt,
            g.LastUpdatedAt,
            g.ownerUsername,
            g.Visibility
        );

        // Calculate pagination
        int tCount = orderedQuery.Count();
        if (!perPage.HasValue || perPage < 20)
            perPage = 20;
        int maxPages = (int)Math.Ceiling(tCount / (double)perPage);
        if (!page.HasValue || page >= maxPages || page < 0)
            page = 0;

        // Apply pagination
        List<Model_TemplateItem> pagedQuery = [.. orderedQuery.Skip(page.Value * perPage.Value).Take(perPage.Value)];

        // Return the final paginated result
        Model_TemplatePaging paging = new(new Paging(tCount, page.Value, maxPages, perPage.Value), pagedQuery);
        return paging;
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
    /// Copies a template for the currentuser
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="RequestException"></exception>
    public Model_Editor Copy(Guid? id)
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
            NameNormalized = $"{copyTemplate.Name} (Kopie)".NormalizeName(),
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
        EditorQueries query = new(DB, CurrentUser);

        return query.GetTemplate(template.ID);
    }
}
