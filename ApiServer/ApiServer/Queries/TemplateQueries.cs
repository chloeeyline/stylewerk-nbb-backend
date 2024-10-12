using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Core;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class TemplateQueries(NbbContext DB, ApplicationUser CurrentUser) : BaseQueries(DB, CurrentUser)
{
    public Model_TemplatePaging List(int? page, int? perPage, string? name, string? username, string? description, string? tags, bool? includePublic)
    {
        // Normalize the username for comparison
        username = username?.Normalize().ToLower();
        tags = tags?.Normalize().ToLower();

        IQueryable<Structure_Template> query = DB.Structure_Template.Include(s => s.O_User).Where(s => s.O_User.UsernameNormalized == username || (s.IsPublic && includePublic == true));

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(s => s.Name.Contains(name));

        if (!string.IsNullOrWhiteSpace(description))
            query = query.Where(s => !string.IsNullOrWhiteSpace(s.Description) && s.Description.Contains(description));

        if (!string.IsNullOrWhiteSpace(tags))
            query = query.Where(s => !string.IsNullOrWhiteSpace(s.Tags) && s.Tags.Contains(tags));

        if (!string.IsNullOrWhiteSpace(username))
            query = query.Where(s => s.O_User.UsernameNormalized.Contains(username));

        query = query.OrderBy(s => s.LastUpdatedAt);

        // Calculate pagination
        int tCount = query.Count();
        if (!perPage.HasValue || perPage < 20)
            perPage = 20;
        int maxPages = (int) Math.Ceiling(tCount / (double) perPage);
        if (!page.HasValue || page >= maxPages || page < 0)
            page = 0;

        // Apply pagination
        List<Model_TemplateItem> pagedQuery = [.. query
            .Skip(page.Value * perPage.Value)
            .Take(perPage.Value)
            .Select(s => new Model_TemplateItem(s.ID, s.Name, s.IsPublic, s.Description, s.Tags, s.CreatedAt, s.LastUpdatedAt, s.O_User.Username))];

        // Return the final paginated result
        Model_TemplatePaging paging = new(new Paging(tCount, page.Value, maxPages, perPage.Value), pagedQuery);
        return paging;
    }

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
            IsPublic = false,
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
