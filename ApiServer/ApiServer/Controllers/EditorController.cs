using Microsoft.AspNetCore.Authorization;
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
    /// gets entry details
    /// </summary>
    /// <param name="id">entryId</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_Editor>))]
    [HttpGet(nameof(GetEntry))]
    public IActionResult GetEntry(Guid? id)
    {
        Model_Editor result = Query.GetEntry(id);
        return Ok(new Model_Result<Model_Editor>(result));
    }

    /// <summary>
    /// gets template details
    /// </summary>
    /// <param name="id">templateId</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_Editor>))]
    [HttpGet(nameof(GetTemplate))]
    public IActionResult GetTemplate(Guid? id)
    {
        Model_Editor result = Query.GetTemplate(id);
        return Ok(new Model_Result<Model_Editor>(result));
    }

    /// <summary>
    /// creates or updates a entry
    /// </summary>
    /// <param name="model">contains entry data and template</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_Editor>))]
    [HttpPost(nameof(UpdateEntry))]
    public IActionResult UpdateEntry([FromBody] Model_Editor? model)
    {
        Model_Editor result = Query.UpdateEntry(model);
        return Ok(new Model_Result<Model_Editor>(result));
    }

    /// <summary>
    /// creates or updates a template
    /// </summary>
    /// <param name="model">contains empty entry data but template data</param>
    /// <returns></returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_Editor>))]
    [HttpPost(nameof(UpdateTemplate))]
    public IActionResult UpdateTemplate([FromBody] Model_Editor? model)
    {
        Model_Editor result = Query.UpdateTemplate(model);
        return Ok(new Model_Result<Model_Editor>(result));
    }
}
