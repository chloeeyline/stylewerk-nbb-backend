using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace StyleWerk.NBB.Controllers;

[ApiController, Route("TemplateOverview"), Authorize]
public class TemplateController(NbbContext db) : BaseController(db)
{
    public TemplateQueries Query => new(DB, CurrentUser);

    #region Load Templates

    [ApiExplorerSettings(GroupName = "Templates")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_TemplatePaging>))]
    [HttpPost(nameof(FilterTemplates))]
    public IActionResult FilterTemplates([FromBody] Model_FilterTemplate filters)
    {
        Model_TemplatePaging templates = Query.LoadFilterTemplates(filters);
        return Ok(new Model_Result<Model_TemplatePaging>(templates));
    }

    [ApiExplorerSettings(GroupName = "Templates")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_Template>))]
    [HttpGet(nameof(GetTemplatePreview))]
    public IActionResult GetTemplatePreview(Guid TemplateId)
    {
        List<Model_Template> preview = Query.LoadPreview(TemplateId);
        return Ok(new Model_Result<List<Model_Template>>(preview));
    }
    #endregion

    #region Actions

    [ApiExplorerSettings(GroupName = "Template Overview Actions")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(RemoveTemplate))]
    public IActionResult RemoveTemplate(Guid? TemplateId)
    {
        if (TemplateId is null)
            throw new RequestException(ResultType.DataIsInvalid);

        //get Entries with that templateId 
        IQueryable<Structure_Entry> entries = DB.Structure_Entry.Where(e => e.TemplateID == TemplateId);

        if (entries.Any())
        {
            foreach (Structure_Entry? entry in entries)
            {
                IQueryable<Structure_Entry_Cell> entryCells = DB.Structure_Entry_Cell.Where(c => c.EntryID == entry.ID);
                if (entryCells.Any())
                    DB.Structure_Entry_Cell.RemoveRange(entryCells);

                //checking if Entry with that Template exists in a Folder
                IQueryable<Structure_Entry_Folder> inFolder = DB.Structure_Entry_Folder.Where(e => e.ID == entry.ID);
                if (inFolder.Any())
                    DB.Structure_Entry_Folder.RemoveRange(inFolder);
            }

            //delete Entries with that TemplateId
            DB.Structure_Entry.RemoveRange(entries);
        }

        //Remove Template 
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

    [ApiExplorerSettings(GroupName = "Template Overview Actions")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Guid>))]
    [HttpPost(nameof(AddTemplate))]
    public IActionResult AddTemplate(Model_AddTemplate newTemplate)
    {
        Structure_Template template = new()
        {
            ID = Guid.NewGuid(),
            Description = newTemplate.Description,
            UserID = CurrentUser.ID,
            Name = newTemplate.Name,
            Tags = newTemplate.Tags
        };

        DB.Structure_Template.Add(template);
        DB.SaveChanges();

        return Ok(new Model_Result<Guid>(template.ID));
    }

    [ApiExplorerSettings(GroupName = "Template Overview Actions")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Guid>))]
    [HttpPost(nameof(CopyTemplate))]
    public IActionResult CopyTemplate(Guid? TemplateId)
    {
        if (TemplateId is null)
            throw new RequestException(ResultType.DataIsInvalid);

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

        return Ok(new Model_Result<Guid>(copyTemplate.ID));
    }

    [ApiExplorerSettings(GroupName = "Template Overview Actions")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Guid>))]
    [HttpPost(nameof(ChangeTemplateName))]
    public IActionResult ChangeTemplateName(Model_ChangeTemplateName? template)
    {
        if (template is null || string.IsNullOrWhiteSpace(template.Name))
            throw new RequestException(ResultType.DataIsInvalid);

        Structure_Template? changeTemplate = DB.Structure_Template.FirstOrDefault(t => t.ID == template.TemplateId)
             ?? throw new RequestException(ResultType.NoDataFound);

        changeTemplate.Name = template.Name;
        DB.SaveChanges();

        return Ok(new Model_Result<Guid>(template.TemplateId));
    }
    #endregion
}