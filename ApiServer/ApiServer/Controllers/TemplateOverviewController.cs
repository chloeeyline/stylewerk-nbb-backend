using Microsoft.AspNetCore.Mvc;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace StyleWerk.NBB.Controllers;

[ApiController, Route("TemplateOverview")]
public class TemplateOverviewController : BaseController
{
    private readonly TemplateQueries _templateQueries;

    public TemplateOverviewController(NbbContext db) : base(db)
    {
        _templateQueries = new TemplateQueries(db, CurrentUser);
    }

    [HttpPost(nameof(FilterTemplates))]
    public IActionResult FilterTemplates([FromBody] Model_FilterTemplate filters)
    {
        List<Model_Templates> templates = _templateQueries.LoadFilterTemplates(filters);
        return Ok(new Model_Result<List<Model_Templates>>(templates));
    }

    [HttpGet(nameof(GetTemplates))]
    public IActionResult GetTemplates()
    {
        List<Model_Templates> templates = _templateQueries.LoadTemplates();
        return Ok(new Model_Result<List<Model_Templates>>(templates));
    }

    [HttpPost(nameof(RemoveTemplate))]
    public IActionResult RemoveTemplate(Guid TemplateId)
    {
        Structure_Template? removeTemplate = DB.Structure_Template.FirstOrDefault(t => t.ID == TemplateId);
        List<Structure_Template_Row> removeTemplateRow = [.. DB.Structure_Template_Row.Where(t => t.TemplateID == TemplateId)];
        List<Structure_Template_Cell> removeTemplateCell = [];

        if (removeTemplateRow != null)
        {
            foreach (Structure_Template_Row row in removeTemplateRow)
            {
                DB.Structure_Template_Cell.RemoveRange(DB.Structure_Template_Cell.Where(t => t.RowID == row.ID));
            }

            DB.Structure_Template_Row.RemoveRange(removeTemplateRow);
        }

        if (removeTemplate != null)
            DB.Structure_Template.Remove(removeTemplate);

        DB.SaveChanges();

        return Ok(new Model_Result<string>());
    }

    [HttpPost(nameof(GetTemplatePreview))]
    public IActionResult GetTemplatePreview(Guid TemplateId)
    {
        List<Model_TemplatePreviewItems> preview = _templateQueries.LoadPreview(TemplateId);
        return Ok(new Model_Result<List<Model_TemplatePreviewItems>>(preview));
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

        return Ok(new Model_Result<string>());
    }

    [HttpPost(nameof(CopyTemplate))]
    public IActionResult CopyTemplate(Guid TemplateId)
    {
        Structure_Template? copyTemplate = DB.Structure_Template.FirstOrDefault(t => t.ID == TemplateId);
        List<Structure_Template_Cell> copyCells = [];

        if (copyTemplate != null)
        {
            Structure_Template template = new()
            {
                ID = Guid.NewGuid(),
                Name = $"{copyTemplate.Name} kopie",
                Description = copyTemplate.Description,
                UserID = CurrentUser.ID
            };

            DB.Structure_Template.Add(template);

            foreach (Structure_Template_Row row in DB.Structure_Template_Row.Where(t => t.TemplateID == TemplateId).ToList())
            {
                Structure_Template_Row newRow = new()
                {
                    ID = Guid.NewGuid(),
                    TemplateID = template.ID,
                    SortOrder = row.SortOrder,
                    CanWrapCells = row.CanWrapCells
                };

                DB.Structure_Template_Row.Add(newRow);

                foreach (Structure_Template_Cell cell in DB.Structure_Template_Cell.Where(c => c.RowID == row.ID).ToList())
                {
                    Structure_Template_Cell newCell = new()
                    {
                        ID = Guid.NewGuid(),
                        RowID = newRow.ID,
                        SortOrder = cell.SortOrder,
                        InputHelper = cell.InputHelper,
                        HideOnEmpty = cell.HideOnEmpty,
                        IsRequiered = cell.IsRequiered,
                        Text = cell.Text,
                        MetaData = cell.MetaData
                    };

                    DB.Structure_Template_Cell.Add(newCell);
                }
            }

            DB.SaveChanges();
        }

        return Ok(new Model_Result<string>());
    }

    [HttpPost(nameof(ChangeTemplateName))]
    public IActionResult ChangeTemplateName(Model_ChangeTemplateName template)
    {
        Structure_Template? changeTemplate = DB.Structure_Template.FirstOrDefault(t => t.ID == template.TemplateId);
        if (changeTemplate != null)
            changeTemplate.Name = template.Name;
        DB.SaveChanges();

        return Ok(new Model_Result<string>());
    }

    [HttpPost(nameof(ChangeTemplateDescription))]
    public IActionResult ChangeTemplateDescription(Model_ChangeTemplateDescription template)
    {
        Structure_Template? changeTemplate = DB.Structure_Template.FirstOrDefault(t => t.ID == template.TemplateId);
        if (changeTemplate != null)
            changeTemplate.Description = template.Description;
        DB.SaveChanges();

        return Ok(new Model_Result<string>());
    }
}
