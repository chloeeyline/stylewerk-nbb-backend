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

	[HttpPost(nameof(Login))]
	public IActionResult Login([FromBody] Model_Login? model)
	{
		if (model is null)
			return Ok(new Model_Result(ResultType.NoDataSend));

		AuthenticationWarning warning = Authentication.GetUser(model, out User_Login user);
		if (warning is not AuthenticationWarning.None)
			return Ok(new Model_Result(warning));

		warning = Authentication.GetLoginToken(user, out string loginToken);
		if (warning is not AuthenticationWarning.None)
			return Ok(new Model_Result(warning));

		string refreshToken = model.RememberMe ? Authentication.GetRefreshToken(user.ID, UserAgent) : "";
		Model_LoginResult result = Authentication.GetLoginResult(user.ID, loginToken, refreshToken);

		return Ok(new Model_Result(result));
	}

	[HttpPost(nameof(AutoLogin))]
	public IActionResult AutoLogin([FromBody] string refreshToken)
	{
		AuthenticationWarning warning = Authentication.GetUserFromRefreshToken(UserAgent, refreshToken, out User_Login user);
		if (warning is not AuthenticationWarning.None)
			return Ok(new Model_Result(warning));

		warning = Authentication.GetLoginToken(user, out string loginToken);
		if (warning is not AuthenticationWarning.None)
			return Ok(new Model_Result(warning));

		refreshToken = Authentication.GetRefreshToken(user.ID, UserAgent);
		Model_LoginResult result = Authentication.GetLoginResult(user.ID, loginToken, refreshToken);

		return Ok(new Model_Result(result));
	}

	[HttpPost(nameof(Registration))]
	public IActionResult Registration([FromBody] Model_Registration? model)
	{
		AuthenticationWarning warning = Authentication.Registration(model);
		return warning is not AuthenticationWarning.None ? Ok(new Model_Result(warning)) : Ok(new Model_Result());
	}

	[HttpPost(nameof(EmailVerification))]
	public IActionResult EmailVerification([FromBody] Model_VerifyEmail model)
	{
		AuthenticationWarning warning = Authentication.VerifyEmail(model);
		return warning is not AuthenticationWarning.None ? Ok(new Model_Result(warning)) : Ok(new Model_Result());
	}

	[HttpPost(nameof(ResetPassword))]
	public IActionResult ResetPassword([FromBody] Model_ResetPassword? model)
	{
		AuthenticationWarning warning = Authentication.ResetPassword(model);
		return warning is not AuthenticationWarning.None ? Ok(new Model_Result(warning)) : Ok(new Model_Result());
	}

	[HttpPost(nameof(ValidatePassword))]
	public IActionResult ValidatePassword(string password)
	{
		PasswordError warning = Authentication.ValidatePassword(password);
		return Ok(new Model_Result(warning));
	}

	protected override bool MissingRight(UserRight right) => false;
}