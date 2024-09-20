using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using StyleWerk.NBB.AWS;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.User;

using UAParser;

namespace StyleWerk.NBB.Authentication;

public partial class AuthenticationService(NbbContext DB, IOptions<SecretData> SecretData) : IAuthenticationService
{
	private const int KeySize = 256;
	private static DateTime StatusTokenDuration => DateTime.UtcNow.AddDays(1);
	private static DateTime LoginTokenDuration => DateTime.UtcNow.AddHours(1);
	private static DateTime RefreshTokenShortDuration => DateTime.UtcNow.AddHours(4);
	private static DateTime RefreshTokenDuration => DateTime.UtcNow.AddDays(7);

	public AuthenticationWarning GetUser(Model_Login? model, out User_Login user)
	{
		user = new User_Login();
		if (model is null ||
			string.IsNullOrWhiteSpace(model.User) ||
			string.IsNullOrWhiteSpace(model.Password))
			return AuthenticationWarning.ModelIncorrect;

		string userName = model.User.ToLower().Normalize();
		User_Login? tempUser = userName.Contains('@')
			? DB.User_Login.FirstOrDefault(s => s.EmailNormalized == userName)
			: DB.User_Login.FirstOrDefault(s => s.UsernameNormalized == userName);

		if (tempUser is null) return AuthenticationWarning.NoUserFound;
		string hashedPassword = HashPassword(model.Password, tempUser.PasswordSalt);
		if (tempUser.PasswordHash != hashedPassword) return AuthenticationWarning.WrongPassword;

		user = tempUser;
		return AuthenticationWarning.None;
	}
	public AuthenticationWarning GetUserFromRefreshToken(string userAgent, string refreshToken, out User_Login user)
	{
		user = new();
		string agent = GetUserAgentString(userAgent);
		User_Token? token = DB.User_Token.Include(s => s.O_User).FirstOrDefault(s => s.Agent == agent && s.RefreshToken == refreshToken);
		if (token is null) return AuthenticationWarning.RefreshTokenNotFound;
		if (DateTime.UtcNow >= token.RefreshTokenExpiryTime) return AuthenticationWarning.RefreshTokenExpired;

		user = token.O_User;
		return AuthenticationWarning.None;
	}

	public AuthenticationWarning GetLoginToken(User_Login user, out string loginToken)
	{
		loginToken = "";
		if (user.StatusCode is UserStatus.EmailVerification or UserStatus.EmailChange) return AuthenticationWarning.EmailIsNotVerified;
		if (user.StatusCode == UserStatus.PasswordReset) return AuthenticationWarning.PasswordResetWasRequested;

		SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(SecretData.Value.JwtKey));
		SigningCredentials credentials = new(securityKey, SecurityAlgorithms.HmacSha256);

		Claim[] claims = [new Claim(ClaimTypes.Sid, user.ID.ToString())];

