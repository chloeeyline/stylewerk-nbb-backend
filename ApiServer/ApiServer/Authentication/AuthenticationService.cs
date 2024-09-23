using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StyleWerk.NBB.AWS;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.User;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UAParser;

namespace StyleWerk.NBB.Authentication;

public partial class AuthenticationService(NbbContext DB, IOptions<SecretData> SecretData) : IAuthenticationService
{
    #region Fixed Values
    private const int KeySize = 256;
    private static DateTime StatusTokenDuration => DateTime.UtcNow.AddDays(1);
    private static DateTime LoginTokenDuration => DateTime.UtcNow.AddHours(1);
    private static DateTime RefreshTokenShortDuration => DateTime.UtcNow.AddHours(4);
    private static DateTime RefreshTokenDuration => DateTime.UtcNow.AddDays(7);
    #endregion

    #region Login
    public User_Login GetUser(Model_Login? model)
    {
        if (model is null ||
            string.IsNullOrWhiteSpace(model.Username) ||
            string.IsNullOrWhiteSpace(model.Password))
            throw new AuthenticationException(AuthenticationErrorCodes.ModelIncorrect);

        string userName = model.Username.ToLower().Normalize();
        User_Login? user = DB.User_Login.FirstOrDefault(s => s.UsernameNormalized == userName)
            ?? throw new AuthenticationException(AuthenticationErrorCodes.NoUserFound);

        string hashedPassword = HashPassword(model.Password, user.PasswordSalt);
        if (user.PasswordHash != hashedPassword)
            throw new AuthenticationException(AuthenticationErrorCodes.NoUserFound);

        return user;
    }

    public User_Login GetUser(Model_RefreshToken? model, string userAgent)
    {
        if (model is null ||
            string.IsNullOrWhiteSpace(model.Token) ||
            string.IsNullOrWhiteSpace(userAgent))
            throw new AuthenticationException(AuthenticationErrorCodes.ModelIncorrect);

        string agent = GetUserAgentString(userAgent);
        User_Token? token = DB.User_Token.FirstOrDefault(s => s.Agent == agent && s.RefreshToken == model.Token)
            ?? throw new AuthenticationException(AuthenticationErrorCodes.RefreshTokenNotFound);

        if (DateTime.UtcNow >= token.RefreshTokenExpiryTime)
            throw new AuthenticationException(AuthenticationErrorCodes.RefreshTokenExpired);

        User_Login? user = DB.User_Login.FirstOrDefault(s => s.ID == token.ID)
        ?? throw new AuthenticationException(AuthenticationErrorCodes.NoUserFound);

        return user;
    }

    public Model_Token GetAccessToken(User_Login user)
    {
        if (user.StatusCode is UserStatus.EmailVerification)
            throw new AuthenticationException(AuthenticationErrorCodes.EmailIsNotVerified);
        if (user.StatusCode == UserStatus.PasswordReset)
            throw new AuthenticationException(AuthenticationErrorCodes.PasswordResetWasRequested);

        SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(SecretData.Value.JwtKey));
        SigningCredentials credentials = new(securityKey, SecurityAlgorithms.HmacSha256);

        Claim[] claims = [new Claim(ClaimTypes.Sid, user.ID.ToString())];

        DateTime expireTime = LoginTokenDuration;
        long expireTimeUnixTime = new DateTimeOffset(expireTime).ToUnixTimeMilliseconds();

