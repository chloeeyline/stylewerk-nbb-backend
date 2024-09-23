
using Microsoft.AspNetCore.Mvc;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Controllers;

[ApiController, Route("EntryOverview")]
public class EntryEditorController : BaseController
{

    public EntryEditorController(NbbContext db) : base(db)
    {
    }

    [HttpPost(nameof(DeleteEntry))]
    public IActionResult DeleteEntry(Guid entryId)
    {
        Structure_Entry? item = DB.Structure_Entry.FirstOrDefault(e => e.ID == entryId);
        if (item != null) DB.Structure_Entry.Remove(item);
        DB.SaveChanges();

        return Ok(new Model_Result());
    }
}
