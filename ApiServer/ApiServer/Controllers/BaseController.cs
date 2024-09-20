using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using StyleWerk.NBB.Authentication;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.User;

namespace StyleWerk.NBB.Controllers;

[ApiController]
public abstract class BaseController(NbbContext db) : Controller
{
    protected NbbContext DB { get; } = db;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);
        Claim? claim = User.Claims.FirstOrDefault(s => s.Type == ClaimTypes.Sid);
        if (claim is not null && Guid.TryParse(claim.Value, out Guid id) && id != Guid.Empty)
        {
            User_Login? login = DB.User_Login.FirstOrDefault(s => s.ID == id);
            User_Information? information = DB.User_Information.FirstOrDefault(s => s.ID == id);
            User_Right? right = DB.User_Right.FirstOrDefault(s => s.ID == id);
            CurrentUser = login is null || information is null || right is null ?
                new ApplicationUser(false, Guid.Empty, new(), new(), new()) :
                new ApplicationUser(true, information.ID, login, information, right);
        }
    }

    public ApplicationUser CurrentUser { get; private set; } = new ApplicationUser(false, Guid.Empty, new(), new(), new());

    protected abstract bool MissingRight(UserRight right);
}

