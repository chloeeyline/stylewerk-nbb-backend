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
            throw new AuthenticationException(AuthenticationWarning.ModelIncorrect);

        string userName = model.Username.ToLower().Normalize();
        User_Login? user = DB.User_Login.FirstOrDefault(s => s.UsernameNormalized == userName)
            ?? throw new AuthenticationException(AuthenticationWarning.NoUserFound);

        string hashedPassword = HashPassword(model.Password, user.PasswordSalt);
        if (user.PasswordHash != hashedPassword)
            throw new AuthenticationException(AuthenticationWarning.WrongPassword);

        return user;
    }

    public User_Login GetUser(Model_RefreshToken? model, string userAgent)
    {
        if (model is null ||
            string.IsNullOrWhiteSpace(model.Token) ||
            string.IsNullOrWhiteSpace(userAgent))
            throw new AuthenticationException(AuthenticationWarning.ModelIncorrect);

        string agent = GetUserAgentString(userAgent);
        User_Token? token = DB.User_Token.FirstOrDefault(s => s.Agent == agent && s.RefreshToken == model.Token)
            ?? throw new AuthenticationException(AuthenticationWarning.RefreshTokenNotFound);

        if (DateTime.UtcNow >= token.RefreshTokenExpiryTime)
            throw new AuthenticationException(AuthenticationWarning.RefreshTokenExpired);

        return token.O_User;
    }

    public Model_Token GetAccessToken(User_Login user)
    {
        if (user.StatusCode is UserStatus.EmailVerification or UserStatus.EmailChange)
            throw new AuthenticationException(AuthenticationWarning.EmailIsNotVerified);
        if (user.StatusCode == UserStatus.PasswordReset)
            throw new AuthenticationException(AuthenticationWarning.PasswordResetWasRequested);

        SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(SecretData.Value.JwtKey));
        SigningCredentials credentials = new(securityKey, SecurityAlgorithms.HmacSha256);

        Claim[] claims = [new Claim(ClaimTypes.Sid, user.ID.ToString())];

        DateTime expireTime = LoginTokenDuration;
        long expireTimeUnixTime = new DateTimeOffset(expireTime).ToUnixTimeMilliseconds();

        JwtSecurityToken token = new(SecretData.Value.JwtIssuer, SecretData.Value.JwtAudience, claims, expires: expireTime, signingCredentials: credentials);
        string loginToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new Model_Token(loginToken, expireTimeUnixTime);
    }

    public Model_Token GetRefreshToken(Guid userID, string userAgent, bool consistOverSession)
    {
        string agent = GetUserAgentString(userAgent);
        string refreshToken = GenerateRandomKey();
        DateTime expireTime = consistOverSession ? RefreshTokenDuration : RefreshTokenShortDuration;
        long expireTimeUnixTime = new DateTimeOffset(expireTime).ToUnixTimeMilliseconds();

        User_Token? userToken = DB.User_Token.FirstOrDefault(s => s.ID == userID && s.Agent == agent);
        if (userToken is not null)
        {
            userToken.RefreshToken = refreshToken;
            userToken.RefreshTokenExpiryTime = expireTime;
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
            };
            DB.User_Token.Add(userToken);
            DB.SaveChanges();
        }

        return new Model_Token(refreshToken, expireTimeUnixTime);
    }
    #endregion

    #region Registration
    public void Registration(Model_Registration? model)
    {
        if (model is null ||
            string.IsNullOrWhiteSpace(model.Username) ||
            string.IsNullOrWhiteSpace(model.Email) ||
            string.IsNullOrWhiteSpace(model.Password))
            throw new AuthenticationException(AuthenticationWarning.ModelIncorrect);

        string email = ValidateEmail(model.Email);
        string username = ValidateUsername(model.Email);

        if (ValidatePassword(model.Password) != PasswordError.None)
            throw new AuthenticationException(AuthenticationWarning.PasswordInvalid);

        Guid id = Guid.NewGuid();
        while (DB.User_Login.Any(s => s.ID == id))
            id = Guid.NewGuid();

        string salt = GetSalt();
        User_Login user = new()
        {
            ID = Guid.NewGuid(),
            Username = model.Username,
            UsernameNormalized = model.Username.ToLower().Normalize(),
            Email = model.Email,
            EmailNormalized = model.Username.ToLower().Normalize(),
            PasswordHash = HashPassword(model.Password, salt),
            PasswordSalt = salt,
            StatusToken = GetStatusToken(),
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
    }

    public void VerifyEmail(Guid? token)
    {
        if (token is null || token == Guid.Empty)
            throw new AuthenticationException(AuthenticationWarning.ModelIncorrect);

        User_Login? user = DB.User_Login.FirstOrDefault(s => s.StatusToken == token)
            ?? throw new AuthenticationException(AuthenticationWarning.StatusTokenNotFound);
        if (user.StatusCode != UserStatus.EmailVerification)
            throw new AuthenticationException(AuthenticationWarning.WrongStatusCode);
        if (DateTime.UtcNow >= user.StatusTokenTime)
            throw new AuthenticationException(AuthenticationWarning.StatusTokenExpired);

        user.StatusCode = UserStatus.None;
        user.StatusToken = null;
        user.StatusTokenTime = null;
        DB.SaveChanges();
    }
    #endregion

    #region Forgot Password
    public void RequestPasswordReset(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new AuthenticationException(AuthenticationWarning.ModelIncorrect);

        email = email.ToLower().Normalize();
        User_Login? user = DB.User_Login.Include(s => s.O_Token).FirstOrDefault(s => s.EmailNormalized == email)
            ?? throw new AuthenticationException(AuthenticationWarning.NoUserFound);
        if (user.StatusCode is not UserStatus.None)
            throw new AuthenticationException(AuthenticationWarning.StatusTokenAlreadyRequested);

        DB.User_Token.RemoveRange(user.O_Token);
        user.StatusCode = UserStatus.PasswordReset;
        user.StatusToken = GetStatusToken();
        user.StatusTokenTime = StatusTokenDuration;
        DB.SaveChanges();
    }

    public void ResetPassword(Model_ResetPassword? model)
    {
        if (model is null ||
            string.IsNullOrWhiteSpace(model.Password)
            || model.Token == Guid.Empty)
            throw new AuthenticationException(AuthenticationWarning.ModelIncorrect);

        User_Login? user = DB.User_Login.FirstOrDefault(s => s.StatusToken == model.Token);

        if (ValidatePassword(model.Password) != PasswordError.None)
            throw new AuthenticationException(AuthenticationWarning.ModelIncorrect);
        if (user is null)
            throw new AuthenticationException(AuthenticationWarning.StatusTokenNotFound);
        if (user.StatusCode == UserStatus.PasswordReset)
            throw new AuthenticationException(AuthenticationWarning.WrongPassword);
        if (DateTime.UtcNow >= user.StatusTokenTime)
            throw new AuthenticationException(AuthenticationWarning.RefreshTokenExpired);

        user.PasswordSalt = GetSalt();
        user.PasswordHash = HashPassword(model.Password, user.PasswordSalt);

        user.StatusCode = UserStatus.None;
        user.StatusToken = null;
        user.StatusTokenTime = null;
        DB.SaveChanges();
    }
    #endregion

    #region Userdata]
    public void UpdateEmail(string? email) { }

    public void VerifiyUpdatedEmail(Guid? token) { }

    public void GetUserData() { }

    public void UpdateUserData(Model_Userdata? model) { }
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
        email = email.ToLower().Normalize();
        if (!email.Contains('@') || !email.Contains('.'))
            throw new AuthenticationException(AuthenticationWarning.EmailInvalid);
        if (DB.User_Login.Any(s => s.EmailNormalized == email))
            throw new AuthenticationException(AuthenticationWarning.EmailAlreadyExists);
        return email;
    }

    public string ValidateUsername(string? username)
    {
        username = username.ToLower().Normalize();
        if (username.Length < 5 || username.Length > 50 || !OnlyUsernameValidChars().IsMatch(username))
            throw new AuthenticationException(AuthenticationWarning.UsernameInvalid);
        if (DB.User_Login.Any(s => s.Username == username))
            throw new AuthenticationException(AuthenticationWarning.UsernameAlreadyExists);
        return username;
    }

    public PasswordError ValidatePassword(string? password)
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
