﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using StyleWerk.NBB.Database;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace StyleWerk.NBB.Controllers;

[ApiController, Route("Editor"), Authorize]
public class EditorController(NbbContext db) : BaseController(db)
{
    public EditorQueries Query => new(DB, CurrentUser);

    /// <summary>
    /// Gets entry details
    /// </summary>
    /// <param name="id">entry ID</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_Editor>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NoDataFound)]
    [HttpGet(nameof(GetEntry))]
    public IActionResult GetEntry(Guid? id)
    {
        Model_Editor result = Query.GetEntry(id);
        return Ok(new Model_Result<Model_Editor>(result));
    }

    /// <summary>
    /// Gets template details
    /// </summary>
    /// <param name="id">template ID</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_Editor>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NoDataFound)]
    [HttpGet(nameof(GetTemplate))]
    public IActionResult GetTemplate(Guid? id)
    {
        Model_Editor result = Query.GetTemplate(id);
        return Ok(new Model_Result<Model_Editor>(result));
    }

    /// <summary>
    /// Creates or updates a entry
    /// </summary>
    /// <param name="model">contains entry data and template that the entry is based on</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_Editor>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NoDataFound, ResultCodes.YouDontOwnTheData, ResultCodes.NameMustBeUnique, ResultCodes.TemplateDoesntMatch)]
    [HttpPost(nameof(UpdateEntry))]
    public IActionResult UpdateEntry([FromBody] Model_Editor? model)
    {
        Model_Editor result = Query.UpdateEntry(model);
        return Ok(new Model_Result<Model_Editor>(result));
    }

    /// <summary>
    /// Creates or updates a template
    /// </summary>
    /// <param name="model">contains empty entry data but template data</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_Editor>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NameMustBeUnique, ResultCodes.YouDontOwnTheData, ResultCodes.NoDataFound)]
    [HttpPost(nameof(UpdateTemplate))]
    public IActionResult UpdateTemplate([FromBody] Model_Editor? model)
    {
        Model_Editor result = Query.UpdateTemplate(model);
        return Ok(new Model_Result<Model_Editor>(result));
    }
}
