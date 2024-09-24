using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using StyleWerk.NBB.Authentication;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.User;
using StyleWerk.NBB.Models;

namespace StyleWerk.NBB.Controllers;

[ApiController, AllowAnonymous, Route("Auth")]

public class AuthController(NbbContext db, IAuthenticationService Authentication) : BaseController(db)
{
    private string UserAgent => Request.Headers.UserAgent.ToString();

    #region Login
    [ApiExplorerSettings(GroupName = "Login")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<AuthenticationResult>))]
    [HttpPost(nameof(Login))]
    public IActionResult Login([FromBody] Model_Login? model)
    {
        User_Login user = Authentication.GetUser(model);
        Model_Token accessToken = Authentication.GetAccessToken(user);
        Model_Token refreshToken = Authentication.GetRefreshToken(user.ID, UserAgent, model?.ConsistOverSession);
        AuthenticationResult result = Authentication.GetAuthenticationResult(user.ID, accessToken, refreshToken, model?.ConsistOverSession);

        return Ok(new Model_Result<AuthenticationResult>(result));
    }

    [ApiExplorerSettings(GroupName = "Login")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<AuthenticationResult>))]
    [HttpPost(nameof(RefreshToken))]
    public IActionResult RefreshToken([FromBody] Model_RefreshToken? model)
    {
        User_Login user = Authentication.GetUser(model, UserAgent);
        Model_Token accessToken = Authentication.GetAccessToken(user);
        Model_Token refreshToken = Authentication.GetRefreshToken(user.ID, UserAgent, model?.ConsistOverSession);
        AuthenticationResult result = Authentication.GetAuthenticationResult(user.ID, accessToken, refreshToken, model?.ConsistOverSession);

        return Ok(new Model_Result<AuthenticationResult>(result));
    }
    #endregion

    #region Registration
    [ApiExplorerSettings(GroupName = "Registration")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<object>))]
    [HttpPost(nameof(Registration))]
    public IActionResult Registration([FromBody] Model_Registration? model)
    {
        Authentication.Registration(model);
        return Ok(new Model_Result<object>());
    }

    [ApiExplorerSettings(GroupName = "Registration")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<object>))]
    [HttpPost(nameof(VerifyEmail))]
    public IActionResult VerifyEmail(Guid? token)
    {
        Authentication.VerifyEmail(token);
        return Ok(new Model_Result<object>());
    }
    #endregion

    #region Forgot Password
    [ApiExplorerSettings(GroupName = "Forgot Password")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<object>))]
    [HttpPost(nameof(RequestPasswordReset))]
    public IActionResult RequestPasswordReset([FromBody] string email)
    {
        Authentication.RequestPasswordReset(email);
        return Ok(new Model_Result<object>());
    }

    [ApiExplorerSettings(GroupName = "Forgot Password")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<object>))]
    [HttpPost(nameof(ResetPassword))]
    public IActionResult ResetPassword([FromBody] Model_ResetPassword? model)
    {
        Authentication.ResetPassword(model);
        return Ok(new Model_Result<object>());
    }
    #endregion

    #region Session
    [ApiExplorerSettings(GroupName = "Session")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<object>))]
    [HttpPost(nameof(RemoveSessions)), Authorize]
    public IActionResult RemoveSessions()
    {
        Authentication.RemoveSessions(CurrentUser.ID, UserAgent);
        return Ok(new Model_Result<object>());
    }

    [ApiExplorerSettings(GroupName = "Session")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<object>))]
    [HttpPost(nameof(Logout)), Authorize]
    public IActionResult Logout()
    {
        Authentication.Logout(CurrentUser.ID, UserAgent);
        return Ok(new Model_Result<object>());
    }
    #endregion

    #region Change Email
    [ApiExplorerSettings(GroupName = "Change Email")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<object>))]
    [HttpPost(nameof(UpdateEmail)), Authorize]
    public IActionResult UpdateEmail(string? email)
    {
        Authentication.UpdateEmail(email, CurrentUser.Login);
        return Ok(new Model_Result<object>());
    }

    [ApiExplorerSettings(GroupName = "Change Email")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<object>))]
    [HttpPost(nameof(VerifyUpdatedEmail)), Authorize]
    public IActionResult VerifyUpdatedEmail(string? code)
    {
        Authentication.VerifyUpdatedEmail(code, CurrentUser.Login, UserAgent);
        return Ok(new Model_Result<object>());
    }
    #endregion

    #region Userdata
    [ApiExplorerSettings(GroupName = "Userdata")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_UserData>))]
    [HttpPost(nameof(GetUserData)), Authorize]
    public IActionResult GetUserData()
    {
        Model_UserData user = Authentication.GetUserData(CurrentUser);
        return Ok(new Model_Result<Model_UserData>(user));
    }

    [ApiExplorerSettings(GroupName = "Userdata")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<object>))]
    [HttpPost(nameof(UpdateUserData)), Authorize]
    public IActionResult UpdateUserData([FromBody] Model_UpdateUserData model)
    {
        Authentication.UpdateUserData(model, CurrentUser.ID);
        return Ok(new Model_Result<object>());
    }
    #endregion

    #region Validation
    [ApiExplorerSettings(GroupName = "Validation")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<object>))]
    [HttpPost(nameof(ValidatePassword))]
    public IActionResult ValidatePassword([FromBody] Model_ValidateIdentification model)
    {
        Authentication.ValidatePassword(model?.ToValidate);
        return Ok(new Model_Result<object>());
    }

    [ApiExplorerSettings(GroupName = "Validation")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<object>))]
    [HttpPost(nameof(ValidateEmail))]
    public IActionResult ValidateEmail([FromBody] Model_ValidateIdentification? model)
    {
        Authentication.ValidateEmail(model?.ToValidate);
        return Ok(new Model_Result<object>());
    }

    [ApiExplorerSettings(GroupName = "Validation")]
    [Produces("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<object>))]
    [HttpPost(nameof(ValidateUsername))]
    public IActionResult ValidateUsername([FromBody] Model_ValidateIdentification? model)
    {
        Authentication.ValidateUsername(model?.ToValidate);
        return Ok(new Model_Result<object>());
    }
    #endregion
}