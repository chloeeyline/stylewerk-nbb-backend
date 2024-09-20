using StyleWerk.NBB.Database.User;

namespace StyleWerk.NBB.Authentication;

public record ApplicationUser(bool Instantiated, Guid ID, User_Login Login, User_Information Information, User_Right Right);

public record Model_Registration(string Username, string Email, string Password, string FirstName, string LastName, UserGender Gender, DateOnly Birthday);

public record Model_Login(string Username, string Password, bool ConsistOverSession);
public record Model_RefreshToken(string Token, bool ConsistOverSession);

public record Model_Userdata(string Email, string? Password, string FirstName, string LastName, UserGender Gender, DateOnly Birthday);
public record Model_ResetPassword(Guid Token, string Password);



#region Result
public record AuthenticationResult(Model_Token AccessToken, Model_Token RefreshToken, string Username, Model_RightList Rights);
public record Model_Token(string Token, long ExpireTime);
public record Model_RightList(bool Admin, Model_RightItem Entries, Model_RightItem Templates)
{
    public Model_RightList() : this(false, new(UserRight.Restricted), new(UserRight.Restricted)) { }
    public Model_RightList(User_Right right) : this(right.Admin, new(UserRight.Restricted), new(UserRight.Restricted)) { }
}


public record Model_RightItem(bool Restricted, bool Access, bool Create, bool Edit, bool Delete, bool Admin)
{
    public Model_RightItem(UserRight right) : this(false, false, false, false, false, false)
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