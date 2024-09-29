using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.User;
using StyleWerk.NBB.Models;

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
            string[] rights = [.. DB.User_Right.Where(s => s.ID == id).Select(s => s.Name)];
            CurrentUser = login is null || information is null ?
                new ApplicationUser() :
                new ApplicationUser(login, information, rights);
        }
    }

    public ApplicationUser CurrentUser { get; private set; } = new ApplicationUser();
}

