using System.ComponentModel.DataAnnotations;

using StyleWerk.NBB.Database.User;

namespace StyleWerk.NBB.Models;

public record Model_UserData(string Username, string Email, string FirstName, string LastName, UserGender Gender, long Birthday);
public record Model_UpdateUserData(string? Password, string? FirstName, string? LastName, UserGender? Gender);

public record Model_Registration(string Username, string Email, string Password, string FirstName, string LastName, UserGender Gender, long Birthday);

public record Model_Login([Required] string Username, [Required] string Password, bool ConsistOverSession);
public record Model_RefreshToken(string Token, bool ConsistOverSession);

public record Model_ResetPassword(string Token, string Password);
public record Model_ValidateIdentification(string ToValidate);
public record Model_StatusToken(string Token);

#region Result
public record AuthenticationResult(Model_Token AccessToken, Model_Token RefreshToken, UserStatus? StatusCode, bool ConsistOverSession, string Username, bool Admin);
public record Model_Token(string Token, long ExpireTime);
#endregion