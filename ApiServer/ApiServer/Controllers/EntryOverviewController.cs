using Microsoft.AspNetCore.Mvc;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Dto;
using StyleWerk.NBB.Queries;
using StyleWerk.NBB.Queries.Dto;
using Microsoft.EntityFrameworkCore;

namespace StyleWerk.NBB.Controllers
{
    [ApiController, Route("Entry/Overview")]
    public class EntryOverviewController : Controller
	{
		private readonly EntryQueries _entryQueries;
		private readonly NbbContext _db;

		public EntryOverviewController(EntryQueries entryQueries)
		{
			_entryQueries = entryQueries;
		}


        [HttpGet(nameof(Index))]
        public IActionResult Index()
        {

            return Ok("Lol");
        }

        [HttpGet(nameof(GetEntriesByUserId))]
		public IActionResult GetEntriesByUserId(Guid userId)
		{
            List<EntryViewModel>  Entries = _entryQueries.LoadEntriesByUserId(userId);
			return Ok(Entries);
		}

		[HttpGet(nameof(GetEntryContent))]
		public IActionResult GetEntryContent(Guid userId)
		{
            EntryContentViewModel Entry = _entryQueries.LoadEntryByUserId(userId);
			return Ok(Entry);
		}

		[HttpPost(nameof(SaveEntry))]
		public IActionResult SaveEntry(EntryDto entry)
		{
			Structure_Entry newEntry = new Structure_Entry();
			newEntry.Name = entry.EntryTitle;
			newEntry.UserID = entry.UserId;
			newEntry.TemplateID = entry.TemplateId;
			newEntry.FolderID = entry.FolderId;

			_db.Structure_Entry.Add(newEntry);
			_db.SaveChanges();

			return Ok();
		}

        [HttpPost(nameof(UpdateEntry))]
        public IActionResult UpdateEntry(EntryDto entry)
        {
			_db.Entry(entry).State = EntityState.Modified;
			_db.SaveChanges();

            return Ok();
        }

        [HttpPost(nameof(DeleteEntry))]
        public IActionResult DeleteEntry(EntryDto entry)
        {
			_db.Entry(entry).State = EntityState.Deleted;
			_db.SaveChanges();

            return Ok();
        }

    }
}
