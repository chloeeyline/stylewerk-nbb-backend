using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using StyleWerk.NBB.AWS;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Core;
using StyleWerk.NBB.Database.User;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

using UAParser;

namespace StyleWerk.NBB.Authentication;

public partial class AuthQueries(NbbContext DB, ApplicationUser CurrentUser, string UserAgent, SecretData SecretData) : BaseQueries(DB, CurrentUser)
{
    #region Fixed Values
    private const int KeySize = 256;
    #endregion

    #region Login
    public User_Login GetUser(Model_Login? model)
    {
        if (model is null ||
            string.IsNullOrWhiteSpace(model.Username) ||
            string.IsNullOrWhiteSpace(model.Password))
            throw new RequestException(ResultCodes.DataIsInvalid);

        string userName = model.Username.ToLower().Normalize();
        User_Login? user = DB.User_Login.FirstOrDefault(s => s.UsernameNormalized == userName)
            ?? throw new RequestException(ResultCodes.NoUserFound);

        string hashedPassword = HashPassword(model.Password, user.PasswordSalt);
        return user.PasswordHash != hashedPassword ? throw new RequestException(ResultCodes.NoUserFound) : user;
    }

    public User_Login GetUser(Model_RefreshToken? model)
    {
        if (model is null ||
            string.IsNullOrWhiteSpace(model.Token) ||
            string.IsNullOrWhiteSpace(UserAgent))
            throw new RequestException(ResultCodes.DataIsInvalid);

        string agent = GetUserAgentString(UserAgent);
        User_Token? token = DB.User_Token.FirstOrDefault(s => s.Agent == agent && s.RefreshToken == model.Token)
            ?? throw new RequestException(ResultCodes.RefreshTokenNotFound);

        if (UserTimeStamps.Now >= token.RefreshTokenExpiryTime)
            throw new RequestException(ResultCodes.RefreshTokenExpired);

        User_Login? user = DB.User_Login.FirstOrDefault(s => s.ID == token.ID)
        ?? throw new RequestException(ResultCodes.NoUserFound);

        return user;
    }

    public Model_Token GetAccessToken(User_Login user)
    {
        if (user.StatusCode is UserStatus.EmailVerification)
            throw new RequestException(ResultCodes.EmailIsNotVerified);
        if (user.StatusCode == UserStatus.PasswordReset)
            throw new RequestException(ResultCodes.PasswordResetWasRequested);

        SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(SecretData.JwtKey));
        SigningCredentials credentials = new(securityKey, SecurityAlgorithms.HmacSha256);

        Claim[] claims = [new Claim(ClaimTypes.Sid, user.ID.ToString())];

        JwtSecurityToken token = new(SecretData.JwtIssuer, SecretData.JwtAudience, claims, expires: UserTimeStamps.LoginTokenDate.DateTime, signingCredentials: credentials);
        string loginToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new Model_Token(loginToken, UserTimeStamps.LoginTokenDuration);
    }

    public Model_Token GetRefreshToken(Guid userID, bool? consistOverSession)
    {
        string agent = GetUserAgentString(UserAgent);
        string refreshToken = GenerateRandomKey();
        long expireTime = consistOverSession is true ? UserTimeStamps.RefreshTokenDuration : UserTimeStamps.RefreshTokenShortDuration;

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

        return new Model_Token(refreshToken, expireTime);
    }

    public AuthenticationResult GetAuthenticationResult(Guid id, Model_Token accessToken, Model_Token refreshToken, bool? consistOverSession)
    {
        User_Login? user = DB.User_Login.Include(s => s.O_Information)
            .FirstOrDefault(s => s.ID == id)
            ?? throw new RequestException(ResultCodes.NoUserFound);

        return new AuthenticationResult(accessToken, refreshToken, user.StatusCode, consistOverSession is true, user.Username, user.Admin);
    }

    public AuthenticationResult Login(Model_Login? model)
    {
        User_Login user = GetUser(model);
        Model_Token accessToken = GetAccessToken(user);
        Model_Token refreshToken = GetRefreshToken(user.ID, model?.ConsistOverSession);
        AuthenticationResult result = GetAuthenticationResult(user.ID, accessToken, refreshToken, model?.ConsistOverSession);

        return result;
    }

    public AuthenticationResult RefreshToken([FromBody] Model_RefreshToken? model)
    {
        User_Login user = GetUser(model);
        Model_Token accessToken = GetAccessToken(user);
        Model_Token refreshToken = GetRefreshToken(user.ID, model?.ConsistOverSession);
        AuthenticationResult result = GetAuthenticationResult(user.ID, accessToken, refreshToken, model?.ConsistOverSession);

        return result;
    }
    #endregion

    #region Registration
    public string Registration(Model_Registration? model)
    {
        if (model is null ||
            string.IsNullOrWhiteSpace(model.Username) ||
            string.IsNullOrWhiteSpace(model.Email) ||
            string.IsNullOrWhiteSpace(model.Password))
            throw new RequestException(ResultCodes.DataIsInvalid);

        string email = ValidateEmail(model.Email);
        string username = ValidateUsername(model.Username);
        string salt = GetSalt();
        long birthday = new DateTimeOffset(
            DateTimeOffset.FromUnixTimeMilliseconds(model.Birthday)
            .Date).ToUnixTimeMilliseconds();
        ValidatePassword(model.Password);

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
            StatusTokenExpireTime = UserTimeStamps.StatusTokenDuration,
            StatusCode = UserStatus.EmailVerification,
            Admin = false
        };

        User_Information userInformation = new()
        {
            ID = user.ID,
            Gender = model.Gender,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Birthday = birthday,
        };

        DB.User_Login.Add(user);
        DB.User_Information.Add(userInformation);
        DB.SaveChanges();

        SendMail_VerifyEmail(email, user.StatusToken);

