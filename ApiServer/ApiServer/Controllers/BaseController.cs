using Microsoft.AspNetCore.Mvc;

using StyleWerk.NBB.Database;

namespace StyleWerk.NBB.Controllers;

[ApiController, Route("Base")]
public class BaseController(NbbContext db) : Controller
{
	protected NbbContext DB { get; } = db;

	[HttpGet(nameof(Index))]
	public IActionResult Index()
	{
		return Ok(new { number = 1 });
	}
}

