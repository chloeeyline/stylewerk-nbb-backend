using Microsoft.AspNetCore.Mvc;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace StyleWerk.NBB.Controllers
{
    [ApiController, Route("TemplateOverview")]
    public class TemplateOverviewController : BaseController
    {
        private readonly TemplateQueries _templateQueries;

        public TemplateOverviewController(NbbContext db) : base(db)
        {
            _templateQueries = new TemplateQueries(db, CurrentUser);
        }

        [HttpPost(nameof(FilterTemplates))]
        public IActionResult FilterTemplates(Model_FilterTemplate filters)
        {
            List<Model_Templates> templates = _templateQueries.LoadFilterTemplates(filters);
            return Ok(new Model_Result(templates));
        }

        public IActionResult GetTemplates()
        {
            List<Model_Templates> templates = _templateQueries.LoadTemplates();
            return Ok(new Model_Result(templates));
        }

        [HttpPost(nameof(RemoveTemplate))]
        public IActionResult RemoveTemplate(Guid TemplateId)
        {
            Structure_Template? removeTemplate = DB.Structure_Template.FirstOrDefault(t => t.ID == TemplateId);
            List<Structure_Template_Row> removeTemplateRow = DB.Structure_Template_Row.Where(t => t.TemplateID == TemplateId).ToList();
            List<Structure_Template_Cell> removeTemplateCell = new();

            if (removeTemplateRow != null)
            {
                foreach (Structure_Template_Row row in removeTemplateRow)
                {
                    IQueryable<Structure_Template_Cell> templateCells = DB.Structure_Template_Cell.Where(t => t.RowID == row.ID);
                    removeTemplateCell.AddRange(templateCells);
                }

                DB.Structure_Template_Cell.RemoveRange(removeTemplateCell);
                DB.Structure_Template_Row.RemoveRange(removeTemplateRow);
            }

            if (removeTemplate != null)
                DB.Structure_Template.Remove(removeTemplate);

            return Ok(new Model_Result());
        }

        [HttpPost(nameof(GetTemplatePreview))]
        public IActionResult GetTemplatePreview(Guid TemplateId)
        {
            return Ok(new Model_Result());
        }

        [HttpPost(nameof(AddTemplate))]
        public IActionResult AddTemplate(Model_AddTemplate newTemplate)
        {
            Structure_Template template = new()
            {
                Description = newTemplate.Description,
                UserID = CurrentUser.ID,
                Name = newTemplate.Name,
                Tags = newTemplate.Tags
            };

            DB.Structure_Template.Add(template);
            DB.SaveChanges();

            return Ok(new Model_Result());
        }

        //not finished
        [HttpPost(nameof(CopyTemplate))]
        public IActionResult CopyTemplate(Guid TemplateId)
        {
            Structure_Template? copyTemplate = DB.Structure_Template.FirstOrDefault(t => t.ID == TemplateId);
            List<Structure_Template_Cell> copyCells = new();

            if (copyTemplate != null)
            {
                List<Structure_Template_Row> copyRows = DB.Structure_Template_Row.Where(t => t.TemplateID == TemplateId).ToList();
                if (copyRows != null)
                {
                    foreach (Structure_Template_Row row in copyRows)
                    {
                        IQueryable<Structure_Template_Cell> cells = DB.Structure_Template_Cell.Where(c => c.RowID == row.ID);
                        copyCells.AddRange(cells);
                    }
                }

                Structure_Template template = new()
                {
                    Name = $"{copyTemplate.Name} kopiert",
                    Description = copyTemplate.Description,
                    UserID = CurrentUser.ID
                };
                DB.Structure_Template.Add(template);
                DB.SaveChanges();

            }
            return Ok(new Model_Result());
        }

        [HttpPost(nameof(ChangeTemplateName))]
        public IActionResult ChangeTemplateName(Model_ChangeTemplateName template)
        {
            Structure_Template? changeTemplate = DB.Structure_Template.FirstOrDefault(t => t.ID == template.TemplateId);
            if (changeTemplate != null)
                changeTemplate.Name = template.Name;
            DB.SaveChanges();

            return Ok(new Model_Result());
        }

        [HttpPost(nameof(ChangeTemplateDescription))]
        public IActionResult ChangeTemplateDescription(Model_ChangeTemplateDescription template)
        {
            Structure_Template? changeTemplate = DB.Structure_Template.FirstOrDefault(t => t.ID == template.TemplateId);
            if (changeTemplate != null)
                changeTemplate.Description = template.Description;
            DB.SaveChanges();

            return Ok(new Model_Result());
        }
    }
}
