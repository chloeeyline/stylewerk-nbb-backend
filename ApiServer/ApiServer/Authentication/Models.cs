using StyleWerk.NBB.Database.User;

namespace StyleWerk.NBB.Authentication;

public record ApplicationUser(bool Instantiated, Guid ID, User_Login Login, User_Information Information, User_Right Right);

public record Model_Registration(string Username, string Email, string Password, string FirstName, string LastName, UserGender Gender, DateOnly Birthday);

public record Model_Login(string Username, string Password, bool ConsistOverSession);
public record Model_RefreshToken(string Token, bool ConsistOverSession);

public record Model_Userdata(string Email, string? Password, string FirstName, string LastName, UserGender Gender, DateOnly Birthday);
public record Model_ResetPassword(Guid Token, string Password);



#region Result
public record AuthenticationResult(Model_Token AccessToken, Model_Token RefreshToken, bool ConsistOverSession, string Username, bool Admin, Model_Rights[] Rights);
public record Model_Token(string Token, long ExpireTime);

public record Model_Rights(string Name, bool Restricted, bool Access, bool Create, bool Edit, bool Delete, bool Admin)
{
    public Model_Rights(User_Right right) : this(right.Name, false, false, false, false, false, false)
    {
        if ((right.Type & UserRight.Restricted) == UserRight.Restricted) Restricted = true;
        if ((right.Type & UserRight.Access) == UserRight.Access) Access = true;
        if ((right.Type & UserRight.Create) == UserRight.Create) Create = true;
        if ((right.Type & UserRight.Edit) == UserRight.Edit) Edit = true;
        if ((right.Type & UserRight.Delete) == UserRight.Delete) Delete = true;
        if ((right.Type & UserRight.Admin) == UserRight.Admin) Admin = true;
    }
    public Model_Rights(string Name, UserRight right) : this(Name, false, false, false, false, false, false)
    {
        if ((right & UserRight.Restricted) == UserRight.Restricted) Restricted = true;
        if ((right & UserRight.Access) == UserRight.Access) Access = true;
        if ((right & UserRight.Create) == UserRight.Create) Create = true;
        if ((right & UserRight.Edit) == UserRight.Edit) Edit = true;
        if ((right & UserRight.Delete) == UserRight.Delete) Delete = true;
        if ((right & UserRight.Admin) == UserRight.Admin) Admin = true;
    }
}
#endregion