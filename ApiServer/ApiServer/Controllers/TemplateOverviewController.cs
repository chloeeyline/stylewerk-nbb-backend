using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace StyleWerk.NBB.Controllers;

[ApiController, Route("TemplateOverview"), Authorize]
public class TemplateOverviewController(NbbContext db) : BaseController(db)
{
    public TemplateQueries Query => new(DB, CurrentUser);

    #region Load Templates

    [HttpPost(nameof(FilterTemplates))]
    public IActionResult FilterTemplates([FromBody] Model_FilterTemplate filters)
    {
        List<Model_Templates> templates = Query.LoadFilterTemplates(filters);
        return Ok(new Model_Result(templates));
    }

    [HttpGet(nameof(GetTemplatePreview))]
    public IActionResult GetTemplatePreview(Guid TemplateId)
    {
        List<Model_TemplatePreviewItems> preview = Query.LoadPreview(TemplateId);
        return Ok(new Model_Result(preview));
    }
    #endregion

    #region Actions

    //Idk if thats correct
    [HttpPost(nameof(ShareTemplate))]
    public IActionResult ShareTemplate([FromBody] Model_ShareTemplate shareTemplate)
    {
        if (shareTemplate is null)
            throw new RequestException(ResultType.DataIsInvalid);

        //check if Template Exists
        Structure_Template template = DB.Structure_Template.FirstOrDefault(t => t.ID == shareTemplate.TemplateId)
            ?? throw new RequestException(ResultType.NoDataFound);

        if (shareTemplate.Share.GroupShared)
        {
            //check if Group exists
            Share_Group group = DB.Share_Group.FirstOrDefault(g => g.ID == shareTemplate.ShareId)
                ?? throw new RequestException(ResultType.NoDataFound);

            //check if User is in Group
            bool isInGroup = DB.Share_GroupUser.Any(g => g.GroupID == group.ID && g.UserID == CurrentUser.ID);
            if (!isInGroup)
                throw new RequestException(ResultType.MissingRight);

            Share(shareTemplate, group.ID, shareTemplate.Share.GroupShared);
        }

        if (shareTemplate.Share.DirectlyShared)
        {
            //check if User exists
            Database.User.User_Information userExists = DB.User_Information.FirstOrDefault(u => u.ID == shareTemplate.ShareId)
                ?? throw new RequestException(ResultType.GeneralError);

            Share(shareTemplate, userExists.ID, shareTemplate.Share.GroupShared);
        }

        if (shareTemplate.Share.Public)
        {
            template.IsPublic = true;
            DB.Structure_Template.Update(template);
            DB.SaveChanges();
        }

        return Ok(new Model_Result());
    }

    private void Share(Model_ShareTemplate shareTemplate, Guid toWhom, bool type)
    {
        Share_Item newShareItem = new()
        {
            ID = Guid.NewGuid(),
            WhoShared = CurrentUser.ID,
            Group = type,
            ItemType = 2,
            ItemID = shareTemplate.TemplateId,
            ToWhom = toWhom,
            CanShare = shareTemplate.Rights.CanShare,
            CanDelete = shareTemplate.Rights.CanDelete,
            CanEdit = shareTemplate.Rights.CanEdit
        };

        DB.Share_Item.Add(newShareItem);
        DB.SaveChanges();
    }

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

        return Ok(new Model_Result());
    }

    [HttpPost(nameof(ChangeTemplateName))]
    public IActionResult ChangeTemplateName(Model_ChangeTemplateName? template)
    {
        if (template is null || string.IsNullOrWhiteSpace(template.Name))
            throw new RequestException(ResultType.DataIsInvalid);

        Structure_Template? changeTemplate = DB.Structure_Template.FirstOrDefault(t => t.ID == template.TemplateId)
             ?? throw new RequestException(ResultType.NoDataFound);

        if (changeTemplate != null)
            changeTemplate.Name = template.Name;
        DB.SaveChanges();

        return Ok(new Model_Result());
    }

    [HttpPost(nameof(ChangeTemplateDescription))]
    public IActionResult ChangeTemplateDescription(Model_ChangeTemplateDescription template)
    {
        Structure_Template? changeTemplate = DB.Structure_Template.FirstOrDefault(t => t.ID == template.TemplateId)
            ?? throw new RequestException(ResultType.NoDataFound);

        if (changeTemplate != null)
            changeTemplate.Description = template.Description;
        DB.SaveChanges();

        return Ok(new Model_Result());
    }

    #endregion
}
