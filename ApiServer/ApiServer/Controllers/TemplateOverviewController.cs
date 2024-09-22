
using Microsoft.AspNetCore.Mvc;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace StyleWerk.NBB.Controllers
{
    [ApiController, Route("Template")]
    public class TemplateOverviewController : BaseController
    {
        private readonly TemplateQueries _templateQueries;

        public TemplateOverviewController(NbbContext db) : base(db)
        {
            _templateQueries = new TemplateQueries(db, CurrentUser);
        }

        [HttpPost(nameof(AddTemplate))]
        public IActionResult AddTemplate(Model_AddTemplate newTemplate)
        {
            Structure_Template template = new()
            {
                Description = newTemplate.Description,
                UserID = CurrentUser.ID,
                Name = newTemplate.Name
            };

            DB.Structure_Template.Add(template);
            DB.SaveChanges();

            return Ok(new Model_Result());
        }

        [HttpGet(nameof(FilterTemplates))]
        public IActionResult FilterTemplates(KeyValuePair<string, string> filters)
        {

            return Ok(new Model_Result());
        }

        [HttpPost(nameof(RemoveTemplate))]
        public IActionResult RemoveTemplate(Guid TemplateId)
        {
            return Ok(new Model_Result());
        }

        [HttpPost(nameof(GetTemplatePreview))]
        public IActionResult GetTemplatePreview(Guid TemplateId)
        {
            return Ok(new Model_Result());
        }

        [HttpPost(nameof(CopyTemplate))]
        public IActionResult CopyTemplate(Guid TemplateId)
        {
            return Ok(new Model_Result());
        }
    }
}