#if Local
        return user.StatusToken;
#else
        return "";
#endif
    }

    public void VerifyEmail(string? token)
    {
        if (token is null)
            throw new RequestException(ResultCodes.DataIsInvalid);

        User_Login? user = DB.User_Login.FirstOrDefault(s => s.StatusToken == token)
            ?? throw new RequestException(ResultCodes.StatusTokenNotFound);
        if (user.StatusCode != UserStatus.EmailVerification)
            throw new RequestException(ResultCodes.WrongStatusCode);
        if (UserTimeStamps.Now >= user.StatusTokenExpireTime)
            throw new RequestException(ResultCodes.StatusTokenExpired);

        user.StatusCode = null;
        user.StatusToken = null;
        user.StatusTokenExpireTime = null;
        DB.SaveChanges();
    }
    #endregion

    #region Forgot Password
    public string RequestPasswordReset(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new RequestException(ResultCodes.DataIsInvalid);

        email = email.ToLower().Normalize();
        User_Login? user = DB.User_Login.Include(s => s.O_Token).FirstOrDefault(s => s.EmailNormalized == email)
            ?? throw new RequestException(ResultCodes.NoUserFound);
        if (user.StatusCode == UserStatus.EmailVerification)
            throw new RequestException(ResultCodes.EmailIsNotVerified);

        DB.User_Token.RemoveRange(user.O_Token);
        user.StatusCode = UserStatus.PasswordReset;
        user.StatusToken = GetStatusToken();
        user.StatusTokenExpireTime = UserTimeStamps.StatusTokenDuration;
        DB.SaveChanges();

        SendMail_ResetPassword(email, user.StatusToken);
#if Local
        return user.StatusToken;
#else
        return "";
#endif
    }

    public void ResetPassword(Model_ResetPassword? model)
    {
        if (model is null ||
            string.IsNullOrWhiteSpace(model.Password))
            throw new RequestException(ResultCodes.DataIsInvalid);

        User_Login? user = DB.User_Login.FirstOrDefault(s => s.StatusToken == model.Token);

        ValidatePassword(model.Password);
        if (user is null)
            throw new RequestException(ResultCodes.StatusTokenNotFound);
        if (user.StatusCode is null || user.StatusCode is not UserStatus.PasswordReset)
            throw new RequestException(ResultCodes.WrongStatusCode);
        if (UserTimeStamps.Now >= user.StatusTokenExpireTime)
            throw new RequestException(ResultCodes.RefreshTokenExpired);

        user.PasswordSalt = GetSalt();
        user.PasswordHash = HashPassword(model.Password, user.PasswordSalt);

        user.StatusCode = null;
        user.StatusToken = null;
        user.StatusTokenExpireTime = null;
        DB.SaveChanges();
    }
    #endregion

    #region Edit Session
    public void RemoveSessions()
    {
        DB.User_Token.RemoveRange(DB.User_Token.Where(s => s.ID == CurrentUser.ID && s.Agent != UserAgent));
    }

    public void Logout()
    {
        DB.User_Token.RemoveRange(DB.User_Token.Where(s => s.ID == CurrentUser.ID && s.Agent == UserAgent));
    }
    #endregion

    #region Change Email
    public string UpdateEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new RequestException(ResultCodes.DataIsInvalid);

        if (CurrentUser.Login.StatusCode is null || CurrentUser.Login.StatusCode is UserStatus.PasswordReset)
            throw new RequestException(ResultCodes.PasswordResetWasRequested);

        CurrentUser.Login.NewEmail = email;
        CurrentUser.Login.StatusCode = UserStatus.EmailChange;
        CurrentUser.Login.StatusToken = new Random().Next(100001).ToString("D6");
        CurrentUser.Login.StatusTokenExpireTime = UserTimeStamps.StatusTokenDuration;
        DB.SaveChanges();

        SendMail_UpdateEmail(email, CurrentUser.Login.StatusToken);

