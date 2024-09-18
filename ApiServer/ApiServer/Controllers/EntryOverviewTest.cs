using Microsoft.AspNetCore.Mvc;

using StyleWerk.NBB.Database;

namespace StyleWerk.NBB.Controllers;


[ApiController, Route("test/EntryList")]
public class EntryOverviewTest(NbbContext db) : Controller
{
	protected NbbContext DB { get; } = db;

	[HttpGet(nameof(FolderList))]
	public IActionResult FolderList()
	{
		return Ok(DB.Share_Group.ToList());
	}
}
