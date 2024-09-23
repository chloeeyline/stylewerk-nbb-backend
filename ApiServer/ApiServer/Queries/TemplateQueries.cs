using Microsoft.EntityFrameworkCore;
using StyleWerk.NBB.Authentication;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries
{
    public class TemplateQueries
    {
        private readonly NbbContext _context;
        private readonly ApplicationUser _user;

        public TemplateQueries(NbbContext context, ApplicationUser user)
        {
            _user = user;
            _context = context;
        }

        //Default
        public List<Model_Templates> LoadTemplates()
        {
            List<Model_Templates> templates = _context.Structure_Template
                .Where(t => t.UserID == _user.ID)
                .Select(t => new Model_Templates(t, new ShareTypes(true, false, false, false)))
                .ToList();
            //.OrderBy(t=> t.IsCopied);

            return templates;
        }

        public List<Model_Templates> LoadFilterTemplates(Model_FilterTemplate filter)
        {
            List<Model_Templates> result = [];
            filter = filter with { Username = filter.Username?.Normalize().ToLower() };

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
            IEnumerable<Structure_Template> list = _context.Structure_Template
                .Where(t => t.UserID == _user.ID)
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

            IQueryable<Share_Group> groups = _context.Share_GroupUser
                .Include(u => u.O_Group)
                .Where(u => u.UserID == _user.ID)
                .Select(g => g.O_Group);

            foreach (Share_Group group in groups)
            {
                IQueryable<Share_Item> items = _context.Share_Item.Where(s => s.Group == true && s.ID == group.ID && s.ItemType == 2);

                foreach (Share_Item item in items)
                {
                    IEnumerable<Structure_Template> list = _context.Structure_Template
                        .Where(t => t.ID == item.ID)
                        .Include(t => t.O_User);

                    if (!string.IsNullOrEmpty(filter.Name) && !filter.directUser)
                        list = list.Where(t => t.Name.Contains(filter.Name));
                    if (!string.IsNullOrEmpty(filter.Name) && filter.directUser)
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

            IQueryable<Share_Item> shared = _context.Share_Item.Where(s => s.Group == false && s.ID == _user.ID && s.ItemType == 2); //ItemType: 1 == entry

            foreach (Share_Item? item in shared)
            {
                IEnumerable<Structure_Template> list = _context.Structure_Template
                .Where(s => s.ID == item.ID)
                .Include(s => s.O_User);

                if (!string.IsNullOrEmpty(filter.Name) && !filter.directUser)
                    list = list.Where(s => s.Name.Contains(filter.Name));
                if (!string.IsNullOrEmpty(filter.Name) && filter.directUser)
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

            IEnumerable<Structure_Template> list = _context.Structure_Template
                .Where(s => s.IsPublic)
                .Include(s => s.O_User);

            if (!string.IsNullOrEmpty(filter.Name) && !filter.directUser)
                list = list.Where(s => s.Name.Contains(filter.Name));
            if (!string.IsNullOrEmpty(filter.Name) && filter.directUser)
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
}
