using ChaosFox.Models;
using Microsoft.AspNetCore.Mvc;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Dto;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace StyleWerk.NBB.Controllers
{
    [ApiController, Route("Entry")]
    public class EntryController : Controller
    {
        private readonly EntryQueries _entryQueries;
        private readonly NbbContext _db;

        public EntryController(NbbContext db)
        {
            _db = db;
            CurrentUser = new ApplicationUser(false, Guid.Empty, new(), new(), new());
            _entryQueries = new EntryQueries(db, CurrentUser);
        }

        private ApplicationUser CurrentUser { get; set; }

        [HttpGet(nameof(GetEntries))]
        public IActionResult GetEntries([FromBody] Model_FilterEntry filter)
        {
            var entries = _entryQueries.LoadEntryItem(filter);
            return Ok(new Model_Result(entries));
        }

        [HttpPost(nameof(AddEntry))]
        public IActionResult AddEntry(EntryDto entry)
        {
            Structure_Entry newEntry = new();
            newEntry.Name = entry.EntryTitle;
            newEntry.UserID = CurrentUser.ID;
            newEntry.TemplateID = entry.TemplateId;

            if (entry.FolderId == null) newEntry.FolderID = null;

            _db.Structure_Entry.Add(newEntry);
            _db.SaveChanges();

            return Ok(new Model_Result());
        }

        [HttpPost(nameof(ChangeName))]
        public IActionResult ChangeName(Model_ChangeEntryName entry)
        {
            var item = _db.Structure_Entry.FirstOrDefault(e => e.ID == entry.EntryID);
            if (item != null) item.Name = entry.Name;
            _db.SaveChanges();

            return Ok(new Model_Result());
        }

        //EntryEditor 
        [HttpPost(nameof(DeleteEntry))]
        public IActionResult DeleteEntry(Guid entryId)
        {
            var item = _db.Structure_Entry.FirstOrDefault(e => e.ID == entryId);
            if (item != null) _db.Structure_Entry.Remove(item);
            _db.SaveChanges();

            return Ok(new Model_Result());
        }
    }
}
