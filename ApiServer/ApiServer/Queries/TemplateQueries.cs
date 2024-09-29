﻿using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class TemplateQueries(NbbContext DB, ApplicationUser CurrentUser) : SharedItemQueries(DB, CurrentUser)
{
    public Model_TemplatePaging List(Model_FilterTemplate? model)
    {
        if (model is null)
            throw new RequestException(ResultCodes.DataIsInvalid);

        List<Model_TemplateItem> result = [];
        model = model with { Username = model.Username?.Normalize().ToLower() };

        if (string.IsNullOrEmpty(model.Username))
        {
            IEnumerable<Structure_Template> list = DB.Structure_Template
                .Where(t => t.UserID == CurrentUser.ID)
                .Include(t => t.O_User);

            list = Filter(list);
        }
        if (model.Public || !string.IsNullOrWhiteSpace(model.Username))
            LoadShared(PublicSharedItems(ShareType.Template), ShareVisibility.Public);
        if (model.Group || !string.IsNullOrWhiteSpace(model.Username))
            LoadShared(SharedViaGroupItems(ShareType.Template), ShareVisibility.Group);
        if (model.Directly || !string.IsNullOrWhiteSpace(model.Username))
            LoadShared(DirectlySharedItems(ShareType.Template), ShareVisibility.Directly);

        List<Model_TemplateItem> templates = result.DistinctBy(s => s.ID).ToList();

        int tCount = templates.Count;
        int maxPages = tCount / model.PerPage;
        if (model.Page > maxPages)
            model = model with { Page = 1 };
        templates = templates.Skip(model.Page * model.PerPage).Take(model.PerPage).ToList();

        Model_TemplatePaging paging = new(tCount, model.Page, maxPages, model.PerPage, templates);
        return paging;

        IEnumerable<Structure_Template> Filter(IEnumerable<Structure_Template> list)
        {
            if (!string.IsNullOrWhiteSpace(model.Name))
                list = list.Where(s => s.Name.Contains(model.Name));
            if (!string.IsNullOrWhiteSpace(model.Username) && !model.DirectUser)
                list = list.Where(s => s.O_User.UsernameNormalized.Contains(model.Username));
            if (!string.IsNullOrWhiteSpace(model.Username) && model.DirectUser)
                list = list.Where(s => s.O_User.UsernameNormalized == model.Username);
            if (!string.IsNullOrWhiteSpace(model.Tags))
                list = list.Where(s => !string.IsNullOrWhiteSpace(s.Tags) && model.Tags.Contains(s.Tags));
            return list.Distinct().OrderBy(s => s.LastUpdatedAt).ThenBy(s => s.Name);
        }

        void LoadShared(List<Model_ShareItem> shareList, ShareVisibility visibility)
        {
            foreach (Model_ShareItem item in shareList)
            {
                IEnumerable<Structure_Template> list = DB.Structure_Template
                    .Where(s => s.ID == item.ItemID)
                    .Include(s => s.O_User);

                list = Filter(list);
                result.AddRange(list.Select(s => new Model_TemplateItem(s.ID, s.Name, s.Description, s.Tags, s.O_User.Username, s.CreatedAt, s.LastUpdatedAt, visibility)));
            }
        }
    }

    public Model_Template Get(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);

        Structure_Template item = DB.Structure_Template.FirstOrDefault(t => t.ID == id)
            ?? throw new RequestException(ResultCodes.NoDataFound);

        List<Model_TemplateRow> rows = [];
        List<Structure_Template_Row> rowTemplate = [.. DB.Structure_Template_Row
            .Where(s => s.TemplateID == item.ID)
            .OrderBy(s => s.SortOrder)];

        foreach (Structure_Template_Row row in rowTemplate)
        {
            List<Model_TemplateCell> cells = [];
            List<Structure_Template_Cell> cellTemplate = [.. DB.Structure_Template_Cell
                .Where(s => s.RowID == row.ID)
                .OrderBy(s => s.SortOrder)];

            foreach (Structure_Template_Cell cell in cellTemplate)
            {
                Model_TemplateCell cellModel = new(cell.ID, cell.RowID, cell.InputHelper, cell.HideOnEmpty, cell.IsRequired, cell.Text, cell.MetaData);
                cells.Add(cellModel);
            }

            Model_TemplateRow rowModel = new(row.ID, row.CanWrapCells, row.CanRepeat, row.HideOnNoInput, cells);
            rows.Add(rowModel);
        }
        Model_Template model = new(item.ID, item.Name, item.Description, item.Tags, rows);

        return model;
    }

    public void Remove(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);

        List<Structure_Entry> entries = [.. DB.Structure_Entry.Where(e => e.TemplateID == id)];
        foreach (Structure_Entry entry in entries)
        {
            List<Structure_Entry_Row> entryRows = [.. DB.Structure_Entry_Row
                .Where(t => t.EntryID == entry.ID)
                .Include(s => s.O_Cells)];
            foreach (Structure_Entry_Row row in entryRows)
                DB.Structure_Entry_Cell.RemoveRange(row.O_Cells);
            DB.Structure_Entry_Row.RemoveRange(entryRows);
        }
        DB.Structure_Entry.RemoveRange(entries);

        Structure_Template? template = DB.Structure_Template.FirstOrDefault(t => t.ID == id)
            ?? throw new RequestException(ResultCodes.NoDataFound);
        List<Structure_Template_Row> rows = [.. DB.Structure_Template_Row
            .Where(t => t.TemplateID == id)
            .Include(s => s.O_Cells)];

        foreach (Structure_Template_Row row in rows)
            DB.Structure_Template_Cell.RemoveRange(row.O_Cells);

        DB.Structure_Template_Row.RemoveRange(rows);
        DB.Structure_Template.Remove(template);

        DB.SaveChanges();
    }

    public void Copy(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);

        Structure_Template? copyTemplate = DB.Structure_Template.FirstOrDefault(t => t.ID == id)
            ?? throw new RequestException(ResultCodes.NoDataFound);

        Structure_Template template = new()
        {
            ID = Guid.NewGuid(),
            UserID = CurrentUser.ID,
            Name = $"{copyTemplate.Name} (Kopie)",
            Description = copyTemplate.Description,
            Tags = copyTemplate.Tags,
        };
        DB.Structure_Template.Add(template);

        foreach (Structure_Template_Row row in DB.Structure_Template_Row.Where(t => t.TemplateID == copyTemplate.ID).ToList())
        {
            Structure_Template_Row newRow = new()
            {
                ID = Guid.NewGuid(),
                TemplateID = template.ID,
                SortOrder = row.SortOrder,
                CanWrapCells = row.CanWrapCells,
                CanRepeat = row.CanRepeat,
                HideOnNoInput = row.CanRepeat,
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
                    IsRequired = cell.IsRequired,
                    Text = cell.Text,
                    Description = cell.Description,
                    MetaData = cell.MetaData
                };
                DB.Structure_Template_Cell.Add(newCell);
            }
        }

        DB.SaveChanges();
    }

    public void Update(Model_Template? model)
    {
        if (model is null || string.IsNullOrWhiteSpace(model.Name))
            throw new RequestException(ResultCodes.DataIsInvalid);

        Structure_Template? template = DB.Structure_Template.FirstOrDefault(s => s.ID == model.ID);
        if (template is null)
        {
            template = new()
            {
                ID = Guid.NewGuid(),
                UserID = CurrentUser.ID,
                Name = model.Name,
                Description = model.Description,
                Tags = model.Tags,
            };
            DB.Structure_Template.Add(template);
        }
        else
        {
            template.Name = model.Name;
            template.Description = model.Description;
            template.Tags = model.Tags;
        }

        int rowSortOrder = 0;
        foreach (Model_TemplateRow rowItem in model.Items)
        {
            Structure_Template_Row? rowTemplate = DB.Structure_Template_Row.SingleOrDefault(t => rowItem.ID == t.ID);

            if (rowTemplate is null)
            {
                rowTemplate = new()
                {
                    ID = Guid.NewGuid(),
                    TemplateID = template.ID,
                    SortOrder = rowSortOrder++,
                    CanWrapCells = rowItem.CanWrapCells,
                    CanRepeat = rowItem.CanRepeat,
                    HideOnNoInput = rowItem.HideOnNoInput
                };
                DB.Structure_Template_Row.Add(rowTemplate);
            }
            else
            {
                rowTemplate.SortOrder = rowSortOrder++;
                rowTemplate.CanWrapCells = rowItem.CanWrapCells;
                rowTemplate.CanRepeat = rowItem.CanRepeat;
                rowTemplate.HideOnNoInput = rowItem.HideOnNoInput;
            }

            int cellSortOrder = 0;
            foreach (Model_TemplateCell cell in rowItem.Items)
            {
                Structure_Template_Cell? cellTemplate = DB.Structure_Template_Cell.SingleOrDefault(c => c.ID == cell.ID);

                if (cellTemplate is null)
                {
                    cellTemplate = new()
                    {
                        ID = Guid.NewGuid(),
                        RowID = rowItem.ID,
                        SortOrder = cellSortOrder++,
                        InputHelper = cell.InputHelper,
                        HideOnEmpty = cell.HideOnEmpty,
                        IsRequired = cell.IsRequired,
                        Text = cell.Text,
                        MetaData = cell.Text
                    };
                    DB.Structure_Template_Cell.Add(cellTemplate);
                }
                else
                {
                    cellTemplate.SortOrder = cellSortOrder++;
                    cellTemplate.InputHelper = cell.InputHelper;
                    cellTemplate.HideOnEmpty = cell.HideOnEmpty;
                    cellTemplate.IsRequired = cell.IsRequired;
                    cellTemplate.Text = cell.Text;
                    cellTemplate.MetaData = cell.MetaData;
                }
            }
        }

        DB.SaveChanges();
    }
}