		JwtSecurityToken token = new(SecretData.Value.JwtIssuer, SecretData.Value.JwtAudience, claims, expires: LoginTokenDuration, signingCredentials: credentials);
		loginToken = new JwtSecurityTokenHandler().WriteToken(token);
		return AuthenticationWarning.None;
	}
	public string GetRefreshToken(Guid userID, string userAgent, bool rememberMe)
	{
		string agent = GetUserAgentString(userAgent);
		string refreshToken = GenerateRandomKey();

		User_Token? userToken = DB.User_Token.FirstOrDefault(s => s.ID == userID && s.Agent == agent);
		if (userToken is not null)
		{
			userToken.RefreshToken = refreshToken;
			userToken.RefreshTokenExpiryTime = rememberMe ? RefreshTokenDuration : RefreshTokenShortDuration;
			DB.SaveChanges();
		}
		else
		{
			userToken = new User_Token()
			{
				ID = userID,
				Agent = agent,
				RefreshToken = refreshToken,
				RefreshTokenExpiryTime = rememberMe ? RefreshTokenDuration : RefreshTokenShortDuration,
			};
			DB.User_Token.Add(userToken);
			DB.SaveChanges();
		}
		return refreshToken;
	}

	public AuthenticationWarning Registration(Model_Registration? model)
	{
		if (model is null) return AuthenticationWarning.ModelIsNull;
		if (string.IsNullOrWhiteSpace(model.Username) ||
			string.IsNullOrWhiteSpace(model.Email) ||
			string.IsNullOrWhiteSpace(model.Password))
			return AuthenticationWarning.ModelIncorrect;

		string email = model.Email.ToLower().Normalize();
		string username = model.Username.ToLower().Normalize();
		string salt = GenerateRandomKey();
		Guid id = Guid.NewGuid();
		Guid statusToken = Guid.NewGuid();

		if (!email.Contains('@') || !email.Contains('.')) return AuthenticationWarning.EmailInvalid;
		if (DB.User_Login.Any(s => s.EmailNormalized == email)) return AuthenticationWarning.EmailAlreadyExists;

		if (username.Length < 5 || username.Length > 50 || !OnlyUsernameValidChars().IsMatch(username)) return AuthenticationWarning.UsernameInvalid;
		if (DB.User_Login.Any(s => s.Username == username)) return AuthenticationWarning.UsernameAlreadyExists;

		if (ValidatePassword(model.Password) != PasswordError.None) return AuthenticationWarning.PasswordInvalid;

		while (DB.User_Login.Any(s => s.PasswordSalt == salt)) salt = GenerateRandomKey();
		while (DB.User_Login.Any(s => s.ID == id)) id = Guid.NewGuid();
		while (DB.User_Login.Any(s => s.StatusToken == statusToken)) statusToken = Guid.NewGuid();

		string hashedPasswod = HashPassword(model.Password, salt);

		User_Login user = new()
		{
			ID = Guid.NewGuid(),
			Username = model.Username,
			UsernameNormalized = model.Username.ToLower().Normalize(),
			Email = model.Email,
			EmailNormalized = model.Username.ToLower().Normalize(),
			PasswordHash = hashedPasswod,
			PasswordSalt = salt,
			StatusToken = Guid.NewGuid(),
			StatusTokenTime = StatusTokenDuration,
			StatusCode = UserStatus.EmailVerification
		};

		User_Information userInformation = new()
		{
			ID = user.ID,
			Gender = model.Gender,
			FirstName = model.FirstName,
			LastName = model.LastName,
			Birthday = model.Birthday,
		};

		User_Right userRight = new()
		{
			ID = user.ID,
		};

		DB.User_Login.Add(user);
		DB.User_Information.Add(userInformation);
		DB.User_Right.Add(userRight);
		DB.SaveChanges();

		return AuthenticationWarning.None;
	}


	public AuthenticationWarning VerifyEmail(Model_VerifyEmail? model)
	{
		if (model is null) return AuthenticationWarning.ModelIsNull;
		if (string.IsNullOrWhiteSpace(model.Email) || model.Token == Guid.Empty) return AuthenticationWarning.ModelIncorrect;

		string email = model.Email.ToLower().Normalize();
		User_Login? user = DB.User_Login.FirstOrDefault(s => s.EmailNormalized == email && s.StatusToken == model.Token);

		if (user is null) return AuthenticationWarning.StatusTokenNotFound;
		if (user.StatusCode != UserStatus.EmailVerification) return AuthenticationWarning.WrongStatusCode;
		if (DateTime.Today.ToUniversalTime().AddDays(1) < user.StatusTokenTime) return AuthenticationWarning.StatusTokenExpired;

		user.StatusCode = UserStatus.None;
		user.StatusToken = null;
		user.StatusTokenTime = null;
		DB.SaveChanges();

		return AuthenticationWarning.None;
	}

	public AuthenticationWarning RequestPasswordReset(string user)
	{
		if (string.IsNullOrWhiteSpace(user)) return AuthenticationWarning.ModelIsNull;
		user = user.ToLower().Normalize();
		User_Login? tempUser = user.Contains('@')
			? DB.User_Login.Include(s => s.O_Token).FirstOrDefault(s => s.EmailNormalized == user)
			: DB.User_Login.Include(s => s.O_Token).FirstOrDefault(s => s.UsernameNormalized == user);
		if (tempUser is null) return AuthenticationWarning.NoUserFound;

		DB.User_Token.RemoveRange(tempUser.O_Token);
		tempUser.StatusCode = UserStatus.PasswordReset;
		tempUser.StatusToken = Guid.NewGuid();
		tempUser.StatusTokenTime = StatusTokenDuration;
		DB.SaveChanges();

		return AuthenticationWarning.None;
	}

	public AuthenticationWarning ResetPassword(Model_ResetPassword? model)
	{
		if (model is null) return AuthenticationWarning.ModelIsNull;
		if (string.IsNullOrWhiteSpace(model.Password)
			|| model.Token == Guid.Empty)
			return AuthenticationWarning.ModelIncorrect;

		User_Login? user = DB.User_Login.FirstOrDefault(s => s.StatusToken == model.Token);

		if (ValidatePassword(model.Password) != PasswordError.None) return AuthenticationWarning.ModelIncorrect;
		if (user is null) return AuthenticationWarning.StatusTokenNotFound;
		if (user.StatusCode == UserStatus.PasswordReset) return AuthenticationWarning.WrongPassword;
		if (DateTime.UtcNow >= user.StatusTokenTime) return AuthenticationWarning.RefreshTokenExpired;

		string salt = GenerateRandomKey();
		while (DB.User_Login.Any(s => s.PasswordSalt == salt)) salt = GenerateRandomKey();
		user.PasswordSalt = salt;

		string hashedPasswod = HashPassword(model.Password, salt);
		user.PasswordHash = hashedPasswod;

		user.StatusCode = UserStatus.None;
		user.StatusToken = null;
		user.StatusTokenTime = null;
		DB.SaveChanges();

		return AuthenticationWarning.None;
	}

	public Model_LoginResult GetLoginResult(Guid userID, string loginToken, string refreshToken)
	{
		User_Login? user = DB.User_Login.Include(s => s.O_Right).Include(s => s.O_Information).FirstOrDefault(s => s.ID == userID);
		if (user is null) return new Model_LoginResult("", "", "", new Model_Right(false, new(UserRight.Restricted), new(UserRight.Restricted)));
		Model_Right right = new(user.O_Right.Admin, new(UserRight.Restricted), new(UserRight.Restricted));
		Model_LoginResult result = new(loginToken, refreshToken, user.Username, right);
		return result;
	}

	#region Helpers
	private string HashPassword(string password, string salt) => ComputeHash(password, salt, SecretData.Value.PasswortPepper, 5);

	private static string ComputeHash(string password, string salt, string pepper, int iteration)
	{
		if (iteration <= 0) return password;
		string passwordSaltPepper = $"{password}{salt}{pepper}";
		byte[] byteValue = Encoding.UTF8.GetBytes(passwordSaltPepper);
		byte[] byteHash = SHA256.HashData(byteValue);
		string hash = Convert.ToBase64String(byteHash);
		return ComputeHash(hash, salt, pepper, iteration - 1);
	}

	private static string GenerateRandomKey()
	{
		using RandomNumberGenerator rng = RandomNumberGenerator.Create();
		byte[] byteSalt = new byte[KeySize];
		rng.GetBytes(byteSalt);
		string salt = Convert.ToBase64String(byteSalt);
		return salt;
	}

	private static string GetUserAgentString(string userAgent)
	{
		ClientInfo c = Parser.GetDefault().Parse(userAgent);
		return $"OS: [Family: {c.OS.Family} Major: {c.OS.Major} Minor: {c.OS.Minor}] UA: [Family: {c.UA.Family} Major: {c.UA.Major} Minor: {c.UA.Minor}] Device: [Family: {c.Device.Family} Brand: {c.Device.Brand} Model: {c.Device.Model}]";
	}
	#endregion

	#region Validate Password
	public PasswordError ValidatePassword(string password)
	{
		return password.Length < 10 ? PasswordError.TooShort
			: !ContainsLowercase().IsMatch(password) ? PasswordError.HasNoLowercaseLetter
			: !ContainsUppercase().IsMatch(password) ? PasswordError.HasNoUppercaseLetter
			: !ContainsDigit().IsMatch(password) ? PasswordError.HasNoNumber
			: !ContainsPasswordSpecialChar().IsMatch(password) ? PasswordError.HasNoSpecialChars
			: ContainsWhitespace().IsMatch(password) ? PasswordError.HasWhitespace
			: !OnlyPasswordValidChars().IsMatch(password) ? PasswordError.UsesInvalidChars
			: PasswordError.None;
	}

	[GeneratedRegex(@"[a-z]")] private static partial Regex ContainsLowercase();
	[GeneratedRegex(@"[A-Z]")] private static partial Regex ContainsUppercase();
	[GeneratedRegex(@"\d")] private static partial Regex ContainsDigit();
	[GeneratedRegex(@"[!#$%&'*+\-./?@\\_|]")] private static partial Regex ContainsPasswordSpecialChar();
	[GeneratedRegex(@"\s")] private static partial Regex ContainsWhitespace();
	[GeneratedRegex(@"[a-zA-Z\d!#$%&'*+\-./?@\\_|^\s]")] private static partial Regex OnlyPasswordValidChars();
	[GeneratedRegex(@"[a-zA-Z\d&'*+\-./\\_|^\s]")] private static partial Regex OnlyUsernameValidChars();
	#endregion
}
