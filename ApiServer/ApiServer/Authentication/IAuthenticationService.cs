using StyleWerk.NBB.Database.User;

namespace StyleWerk.NBB.Authentication;

public interface IAuthenticationService
{
    User_Login GetUser(Model_Login? model);
    User_Login GetUser(Model_RefreshToken? model, string userAgent);

    Model_Token GetAccessToken(User_Login user);
    Model_Token GetRefreshToken(Guid userID, string userAgent, bool? consistOverSession);
    AuthenticationResult GetAuthenticationResult(Guid id, Model_Token accessToken, Model_Token refreshToken, bool? consistOverSession);

    void Registration(Model_Registration? model);
    void VerifyEmail(Guid? token);

    void RequestPasswordReset(string email);
    void ResetPassword(Model_ResetPassword? model);
    void UpdateEmail(string? email, Guid userID, string userAgent);
    void VerifyUpdatedEmail(Guid? token);
    void GetUserData();
    void UpdateUserData(Model_Userdata? model);
    string ValidateEmail(string? email);
    void ValidatePassword(string? password);
    string ValidateUsername(string? username);
}
