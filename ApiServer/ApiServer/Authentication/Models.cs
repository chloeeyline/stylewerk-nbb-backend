using StyleWerk.NBB.Database.User;

namespace StyleWerk.NBB.Authentication;

public record ApplicationUser
{
    public ApplicationUser()
    {
        Instantiated = false;
        ID = Guid.Empty;
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
            Birthday = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day)
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

public record Model_Registration(string Username, string Email, string Password, string FirstName, string LastName, UserGender Gender, DateOnly Birthday);

public record Model_Login(string Username, string Password, bool ConsistOverSession);
public record Model_RefreshToken(string Token, bool ConsistOverSession);

public record Model_Userdata(string Email, string? Password, string FirstName, string LastName, UserGender Gender, DateOnly Birthday);
public record Model_ResetPassword(Guid Token, string Password);
public record Model_ValidateIdentification(string ToValidate);
public record Model_StatusToken(Guid Token);

#region Result
public record AuthenticationResult(Model_Token AccessToken, Model_Token RefreshToken, UserStatus? StatusCode, bool ConsistOverSession, string Username, bool Admin, string[] Rights);
public record Model_Token(string Token, long ExpireTime);
#endregion