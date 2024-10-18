using System.ComponentModel.DataAnnotations;

using StyleWerk.NBB.Database.User;

namespace StyleWerk.NBB.Models;

/// <summary>
/// Used to get userdata of the current user
/// </summary>
/// <param name="Username">username of the current user</param>
/// <param name="Email">email of the current user</param>
/// <param name="FirstName">firstname of the current user</param>
/// <param name="LastName">lastname of the current user</param>
/// <param name="Gender">gender of the current user. Can be male, female, not specified or non binary</param>
/// <param name="Birthday">birthday converted to unix timestamp</param>
public record Model_UserData(string Username, string Email, string FirstName, string LastName, UserGender Gender, long Birthday);

/// <summary>
/// Used to update updateable userdata
/// </summary>
/// <param name="Password">new password or null if the password shouldn't be changed</param>
/// <param name="FirstName">new firstname or null if the firstname shouldn't be changed</param>
/// <param name="LastName">new lastname or null if the lastname shouldn't be changed</param>
/// <param name="Gender">new gender or null if the gender shouldn't be changed</param>
public record Model_UpdateUserData(string? Password, string? FirstName, string? LastName, UserGender? Gender);

/// <summary>
/// Used to register a new user
/// </summary>
/// <param name="Username">username of the new user</param>
/// <param name="Email">email of the new user</param>
/// <param name="Password">password of the new user</param>
/// <param name="FirstName">firstname of the new user</param>
/// <param name="LastName">lastname of the new user</param>
/// <param name="Gender">gender of the new user</param>
/// <param name="Birthday">birthday as unix timestamp</param>
public record Model_Registration(string Username, string Email, string Password, string FirstName, string LastName, UserGender Gender, long Birthday);

/// <summary>
/// Used to login with the given parameters
/// </summary>
/// <param name="Username">username of the current user</param>
/// <param name="Password">password of the current user</param>
/// <param name="ConsistOverSession">decides whether the login will persist. Can be true or false</param>
public record Model_Login([Required] string Username, [Required] string Password, bool ConsistOverSession);

/// <summary>
/// Used to login via refresh token
/// </summary>
/// <param name="Token">refresh token. Used to keep the user logged in</param>
/// <param name="ConsistOverSession">decides whether the login should persist. Can be true or false</param>
public record Model_RefreshToken(string Token, bool ConsistOverSession);

/// <summary>
/// Used to reset the current password of the current user
/// </summary>
/// <param name="Token">the token associated for the password reset</param>
/// <param name="Password">new password</param>
public record Model_ResetPassword(string Token, string Password);

/// <summary>
/// Used to validate one of these options: username, password or email<br/>
/// Also used request a password resed, because an extra model was not usefull here
/// </summary>
/// <param name="ToValidate">can be username, password or email as string</param>
public record Model_ValidateIdentification(string ToValidate);

public record Model_StatusToken(string Token);

#region Result
/// <summary>
/// Will be returned on successful login
/// </summary>
/// <param name="AccessToken">access token as string and its expire time in form of a unix timestamp. Used to identifiy the user and granting access to the application on login</param>
/// <param name="RefreshToken">refresh token as string  and its expire time in form of a unix timestamp, to keep the users login</param>
/// <param name="StatusCode">status code as string. Can be one of the following: null, EmailVerification, EmailChange, PasswordReset</param>
/// <param name="ConsistOverSession">decides whether the login should persist. Can be true or false</param>
/// <param name="Username">username as string</param>
/// <param name="Admin">shows if the current user is an admin. Can be true or false</param>
public record AuthenticationResult(Model_Token AccessToken, Model_Token RefreshToken, UserStatus? StatusCode, bool ConsistOverSession, string Username, bool Admin);

/// <summary>
/// Contains a login related token with its expire time
/// </summary>
/// <param name="Token">refresh or access token as string</param>
/// <param name="ExpireTime">expire time of the token as an unix timestamp</param>
public record Model_Token(string Token, long ExpireTime);
#endregion