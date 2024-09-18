using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Database.User;
using StyleWerk.NBB.Dto;
using StyleWerk.NBB.Queries;
using StyleWerk.NBB.Queries.Dto;

namespace StyleWerk.NBB.Controllers
{
    [ApiController, Route("Entry")]
    public class EntryController : Controller
	{
		private readonly EntryQueries _entryQueries;
		private readonly NbbContext _db;

		public EntryController(EntryQueries entryQueries, NbbContext db)
		{
			_entryQueries = entryQueries;
			_db = db;
		}


		[HttpGet(nameof(Index))]
		public IActionResult Index()
		{

			return Ok("Lol");
		}

		[HttpGet(nameof(GetEntriesByUserId))]
		public IActionResult GetEntriesByUserId(Guid userId)
		{
			List<EntryViewModel> Entries = _entryQueries.LoadEntriesByUserId(userId);
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
			Structure_Entry newEntry = new();
			newEntry.Name = entry.EntryTitle;
			newEntry.UserID = entry.UserId;
			newEntry.TemplateID = entry.TemplateId;

			if(entry.FolderId == null)
			{
				newEntry.FolderID = null;
			}
			
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

		[HttpGet(nameof(AddTestUser))]
		public IActionResult AddTestUser()
		{
			User_Login user = new()
			{
				ID = Guid.NewGuid(),
				Username = "test",
				UsernameNormalized = "test",
				Email = "test",
				EmailNormalized = "test",
				PasswordHash = "test",
				PasswordSalt = "test",
			};

			User_Information userInformation = new()
			{
				ID = user.ID,
				Gender = UserGender.NonBinary,
				FirstName = "test",
				LastName = "test",
				Birthday = new DateOnly(2000, 9, 2),
			};

			User_Right userRight = new()
			{
				ID = user.ID,
			};

			_db.User_Login.Add(user);
			_db.User_Information.Add(userInformation);
			_db.User_Right.Add(userRight);
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
