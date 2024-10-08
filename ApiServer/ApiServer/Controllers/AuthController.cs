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
    public AuthQueries Authentication => new(DB, CurrentUser, Request.Headers.UserAgent.ToString(), SecretData.Value);

    #region Login
    /// <summary>
    /// login for the user
    /// </summary>
    /// <param name="model">contains username and password</param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Login")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<AuthenticationResult>))]
    [HttpPost(nameof(Login))]
    public IActionResult Login([FromBody] Model_Login? model)
    {
        AuthenticationResult result = Authentication.Login(model);
        return Ok(new Model_Result<AuthenticationResult>(result));
    }

    /// <summary>
    /// sets the refresh token
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
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
    /// <summary>
    /// registers a user
    /// </summary>
    /// <param name="model">contains the user information</param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Registration")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(Registration))]
    public IActionResult Registration([FromBody] Model_Registration? model)
    {
        string? result = Authentication.Registration(model);
        return Ok(new Model_Result<string>(result));
    }

    /// <summary>
    /// verifies the email of the registered user
    /// </summary>
    /// <param name="token">status token of the user</param>
    /// <returns></returns>
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
    /// <summary>
    /// sets the status code to Password reset and sends an email 
    /// </summary>
    /// <param name="email">email of the user</param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Forgot Password")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(RequestPasswordReset))]
    public IActionResult RequestPasswordReset([FromBody] string? email)
    {
        string? result = Authentication.RequestPasswordReset(email);
        return Ok(new Model_Result<string>(result));
    }

    /// <summary>
    /// resets password and sets the status code, token and expire time to null 
    /// </summary>
    /// <param name="model">contains new password and status token</param>
    /// <returns></returns>
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

    /// <summary>
    /// removes the current session for the current user
    /// </summary>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Session")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(RemoveSessions)), Authorize]
    public IActionResult RemoveSessions()
    {
        Authentication.RemoveSessions();
        return Ok(new Model_Result<string>());
    }

    /// <summary>
    /// log out for the current user
    /// </summary>
    /// <returns></returns>
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
    /// <summary>
    /// changes the email
    /// </summary>
    /// <param name="email">email of the user</param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Change Email")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(UpdateEmail)), Authorize]
    public IActionResult UpdateEmail(string? email)
    {
        string? result = Authentication.UpdateEmail(email);
        return Ok(new Model_Result<string>(result));
    }

    /// <summary>
    /// verifies the updated email 
    /// </summary>
    /// <param name="code">status token</param>
    /// <returns></returns>
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
    /// <summary>
    /// Getting the User details of the current user
    /// </summary>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Userdata")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_UserData>))]
    [HttpPost(nameof(GetUserData)), Authorize]
    public IActionResult GetUserData()
    {
        Model_UserData user = Authentication.GetUserData();
        return Ok(new Model_Result<Model_UserData>(user));
    }

    /// <summary>
    /// updates the User details for the current user
    /// </summary>
    /// <param name="model">contains password, firstname, lastname and gender</param>
    /// <returns></returns>
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
    /// <summary>
    /// decides whether the password complies with our conditions
    /// </summary>
    /// <param name="model">contains the password</param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Validation")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(ValidatePassword))]
    public IActionResult ValidatePassword([FromBody] Model_ValidateIdentification model)
    {
        Authentication.ValidatePassword(model?.ToValidate);
        return Ok(new Model_Result<string>());
    }

    /// <summary>
    /// decides whether the email complies with our conditions
    /// </summary>
    /// <param name="model">contains the email</param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Validation")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [HttpPost(nameof(ValidateEmail))]
    public IActionResult ValidateEmail([FromBody] Model_ValidateIdentification? model)
    {
        Authentication.ValidateEmail(model?.ToValidate);
        return Ok(new Model_Result<string>());
    }

    /// <summary>
    /// decides whether the username complies with our conditions
    /// </summary>
    /// <param name="model">contains the username</param>
    /// <returns></returns>
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