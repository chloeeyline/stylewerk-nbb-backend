using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using StyleWerk.NBB.Authentication;
using StyleWerk.NBB.AWS;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Controllers;

[ApiController, AllowAnonymous, Route("Auth")]

public class AuthController(NbbContext db, IOptions<SecretData> SecretData) : BaseController(db)
{
    public AuthQueries Authentication => new(DB, CurrentUser, Request.Headers.UserAgent.ToString(), SecretData);

    #region Login
    [ApiExplorerSettings(GroupName = "Login")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<AuthenticationResult>))]
    [HttpPost(nameof(Login))]
    public IActionResult Login([FromBody] Model_Login? model)
    {
        AuthenticationResult result = Authentication.Login(model);
        return Ok(new Model_Result<AuthenticationResult>(result));
    }

    [ApiExplorerSettings(GroupName = "Login")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<AuthenticationResult>))]
    [HttpPost(nameof(RefreshToken))]
    public IActionResult RefreshToken([FromBody] Model_RefreshToken? model)
    {
        AuthenticationResult result = Authentication.RefreshToken(model);
        return Ok(new Model_Result<AuthenticationResult>(result));
    }
    #endregion

    #region Registration
    [ApiExplorerSettings(GroupName = "Registration")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(Registration))]
    public IActionResult Registration([FromBody] Model_Registration? model)
    {
        Authentication.Registration(model);
        return Ok(new Model_Result<string>());
    }

    [ApiExplorerSettings(GroupName = "Registration")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(VerifyEmail))]
    public IActionResult VerifyEmail(string? token)
    {
        Authentication.VerifyEmail(token);
        return Ok(new Model_Result<string>());
    }
    #endregion

    #region Forgot Password
    [ApiExplorerSettings(GroupName = "Forgot Password")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(RequestPasswordReset))]
    public IActionResult RequestPasswordReset([FromBody] string email)
    {
        Authentication.RequestPasswordReset(email);
        return Ok(new Model_Result<string>());
    }

    [ApiExplorerSettings(GroupName = "Forgot Password")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(ResetPassword))]
    public IActionResult ResetPassword([FromBody] Model_ResetPassword? model)
    {
        Authentication.ResetPassword(model);
        return Ok(new Model_Result<string>());
    }
    #endregion

    #region Session
    [ApiExplorerSettings(GroupName = "Session")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(RemoveSessions)), Authorize]
    public IActionResult RemoveSessions()
    {
        Authentication.RemoveSessions();
        return Ok(new Model_Result<string>());
    }

    [ApiExplorerSettings(GroupName = "Session")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(Logout)), Authorize]
    public IActionResult Logout()
    {
        Authentication.Logout();
        return Ok(new Model_Result<string>());
    }
    #endregion

    #region Change Email
    [ApiExplorerSettings(GroupName = "Change Email")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(UpdateEmail)), Authorize]
    public IActionResult UpdateEmail(string? email)
    {
        Authentication.UpdateEmail(email);
        return Ok(new Model_Result<string>());
    }

    [ApiExplorerSettings(GroupName = "Change Email")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(VerifyUpdatedEmail)), Authorize]
    public IActionResult VerifyUpdatedEmail(string? code)
    {
        Authentication.VerifyUpdatedEmail(code);
        return Ok(new Model_Result<string>());
    }
    #endregion

    #region Userdata
    [ApiExplorerSettings(GroupName = "Userdata")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_UserData>))]
    [HttpPost(nameof(GetUserData)), Authorize]
    public IActionResult GetUserData()
    {
        Model_UserData user = Authentication.GetUserData();
        return Ok(new Model_Result<Model_UserData>(user));
    }

    [ApiExplorerSettings(GroupName = "Userdata")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(UpdateUserData)), Authorize]
    public IActionResult UpdateUserData([FromBody] Model_UpdateUserData model)
    {
        Authentication.UpdateUserData(model);
        return Ok(new Model_Result<string>());
    }
    #endregion

    #region Validation
    [ApiExplorerSettings(GroupName = "Validation")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(ValidatePassword))]
    public IActionResult ValidatePassword([FromBody] Model_ValidateIdentification model)
    {
        Authentication.ValidatePassword(model?.ToValidate);
        return Ok(new Model_Result<string>());
    }

    [ApiExplorerSettings(GroupName = "Validation")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(ValidateEmail))]
    public IActionResult ValidateEmail([FromBody] Model_ValidateIdentification? model)
    {
        Authentication.ValidateEmail(model?.ToValidate);
        return Ok(new Model_Result<string>());
    }

    [ApiExplorerSettings(GroupName = "Validation")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(ValidateUsername))]
    public IActionResult ValidateUsername([FromBody] Model_ValidateIdentification? model)
    {
        Authentication.ValidateUsername(model?.ToValidate);
        return Ok(new Model_Result<string>());
    }
    #endregion
}