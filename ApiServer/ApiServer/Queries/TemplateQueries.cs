using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Authentication;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class TemplateQueries(NbbContext DB, ApplicationUser CurrentUser) : SharedItemQueries(DB, CurrentUser)
{
    public Model_DetailedTemplate LoadTemplate(Guid templateId)
    {
        Structure_Template? item = DB.Structure_Template.FirstOrDefault(t => t.ID == templateId) ?? throw new RequestException(ResultCodes.NoDataFound);

        Model_TemplateRow[] rows =
        [
            .. DB.Structure_Template_Row
                    .Include(r => r.O_Cells)
                    .Where(r => r.TemplateID == templateId)
                    .Select(r => new Model_TemplateRow(r))
        ];

        Model_DetailedTemplate model = new(item.ID, item.Name, item.Description, rows);

        return model;
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

        List<Model_Template> templates = result.DistinctBy(s => s.ID).ToList();

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
        if (!string.IsNullOrWhiteSpace(filter.Tags))
            list = list.Where(s => !string.IsNullOrWhiteSpace(s.Tags) && filter.Tags.Contains(s.Tags));
        return list.Distinct().OrderBy(s => s.LastUpdatedAt).ThenBy(s => s.Name);
    }
}