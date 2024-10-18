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
    /// Login with username and password
    /// </summary>
    /// <param name="model">contains username and password and whether the login should consist over session</param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Login")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<AuthenticationResult>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NoUserFound, ResultCodes.EmailIsNotVerified,
    ResultCodes.PasswordResetWasRequested)]
    [HttpPost(nameof(Login))]
    public IActionResult Login([FromBody] Model_Login? model)
    {
        AuthenticationResult result = Authentication.Login(model);
        return Ok(new Model_Result<AuthenticationResult>(result));
    }

    /// <summary>
    /// Login with refresh token
    /// </summary>
    /// <param name="model">contains token and whether the login should consist over session</param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Login")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<AuthenticationResult>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.RefreshTokenNotFound, ResultCodes.RefreshTokenExpired,
        ResultCodes.NoUserFound, ResultCodes.EmailIsNotVerified, ResultCodes.PasswordResetWasRequested)]
    [HttpPost(nameof(RefreshToken))]
    public IActionResult RefreshToken([FromBody] Model_RefreshToken? model)
    {
        AuthenticationResult result = Authentication.RefreshToken(model);
        return Ok(new Model_Result<AuthenticationResult>(result));
    }
    #endregion

    #region Registration
    /// <summary>
    /// Registers a user
    /// </summary>
    /// <param name="model">contains the user information</param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Registration")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.EmailInvalid, ResultCodes.EmailAlreadyExists,
        ResultCodes.UnToShort, ResultCodes.UnUsesInvalidChars, ResultCodes.UsernameAlreadyExists, ResultCodes.PwTooShort,
        ResultCodes.PwHasNoLowercaseLetter, ResultCodes.PwHasNoUppercaseLetter, ResultCodes.PwHasNoNumber,
        ResultCodes.PwHasNoSpecialChars, ResultCodes.PwHasWhitespace, ResultCodes.PwUsesInvalidChars)]
    [HttpPost(nameof(Registration))]
    public IActionResult Registration([FromBody] Model_Registration? model)
    {
        bool isLocal = false;
#if Local
        isLocal = true;
#endif
        string? result = Authentication.Registration(model, isLocal);
        return Ok(new Model_Result<string>(result));
    }

    /// <summary>
    /// Verifies the email of the registered user
    /// </summary>
    /// <param name="token">status token of the user</param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Registration")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.StatusTokenNotFound,
        ResultCodes.WrongStatusCode, ResultCodes.StatusTokenExpired)]
    [HttpPost(nameof(VerifyEmail))]
    public IActionResult VerifyEmail(string? token)
    {
        bool isLocal = false;
#if Local
        isLocal = true;
#endif
        Authentication.VerifyEmail(token, isLocal);
        return Ok(new Model_Result<string>());
    }
    #endregion

    #region Forgot Password
    /// <summary>
    /// Sends email to user to reset password
    /// </summary>
    /// <param name="model">email of the user</param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Forgot Password")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.NoUserFound, ResultCodes.EmailIsNotVerified)]
    [HttpPost(nameof(RequestPasswordReset))]
    public IActionResult RequestPasswordReset([FromBody] Model_ValidateIdentification? model)
    {
        bool isLocal = false;
#if Local
        isLocal = true;
#endif
        string? result = Authentication.RequestPasswordReset(model?.ToValidate, isLocal);
        return Ok(new Model_Result<string>(result));
    }

    /// <summary>
    /// Resets the password of the user
    /// </summary>
    /// <param name="model">contains new password and status token</param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Forgot Password")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.StatusTokenNotFound, ResultCodes.WrongStatusCode,
        ResultCodes.StatusTokenExpired, ResultCodes.RefreshTokenExpired, ResultCodes.PwTooShort, ResultCodes.PwHasNoLowercaseLetter,
        ResultCodes.PwHasNoUppercaseLetter, ResultCodes.PwHasNoNumber, ResultCodes.PwHasNoSpecialChars, ResultCodes.PwHasWhitespace,
        ResultCodes.PwUsesInvalidChars)]
    [HttpPost(nameof(ResetPassword))]
    public IActionResult ResetPassword([FromBody] Model_ResetPassword? model)
    {
        bool isLocal = false;
#if Local
        isLocal = true;
#endif
        Authentication.ResetPassword(model, isLocal);
        return Ok(new Model_Result<string>());
    }
    #endregion

    #region Session

    /// <summary>
    /// Removes all login information on all devices except the current one
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
    /// Logout on current device
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
    /// Changes the email of the current user
    /// </summary>
    /// <param name="email">email of the user</param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Change Email")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.PasswordResetWasRequested)]
    [HttpPost(nameof(UpdateEmail)), Authorize]
    public IActionResult UpdateEmail(string? email)
    {
        bool isLocal = false;
#if Local
        isLocal = true;
#endif
        string? result = Authentication.UpdateEmail(email, isLocal);
        return Ok(new Model_Result<string>(result));
    }

    /// <summary>
    /// Verifies the updated email 
    /// </summary>
    /// <param name="code">status token</param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Change Email")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.EmailChangeCodeWrong,
        ResultCodes.WrongStatusCode, ResultCodes.StatusTokenExpired)]
    [HttpPost(nameof(VerifyUpdatedEmail)), Authorize]
    public IActionResult VerifyUpdatedEmail(string? code)
    {
        bool isLocal = false;
#if Local
        isLocal = true;
#endif
        Authentication.VerifyUpdatedEmail(code, isLocal);
        return Ok(new Model_Result<string>());
    }
    #endregion

    #region Userdata
    /// <summary>
    /// Gets user details of current user
    /// </summary>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Userdata")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<Model_UserData>))]
    [HttpGet(nameof(GetUserData)), Authorize]
    public IActionResult GetUserData()
    {
        Model_UserData user = Authentication.GetUserData();
        return Ok(new Model_Result<Model_UserData>(user));
    }

    /// <summary>
    /// Updates user details for the current user
    /// </summary>
    /// <param name="model">contains data that can be updated</param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Userdata")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [ResultCodesResponse(ResultCodes.DataIsInvalid, ResultCodes.PendingChangeOpen, ResultCodes.PwTooShort,
        ResultCodes.PwHasNoLowercaseLetter, ResultCodes.PwHasNoUppercaseLetter, ResultCodes.PwHasNoNumber,
        ResultCodes.PwHasNoSpecialChars, ResultCodes.PwHasWhitespace, ResultCodes.PwUsesInvalidChars)]
    [HttpPost(nameof(UpdateUserData)), Authorize]
    public IActionResult UpdateUserData([FromBody] Model_UpdateUserData model)
    {
        Authentication.UpdateUserData(model);
        return Ok(new Model_Result<string>());
    }
    #endregion

    #region Validation
    /// <summary>
    /// Decides whether the password complies with the conditions
    /// </summary>
    /// <param name="model">contains the password</param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Validation")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [ResultCodesResponse(ResultCodes.PwTooShort, ResultCodes.PwHasNoLowercaseLetter, ResultCodes.PwHasNoUppercaseLetter,
        ResultCodes.PwHasNoNumber, ResultCodes.PwHasNoSpecialChars, ResultCodes.PwHasWhitespace, ResultCodes.PwUsesInvalidChars)]
    [HttpPost(nameof(ValidatePassword))]
    public IActionResult ValidatePassword([FromBody] Model_ValidateIdentification model)
    {
        Authentication.ValidatePassword(model?.ToValidate);
        return Ok(new Model_Result<string>());
    }

    /// <summary>
    /// Decides whether the email complies with the conditions
    /// </summary>
    /// <param name="model">contains the email</param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Validation")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [ResultCodesResponse(ResultCodes.EmailInvalid, ResultCodes.EmailAlreadyExists)]
    [HttpPost(nameof(ValidateEmail))]
    public IActionResult ValidateEmail([FromBody] Model_ValidateIdentification? model)
    {
        Authentication.ValidateEmail(model?.ToValidate);
        return Ok(new Model_Result<string>());
    }

    /// <summary>
    /// Decides whether the username complies with the conditions
    /// </summary>
    /// <param name="model">contains the username</param>
    /// <returns></returns>
    [ApiExplorerSettings(GroupName = "Validation")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result<string>))]
    [ResultCodesResponse(ResultCodes.UnToShort, ResultCodes.UnUsesInvalidChars, ResultCodes.UsernameAlreadyExists)]
    [HttpPost(nameof(ValidateUsername))]
    public IActionResult ValidateUsername([FromBody] Model_ValidateIdentification? model)
    {
        Authentication.ValidateUsername(model?.ToValidate);
        return Ok(new Model_Result<string>());
    }
    #endregion
}