#if Local
        return CurrentUser.Login.StatusToken;
#else
        return "";
#endif
    }

    public void VerifyUpdatedEmail(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new RequestException(ResultCodes.DataIsInvalid);

        if (string.IsNullOrEmpty(CurrentUser.Login.StatusToken) || !token.Equals(CurrentUser.Login.StatusToken))
            throw new RequestException(ResultCodes.EmailChangeCodeWrong);
        if (CurrentUser.Login.StatusCode is null || CurrentUser.Login.StatusCode is not UserStatus.EmailChange)
            throw new RequestException(ResultCodes.WrongStatusCode);
        if (string.IsNullOrWhiteSpace(CurrentUser.Login.NewEmail))
            throw new RequestException(ResultCodes.WrongStatusCode);
        if (UserTimeStamps.Now >= CurrentUser.Login.StatusTokenExpireTime)
            throw new RequestException(ResultCodes.StatusTokenExpired);

        DB.User_Token.RemoveRange(DB.User_Token.Where(s => s.ID == CurrentUser.ID && s.Agent != UserAgent));
        CurrentUser.Login.Email = CurrentUser.Login.NewEmail;
        CurrentUser.Login.EmailNormalized = CurrentUser.Login.NewEmail.NormalizeName();
        CurrentUser.Login.NewEmail = null;

        CurrentUser.Login.StatusCode = null;
        CurrentUser.Login.StatusToken = null;
        CurrentUser.Login.StatusTokenExpireTime = null;
        DB.SaveChanges();
    }
    #endregion

    #region Userdata
    public Model_UserData GetUserData()
    {
        return new Model_UserData(
            CurrentUser.Login.Username,
            CurrentUser.Login.Email,
            CurrentUser.Information.FirstName,
            CurrentUser.Information.LastName,
            CurrentUser.Information.Gender,
            CurrentUser.Information.Birthday);
    }

    public void UpdateUserData(Model_UpdateUserData? model)
    {
        if (model is null)
            throw new RequestException(ResultCodes.DataIsInvalid);

        User_Login? user = CurrentUser.Login ?? throw new RequestException(ResultCodes.NoUserFound);
        if (user.StatusCode is not null)
            throw new RequestException(ResultCodes.PendingChangeOpen);

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
    private string GetStatusToken()
    {
        string statusToken = Guid.NewGuid().ToString();
        while (DB.User_Login.Any(s => s.StatusToken == statusToken))
            statusToken = Guid.NewGuid().ToString();
        return statusToken;
    }
    public string GetSalt()
    {
        string salt = GenerateRandomKey();
        while (DB.User_Login.Any(s => s.PasswordSalt == salt))
            salt = GenerateRandomKey();
        return salt;
    }
    public string HashPassword(string password, string salt) => ComputeHash(password, salt, SecretData.PasswortPepper, 5);

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
        email = email?.NormalizeName();
        return string.IsNullOrWhiteSpace(email) || !email.Contains('@') || !email.Contains('.')
            ? throw new RequestException(ResultCodes.EmailInvalid)
            : DB.User_Login.Any(s => s.EmailNormalized == email)
            ? throw new RequestException(ResultCodes.EmailAlreadyExists)
            : email;
    }

    public string ValidateUsername(string? username)
    {
        username = username?.NormalizeName();
        return string.IsNullOrWhiteSpace(username) || username.Length < 5
            ? throw new RequestException(ResultCodes.UnToShort)
            : username.Length > 50
            ? throw new RequestException(ResultCodes.UnToShort)
            : !UsedRegex.OnlyUsernameValidChars().IsMatch(username)
            ? throw new RequestException(ResultCodes.UnUsesInvalidChars)
            : DB.User_Login.Any(s => s.UsernameNormalized == username)
            ? throw new RequestException(ResultCodes.UsernameAlreadyExists)
            : username;
    }

    public void ValidatePassword(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new RequestException(ResultCodes.PwTooShort);
        if (password.Length < 10)
            throw new RequestException(ResultCodes.PwTooShort);
        if (!UsedRegex.ContainsLowercase().IsMatch(password))
            throw new RequestException(ResultCodes.PwHasNoLowercaseLetter);
        if (!UsedRegex.ContainsUppercase().IsMatch(password))
            throw new RequestException(ResultCodes.PwHasNoUppercaseLetter);
        if (!UsedRegex.ContainsDigit().IsMatch(password))
            throw new RequestException(ResultCodes.PwHasNoNumber);
        if (!UsedRegex.ContainsPasswordSpecialChar().IsMatch(password))
            throw new RequestException(ResultCodes.PwHasNoSpecialChars);
        if (UsedRegex.ContainsWhitespace().IsMatch(password))
            throw new RequestException(ResultCodes.PwHasWhitespace);
        if (!UsedRegex.OnlyPasswordValidChars().IsMatch(password))
            throw new RequestException(ResultCodes.PwUsesInvalidChars);
    }
    #endregion

    #region Email
    private bool SendMail_VerifyEmail(string email, string token)
    {
        string content = SimpleEmailService.AccessEmailTemplate("EmailVerification.html");
        string url = $"{SecretData.FrontendUrl}/user/verify-email?id={token}";
        content = content.Replace("YOUR_VERIFICATION_LINK_HERE", url);
#if Local
        return true;
#endif
        return SimpleEmailService.SendMail("noreply@stylewerk.org", email, "Stylewerk NBB - Email Verification for new Account", content);
    }

    private bool SendMail_ResetPassword(string email, string token)
    {
        string content = SimpleEmailService.AccessEmailTemplate("ResetPassword.html");
        string url = $"{SecretData.FrontendUrl}/user/reset-password?id={token}";
        content = content.Replace("YOUR_VERIFICATION_LINK_HERE", url);
#if Local
        return true;
#endif
        return SimpleEmailService.SendMail("noreply@stylewerk.org", email, "Stylewerk NBB - Email Verification for new Account", content);
    }

    private static bool SendMail_UpdateEmail(string email, string status)
    {
        string content = SimpleEmailService.AccessEmailTemplate("EmailChange.html");
        content = content.Replace("YOUR_VERIFICATION_LINK_HERE", status);
#if Local
        return true;
#endif
        return SimpleEmailService.SendMail("noreply@stylewerk.org", email, "Stylewerk NBB - Email Verification for new Account", content);
    }
    #endregion
}

