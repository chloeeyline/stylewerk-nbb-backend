using StyleWerk.NBB.Database.User;

namespace StyleWerk.NBB.Authentication;

public interface IAuthenticationService
{
	public AuthenticationWarning GetUser(Model_Login? model, out User_Login user);
	public AuthenticationWarning GetUserFromRefreshToken(string userAgent, string refreshToken, out User_Login user);

	public AuthenticationWarning GetLoginToken(User_Login user, out string loginToken);
	public string GetRefreshToken(Guid userID, string UserAgent, bool rememberMe);

	public AuthenticationWarning Registration(Model_Registration? model);
	public AuthenticationWarning VerifyEmail(Model_VerifyEmail? model);
	public AuthenticationWarning RequestPasswordReset(string user);
	public AuthenticationWarning ResetPassword(Model_ResetPassword? model);
	public PasswordError ValidatePassword(string password);

	public Model_LoginResult GetLoginResult(Guid userID, string loginToken, string refreshToken);
}
