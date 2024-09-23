using Microsoft.AspNetCore.Mvc;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace StyleWerk.NBB.Controllers;

//Authorize Attribute einfuegen entweder bei Controller oder jeder Methode wo er angemeldet sein muss
[ApiController, Route("TemplateOverview")]
public class TemplateOverviewController : BaseController
{
    private readonly TemplateQueries _templateQueries;

    //Zusaetzlich erstelle region um die funktionen zu gruppieren dann kann man den code besser einklappen und ist leichter zu bearbeiten wenn man nicht alles offen haben muss und co siehe wieder Overview von den Entrys
    public TemplateOverviewController(NbbContext db) : base(db)
    {
        //Siehe EntryOverviewController musst anders machen weil User erst nach Constructor authentifieziert wird
        _templateQueries = new TemplateQueries(db, CurrentUser);
    }

    [HttpPost(nameof(FilterTemplates))]
    public IActionResult FilterTemplates([FromBody] Model_FilterTemplate filters)
    {
        List<Model_Templates> templates = _templateQueries.LoadFilterTemplates(filters);
        return Ok(new Model_Result(templates));
    }

    //brauchst eigentlich keine eigene methode weil du einfach die FilterTemplate hernehmen kannst mit filter own sollte eigentlich aufs gleiche laufen
    [HttpGet(nameof(GetTemplates))]
    public IActionResult GetTemplates()
    {
        List<Model_Templates> templates = _templateQueries.LoadTemplates();
        return Ok(new Model_Result(templates));
    }

    [HttpPost(nameof(RemoveTemplate))]
    public IActionResult RemoveTemplate(Guid TemplateId)
    {
        //Du musst hier auch alle Entries loeschen die das Template nutzen
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

        return Ok(new Model_Result());
    }

    [HttpPost(nameof(GetTemplatePreview))]
    public IActionResult GetTemplatePreview(Guid TemplateId)
    {
        List<Model_TemplatePreviewItems> preview = _templateQueries.LoadPreview(TemplateId);
        return Ok(new Model_Result(preview));
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

    //Parameter bestenfalls immer nullable und testen ob null oder nicht gueltig hier zum beispiel obs ne leere id ist oder was halt geprueft werden muss sonst fehler schmeisen siehe EntryOverview einfach um bessere Controlle zu haben wie fehlerhafte Requests gehandelt werden
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

        return Ok(new Model_Result());
    }

    //Models bestenfalls immer nullable und testen ob null sonst fehler schmeisen siehe EntryOverview einfach um bessere Controlle zu haben wie fehlerhafte Requests gehandelt werden
    [HttpPost(nameof(ChangeTemplateName))]
    public IActionResult ChangeTemplateName(Model_ChangeTemplateName template)
    {
        //Fehler schmeissen wie bei EntryOverview das FrontEnd weiss das Template gibts nimma oder halt das was schief gelaufen ist
        Structure_Template? changeTemplate = DB.Structure_Template.FirstOrDefault(t => t.ID == template.TemplateId);
        if (changeTemplate != null)
            changeTemplate.Name = template.Name;
        DB.SaveChanges();

        return Ok(new Model_Result());
    }

    [HttpPost(nameof(ChangeTemplateDescription))]
    public IActionResult ChangeTemplateDescription(Model_ChangeTemplateDescription template)
    {
        //Fehler schmeissen wie bei EntryOverview das FrontEnd weiss das Template gibts nimma oder halt das was schief gelaufen ist
        Structure_Template? changeTemplate = DB.Structure_Template.FirstOrDefault(t => t.ID == template.TemplateId);
        if (changeTemplate != null)
            changeTemplate.Description = template.Description;
        DB.SaveChanges();

        return Ok(new Model_Result());
    }
}
