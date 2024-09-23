using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Authentication;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class TemplateQueries(NbbContext DB, ApplicationUser User)
{
    //Default
    public List<Model_Templates> LoadTemplates()
    {
        List<Model_Templates> templates =
        [
            .. DB.Structure_Template
                            .Include(t => t.O_User)
                            .Where(t => t.UserID == User.ID)
                            .Select(t => new Model_Templates(t, new ShareTypes(true, false, false, false)))
,
        ];
        //.OrderBy(t=> t.IsCopied);

        return templates;
    }

    public List<Model_TemplatePreviewItems> LoadPreview(Guid templateId)
    {
        //Sollte so nicht funktionieren duerfen, du laedst hier mehr oder wenioger alle Cells des Tempalte und nicht nur einer Zeile du must das nach am besten mit nem foreach loesen weil die Row sollte ja nur die Cells haben die zur jeweiligen Row gehoeren
        Model_TemplateCell[] cells =
        [
            .. DB.Structure_Template_Cell
                        .Include(c => c.O_Row)
                        .Where(c => c.RowID == c.O_Row.ID && c.O_Row.TemplateID == templateId)
                        .Select(c => new Model_TemplateCell(c.ID, c.RowID, c.SortOrder, c.HideOnEmpty, c.IsRequiered, c.Text, c.MetaData))
,
        ];

        Model_TemplateRow[] rows =
        [
            .. DB.Structure_Template_Row
                        .Include(r => r.O_Cells)
                        .Include(r => r.O_Template)
                        .Where(r => r.TemplateID == templateId)
                        .Select(r => new Model_TemplateRow(r.ID, r.O_Template.ID, r.SortOrder, r.CanWrapCells, cells)),
        ];

        List<Model_TemplatePreviewItems> preview =
        [
            .. DB.Structure_Template
                        .Where(t => t.ID == templateId)
                        .Select(t => new Model_TemplatePreviewItems(t.ID, t.Name, rows))
,
        ];

        return preview;

    }

    public List<Model_Templates> LoadFilterTemplates(Model_FilterTemplate filter)
    {
        List<Model_Templates> result = [];
        filter = filter with { Username = filter.Username?.Normalize().ToLower() };

        //Schau mal bei EntryOverview habe dir dafuer generic Code geschrieben
        if (filter.Share.Own && string.IsNullOrEmpty(filter.Username))
            result.AddRange(LoadUserTemplates(filter));
        if (filter.Share.GroupShared || !string.IsNullOrEmpty(filter.Username))
            result.AddRange(LoadGroupTemplates(filter));
        if (filter.Share.DirectlyShared || !string.IsNullOrEmpty(filter.Username))
            result.AddRange(LoadDirectlySharedTemplates(filter));
        if (filter.Share.Public || !string.IsNullOrEmpty(filter.Username))
            result.AddRange(LoadPublicTemplates(filter));

        List<Model_Templates> templates = result.DistinctBy(s => s.Id).ToList();
        return templates;
    }

    private List<Model_Templates> LoadUserTemplates(Model_FilterTemplate filter)
    {
        IEnumerable<Structure_Template> list = DB.Structure_Template
            .Where(t => t.UserID == User.ID)
            .Include(t => t.O_User);

        if (!string.IsNullOrEmpty(filter.Name))
            list = list.Where(t => t.Name.Contains(filter.Name));
        if (!string.IsNullOrEmpty(filter.Username))
            list = list.Where(t => t.O_User.UsernameNormalized.Contains(filter.Username));
        if (filter.Tags != null)
        {
            foreach (string tag in filter.Tags)
            {
                list = list.Where(t => t.Tags != null && t.Tags.Contains(tag));
            }
        }

        List<Model_Templates> result = list.Select(s => new Model_Templates(s, new ShareTypes(true, false, false, false))).ToList();
        return result;
    }

    private List<Model_Templates> LoadGroupTemplates(Model_FilterTemplate filter)
    {
        List<Model_Templates> result = [];

        IQueryable<Share_Group> groups = DB.Share_GroupUser
            .Include(u => u.O_Group)
            .Where(u => u.UserID == User.ID)
            .Select(g => g.O_Group);

        foreach (Share_Group group in groups)
        {
            IQueryable<Share_Item> items = DB.Share_Item.Where(s => s.Group == true && s.ID == group.ID && s.ItemType == 2);

            foreach (Share_Item item in items)
            {
                IEnumerable<Structure_Template> list = DB.Structure_Template
                    .Where(t => t.ID == item.ID)
                    .Include(t => t.O_User);

                //Lagere diese Filter am besten in ne extra Methode wie in EntryOverviewQuery ist einfach das er immer gleich ist und es passieren weniger fehler
                //Zudem sollte der Filter DirectUser auf den UserNamen und nicht den TemplateNamen verwendet werden
                if (!string.IsNullOrEmpty(filter.Name) && !filter.DirectUser)
                    list = list.Where(t => t.Name.Contains(filter.Name));
                if (!string.IsNullOrEmpty(filter.Name) && filter.DirectUser)
                    list = list.Where(t => t.Name == filter.Name);
                if (!string.IsNullOrEmpty(filter.Username))
                    list = list.Where(t => t.O_User.UsernameNormalized.Contains(filter.Username));
                if (filter.Tags != null)
                {
                    foreach (string tag in filter.Tags)
                    {
                        list = list.Where(t => t.Tags != null && t.Tags.Contains(tag));
                    }
                }

                result.AddRange(list.Select(s => new Model_Templates(s, new ShareTypes(false, true, false, false))));
            }
        }

        return result;
    }

    private List<Model_Templates> LoadDirectlySharedTemplates(Model_FilterTemplate filter)
    {
        List<Model_Templates> result = [];

        IQueryable<Share_Item> shared = DB.Share_Item.Where(s => s.Group == false && s.ID == User.ID && s.ItemType == 2); //ItemType: 1 == entry

        foreach (Share_Item? item in shared)
        {
            IEnumerable<Structure_Template> list = DB.Structure_Template
            .Where(s => s.ID == item.ID)
            .Include(s => s.O_User);

            if (!string.IsNullOrEmpty(filter.Name) && !filter.DirectUser)
                list = list.Where(s => s.Name.Contains(filter.Name));
            if (!string.IsNullOrEmpty(filter.Name) && filter.DirectUser)
                list = list.Where(s => s.Name == filter.Name);
            if (!string.IsNullOrEmpty(filter.Username))
                list = list.Where(s => s.O_User.UsernameNormalized.Contains(filter.Username));
            if (filter.Tags != null)
            {
                foreach (string tag in filter.Tags)
                {
                    list = list.Where(t => t.Tags != null && t.Tags.Contains(tag));
                }
            }

            result.AddRange(list.Select(s => new Model_Templates(s, new ShareTypes(false, false, false, true))));
        }

        return result;
    }

    private List<Model_Templates> LoadPublicTemplates(Model_FilterTemplate filter)
    {
        List<Model_Templates> publicEntryItem = [];

        IEnumerable<Structure_Template> list = DB.Structure_Template
            .Where(s => s.IsPublic)
            .Include(s => s.O_User);

        if (!string.IsNullOrEmpty(filter.Name) && !filter.DirectUser)
            list = list.Where(s => s.Name.Contains(filter.Name));
        if (!string.IsNullOrEmpty(filter.Name) && filter.DirectUser)
            list = list.Where(s => s.Name == filter.Name);
        if (!string.IsNullOrEmpty(filter.Username))
            list = list.Where(s => s.O_User.UsernameNormalized.Contains(filter.Username));
        if (filter.Tags != null)
        {
            foreach (string tag in filter.Tags)
            {
                list = list.Where(t => t.Tags != null && t.Tags.Contains(tag));
            }
        }

        List<Model_Templates> result = list.Select(s => new Model_Templates(s, new ShareTypes(false, false, true, false))).ToList();
        return result;
    }

}
