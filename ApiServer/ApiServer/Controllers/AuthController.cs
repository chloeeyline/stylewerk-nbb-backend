﻿using Microsoft.AspNetCore.Authorization;
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Model_Result))] // Success Response
    [HttpPost(nameof(Login))]
    public IActionResult Login([FromBody] Model_Login? model)
    {
        User_Login user = Authentication.GetUser(model);
        Model_Token accessToken = Authentication.GetAccessToken(user);
        Model_Token refreshToken = Authentication.GetRefreshToken(user.ID, UserAgent, model?.ConsistOverSession);
        AuthenticationResult result = Authentication.GetAuthenticationResult(user.ID, accessToken, refreshToken, model?.ConsistOverSession);

        return Ok(new Model_Result(result));
    }

    [ApiExplorerSettings(GroupName = "Login")]
    [HttpPost(nameof(RefreshToken))]
    public IActionResult RefreshToken([FromBody] Model_RefreshToken? model)
    {
        User_Login user = Authentication.GetUser(model, UserAgent);
        Model_Token accessToken = Authentication.GetAccessToken(user);
        Model_Token refreshToken = Authentication.GetRefreshToken(user.ID, UserAgent, model?.ConsistOverSession);
        AuthenticationResult result = Authentication.GetAuthenticationResult(user.ID, accessToken, refreshToken, model?.ConsistOverSession);

        return Ok(new Model_Result(result));
    }
    #endregion

    #region Registration
    [ApiExplorerSettings(GroupName = "Registration")]
    [HttpPost(nameof(Registration))]
    public IActionResult Registration([FromBody] Model_Registration? model)
    {
        Authentication.Registration(model);
        return Ok(new Model_Result());
    }

    [ApiExplorerSettings(GroupName = "Registration")]
    [HttpPost(nameof(VerifyEmail))]
    public IActionResult VerifyEmail(Guid? token)
    {
        Authentication.VerifyEmail(token);
        return Ok(new Model_Result());
    }
    #endregion

    #region Forgot Password
    [ApiExplorerSettings(GroupName = "Forgot Password")]
    [HttpPost(nameof(RequestPasswordReset))]
    public IActionResult RequestPasswordReset([FromBody] string email)
    {
        Authentication.RequestPasswordReset(email);
        return Ok(new Model_Result());
    }

    [ApiExplorerSettings(GroupName = "Forgot Password")]
    [HttpPost(nameof(ResetPassword))]
    public IActionResult ResetPassword([FromBody] Model_ResetPassword? model)
    {
        Authentication.ResetPassword(model);
        return Ok(new Model_Result());
    }
    #endregion

    #region Session
    [ApiExplorerSettings(GroupName = "Session")]
    [HttpPost(nameof(RemoveSessions)), Authorize]
    public IActionResult RemoveSessions()
    {
        Authentication.RemoveSessions(CurrentUser.ID, UserAgent);
        return Ok(new Model_Result());
    }

    [ApiExplorerSettings(GroupName = "Session")]
    [HttpPost(nameof(Logout)), Authorize]
    public IActionResult Logout()
    {
        Authentication.Logout(CurrentUser.ID, UserAgent);
        return Ok(new Model_Result());
    }
    #endregion

    #region Change Email
    [ApiExplorerSettings(GroupName = "Change Email")]
    [HttpPost(nameof(UpdateEmail)), Authorize]
    public IActionResult UpdateEmail(string? email)
    {
        Authentication.UpdateEmail(email, CurrentUser.Login);
        return Ok(new Model_Result());
    }

    [ApiExplorerSettings(GroupName = "Change Email")]
    [HttpPost(nameof(VerifyUpdatedEmail)), Authorize]
    public IActionResult VerifyUpdatedEmail(string? code)
    {
        Authentication.VerifyUpdatedEmail(code, CurrentUser.Login, UserAgent);
        return Ok(new Model_Result());
    }
    #endregion

    #region Userdata
    [ApiExplorerSettings(GroupName = "Userdata")]
    [HttpPost(nameof(GetUserData)), Authorize]
    public IActionResult GetUserData()
    {
        Model_UserData user = Authentication.GetUserData(CurrentUser);
        return Ok(new Model_Result(user));
    }

    [ApiExplorerSettings(GroupName = "Userdata")]
    [HttpPost(nameof(UpdateUserData)), Authorize]
    public IActionResult UpdateUserData([FromBody] Model_UpdateUserData model)
    {
        Authentication.UpdateUserData(model, CurrentUser.ID);
        return Ok(new Model_Result());
    }
    #endregion

    #region Validation
    [ApiExplorerSettings(GroupName = "Validation")]
    [HttpPost(nameof(ValidatePassword))]
    public IActionResult ValidatePassword([FromBody] Model_ValidateIdentification model)
    {
        Authentication.ValidatePassword(model?.ToValidate);
        return Ok(new Model_Result());
    }

    [ApiExplorerSettings(GroupName = "Validation")]
    [HttpPost(nameof(ValidateEmail))]
    public IActionResult ValidateEmail([FromBody] Model_ValidateIdentification? model)
    {
        Authentication.ValidateEmail(model?.ToValidate);
        return Ok(new Model_Result());
    }

    [ApiExplorerSettings(GroupName = "Validation")]
    [HttpPost(nameof(ValidateUsername))]
    public IActionResult ValidateUsername([FromBody] Model_ValidateIdentification? model)
    {
        Authentication.ValidateUsername(model?.ToValidate);
        return Ok(new Model_Result());
    }
    #endregion
}