using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using StyleWerk.NBB.Database;

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
            CurrentUser = DB.GetUser(id);
    }

    public ApplicationUser CurrentUser { get; private set; } = new ApplicationUser();
}

