using Microsoft.EntityFrameworkCore;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Database.User;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries
{
    public class EntryQueries : IEntryQueries
    {
        private readonly NbbContext _context;

        public EntryQueries(NbbContext context) 
        { 
            _context = context;
        }

        public List<Model_EntryFolders> LoadEntryFolders(Guid userId)
        {
            var entryFolders = _context.Structure_Entry_Folder
                .Select(f=> new Model_EntryFolders(f.ID, f.Name, f.SortOrder, ???))
                .ToList();

            return entryFolders;
        }

        //merge entries
        public List<Model_EntryItem> LoadEntryItem(string username, string templateName, string entryName)
        {
            var userEntryItems = LoadUserEntryItems(username, templateName, entryName);
            var groupEntryItems = LoadGroupEntryItems(username, templateName, entryName);
            var publicEntryItems = LoadPublicEntryItems();
            var directlySharedEntryItems = LoadDirectlySharedEntryItems(username, templateName, entryName );
            
            //create one lists of all the above lists
            List<List<Model_EntryItem>> mergeEntries = new List<List<Model_EntryItem>>
            {
                userEntryItems,
                groupEntryItems,
                directlySharedEntryItems,
                publicEntryItems
            };

            var entries = mergeEntries.SelectMany(l => l).Distinct().ToList();
            return entries;
        }

        //username is not nullable bc even when there is no explicitly selected filter on the username,
        //there is still the currentUser
        private User_Information LoadUser(string username) 
        { 
            var user = _context.User_Information
                .FirstOrDefault(u => u.O_User.Username == username);

            return user;
        }

        //UserItems
        private List<Model_EntryItem> LoadUserEntryItems(string username, string? templateName, string? entryName)
        {
            User_Information user = LoadUser(username);
            Structure_Entry? entry = new Structure_Entry();
            List<Model_EntryItem> entries = new List<Model_EntryItem>();

            var userEntries = _context.Structure_Entry
                .Where(e => e.UserID == user.ID).ToList();

            foreach(var item in userEntries)
            {
                if(templateName == null && entryName != null)
                {
                    entry = _context.Structure_Entry
                        .FirstOrDefault(e => e.Name.Contains(entryName));
                }

                if(templateName != null && entryName == null)
                {
                    entry = _context.Structure_Entry
                        .FirstOrDefault(e => e.O_Template.Name.Contains(templateName));
                }

                if(templateName != null && entryName != null)
                {
                    entry = _context.Structure_Entry
                        .FirstOrDefault(e => e.O_Template.Name.Contains(templateName) &&
                        e.Name.Contains(entryName));
                }

                entries.Add(new Model_EntryItem(entry.ID, entry.Name, entry.O_User.Username, entry.O_Template.Name, entry.CreatedAt, entry.LastUpdatedAt, ShareType.Own));
            }

            return entries;
        }

        //directly shared
        private List<Model_EntryItem> LoadDirectlySharedEntryItems(string username, string? template, string? entryName)
        {
            List<Model_EntryItem> directly = new List<Model_EntryItem>();
            Structure_Entry? entry = new Structure_Entry();

            User_Information user = LoadUser(username);
            
            //directly shared entries 
            var directylSharedItem = _context.Share_Item.Where(s => s.Group == false && s.ID == user.O_User.ID && s.ItemType == 1); //ItemType: 1 == entry

            foreach (var directEntry in directylSharedItem)
            {
                //get entry Information by ShareType: direct , user , template , entryName
                if(template == null && username != null && entryName != null)
                {
                    entry = _context.Structure_Entry
                       .Include(t => t.O_Template)
                       .FirstOrDefault(e => e.ID == directEntry.ID &&
                       e.UserID == user.ID &&
                       e.Name.Contains(entryName));
                }

                if(template != null && username != null && entryName == null)
                {
                    entry = _context.Structure_Entry
                       .Include(t => t.O_Template)
                       .FirstOrDefault(e => e.ID == directEntry.ID &&
                       e.UserID == user.ID &&
                       e.O_Template.Name.Contains(template));
                }

                if(template != null && username != null && entryName != null)
                {
                    entry = _context.Structure_Entry
                       .Include(t => t.O_Template)
                       .FirstOrDefault(e => e.ID == directEntry.ID &&
                       e.UserID == user.ID &&
                       e.O_Template.Name.Contains(template) &&
                       e.Name.Contains(entryName));
                }
                
                //adding entries to List
                directly.Add(new Model_EntryItem(entry.ID, entry.Name, entry.O_User.Username, entry.O_Template.Name, entry.CreatedAt, entry.LastUpdatedAt, ShareType.Direcly));
            }

            return directly;
        }

        private List<Model_EntryItem> LoadPublicEntryItems()
        {
            List<Model_EntryItem> publicEntryItem = new List<Model_EntryItem>();

            //all public entries 
            return publicEntryItem = _context.Structure_Entry
                .Include(u => u.O_User)
                .Include(t => t.O_Template)
                //.Where(u => u.IsPublic)
                .Select(e => new Model_EntryItem(e.ID, e.Name, e.O_User.Username, e.O_Template.Name, e.CreatedAt, e.LastUpdatedAt, ShareType.Public)).ToList();
        }

        //group items
        private List<Model_EntryItem> LoadGroupEntryItems(string username, string? templateName, string? entryName)
        {
            User_Information user = LoadUser(username);
            Structure_Entry? entry = new Structure_Entry();

            //get all ShareGroups of User
            var shareGroup = _context.Share_GroupUser
                .Include(u => u.O_Group)
                .Where(u => u.UserID == user.ID)
                .Select(g => g.O_Group).ToList();

            List<Model_EntryItem> groupEntryItems = new List<Model_EntryItem>();

            foreach (var groupItem in shareGroup)
            {
                //All shared entries in the group 
                var shareItem = _context.Share_Item.Where(s => s.Group == true && s.ID == groupItem.ID && s.ItemType == 1);

                foreach (var item in shareItem)
                {
                    //get entry information from shared item  

                    if(templateName != null && entryName == null)
                    {
                        entry = _context.Structure_Entry
                        .Include(t => t.O_Template)
                        .FirstOrDefault(e => e.ID == item.ID && e.O_Template.Name.Contains(templateName));
                    }

                    if(templateName == null && entryName != null)
                    {
                        entry = _context.Structure_Entry
                        .Include(t => t.O_Template)
                        .FirstOrDefault(e => e.ID == item.ID && e.Name.Contains(entryName));
                    }

                    if(templateName != null && entryName != null)
                    {

                        entry = _context.Structure_Entry
                            .Include(t => t.O_Template)
                            .FirstOrDefault(e => e.ID == item.ID &&
                            e.O_Template.Name.Contains(templateName) &&
                            e.Name.Contains(entryName));
                    }

                    //adding entries to list
                    groupEntryItems.Add(new Model_EntryItem(entry.ID, entry.Name, entry.O_User.Username, entry.O_Template.Name, entry.CreatedAt, entry.LastUpdatedAt, ShareType.Group));
                }
            }

            return groupEntryItems;
        }
    }
}