public static class UserTimeStamps
{
    public static DateTimeOffset StatusTokenDate => DateTimeOffset.UtcNow.AddDays(1);
    public static DateTimeOffset LoginTokenDate => DateTimeOffset.UtcNow.AddHours(3);
    public static DateTimeOffset RefreshTokenShortDate => DateTimeOffset.UtcNow.AddHours(4);
    public static DateTimeOffset RefreshTokenDate => DateTimeOffset.UtcNow.AddDays(7);
    public static DateTimeOffset CurrentDate => DateTimeOffset.UtcNow;

    public static long StatusTokenDuration => StatusTokenDate.ToUnixTimeMilliseconds();
    public static long LoginTokenDuration => LoginTokenDate.ToUnixTimeMilliseconds();
    public static long RefreshTokenShortDuration => RefreshTokenShortDate.ToUnixTimeMilliseconds();
    public static long RefreshTokenDuration => RefreshTokenDate.ToUnixTimeMilliseconds();
    public static long Now => CurrentDate.ToUnixTimeMilliseconds();
}

public static partial class UsedRegex
{
    [GeneratedRegex(@"[a-z]")] public static partial Regex ContainsLowercase();
    [GeneratedRegex(@"[A-Z]")] public static partial Regex ContainsUppercase();
    [GeneratedRegex(@"\d")] public static partial Regex ContainsDigit();
    [GeneratedRegex(@"[!#$%&'*+\-./?@\\_|]")] public static partial Regex ContainsPasswordSpecialChar();
    [GeneratedRegex(@"\s")] public static partial Regex ContainsWhitespace();
    [GeneratedRegex(@"[a-zA-Z\d!#$%&'*+\-./?@\\_|^\s]")] public static partial Regex OnlyPasswordValidChars();
    [GeneratedRegex(@"[a-zA-Z\d&'*+\-./\\_|^\s]")] public static partial Regex OnlyUsernameValidChars();
}
