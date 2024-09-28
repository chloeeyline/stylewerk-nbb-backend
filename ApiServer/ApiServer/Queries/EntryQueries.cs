using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Authentication;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class EntryQueries(NbbContext DB, ApplicationUser CurrentUser) : SharedItemQueries(DB, CurrentUser)
{
    #region Folder
    public List<Model_EntryFolders> GetFolders()
    {
        List<Model_EntryFolders> entryFolders =
        [
            .. DB.Structure_Entry_Folder
                .Where(f => f.UserID == CurrentUser.ID)
                .OrderBy(f => f.SortOrder)
                .Select(f => new Model_EntryFolders(f.ID, f.Name, f.SortOrder, new Model_EntryItem[0]))
        ];

        //all Entries without Folder
        Model_EntryItem[] result =
        [
            .. DB.Structure_Entry
                .Where(s => s.UserID == CurrentUser.ID && s.FolderID == null)
                .Include(s => s.O_Folder)
                .Include(s => s.O_Template)
                .Include(s => s.O_User)
                .Select(s => new Model_EntryItem(s, new ShareTypes(true, false, false, false))),
        ];
        entryFolders.Insert(0, new Model_EntryFolders(null, null, 0, result));
        return entryFolders;
    }

    public List<Model_EntryItem> GetFolderContent(Guid? folderId)
    {
        if (folderId is null || folderId == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);

        List<Model_EntryItem> list =
        [
            .. DB.Structure_Entry
                .Include(e => e.O_Folder)
                .Include(e => e.O_Template)
                .Include(e => e.O_User)
                .Where(e => e.FolderID == folderId)
                .Select(e => new Model_EntryItem(e, new ShareTypes(true, false, false, false))),
        ];

        return list.Count != 0 ? list : throw new RequestException(ResultCodes.NoDataFound);
    }
    #endregion

    #region Filter
    public List<Model_EntryItem> LoadEntryItem(Model_FilterEntry filter)
    {
        List<Model_EntryItem> result = [];
        filter = filter with { Username = filter.Username?.Normalize().ToLower() };

        if (filter.Share.Own && string.IsNullOrWhiteSpace(filter.Username))
            result.AddRange(LoadUserEntryItems(filter));
        if (filter.Share.GroupShared || !string.IsNullOrWhiteSpace(filter.Username))
            result.AddRange(LoadGroupEntryItems(filter));
        if (filter.Share.DirectlyShared || !string.IsNullOrWhiteSpace(filter.Username))
            result.AddRange(LoadDirectlySharedEntryItems(filter));
        if (filter.Share.Public || !string.IsNullOrWhiteSpace(filter.Username))
            result.AddRange(LoadPublicEntryItems(filter));

        List<Model_EntryItem> entries = [.. result
            .DistinctBy(s => s.ID)
            .OrderBy(s => s.LastUpdatedAt)
            .ThenBy(s => s.Name)];

        return entries;
    }

    private List<Model_EntryItem> LoadUserEntryItems(Model_FilterEntry filter)
    {
        IEnumerable<Structure_Entry> list = DB.Structure_Entry
            .Where(s => s.UserID == CurrentUser.ID)
            .Include(s => s.O_Folder)
            .Include(s => s.O_Template)
            .Include(s => s.O_User);

        list = FilterEntries(list, filter);

        List<Model_EntryItem> result = list.Select(s => new Model_EntryItem(s, new ShareTypes(true, false, false, false))).ToList();
        return result;
    }

    private List<Model_EntryItem> LoadPublicEntryItems(Model_FilterEntry filter)
    {
        List<Model_EntryItem> publicEntryItem = [];

        IEnumerable<Structure_Entry> list = DB.Structure_Entry
            //.Where(s => s.IsPublic)
            .Include(s => s.O_Folder)
            .Include(s => s.O_Template)
            .Include(s => s.O_User);

        list = FilterEntries(list, filter);

        List<Model_EntryItem> result = list.Select(s => new Model_EntryItem(s, new ShareTypes(false, false, true, false))).ToList();
        return result;
    }

    private List<Model_EntryItem> LoadDirectlySharedEntryItems(Model_FilterEntry filter)
    {
        List<Model_EntryItem> result = [];
        List<Model_SharedItem> shareList = DirectlySharedItems(1);

        foreach (Model_SharedItem item in shareList)
        {
            IEnumerable<Structure_Entry> list = DB.Structure_Entry
            .Where(s => s.ID == item.ID)
            .Include(s => s.O_Folder)
            .Include(s => s.O_Template)
            .Include(s => s.O_User);

            list = FilterEntries(list, filter);

            //adding entries to List
            result.AddRange(list.Select(s => new Model_EntryItem(s, new ShareTypes(false, false, false, true))));
        }

        return result;
    }

    private List<Model_EntryItem> LoadGroupEntryItems(Model_FilterEntry filter)
    {
        List<Model_EntryItem> result = [];
        List<Model_SharedItem> shareList = SharedViaGroupItems(1);

        foreach (Model_SharedItem item in shareList)
        {
            IEnumerable<Structure_Entry> list = DB.Structure_Entry
            .Where(s => s.ID == item.ID)
            .Include(s => s.O_Folder)
            .Include(s => s.O_Template)
            .Include(s => s.O_User);

            list = FilterEntries(list, filter);

            //adding entries to List
            result.AddRange(list.Select(s => new Model_EntryItem(s, new ShareTypes(false, true, false, false))));
        }

        return result;
    }

    private static IEnumerable<Structure_Entry> FilterEntries(IEnumerable<Structure_Entry> list, Model_FilterEntry filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Name))
            list = list.Where(s => s.Name.Contains(filter.Name));
        if (!string.IsNullOrWhiteSpace(filter.TemplateName))
            list = list.Where(s => s.O_Template.Name.Contains(filter.TemplateName));
        if (!string.IsNullOrWhiteSpace(filter.Username) && !filter.DirectUser)
            list = list.Where(s => s.O_User.UsernameNormalized.Contains(filter.Username));
        if (!string.IsNullOrWhiteSpace(filter.Username) && filter.DirectUser)
            list = list.Where(s => s.O_User.UsernameNormalized == filter.Username);
        if (!string.IsNullOrWhiteSpace(filter.Tags))
            list = list.Where(s => !string.IsNullOrWhiteSpace(s.Tags) && filter.Tags.Contains(s.Tags));
        return list.Distinct().OrderBy(s => s.LastUpdatedAt).ThenBy(s => s.Name);
    }
    #endregion
}