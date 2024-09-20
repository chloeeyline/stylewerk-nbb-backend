using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.User;
using StyleWerk.NBB.Queries;

namespace StyleWerk.NBB.Controllers;

[ApiController, AllowAnonymous, Route("Template")]
public class TemplateController : BaseController
{
    private readonly TemplateQueries _templateQueries;

    public TemplateController(NbbContext db) : base(db)
    {
        _templateQueries = new TemplateQueries(db);
    }

    protected override bool MissingRight(UserRight right) => throw new NotImplementedException();
}
