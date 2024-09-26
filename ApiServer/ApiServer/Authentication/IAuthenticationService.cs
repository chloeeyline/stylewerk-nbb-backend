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
    void VerifyEmail(string? token);

    void RemoveSessions(Guid userID, string userAgent);
    void Logout(Guid userID, string userAgent);

    void RequestPasswordReset(string email);
    void ResetPassword(Model_ResetPassword? model);

    void UpdateEmail(string? email, User_Login user);
    void VerifyUpdatedEmail(string? code, User_Login user, string userAgent);

    Model_UserData GetUserData(ApplicationUser user);
    void UpdateUserData(Model_UpdateUserData? model, Guid userID);

    string ValidateEmail(string? email);
    void ValidatePassword(string? password);
    string ValidateUsername(string? username);
}
