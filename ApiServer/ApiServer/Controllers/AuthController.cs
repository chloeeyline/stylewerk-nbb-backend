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
    [HttpPost(nameof(Login))]
    public IActionResult Login([FromBody] Model_Login? model)
    {
        try
        {
            User_Login user = Authentication.GetUser(model);
            Model_Token accessToken = Authentication.GetAccessToken(user);
            Model_Token refreshToken = Authentication.GetRefreshToken(user.ID, UserAgent, model?.ConsistOverSession);
            AuthenticationResult result = Authentication.GetAuthenticationResult(user.ID, accessToken, refreshToken, model?.ConsistOverSession);

            return Ok(new Model_Result(result));
        }
        catch (AuthenticationException ex)
        {
            return Ok(new Model_Result(ex.ErrorCode));
        }
    }

    [HttpPost(nameof(RefreshToken))]
    public IActionResult RefreshToken([FromBody] Model_RefreshToken? model)
    {
        try
        {
            User_Login user = Authentication.GetUser(model, UserAgent);
            Model_Token accessToken = Authentication.GetAccessToken(user);
            Model_Token refreshToken = Authentication.GetRefreshToken(user.ID, UserAgent, model?.ConsistOverSession);
            AuthenticationResult result = Authentication.GetAuthenticationResult(user.ID, accessToken, refreshToken, model?.ConsistOverSession);

            return Ok(new Model_Result(result));
        }
        catch (AuthenticationException ex)
        {
            return Ok(new Model_Result(ex.ErrorCode));
        }
    }
    #endregion

    #region Registration
    [HttpPost(nameof(Registration))]
    public IActionResult Registration([FromBody] Model_Registration? model)
    {
        try
        {
            Authentication.Registration(model);
            return Ok(new Model_Result());
        }
        catch (AuthenticationException ex)
        {
            return Ok(new Model_Result(ex.ErrorCode));
        }
    }

    [HttpPost(nameof(VerifyEmail))]
    public IActionResult VerifyEmail(Guid? token)
    {
        try
        {
            Authentication.VerifyEmail(token);
            return Ok(new Model_Result());
        }
        catch (AuthenticationException ex)
        {
            return Ok(new Model_Result(ex.ErrorCode));
        }
    }
    #endregion

    #region Forgot Password
    [HttpPost(nameof(RequestPasswordReset))]
    public IActionResult RequestPasswordReset([FromBody] string email)
    {
        try
        {
            Authentication.RequestPasswordReset(email);
            return Ok(new Model_Result());
        }
        catch (AuthenticationException ex)
        {
            return Ok(new Model_Result(ex.ErrorCode));
        }
    }

    [HttpPost(nameof(ResetPassword))]
    public IActionResult ResetPassword([FromBody] Model_ResetPassword? model)
    {
        try
        {
            Authentication.ResetPassword(model);
            return Ok(new Model_Result());
        }
        catch (AuthenticationException ex)
        {
            return Ok(new Model_Result(ex.ErrorCode));
        }
    }
    #endregion

    #region Userdata
    //Muss angemeldet sein
    [HttpPost(nameof(UpdateEmail)), Authorize]
    public IActionResult UpdateEmail(string? email)
    {
        try
        {
            Authentication.UpdateEmail(email, CurrentUser.ID, UserAgent);
            return Ok(new Model_Result());
        }
        catch (AuthenticationException ex)
        {
            return Ok(new Model_Result(ex.ErrorCode));
        }
    }

    //Muss angemeldet sein
    [HttpPost(nameof(VerifyUpdatedEmail)), Authorize]
    public IActionResult VerifyUpdatedEmail(Guid? token)
    {
        try
        {
            Authentication.VerifyUpdatedEmail(token);
            return Ok(new Model_Result());
        }
        catch (AuthenticationException ex)
        {
            return Ok(new Model_Result(ex.ErrorCode));
        }
    }

    //Muss angemeldet sein
    [HttpPost(nameof(GetUserData))]
    public IActionResult GetUserData()
    {
        try
        {
            return Ok(new Model_Result());
        }
        catch (AuthenticationException ex)
        {
            return Ok(new Model_Result(ex.ErrorCode));
        }
    }

    //Muss angemeldet sein
    [HttpPost(nameof(UpdateUserData))]
    public IActionResult UpdateUserData([FromBody] Model_Userdata model)
    {
        try
        {
            return Ok(new Model_Result());
        }
        catch (AuthenticationException ex)
        {
            return Ok(new Model_Result(ex.ErrorCode));
        }
    }
    #endregion

    #region Validation
    [HttpPost(nameof(ValidatePassword))]
    public IActionResult ValidatePassword([FromBody] Model_ValidateIdentification model)
    {
        try
        {
            Authentication.ValidatePassword(model?.ToValidate);
            return Ok(new Model_Result());
        }
        catch (AuthenticationException ex)
        {
            return Ok(new Model_Result(ex.ErrorCode));
        }
    }

    [HttpPost(nameof(ValidateEmail))]
    public IActionResult ValidateEmail([FromBody] Model_ValidateIdentification? model)
    {
        try
        {
            Authentication.ValidateEmail(model?.ToValidate);
            return Ok(new Model_Result());
        }
        catch (AuthenticationException ex)
        {
            return Ok(new Model_Result(ex.ErrorCode));
        }
    }

    [HttpPost(nameof(ValidateUsername))]
    public IActionResult ValidateUsername([FromBody] Model_ValidateIdentification? model)
    {
        try
        {
            Authentication.ValidateUsername(model?.ToValidate);
            return Ok(new Model_Result());
        }
        catch (AuthenticationException ex)
        {
            return Ok(new Model_Result(ex.ErrorCode));
        }
    }
    #endregion
}