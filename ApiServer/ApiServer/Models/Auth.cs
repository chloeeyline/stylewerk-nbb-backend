using System.ComponentModel.DataAnnotations;

using StyleWerk.NBB.Database.User;

namespace StyleWerk.NBB.Models;

public record ApplicationUser
{
    public ApplicationUser()
    {
        Instantiated = false;
        Login = new()
        {
            ID = Guid.Empty,
            Email = string.Empty,
            EmailNormalized = string.Empty,
            Username = string.Empty,
            UsernameNormalized = string.Empty,
            PasswordSalt = string.Empty,
            PasswordHash = string.Empty,
            Admin = false,
        };
        Information = new()
        {
            ID = Guid.Empty,
            FirstName = string.Empty,
            LastName = string.Empty,
            Gender = UserGender.NotSpecified,
            Birthday = new DateTimeOffset(new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day)).ToUnixTimeMilliseconds(),
        };
        Rights = [];
    }

    public ApplicationUser(User_Login login, User_Information information, string[] rights)
    {
        Login = login ?? throw new ArgumentNullException(nameof(login));
        Information = information ?? throw new ArgumentNullException(nameof(information));
        Rights = rights;
        ID = login.ID;
        Instantiated = true;
    }

    public bool Instantiated { get; init; }
    public Guid ID { get; init; }
    public User_Login Login { get; init; }
    public User_Information Information { get; init; }
    public string[] Rights { get; init; }
}

public record Model_UserData(string Username, string Email, string FirstName, string LastName, UserGender Gender, long Birthday);
public record Model_UpdateUserData(string? Password, string? FirstName, string? LastName, UserGender? Gender);

public record Model_Registration(string Username, string Email, string Password, string FirstName, string LastName, UserGender Gender, long Birthday);

public record Model_Login([Required] string Username, [Required] string Password, bool ConsistOverSession);
public record Model_RefreshToken(string Token, bool ConsistOverSession);

public record Model_ResetPassword(string Token, string Password);
public record Model_ValidateIdentification(string ToValidate);
public record Model_StatusToken(string Token);

#region Result
public record AuthenticationResult(Model_Token AccessToken, Model_Token RefreshToken, UserStatus? StatusCode, bool ConsistOverSession, string Username, bool Admin, string[] Rights);
public record Model_Token(string Token, long ExpireTime);
#endregion