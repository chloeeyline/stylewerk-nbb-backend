using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace StyleWerk.NBB.Controllers;

[ApiController, Route("ShareNew"), Authorize]
public class ShareNewController(NbbContext db) : BaseController(db)
{
    public ShareNewQueries Query => new(DB, CurrentUser);

    //Welche Gruppen man selbst erstellt hat
    public IActionResult GetOwnedGroups()
    {
        List<Model_Group2> list = Query.GetOwnedGroups();
        return Ok(new Model_Result<List<Queries.Model_Group2>>(list));
    }

    //Welche User in der Gruppe sind
    public IActionResult GetUsersInGroup(Guid? id)
    {
        List<Model_GroupUser2> list = Query.GetUsersInGroup(id);
        return Ok(new Model_Result<List<Model_GroupUser2>>(list));
    }

    public IActionResult UpdateGroup([FromBody] Model_UpdateGroup2? model)
    {
        model = Query.UpdateGroup(model);
        return Ok(new Model_Result<Model_UpdateGroup2>(model));
    }

    //Gruppe loeschen
    public IActionResult RemoveGroup(Guid? id)
    {
        Query.RemoveGroup(id);
        return Ok(new Model_Result<string>());
    }

    //User zur Gruppe hinzufuegen
    public IActionResult AddUserToGroup()
    {
        return Ok(new Model_Result<string>());
    }

    //User aus Gruppe entfernen
    public IActionResult RemoveUserFromGroup()
    {
        return Ok(new Model_Result<string>());
    }

    //Rechte eines Users innerhalb einer Gruppe bearbeiten
    public IActionResult EditUserRightsForGroup()
    {
        return Ok(new Model_Result<string>());
    }
}
