﻿using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Core;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Queries;

public class EditorQueries(NbbContext DB, ApplicationUser CurrentUser) : BaseQueries(DB, CurrentUser)
{
    private static List<EntryCell> CreateCellsFromTemplate(List<Structure_Template_Cell> list)
    {
        return [.. list.OrderBy(s => s.SortOrder).Select(s =>
            new EntryCell(
                Guid.NewGuid(),
                s.ID,
                null,
                new TemplateCell(
                    s.ID,
                    s.InputHelper,
                    s.HideOnEmpty,
                    s.IsRequired,
                    s.Text,
                    s.Description,
                    s.MetaData
                )
            )
        )];
    }

    public Model_Editor GetEntry(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);

        Structure_Entry eEntity = DB.Structure_Entry
            .Include(s => s.O_Template)
                .ThenInclude(s => s.O_Rows)
                .ThenInclude(s => s.O_Cells)
            .Include(s => s.O_Rows) // Load rows for the entry
                .ThenInclude(s => s.O_Template) // Include template for each row
            .Include(s => s.O_Rows) // Load cells for each row
                .ThenInclude(s => s.O_Cells)
                .ThenInclude(s => s.O_Template) // Include template for each cell
            .FirstOrDefault(e => e.ID == id)
            ?? throw new RequestException(ResultCodes.NoDataFound);

        Structure_Template tEntity = eEntity.O_Template ?? throw new RequestException(ResultCodes.NoDataFound);
        List<Structure_Template_Row> tRowEntities = [.. tEntity.O_Rows.OrderBy(s => s.SortOrder)];

        List<EntryRow> entryRows = [];
        List<EntryCell> entryCells = [];
        foreach (Structure_Template_Row tRowEntity in tRowEntities)
        {
            List<Structure_Entry_Row> eRowEntities = [.. eEntity.O_Rows
                .Where(s => s.TemplateID == tRowEntity.ID)
                .OrderBy(s => s.SortOrder)];
            TemplateRow tRowModel = new(tRowEntity.ID, tRowEntity.CanRepeat, tRowEntity.HideOnNoInput);

            if (eRowEntities.Count == 0)
            {
                List<EntryCell> cells = CreateCellsFromTemplate(tRowEntity.O_Cells);
                EntryRow eRowModel = new(Guid.NewGuid(), tRowEntity.ID, tRowModel, cells);
                entryRows.Add(eRowModel);
            }
            else
            {
                foreach (Structure_Entry_Row eRowEntity in eRowEntities)
                {
                    List<Structure_Template_Cell> tCellEntities = [.. tRowEntity.O_Cells.OrderBy(s => s.SortOrder)];

                    entryCells = [];
                    foreach (Structure_Template_Cell tCellEntity in tCellEntities)
                    {
                        Structure_Entry_Cell? eCellEntity = DB.Structure_Entry_Cell.FirstOrDefault(s => s.TemplateID == tCellEntity.ID && s.RowID == eRowEntity.ID);

                        EntryCell cellModel;
                        TemplateCell cellModelTemplate = new(tCellEntity.ID, tCellEntity.InputHelper, tCellEntity.HideOnEmpty, tCellEntity.IsRequired, tCellEntity.Text, tCellEntity.Description, tCellEntity.MetaData);
                        cellModel = eCellEntity is null
                            ? new(Guid.NewGuid(), tCellEntity.ID, null, cellModelTemplate)
                            : new(eCellEntity.ID, tCellEntity.ID, eCellEntity.Data, cellModelTemplate);
                        entryCells.Add(cellModel);
                    }

                    EntryRow eRowModel = new(eRowEntity.ID, tRowEntity.ID, tRowModel, entryCells);
                    entryRows.Add(eRowModel);
                }
            }
        }

        Template templateModel = new(tEntity.ID, tEntity.Name, tEntity.IsPublic, tEntity.Description, tEntity.Description);
        Model_Editor editorModel = new(eEntity.ID, eEntity.FolderID, tEntity.ID, eEntity.Name, eEntity.Tags, eEntity.IsPublic, templateModel, eEntity.UserID == CurrentUser.ID, entryRows);

