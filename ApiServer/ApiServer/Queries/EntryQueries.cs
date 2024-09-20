using ChaosFox.Models;
using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries
{
    public class EntryQueries
    {
        private readonly NbbContext _context;
        private readonly ApplicationUser _user;

        public EntryQueries(NbbContext context, ApplicationUser user)
        {
            _user = user;
            _context = context;
        }

        public List<Model_EntryFolders> LoadEntryFolders()
        {
            var entryFolders = _context.Structure_Entry_Folder
                .OrderBy(f => f.SortOrder)
                .Select(f => new Model_EntryFolders(f.ID, f.Name, f.SortOrder, new Model_EntryItem[0]))
                .ToList();

            //alle entries die keinen folder haben 
            IEnumerable<Structure_Entry> list = _context.Structure_Entry
                .Where(s => s.UserID == _user.ID && s.FolderID == null)
                .Include(s => s.O_Folder)
                .Include(s => s.O_Template)
                .Include(s => s.O_User);

            Model_EntryItem[] result = list.Select(s => new Model_EntryItem(s, ShareType.Own)).ToArray();
            entryFolders.Add(new Model_EntryFolders(null, null, 0, result));
            return entryFolders;
        }

        //merge entries
        public List<Model_EntryItem> LoadEntryItem(Model_FilterEntry filter)
        {
            List<Model_EntryItem> result = [];
            if (filter.Own) result.AddRange(LoadUserEntryItems(filter));
            if (filter.GroupShared) result.AddRange(LoadGroupEntryItems(filter));
            if (filter.DirectlyShared) result.AddRange(LoadDirectlySharedEntryItems(filter));
            if (filter.Public) result.AddRange(LoadPublicEntryItems(filter));

            //Dont think that it is neccacery because that should already be all unique items
            List<Model_EntryItem> entries = result.DistinctBy(s => s.ID).ToList();
            return entries;
        }

        //UserItems
        private List<Model_EntryItem> LoadUserEntryItems(Model_FilterEntry filter)
        {
            IEnumerable<Structure_Entry> list = _context.Structure_Entry
                .Where(s => s.UserID == _user.ID)
                .Include(s => s.O_Folder)
                .Include(s => s.O_Template)
                .Include(s => s.O_User);

            if (!string.IsNullOrEmpty(filter.Name)) list = list.Where(s => s.Name.Contains(filter.Name));
            if (!string.IsNullOrEmpty(filter.TemplateName)) list = list.Where(s => s.O_Template.Name.Contains(filter.TemplateName));
            if (!string.IsNullOrEmpty(filter.Username)) list = list.Where(s => s.O_User.UsernameNormalized.Contains(filter.Username));

            List<Model_EntryItem> result = list.Select(s => new Model_EntryItem(s, ShareType.Own)).ToList();
            return result;
        }

        //directly shared
        private List<Model_EntryItem> LoadDirectlySharedEntryItems(Model_FilterEntry filter)
        {
            List<Model_EntryItem> result = [];

            IQueryable<Share_Item> sharedList = _context.Share_Item.Where(s => s.Group == false && s.ID == _user.ID && s.ItemType == 1); //ItemType: 1 == entry

            foreach (Share_Item? item in sharedList)
            {
                IEnumerable<Structure_Entry> list = _context.Structure_Entry
                .Where(s => s.ID == item.ID)
                .Include(s => s.O_Folder)
                .Include(s => s.O_Template)
                .Include(s => s.O_User);

                if (!string.IsNullOrEmpty(filter.Name)) list = list.Where(s => s.Name.Contains(filter.Name));
                if (!string.IsNullOrEmpty(filter.TemplateName)) list = list.Where(s => s.O_Template.Name.Contains(filter.TemplateName));
                if (!string.IsNullOrEmpty(filter.Username)) list = list.Where(s => s.O_User.UsernameNormalized.Contains(filter.Username));

                //adding entries to List
                result.AddRange(list.Select(s => new Model_EntryItem(s, ShareType.Direcly)));
            }

            return result;
        }

        private List<Model_EntryItem> LoadPublicEntryItems(Model_FilterEntry filter)
        {
            List<Model_EntryItem> publicEntryItem = [];

            IEnumerable<Structure_Entry> list = _context.Structure_Entry
                //.Where(s => s.IsPublic)
                .Include(s => s.O_Folder)
                .Include(s => s.O_Template)
                .Include(s => s.O_User);

            if (!string.IsNullOrEmpty(filter.Name)) list = list.Where(s => s.Name.Contains(filter.Name));
            if (!string.IsNullOrEmpty(filter.TemplateName)) list = list.Where(s => s.O_Template.Name.Contains(filter.TemplateName));
            if (!string.IsNullOrEmpty(filter.Username)) list = list.Where(s => s.O_User.UsernameNormalized.Contains(filter.Username));

            List<Model_EntryItem> result = list.Select(s => new Model_EntryItem(s, ShareType.Public)).ToList();
            return result;
        }

        //group items
        private List<Model_EntryItem> LoadGroupEntryItems(Model_FilterEntry filter)
        {
            List<Model_EntryItem> result = [];

            //get all ShareGroups of User
            IQueryable<Share_Group> sharedList = _context.Share_GroupUser
                .Include(u => u.O_Group)
                .Where(u => u.UserID == _user.ID)
                .Select(g => g.O_Group);

            foreach (Share_Group? groupItem in sharedList)
            {
                //All shared entries in the group 
                IQueryable<Share_Item> shareItem = _context.Share_Item.Where(s => s.Group == true && s.ID == groupItem.ID && s.ItemType == 1);

                foreach (Share_Item? item in shareItem)
                {
                    IEnumerable<Structure_Entry> list = _context.Structure_Entry
                    .Where(s => s.ID == item.ID)
                    .Include(s => s.O_Folder)
                    .Include(s => s.O_Template)
                    .Include(s => s.O_User);

                    if (!string.IsNullOrEmpty(filter.Name)) list = list.Where(s => s.Name.Contains(filter.Name));
                    if (!string.IsNullOrEmpty(filter.TemplateName)) list = list.Where(s => s.O_Template.Name.Contains(filter.TemplateName));
                    if (!string.IsNullOrEmpty(filter.Username)) list = list.Where(s => s.O_User.UsernameNormalized.Contains(filter.Username));

                    //adding entries to List
                    result.AddRange(list.Select(s => new Model_EntryItem(s, ShareType.Group)));
                }
            }

            return result;
        }
    }
}
