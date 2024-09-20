using StyleWerk.NBB.Database.User;

namespace StyleWerk.NBB.Authentication;

public record Model_Login(string User, string Password, bool RememberMe);
public record Model_LoginResult(string Token, DateTime TokenExpiresAt, string? RefreshToken, string Username, Model_Right Right);
public record Model_Right(bool Admin, Model_UserRight Entries, Model_UserRight Templates);
public record Model_VerifyEmail(Guid Token, string Email);
public record Model_ResetPassword(Guid Token, string Password);
public record Model_Registration(string Username, string Email, string Password, string FirstName, string LastName, UserGender Gender, DateOnly Birthday);

public record Model_UserRight(bool Restricted, bool Access, bool Create, bool Edit, bool Delete, bool Admin)
{
	public Model_UserRight(UserRight right) : this(false, false, false, false, false, false)
	{
		if ((right & UserRight.Restricted) == UserRight.Restricted) Restricted = true;
		if ((right & UserRight.Access) == UserRight.Access) Access = true;
		if ((right & UserRight.Create) == UserRight.Create) Create = true;
		if ((right & UserRight.Edit) == UserRight.Edit) Edit = true;
		if ((right & UserRight.Delete) == UserRight.Delete) Delete = true;
		if ((right & UserRight.Admin) == UserRight.Admin) Admin = true;
	}
}