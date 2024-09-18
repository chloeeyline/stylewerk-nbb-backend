using Microsoft.AspNetCore.Mvc;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Dto;
using StyleWerk.NBB.Queries;
using StyleWerk.NBB.Queries.ViewModel;

namespace StyleWerk.NBB.Controllers
{
    [ApiController, Route("Template")]
    public class TemplateController : Controller
    {
        private readonly TemplateQueries _templateQueries;
        private readonly NbbContext _db;

        public TemplateController(TemplateQueries templateQueries, NbbContext db)
        {
            _templateQueries = templateQueries;
            _db = db;
        }

        [HttpPost(nameof(AddTemplate))]
        public IActionResult AddTemplate(TemplateDto newTemplate)
        {
            Structure_Template template = new Structure_Template();
            template.Description = newTemplate.TemplateDescription;
            template.UserID = newTemplate.UserID;
            template.IsPublic = newTemplate.IsPublic;
            template.Name = newTemplate.TemplateName;

            _db.Structure_Template.Add(template);
            _db.SaveChanges();

            return Ok();
        }

        //Funktioniert
        [HttpGet(nameof(GetTemplatesByUserId))]
        public IActionResult GetTemplatesByUserId(Guid UserId)
        {
            List<TemplateViewModel> templates = _templateQueries.GetTemplatesByUserId(UserId);
            return Ok(templates);
        }

        [HttpGet(nameof(GetPublicTemplates))]
        public IActionResult GetPublicTemplates()
        {
            List<TemplateViewModel> templates = _templateQueries.GetAllPublicTemplates();
            return Ok(templates);
        }

        [HttpGet(nameof(FilterTemplates))]
        public IActionResult FilterTemplates(KeyValuePair<string,string> filters)
        {

            return Ok();
        }

    }
}
