using Microsoft.AspNetCore.Mvc;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace StyleWerk.NBB.Controllers
{
    [ApiController, Route("EntryOverview")]
    public class EntryOverviewController : BaseController
    {
        private readonly EntryQueries _entryQueries;

        public EntryOverviewController(NbbContext db) : base(db)
        {
            _entryQueries = new EntryQueries(db, CurrentUser);
        }

        [HttpGet(nameof(GetFolders))]
        public IActionResult GetFolders()
        {
            List<Model_EntryFolders> entries = _entryQueries.LoadEntryFolders();
            return Ok(new Model_Result(entries));
        }

        [HttpGet(nameof(GetEntriesFromFolder))]
        public IActionResult GetEntriesFromFolder(Guid folderId)
        {
            List<Model_EntryItem> entries = _entryQueries.GetEntriesFromFolder(folderId);
            return Ok(new Model_Result(entries));
        }

        //Filter by Tags isn't possible yet
        [HttpPost(nameof(GetEntries))]
        public IActionResult GetEntries([FromBody] Model_FilterEntry filter)
        {
            List<Model_EntryItem> entries = _entryQueries.LoadEntryItem(filter);
            return Ok(new Model_Result(entries));
        }

        [HttpPost(nameof(AddEntry))]
        public IActionResult AddEntry([FromBody] Model_AddEntry entry)
        {
            Structure_Entry newEntry = new()
            {
                Name = entry.Name,
                UserID = CurrentUser.ID,
                TemplateID = entry.TemplateId,
            };

            if (entry.FolderId == null)
                newEntry.FolderID = null;

            DB.Structure_Entry.Add(newEntry);
            DB.SaveChanges();

            return Ok(new Model_Result());
        }

        [HttpPost(nameof(AddFolder))]
        public IActionResult AddFolder([FromBody] Model_AddFolder folder)
        {
            bool isFilled = DB.Structure_Entry_Folder.Any();
            int sortOrder = isFilled ? (DB.Structure_Entry_Folder.Max(f => f.SortOrder) + 1) : 1;

            Structure_Entry_Folder newFolder = new()
            {
                Name = folder.Name,
                SortOrder = sortOrder,
                UserID = folder.UserId
            };

            DB.Structure_Entry_Folder.Add(newFolder);
            DB.SaveChanges();

            return Ok(new Model_Result());
        }

        [HttpPost(nameof(ChangeEntryName))]
        public IActionResult ChangeEntryName(Model_ChangeEntryName entry)
        {
            Structure_Entry? item = DB.Structure_Entry.FirstOrDefault(e => e.ID == entry.EntryID);
            if (item != null) item.Name = entry.Name;
            DB.SaveChanges();

            return Ok(new Model_Result());
        }

        [HttpPost(nameof(ChangeFolder))]
        public IActionResult ChangeFolder([FromBody] Model_ChangeFolder folder)
        {
            Structure_Entry? item = DB.Structure_Entry.FirstOrDefault(e => e.ID == folder.EntryID);
            if (item != null)
                item.FolderID = folder.FolderID;
            DB.SaveChanges();

            return Ok(new Model_Result());
        }

        [HttpPost(nameof(DragAndDrop))]
        public IActionResult DragAndDrop([FromBody] Model_ListFolderSortOrder listFolder)
        {

            return Ok(new Model_Result());
        }
    }
}