        JwtSecurityToken token = new(SecretData.Value.JwtIssuer, SecretData.Value.JwtAudience, claims, expires: expireTime, signingCredentials: credentials);
        string loginToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new Model_Token(loginToken, expireTimeUnixTime);
    }

    public Model_Token GetRefreshToken(Guid userID, string userAgent, bool? consistOverSession)
    {
        string agent = GetUserAgentString(userAgent);
        string refreshToken = GenerateRandomKey();
        DateTime expireTime = consistOverSession is true ? RefreshTokenDuration : RefreshTokenShortDuration;
        long expireTimeUnixTime = new DateTimeOffset(expireTime).ToUnixTimeMilliseconds();

        User_Token? userToken = DB.User_Token.FirstOrDefault(s => s.ID == userID && s.Agent == agent);
        if (userToken is not null)
        {
            userToken.RefreshToken = refreshToken;
            userToken.RefreshTokenExpiryTime = expireTime;
            userToken.ConsistOverSession = consistOverSession is true;
            DB.SaveChanges();
        }
        else
        {
            userToken = new User_Token()
            {
                ID = userID,
                Agent = agent,
                RefreshToken = refreshToken,
                RefreshTokenExpiryTime = expireTime,
                ConsistOverSession = consistOverSession is true
            };
            DB.User_Token.Add(userToken);
            DB.SaveChanges();
        }

        return new Model_Token(refreshToken, expireTimeUnixTime);
    }

    public AuthenticationResult GetAuthenticationResult(Guid id, Model_Token accessToken, Model_Token refreshToken, bool? consistOverSession)
    {
        User_Login? user = DB.User_Login.Include(s => s.O_Information)
            .FirstOrDefault(s => s.ID == id)
            ?? throw new AuthenticationException(AuthenticationErrorCodes.NoUserFound);

        string[] rights = [.. DB.User_Right.Where(s => s.ID == id).Select(s => s.Name)];

        return new AuthenticationResult(accessToken, refreshToken, user.StatusCode, consistOverSession is true, user.Username, user.Admin, rights);
    }
    #endregion

    #region Registration
    public void Registration(Model_Registration? model)
    {
        if (model is null ||
            string.IsNullOrWhiteSpace(model.Username) ||
            string.IsNullOrWhiteSpace(model.Email) ||
            string.IsNullOrWhiteSpace(model.Password))
            throw new AuthenticationException(AuthenticationErrorCodes.ModelIncorrect);

        string email = ValidateEmail(model.Email);
        string username = ValidateUsername(model.Username);
        string salt = GetSalt();
        DateTimeOffset birthday = DateTimeOffset.FromUnixTimeMilliseconds(model.Birthday);
        ValidatePassword(model.Password);

        Guid id = Guid.NewGuid();
        while (DB.User_Login.Any(s => s.ID == id))
            id = Guid.NewGuid();

        User_Login user = new()
        {
            ID = Guid.NewGuid(),
            Username = model.Username,
            UsernameNormalized = username,
            Email = model.Email,
            EmailNormalized = email,
            PasswordHash = HashPassword(model.Password, salt),
            PasswordSalt = salt,
            StatusToken = GetStatusToken(),
            StatusTokenExpireTime = StatusTokenDuration,
            StatusCode = UserStatus.EmailVerification,
            Admin = false
        };

        User_Information userInformation = new()
        {
            ID = user.ID,
            Gender = model.Gender,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Birthday = new DateOnly(birthday.Year, birthday.Month, birthday.Day),
        };

        DB.User_Login.Add(user);
        DB.User_Information.Add(userInformation);
        DB.SaveChanges();
    }

    public void VerifyEmail(Guid? token)
    {
        if (token is null || token == Guid.Empty)
            throw new AuthenticationException(AuthenticationErrorCodes.ModelIncorrect);

        User_Login? user = DB.User_Login.FirstOrDefault(s => s.StatusToken == token)
            ?? throw new AuthenticationException(AuthenticationErrorCodes.StatusTokenNotFound);
        if (user.StatusCode is not UserStatus.EmailVerification)
            throw new AuthenticationException(AuthenticationErrorCodes.WrongStatusCode);
        if (DateTime.UtcNow >= user.StatusTokenExpireTime)
            throw new AuthenticationException(AuthenticationErrorCodes.StatusTokenExpired);

        user.StatusCode = null;
        user.StatusToken = null;
        user.StatusTokenExpireTime = null;
        DB.SaveChanges();
    }
    #endregion

    #region Forgot Password
    public void RequestPasswordReset(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new AuthenticationException(AuthenticationErrorCodes.ModelIncorrect);

        email = email.ToLower().Normalize();
        User_Login? user = DB.User_Login.Include(s => s.O_Token).FirstOrDefault(s => s.EmailNormalized == email && s.StatusCode != null)
            ?? throw new AuthenticationException(AuthenticationErrorCodes.NoUserFound);
        if (user.StatusCode is UserStatus.EmailVerification)
            throw new AuthenticationException(AuthenticationErrorCodes.EmailIsNotVerified);

        DB.User_Token.RemoveRange(user.O_Token);
        user.StatusCode = UserStatus.PasswordReset;
        user.StatusToken = GetStatusToken();
        user.StatusTokenExpireTime = StatusTokenDuration;
        DB.SaveChanges();
    }

    public void ResetPassword(Model_ResetPassword? model)
    {
        if (model is null ||
            string.IsNullOrWhiteSpace(model.Password)
            || model.Token == Guid.Empty)
            throw new AuthenticationException(AuthenticationErrorCodes.ModelIncorrect);

        User_Login? user = DB.User_Login.FirstOrDefault(s => s.StatusToken == model.Token);

        ValidatePassword(model.Password);
        if (user is null)
            throw new AuthenticationException(AuthenticationErrorCodes.StatusTokenNotFound);
        if (user.StatusCode is not UserStatus.PasswordReset)
            throw new AuthenticationException(AuthenticationErrorCodes.WrongStatusCode);
        if (DateTime.UtcNow >= user.StatusTokenExpireTime)
            throw new AuthenticationException(AuthenticationErrorCodes.RefreshTokenExpired);

        user.PasswordSalt = GetSalt();
        user.PasswordHash = HashPassword(model.Password, user.PasswordSalt);

        user.StatusCode = null;
        user.StatusToken = null;
        user.StatusTokenExpireTime = null;
        DB.SaveChanges();
    }
    #endregion

    #region Edit Session
    public void RemoveSessions(Guid userID, string userAgent)
    {
        DB.User_Token.RemoveRange(DB.User_Token.Where(s => s.ID == userID && s.Agent != userAgent));
    }

    public void Logout(Guid userID, string userAgent)
    {
        DB.User_Token.RemoveRange(DB.User_Token.Where(s => s.ID == userID && s.Agent == userAgent));
    }
    #endregion

    #region Change Email
    public void UpdateEmail(string? email, User_Login user)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new AuthenticationException(AuthenticationErrorCodes.ModelIncorrect);

        if (user.StatusCode is UserStatus.PasswordReset)
            throw new AuthenticationException(AuthenticationErrorCodes.PasswordResetWasRequested);

        user.NewEmail = email;
        user.StatusCode = UserStatus.EmailChange;
        user.EmailChangeCode = new Random().Next(100001).ToString("D6");
        user.StatusTokenExpireTime = StatusTokenDuration;
        DB.SaveChanges();
    }

    public void VerifyUpdatedEmail(string? code, User_Login user, string userAgent)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new AuthenticationException(AuthenticationErrorCodes.ModelIncorrect);

        if (string.IsNullOrEmpty(user.EmailChangeCode) || !code.Equals(user.EmailChangeCode))
            throw new AuthenticationException(AuthenticationErrorCodes.EmailChangeCodeWrong);
        if (user.StatusCode is not UserStatus.EmailChange)
            throw new AuthenticationException(AuthenticationErrorCodes.WrongStatusCode);
        if (string.IsNullOrWhiteSpace(user.NewEmail))
            throw new AuthenticationException(AuthenticationErrorCodes.WrongStatusCode);
        if (DateTime.UtcNow >= user.StatusTokenExpireTime)
            throw new AuthenticationException(AuthenticationErrorCodes.StatusTokenExpired);

        DB.User_Token.RemoveRange(DB.User_Token.Where(s => s.ID == user.ID && s.Agent != userAgent));
        user.Email = user.NewEmail;
        user.EmailNormalized = user.NewEmail.ToLower().Normalize();
        user.NewEmail = null;

        user.EmailChangeCode = null;
        user.StatusCode = null;
        user.StatusToken = null;
        user.StatusTokenExpireTime = null;
        DB.SaveChanges();
    }
    #endregion

    #region Userdata
    public Model_UserData GetUserData(ApplicationUser user)
    {
        DateTimeOffset birthday = new(user.Information.Birthday.ToDateTime(TimeOnly.MinValue));
        return new Model_UserData(
            user.Login.Username,
            user.Login.Email,
            user.Information.FirstName,
            user.Information.LastName,
            user.Information.Gender,
            birthday.ToUnixTimeMilliseconds());
    }

    public void UpdateUserData(Model_UpdateUserData? model, Guid userID)
    {
        if (model is null)
            throw new AuthenticationException(AuthenticationErrorCodes.ModelIncorrect);

        User_Login? user = DB.User_Login.Include(s => s.O_Information).FirstOrDefault(s => s.ID == userID)
            ?? throw new AuthenticationException(AuthenticationErrorCodes.NoUserFound);
        if (user.StatusCode is not null)
            throw new AuthenticationException(AuthenticationErrorCodes.PendingChangeOpen);

        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            ValidatePassword(model.Password);
            user.PasswordSalt = GetSalt();
            user.PasswordHash = HashPassword(model.Password, user.PasswordSalt);

        }
        if (!string.IsNullOrWhiteSpace(model.FirstName)) user.O_Information.FirstName = model.FirstName;
        if (!string.IsNullOrWhiteSpace(model.LastName)) user.O_Information.LastName = model.LastName;
        if (model.Gender is not null) user.O_Information.Gender = model.Gender.Value;

        DB.SaveChanges();
    }
    #endregion

    #region Helpers
    private Guid GetStatusToken()
    {
        Guid statusToken = Guid.NewGuid();
        while (DB.User_Login.Any(s => s.StatusToken == statusToken))
            statusToken = Guid.NewGuid();
        return statusToken;
    }
    private string GetSalt()
    {
        string salt = GenerateRandomKey();
        while (DB.User_Login.Any(s => s.PasswordSalt == salt))
            salt = GenerateRandomKey();
        return salt;
    }
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

    #region Validate Password and Account Identifier
    public string ValidateEmail(string? email)
    {
        email = email?.ToLower().Normalize();
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@') || !email.Contains('.'))
            throw new AuthenticationException(AuthenticationErrorCodes.EmailInvalid);
        if (DB.User_Login.Any(s => s.EmailNormalized == email))
            throw new AuthenticationException(AuthenticationErrorCodes.EmailAlreadyExists);
        return email;
    }

    public string ValidateUsername(string? username)
    {
        username = username?.ToLower().Normalize();
        if (string.IsNullOrWhiteSpace(username) || username.Length < 5)
            throw new AuthenticationException(AuthenticationErrorCodes.UnToShort);
        if (username.Length > 50)
            throw new AuthenticationException(AuthenticationErrorCodes.UnToShort);
        if (!OnlyUsernameValidChars().IsMatch(username))
            throw new AuthenticationException(AuthenticationErrorCodes.UnUsesInvalidChars);
        if (DB.User_Login.Any(s => s.Username == username))
            throw new AuthenticationException(AuthenticationErrorCodes.UsernameAlreadyExists);
        return username;
    }

    public void ValidatePassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new AuthenticationException(AuthenticationErrorCodes.PwTooShort);
        if (password.Length < 10)
            throw new AuthenticationException(AuthenticationErrorCodes.PwTooShort);
        if (!ContainsLowercase().IsMatch(password))
            throw new AuthenticationException(AuthenticationErrorCodes.PwHasNoLowercaseLetter);
        if (!ContainsUppercase().IsMatch(password))
            throw new AuthenticationException(AuthenticationErrorCodes.PwHasNoUppercaseLetter);
        if (!ContainsDigit().IsMatch(password))
            throw new AuthenticationException(AuthenticationErrorCodes.PwHasNoNumber);
        if (!ContainsPasswordSpecialChar().IsMatch(password))
            throw new AuthenticationException(AuthenticationErrorCodes.PwHasNoSpecialChars);
        if (ContainsWhitespace().IsMatch(password))
            throw new AuthenticationException(AuthenticationErrorCodes.PwHasWhitespace);
        if (!OnlyPasswordValidChars().IsMatch(password))
            throw new AuthenticationException(AuthenticationErrorCodes.PwUsesInvalidChars);
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