        return editorModel;
    }

    public Model_Editor GetTemplate(Guid? id)
    {
        if (id is null || id == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);

        Structure_Template tEntity = DB.Structure_Template
            .Include(s => s.O_Rows)
                .ThenInclude(s => s.O_Cells)
            .FirstOrDefault(e => e.ID == id)
            ?? throw new RequestException(ResultCodes.NoDataFound);

        List<Structure_Template_Row> tRowEntities = [.. tEntity.O_Rows.OrderBy(s => s.SortOrder)];

        List<EntryRow> entryRows = [];
        foreach (Structure_Template_Row tRowEntity in tRowEntities)
        {
            List<Structure_Template_Cell> tCellEntities = [.. tRowEntity.O_Cells.OrderBy(cell => cell.SortOrder)];
            List<EntryCell> entryCells = CreateCellsFromTemplate(tCellEntities);

            TemplateRow tRowModel = new(tRowEntity.ID, tRowEntity.CanRepeat, tRowEntity.HideOnNoInput);
            EntryRow eRowModel = new(Guid.NewGuid(), tRowEntity.ID, tRowModel, entryCells);
            entryRows.Add(eRowModel);
        }

        Template templateModel = new(tEntity.ID, tEntity.Name, tEntity.IsPublic, tEntity.Description, tEntity.Tags);
        Model_Editor editorModel = new(Guid.NewGuid(), null, tEntity.ID, null, null, false, templateModel, tEntity.UserID == CurrentUser.ID, entryRows);

        return editorModel;
    }

    public Model_Editor UpdateTemplate(Model_Editor? model)
    {
        if (model is null || model.Template is null || string.IsNullOrWhiteSpace(model.Template.Name))
            throw new RequestException(ResultCodes.DataIsInvalid);

        string name = model.Template.Name.NormalizeName();
        Template templateModel = model.Template;
        Structure_Template? templateEntity = DB.Structure_Template.FirstOrDefault(s => s.ID == templateModel.ID);
        if (templateEntity is null)
        {
            if (DB.Structure_Template.Any(s => s.UserID == CurrentUser.ID && s.NameNormalized == name))
                throw new RequestException(ResultCodes.NameMustBeUnique);

            templateEntity = new()
            {
                ID = Guid.NewGuid(),
                UserID = CurrentUser.ID,
                Name = templateModel.Name,
                NameNormalized = templateModel.Name.NormalizeName(),
                IsPublic = templateModel.IsPublic,
                Description = string.IsNullOrWhiteSpace(templateModel.Description) ? null : templateModel.Description,
                Tags = string.IsNullOrWhiteSpace(templateModel.Tags) ? null : string.Join(",", templateModel.Tags.Normalize().ToLower().Split(',').Order())
            };
            DB.Structure_Template.Add(templateEntity);
        }
        else
        {
            if (templateEntity.UserID != CurrentUser.ID)
                throw new RequestException(ResultCodes.YouDontOwnTheData);
            if (templateEntity.NameNormalized != name && DB.Structure_Template.Any(s => s.UserID == CurrentUser.ID && s.NameNormalized == name))
                throw new RequestException(ResultCodes.NameMustBeUnique);

            templateEntity.Name = templateModel.Name;
            templateEntity.Description = string.IsNullOrWhiteSpace(templateModel.Description) ? null : templateModel.Description;
            templateEntity.Tags = string.IsNullOrWhiteSpace(templateModel.Tags) ? null : templateModel.Tags.Normalize().ToLower();
            templateEntity.IsPublic = templateModel.IsPublic;
        }

        List<Guid> rowIDs = [];
        int rowSortOrder = 0;
        foreach (EntryRow eRowModel in model.Items)
        {
            TemplateRow tRowModel = eRowModel.Template ?? throw new RequestException(ResultCodes.NoDataFound);
            Structure_Template_Row? tRowEntity = DB.Structure_Template_Row.SingleOrDefault(t => tRowModel.ID == t.ID);

            if (tRowEntity is null)
            {
                tRowEntity = new()
                {
                    ID = Guid.NewGuid(),
                    TemplateID = templateEntity.ID,
                    SortOrder = rowSortOrder++,
                    CanRepeat = tRowModel.CanRepeat,
                    HideOnNoInput = tRowModel.HideOnNoInput
                };
                DB.Structure_Template_Row.Add(tRowEntity);
            }
            else
            {
                tRowEntity.SortOrder = rowSortOrder++;
                tRowEntity.CanRepeat = tRowModel.CanRepeat;
                tRowEntity.HideOnNoInput = tRowModel.HideOnNoInput;
            }

            rowIDs.Add(tRowEntity.ID);
            int cellSortOrder = 0;
            List<Guid> cellIDs = [];

            foreach (EntryCell eCellModel in eRowModel.Items)
            {
                TemplateCell tCellModel = eCellModel.Template ?? throw new RequestException(ResultCodes.NoDataFound);
                Structure_Template_Cell? tCellEntity = DB.Structure_Template_Cell.SingleOrDefault(c => c.ID == tCellModel.ID);

                if (tCellEntity is null)
                {
                    tCellEntity = new()
                    {
                        ID = Guid.NewGuid(),
                        RowID = tRowEntity.ID,
                        SortOrder = cellSortOrder++,
                        InputHelper = tCellModel.InputHelper,
                        HideOnEmpty = tCellModel.HideOnEmpty,
                        IsRequired = tCellModel.IsRequired,
                        Text = tCellModel.Text,
                        Description = tCellModel.Description,
                        MetaData = tCellModel.MetaData
                    };
                    DB.Structure_Template_Cell.Add(tCellEntity);
                }
                else
                {
                    tCellEntity.SortOrder = cellSortOrder++;
                    tCellEntity.InputHelper = tCellModel.InputHelper;
                    tCellEntity.HideOnEmpty = tCellModel.HideOnEmpty;
                    tCellEntity.IsRequired = tCellModel.IsRequired;
                    tCellEntity.Text = tCellModel.Text;
                    tCellEntity.Description = tCellModel.Description;
                    tCellEntity.MetaData = tCellModel.MetaData;
                }
                cellIDs.Add(tCellEntity.ID);
            }
            List<Structure_Template_Cell> deleteCellList = [.. DB.Structure_Template_Cell.Where(s => !cellIDs.Contains(s.ID) && s.RowID == tRowEntity.ID)];
            DB.Structure_Template_Cell.RemoveRange(deleteCellList);
        }
        List<Structure_Template_Row> deleteRowList = [.. DB.Structure_Template_Row.Where(s => !rowIDs.Contains(s.ID) && s.TemplateID == templateEntity.ID)];
        DB.Structure_Template_Row.RemoveRange(deleteRowList);

        DB.SaveChanges();
        return GetTemplate(templateEntity.ID);
    }

    public Model_Editor UpdateEntry(Model_Editor? model)
    {
        if (model is null || string.IsNullOrWhiteSpace(model.Name) || model.TemplateID == Guid.Empty)
            throw new RequestException(ResultCodes.DataIsInvalid);

        string name = model.Name.NormalizeName();
        Structure_Entry? entryEntity = DB.Structure_Entry.FirstOrDefault(s => s.ID == model.ID);

        Structure_Template templateEntity = DB.Structure_Template.FirstOrDefault(s => s.ID == model.TemplateID)
            ?? throw new RequestException(ResultCodes.NoDataFound);
        if (templateEntity.UserID != CurrentUser.ID)
            throw new RequestException(ResultCodes.YouDontOwnTheData);

        if (model.FolderID is not null && model.FolderID != Guid.Empty)
        {
            Structure_Entry_Folder? folderEntity = DB.Structure_Entry_Folder.FirstOrDefault(s => s.ID == model.FolderID)
                ?? throw new RequestException(ResultCodes.NoDataFound);
            if (folderEntity.UserID != CurrentUser.ID)
                throw new RequestException(ResultCodes.YouDontOwnTheData);
        }

        if (entryEntity is null)
        {
            if (DB.Structure_Entry.Any(s => s.UserID == CurrentUser.ID && s.NameNormalized == name))
                throw new RequestException(ResultCodes.NameMustBeUnique);

            entryEntity = new()
            {
                ID = Guid.NewGuid(),
                UserID = CurrentUser.ID,
                FolderID = model.FolderID == Guid.Empty ? null : model.FolderID,
                TemplateID = model.TemplateID,
                Name = model.Name,
                NameNormalized = model.Name.NormalizeName(),
                Tags = string.IsNullOrWhiteSpace(model.Tags) ? null : string.Join(",", model.Tags.Normalize().ToLower().Split(',').Order()),
                IsPublic = model.IsPublic,
            };

            DB.Structure_Entry.Add(entryEntity);
        }
        else
        {
            if (entryEntity.UserID != CurrentUser.ID)
                throw new RequestException(ResultCodes.YouDontOwnTheData);
            if (entryEntity.NameNormalized != name && DB.Structure_Entry.Any(s => s.UserID == CurrentUser.ID && s.NameNormalized == name))
                throw new RequestException(ResultCodes.NameMustBeUnique);
            if (entryEntity.TemplateID != model.TemplateID)
                throw new RequestException(ResultCodes.TemplateDoesntMatch);

            entryEntity.FolderID = model.FolderID == Guid.Empty ? null : model.FolderID;
            entryEntity.Name = model.Name;
            entryEntity.Tags = string.IsNullOrWhiteSpace(model.Tags) ? null : model.Tags?.Normalize().ToLower();
            entryEntity.IsPublic = model.IsPublic;
        }

        List<Guid> rowIDs = [];
        Guid rowTemplateID = Guid.Empty;
        int rowSortOrder = 0;
        foreach (EntryRow row in model.Items)
        {
            Structure_Entry_Row? entryRow = DB.Structure_Entry_Row.FirstOrDefault(s => s.ID == row.ID);
            if (rowTemplateID == row.TemplateID)
                rowSortOrder++;

            bool isNewRow = false;
            if (entryRow is null)
            {
                isNewRow = true;
                entryRow = new()
                {
                    ID = Guid.NewGuid(),
                    EntryID = entryEntity.ID,
                    TemplateID = row.TemplateID,
                    SortOrder = rowSortOrder
                };
            }
            else
            {
                entryRow.SortOrder = rowSortOrder;
            }

            bool hasData = false;
            foreach (EntryCell cell in row.Items)
            {
                Structure_Entry_Cell? entryCell = DB.Structure_Entry_Cell.FirstOrDefault(s => s.ID == cell.ID);
                if (entryCell is null)
                {
                    if (string.IsNullOrWhiteSpace(cell.Data))
                        continue;

                    hasData = true;
                    entryCell = new()
                    {
                        ID = Guid.NewGuid(),
                        RowID = entryRow.ID,
                        TemplateID = cell.TemplateID,
                        Data = cell.Data
                    };
                    DB.Structure_Entry_Cell.Add(entryCell);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(cell.Data))
                    {
                        DB.Structure_Entry_Cell.Remove(entryCell);
                        continue;
                    }
                    else
                    {
                        entryCell.Data = cell.Data;
                        hasData = true;
                    }
                }
            }

            if (!isNewRow && !hasData)
                DB.Structure_Entry_Row.Remove(entryRow);
            if (isNewRow && hasData)
            {
                DB.Structure_Entry_Row.Add(entryRow);
                rowIDs.Add(row.ID);
            }
            else
            {
                rowIDs.Add(row.ID);
            }
            rowSortOrder = rowTemplateID == entryRow.TemplateID ? rowSortOrder + 1 : 0;
            rowTemplateID = entryRow.TemplateID;
        }
        List<Structure_Entry_Row> deleteRowList = [.. DB.Structure_Entry_Row.Where(s => !rowIDs.Contains(s.ID) && s.EntryID == entryEntity.ID)];
        DB.Structure_Entry_Row.RemoveRange(deleteRowList);
        DB.SaveChanges();

        return GetEntry(entryEntity.ID);
    }
}
