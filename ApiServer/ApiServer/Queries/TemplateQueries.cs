using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Authentication;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class TemplateQueries(NbbContext DB, ApplicationUser CurrentUser) : ShareQueries(DB, CurrentUser)
{
    public List<Model_DetailedTemplate> LoadPreview(Guid templateId)
    {
        Model_TemplateRow[] rows =
        [
            .. DB.Structure_Template_Row
                    .Include(r => r.O_Cells)
                    .Include(r => r.O_Template)
                    .Where(r => r.TemplateID == templateId)
                    .Select(r => new Model_TemplateRow(r.ID, r.O_Template.ID, r.SortOrder, r.CanWrapCells,r.O_Cells
                    .Select(c => new Model_TemplateCell(c.ID, c.RowID, c.SortOrder, c.HideOnEmpty, c.InputHelper, c.IsRequiered, c.Text, c.MetaData)).ToArray())),
        ];

        List<Model_DetailedTemplate> preview =
        [
            .. DB.Structure_Template
                    .Where(t => t.ID == templateId)
                    .Select(t => new Model_DetailedTemplate(t.ID, t.Name, t.Description, rows))
        ];

        return preview;
    }

    public Model_TemplatePaging LoadFilterTemplates(Model_FilterTemplate filter)
    {
        List<Model_Template> result = [];
        filter = filter with { Username = filter.Username?.Normalize().ToLower() };

        if (filter.Share.Own && string.IsNullOrEmpty(filter.Username))
            result.AddRange(LoadUserTemplates(filter));
        if (filter.Share.GroupShared || !string.IsNullOrEmpty(filter.Username))
            result.AddRange(LoadGroupTemplates(filter));
        if (filter.Share.DirectlyShared || !string.IsNullOrEmpty(filter.Username))
            result.AddRange(LoadDirectlySharedTemplates(filter));
        if (filter.Share.Public || !string.IsNullOrEmpty(filter.Username))
            result.AddRange(LoadPublicTemplates(filter));

        List<Model_Template> templates = result.DistinctBy(s => s.Id).ToList();

        int tCount = templates.Count;
        int maxPages = tCount / filter.PerPage;
        if (filter.Page > maxPages)
            filter = filter with { Page = 1 };
        templates = templates.Skip(filter.Page * filter.PerPage).Take(filter.PerPage).ToList();

        Model_TemplatePaging paging = new(tCount, filter.Page, maxPages, filter.PerPage, templates);

        return paging;
    }

    private List<Model_Template> LoadUserTemplates(Model_FilterTemplate filter)
    {
        IEnumerable<Structure_Template> list = DB.Structure_Template
            .Where(t => t.UserID == CurrentUser.ID)
            .Include(t => t.O_User);

        list = FilterTemplates(list, filter);

        List<Model_Template> result = list.Select(s => new Model_Template(s, new ShareTypes(true, false, false, false))).ToList();
        return result;
    }

    private List<Model_Template> LoadGroupTemplates(Model_FilterTemplate filter)
    {
        List<Model_Template> result = [];
        List<Model_SharedItem> shareList = SharedViaGroupItems(2);

        foreach (Model_SharedItem item in shareList)
        {
            IEnumerable<Structure_Template> list = DB.Structure_Template
                .Where(t => t.ID == item.ID)
                .Include(t => t.O_User);

            list = FilterTemplates(list, filter);

            result.AddRange(list.Select(s => new Model_Template(s, new ShareTypes(false, true, false, false))));
        }

        return result;
    }

    private List<Model_Template> LoadDirectlySharedTemplates(Model_FilterTemplate filter)
    {
        List<Model_Template> result = [];
        List<Model_SharedItem> shareList = DirectlySharedItems(2);

        foreach (Model_SharedItem item in shareList)
        {
            IEnumerable<Structure_Template> list = DB.Structure_Template
            .Where(s => s.ID == item.ID)
            .Include(s => s.O_User);

            list = FilterTemplates(list, filter);

            result.AddRange(list.Select(s => new Model_Template(s, new ShareTypes(false, false, false, true))));
        }

        return result;
    }

    private List<Model_Template> LoadPublicTemplates(Model_FilterTemplate filter)
    {
        List<Model_Template> publicEntryItem = [];

        IEnumerable<Structure_Template> list = DB.Structure_Template
            .Where(s => s.IsPublic)
            .Include(s => s.O_User);

        list = FilterTemplates(list, filter);

        List<Model_Template> result = list.Select(s => new Model_Template(s, new ShareTypes(false, false, true, false))).ToList();
        return result;
    }

    private static IEnumerable<Structure_Template> FilterTemplates(IEnumerable<Structure_Template> list, Model_FilterTemplate filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Name))
            list = list.Where(s => s.Name.Contains(filter.Name));
        if (!string.IsNullOrWhiteSpace(filter.Username) && !filter.DirectUser)
            list = list.Where(s => s.O_User.UsernameNormalized.Contains(filter.Username));
        if (!string.IsNullOrWhiteSpace(filter.Username) && filter.DirectUser)
            list = list.Where(s => s.O_User.UsernameNormalized == filter.Username);
        if (filter.Tags.Length > 0)
            list = list.Where(s => s.Tags != null && s.Tags.Any(tag => filter.Tags.Contains(tag)));
        return list.Distinct().OrderBy(s => s.LastUpdatedAt).ThenBy(s => s.Name);
    }
}