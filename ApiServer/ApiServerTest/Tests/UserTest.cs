using StyleWerk.NBB.Authentication;
using StyleWerk.NBB.AWS;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.User;
using StyleWerk.NBB.Models;

namespace ApiServerTest.Tests
{
    [Collection("Sequential")]

    public class UserTest
    {
        private Guid DefaultUserGuid { get; set; }

        private readonly string DefaultPassword = "TestTest@123";
        private readonly string DefaultEmail = "test@gmail.com";
        private readonly string DefaultUser = "TestUser";


        #region Register

        [Fact]
        public void Register()
        {
            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());
            Model_Registration register = new("Test123Juliane", "juliane.krenek@lbs4.salzburg.at", "TestTest123@", "Juliane", "Krenek", UserGender.Female, 0);
            query.Registration(register);
            Assert.True(true);
        }

        [Fact]
        public void RegisterDataInvalid()
        {
            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());
            DateTime birthday = new(2003, 4, 24);
            Model_Registration register = new("", "", "", "Juliane", "Krenek", UserGender.Female, birthday.Ticks);

            try
            {
                query.Registration(register);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.DataIsInvalid);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        #endregion

        #region Login
        [Fact]
        public void Login_GetUser_DataInvalid_Null()
        {
            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());
            Model_Login login = new("", "", true);
            User_Login action() => query.GetUser(login);

            RequestException exception = Assert.Throws<RequestException>((Func<User_Login>) action);
            RequestException result = new(ResultCodes.DataIsInvalid);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void Login_GetUser_DataInvalid_WhiteSpace()
        {
            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());
            Model_Login login = new(" ", " ", true);
            User_Login action() => query.GetUser(login);

            RequestException exception = Assert.Throws<RequestException>((Func<User_Login>) action);
            RequestException result = new(ResultCodes.DataIsInvalid);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void Login_GetUser_NotFound_Username()
        {
            Helpers.DeleteAll();
            Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);

            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());
            Model_Login login = new("Default", DefaultPassword, true);
            User_Login action() => query.GetUser(login);

            RequestException exception = Assert.Throws<RequestException>((Func<User_Login>) action);
            RequestException result = new(ResultCodes.NoUserFound);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void Login_GetUser_NotFound_Password()
        {
            Helpers.DeleteAll();
            Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);

            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());
            Model_Login login = new(DefaultUser, "TestAdmin@123", true);
            User_Login action() => query.GetUser(login);

            RequestException exception = Assert.Throws<RequestException>((Func<User_Login>) action);
            RequestException result = new(ResultCodes.NoUserFound);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void Login_GetAccessToken_EmailNotVerified()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);

            User_Login? setupUser = Helpers.DB.User_Login.First(u => u.ID == DefaultUserGuid);
            setupUser.StatusCode = UserStatus.EmailVerification;
            Helpers.DB.SaveChanges();

            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());
            Model_Login login = new(DefaultUser, DefaultPassword, true);
            User_Login user = query.GetUser(login);
            Model_Token action() => query.GetAccessToken(user);

            RequestException exception = Assert.Throws<RequestException>((Func<Model_Token>) action);
            RequestException result = new(ResultCodes.EmailIsNotVerified);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void Login_GetAccessToken_PasswordReset()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);

            User_Login? setupUser = Helpers.DB.User_Login.First(u => u.ID == DefaultUserGuid);
            setupUser.StatusCode = UserStatus.PasswordReset;
            Helpers.DB.SaveChanges();

            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());
            Model_Login login = new(DefaultUser, DefaultPassword, true);
            User_Login user = query.GetUser(login);

            Model_Token action() => query.GetAccessToken(user);

            RequestException exception = Assert.Throws<RequestException>((Func<Model_Token>) action);
            RequestException result = new(ResultCodes.PasswordResetWasRequested);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void Login_GetAuthenticationResult_NoUserFound()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);
            Helpers.SetUserTokensNull(DefaultUserGuid, DefaultEmail);

            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());
            Model_Login login = new(DefaultUser, DefaultPassword, true);
            User_Login user = query.GetUser(login);
            Model_Token token = query.GetAccessToken(user);
            Model_Token refreshToken = query.GetRefreshToken(user.ID, true);

            AuthenticationResult action() => query.GetAuthenticationResult(Guid.NewGuid(), token, refreshToken, true);

            RequestException exception = Assert.Throws<RequestException>((Func<AuthenticationResult>) action);
            RequestException result = new(ResultCodes.NoUserFound);
            Assert.Equal(result.Code, exception.Code);
        }
        #endregion

        #region VerifyEmail

        [Fact]
        public void VerifyEmail()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);

            User_Login? user = Helpers.DB.User_Login.FirstOrDefault(u => u.ID == DefaultUserGuid);

            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());
            query.VerifyEmail(user?.StatusToken);

            User_Login user2 = Helpers.DB.User_Login.First(u => u.ID == DefaultUserGuid);
            Assert.Null(user2.StatusCode);
            Assert.Null(user2.StatusToken);
            Assert.Null(user2.StatusTokenExpireTime);
        }


        [Fact]
        public void VerifyEmailDataInvalid()
        {
            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());
            try
            {
                query.VerifyEmail(null);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.DataIsInvalid);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void VerifyEmailTokenNotFound()
        {
            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());
            try
            {
                query.VerifyEmail(Guid.NewGuid().ToString());
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.StatusTokenNotFound);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void VerifyEmailWrongStatusCode()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);

            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());
            User_Login? user = Helpers.DB.User_Login.FirstOrDefault(u => u.ID == DefaultUserGuid);

            User_Login? dbUser = Helpers.DB.User_Login.FirstOrDefault(u => u.ID == DefaultUserGuid);

            string newToken = Guid.NewGuid().ToString();
            Assert.NotNull(user);
            Assert.NotNull(dbUser);
            user.StatusToken = newToken;
            dbUser.StatusCode = UserStatus.PasswordReset;
            Helpers.DB.SaveChanges();

            try
            {
                query.VerifyEmail(user?.StatusToken);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.WrongStatusCode);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void VerifyEmailTokenExpired()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);

            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());
            User_Login? user = Helpers.DB.User_Login.First(u => u.ID == DefaultUserGuid);
            long todayPlusTwo = new DateTimeOffset(DateTime.UtcNow).AddDays(2).ToUnixTimeMilliseconds();
            string newToken = Guid.NewGuid().ToString();
            user.StatusToken = newToken;
            user.StatusCode = UserStatus.EmailVerification;
            user.StatusTokenExpireTime = todayPlusTwo;
            Helpers.DB.SaveChanges();

            try
            {
                query.VerifyEmail(user?.StatusToken);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.StatusTokenExpired);
                Assert.Equal(result.Code, ex.Code);
            }
        }
        #endregion

        #region Password Reset
        [Fact]
        public void RequestPasswordReset()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);

            Helpers.SetUserTokensNull(DefaultUserGuid, DefaultEmail);
            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());
            query.RequestPasswordReset(DefaultEmail);
            Assert.True(true);
        }

        [Fact]
        public void ResetPassword()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);
            Helpers.SetUserTokensNull(DefaultUserGuid, DefaultEmail);

            AuthQueries query = Helpers.ReturnAuthQuery(DefaultUserGuid.ToString());

            string? token = query.RequestPasswordReset(DefaultEmail);
            Model_ResetPassword password = new(token, DefaultPassword);
            query.ResetPassword(password);
            Assert.True(true);
        }


        [Fact]
        public void RequestPasswordReset_NoUserFound()
        {
            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());

            try
            {
                query.RequestPasswordReset("schwammal666@gmail.com");
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.NoUserFound);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void RequestPasswordReset_DataInvalid()
        {
            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());

            try
            {
                query.RequestPasswordReset(null);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.DataIsInvalid);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void RequestPasswordReset_EmailNotVerified()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);

            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());
            User_Login? user = Helpers.DB.User_Login.FirstOrDefault(u => u.ID == DefaultUserGuid);
            Assert.NotNull(user);
            user.StatusCode = UserStatus.EmailVerification;
            Helpers.DB.SaveChanges();

            try
            {
                query.RequestPasswordReset(user.Email);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.EmailIsNotVerified);
                Assert.Equal(result.Code, ex.Code);
            }

        }

        [Fact]
        public void ResetPassword_DataInvalid()
        {
            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());
            try
            {
                query.ResetPassword(null);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.DataIsInvalid);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void ResetPassword_TokenNotFound()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);

            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());
            Model_ResetPassword password = new(Guid.NewGuid().ToString(), DefaultPassword);
            try
            {
                query.ResetPassword(password);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.StatusTokenNotFound);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void ResetPassword_StatusCode()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);
            Helpers.SetUserTokensNull(DefaultUserGuid, DefaultEmail);

            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());
            string? token = query.RequestPasswordReset(DefaultEmail);
            Model_ResetPassword password = new(token, DefaultPassword);

            try
            {
                query.ResetPassword(password);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.WrongStatusCode);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void ResetPassword_RefreshTokenExpired()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);

            AuthQueries query = Helpers.ReturnAuthQuery(Guid.NewGuid().ToString());

            User_Login? user = Helpers.DB.User_Login.FirstOrDefault(u => u.ID == DefaultUserGuid);
            long todayPlusTwo = new DateTimeOffset(DateTime.UtcNow).AddDays(2).ToUnixTimeMilliseconds();
            Assert.NotNull(user);
            user.StatusTokenExpireTime = todayPlusTwo;
            user.StatusCode = UserStatus.PasswordReset;
            user.StatusToken = null;
            Helpers.DB.SaveChanges();

            string? token = query.RequestPasswordReset(DefaultEmail);
            Model_ResetPassword password = new(token, DefaultPassword);

            try
            {
                query.ResetPassword(password);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.RefreshTokenExpired);
                Assert.Equal(result.Code, ex.Code);
            }
        }
        #endregion

        #region GetUser by User_Login Function
        [Fact]
        public void Login_GetUser_UserLogin()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);

            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());
            Model_Login login = new(DefaultUser, DefaultPassword, true);
            User_Login user = query.GetUser(login);

            Assert.NotNull(user);
            Assert.IsType<User_Login>(user);
        }
        #endregion

        #region GetUser by RefreshToken Function
        [Fact]
        public void RefreshToken_GetUser()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);
            Helpers.SetUserTokensNull(DefaultUserGuid, DefaultEmail);

            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());
            _ = new Model_Login(DefaultUser, DefaultPassword, true);
            Model_Token refreshToken = query.GetRefreshToken(DefaultUserGuid, true);
            Model_RefreshToken rToken = new(refreshToken.Token, true);
            User_Login user = query.GetUser(rToken);

            Assert.NotNull(user);
            Assert.NotNull(user.Username);
        }
        #endregion

        #region GetAccessToken

        [Fact]
        public void Login_GetAccessToken_UserLogin()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);
            Helpers.SetUserTokensNull(DefaultUserGuid, DefaultEmail);

            AuthQueries query = Helpers.ReturnAuthQuery(DefaultUserGuid.ToString());
            Model_Login login = new(DefaultUser, DefaultPassword, true);
            User_Login user = query.GetUser(login);
            Model_Token token = query.GetAccessToken(user);

            Assert.NotNull(token);
            Assert.IsType<Model_Token>(token);
        }
        #endregion

        #region RefreshToken Functions
        [Fact]
        public void Login_GetRefreshToken_UserLogin()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);
            Helpers.SetUserTokensNull(DefaultUserGuid, DefaultEmail);

            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());
            string? statusToken = query.RequestPasswordReset(DefaultEmail);
            Model_ResetPassword password = new(statusToken, DefaultPassword);
            query.ResetPassword(password);

            Model_Login login = new(DefaultUser, DefaultPassword, true);
            User_Login user = query.GetUser(login);
            _ = query.GetAccessToken(user);
            Model_Token refreshToken = query.GetRefreshToken(DefaultUserGuid, true);

            Assert.NotNull(refreshToken.Token);
        }

        [Fact]
        public void RefreshToken_Getuser_DataInvalid()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);
            Helpers.SetUserTokensNull(DefaultUserGuid, DefaultEmail);

            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());

            Model_RefreshToken token = new("", true);
            User_Login action() => query.GetUser(token);

            RequestException exception = Assert.Throws<RequestException>((Func<User_Login>) action);
            RequestException result = new(ResultCodes.DataIsInvalid);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void RefreshToken_Getuser_RefreshTokenNotFound()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);
            Helpers.SetUserTokensNull(DefaultUserGuid, DefaultEmail);

            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());

            Model_RefreshToken token = new("abcdefg", true);
            User_Login action() => query.GetUser(token);

            RequestException exception = Assert.Throws<RequestException>((Func<User_Login>) action);
            RequestException result = new(ResultCodes.RefreshTokenNotFound);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void RefreshToken_Getuser_RefreshTokenExpired()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);

            AuthQueries setup = Helpers.ReturnAuthQuery(Guid.Empty.ToString());

            Model_Token refreshTokenSetup = setup.GetRefreshToken(DefaultUserGuid, true);

            long todayPlusTwo = new DateTimeOffset(DateTime.UtcNow).AddDays(-2).ToUnixTimeMilliseconds();
            User_Token rToken = Helpers.DB.User_Token.First(t => t.ID == DefaultUserGuid);
            rToken.RefreshTokenExpiryTime = todayPlusTwo;
            Helpers.DB.SaveChanges();

            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());
            User_Token myToken = Helpers.DB.User_Token.First(t => t.ID == DefaultUserGuid);
            Model_RefreshToken token = new(myToken.RefreshToken, true);
            User_Login action() => query.GetUser(token);

            RequestException exception = Assert.Throws<RequestException>((Func<User_Login>) action);
            RequestException result = new(ResultCodes.RefreshTokenExpired);
            Assert.Equal(result.Code, exception.Code);
        }

        #endregion

        #region GetAuthenticationresult
        [Fact]
        public void Login_GetAuthenticationResult()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);
            Helpers.SetUserTokensNull(DefaultUserGuid, DefaultEmail);

            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());
            Model_Login login = new(DefaultUser, DefaultPassword, true);
            User_Login user = query.GetUser(login);
            Model_Token token = query.GetAccessToken(user);
            Model_Token refreshToken = query.GetRefreshToken(user.ID, true);

            AuthenticationResult result = query.GetAuthenticationResult(user.ID, token, refreshToken, true);
            Assert.NotNull(result);
            Assert.NotNull(result.AccessToken);
            Assert.NotNull(result.RefreshToken);
            Assert.Null(result.StatusCode);
            Assert.NotNull(result.Username);
        }
        #endregion

        #region Logout

        [Fact]
        public void RemoveSession()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);

            ApplicationUser user = Helpers.DB.GetUser(DefaultUserGuid);

            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/139.0.0.0 Safari/537.36";
            AuthQueries query = new(Helpers.DB, user, userAgent, SecretData.GetData());

            query.RemoveSessions();
            Assert.True(true);
        }

        [Fact]
        public void Logout()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);

            AuthQueries query = Helpers.ReturnAuthQuery(DefaultUserGuid.ToString());
            query.Logout();
            Assert.True(true);
        }
        #endregion

        #region ChangeEmail
        [Fact]
        public void ChangeEmail()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);

            User_Login user = Helpers.DB.User_Login.First(u => u.ID == DefaultUserGuid);
            user.StatusCode = UserStatus.EmailChange;
            user.StatusToken = null;
            user.StatusTokenExpireTime = null;
            Helpers.DB.SaveChanges();

            AuthQueries query = Helpers.ReturnAuthQuery(DefaultUserGuid.ToString());
            string? statusToken = query.UpdateEmail("chloe.hauer@lbs4.salzburg.at");
            Assert.NotNull(statusToken);
        }

        [Fact]
        public void VerifyUpdatedEmail()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);

            User_Login user = Helpers.DB.User_Login.First(u => u.ID == DefaultUserGuid);
            user.StatusCode = UserStatus.EmailChange;
            user.StatusToken = null;
            user.StatusTokenExpireTime = null;
            Helpers.DB.SaveChanges();

            ApplicationUser applicationUser = Helpers.DB.GetUser(DefaultUserGuid);

            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.38";
            AuthQueries query = new(Helpers.DB, applicationUser, userAgent, SecretData.GetData());

            string? statusToken = query.UpdateEmail("juliane.krenek@cablelink.at");
            query.VerifyUpdatedEmail(statusToken);
            Assert.True(true);
        }

        [Fact]
        public void ChangeEmail_DataInvalid()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);

            AuthQueries query = Helpers.ReturnAuthQuery(DefaultUserGuid.ToString());
            try
            {
                query.UpdateEmail(" ");
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.DataIsInvalid);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void ChangeEmail_PasswordRequestSet()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);
            Helpers.SetUserTokensNull(DefaultUserGuid, DefaultEmail);

            AuthQueries query = Helpers.ReturnAuthQuery(DefaultUserGuid.ToString());
            Helpers.SetUserTokensNull(DefaultUserGuid, DefaultEmail);

            query.RequestPasswordReset(DefaultEmail);

            try
            {
                query.UpdateEmail("chloe.hauer@lbs4.salzburg.at");
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.PasswordResetWasRequested);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void VerifyUpdatedEmail_DataInvalid()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);
            Helpers.SetUserTokensNull(DefaultUserGuid, DefaultEmail);

            AuthQueries query = Helpers.ReturnAuthQuery(DefaultUserGuid.ToString());

            try
            {
                query.VerifyUpdatedEmail(" ");
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.DataIsInvalid);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void VerifyUpdatedEmail_EmailChngeCodeWrong()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);

            User_Login user = Helpers.DB.User_Login.First(u => u.ID == DefaultUserGuid);
            user.StatusToken = null;
            user.StatusCode = null;
            user.StatusTokenExpireTime = null;
            Helpers.DB.SaveChanges();

            try
            {

                AuthQueries query = Helpers.ReturnAuthQuery(DefaultUserGuid.ToString());
                query.VerifyUpdatedEmail(Guid.NewGuid().ToString());
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.EmailChangeCodeWrong);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void VerifyUpdatedEmail_WrongStatusCode()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);

            User_Login user = Helpers.DB.User_Login.First(u => u.ID == DefaultUserGuid);
            user.StatusToken = null;
            user.StatusCode = UserStatus.EmailChange;
            user.StatusTokenExpireTime = null;
            Helpers.DB.SaveChanges();

            AuthQueries updateEmail = Helpers.ReturnAuthQuery(DefaultUserGuid.ToString());
            string? token = updateEmail.UpdateEmail("chloe.hauer@lbs4.salzburg.at");

            User_Login user2 = Helpers.DB.User_Login.First(u => u.ID == DefaultUserGuid);
            user.StatusCode = null;
            Helpers.DB.SaveChanges();

            try
            {
                AuthQueries query = Helpers.ReturnAuthQuery(DefaultUserGuid.ToString());
                query.VerifyUpdatedEmail(token);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.WrongStatusCode);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void VerifyUpdatedEmail_StatusTokenExpired()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);

            User_Login user = Helpers.DB.User_Login.First(u => u.ID == DefaultUserGuid);
            user.StatusToken = null;
            user.StatusCode = UserStatus.EmailChange;
            user.StatusTokenExpireTime = null;
            Helpers.DB.SaveChanges();

            AuthQueries updateEmail = Helpers.ReturnAuthQuery(DefaultUserGuid.ToString());
            string? token = updateEmail.UpdateEmail("chloe.hauer@lbs4.salzburg.at");

            User_Login user2 = Helpers.DB.User_Login.First(u => u.ID == DefaultUserGuid);
            long todayPlusTwo = new DateTimeOffset(DateTime.UtcNow).AddDays(-2).ToUnixTimeMilliseconds();
            user.StatusTokenExpireTime = todayPlusTwo;
            Helpers.DB.SaveChanges();

            try
            {
                AuthQueries query = Helpers.ReturnAuthQuery(DefaultUserGuid.ToString());
                query.VerifyUpdatedEmail(token);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.StatusTokenExpired);
                Assert.Equal(result.Code, ex.Code);
            }
        }
        #endregion

        #region UserData Functions

        [Fact]
        public void GetUserData()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);

            AuthQueries query = Helpers.ReturnAuthQuery(DefaultUserGuid.ToString());
            Model_UserData userData = query.GetUserData();
            Assert.NotNull(userData.Username);
            Assert.NotNull(userData.Email);
            Assert.NotNull(userData.FirstName);
            Assert.NotNull(userData.LastName);
        }

        [Fact]
        public void UpdateUserData()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);

            Helpers.SetUserTokensNull(DefaultUserGuid, DefaultEmail);

            AuthQueries query = Helpers.ReturnAuthQuery(DefaultUserGuid.ToString());
            Model_UpdateUserData data = new(null, "Violet", "Sorrengail", null);
            query.UpdateUserData(data);
            Assert.True(true);
        }

        [Fact]
        public void UpdateUserData_DataInvalid()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);

            AuthQueries query = Helpers.ReturnAuthQuery(DefaultUserGuid.ToString());
            Model_UpdateUserData? data = null;
            try
            {
                query.UpdateUserData(data);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.DataIsInvalid);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void UpdateUserData_PendingChangeOpen()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);

            User_Login user = Helpers.DB.User_Login.First(u => u.ID == DefaultUserGuid);
            user.StatusCode = UserStatus.EmailChange;
            Helpers.DB.SaveChanges();

            AuthQueries query = Helpers.ReturnAuthQuery(DefaultUserGuid.ToString());
            Model_UpdateUserData data = new(null, "Violet", "Riorson", null);

            try
            {
                query.UpdateUserData(data);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.PendingChangeOpen);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        #endregion

        #region Validate Password

        [Fact]
        public void ValidatePassword_Short()
        {
            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());
            try
            {
                query.ValidatePassword("Test123@");
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.PwTooShort);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void ValidatePassword_ShortNull()
        {
            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());
            try
            {
                query.ValidatePassword(null);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.PwTooShort);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void ValidatePassword_LowerCase()
        {
            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());
            try
            {
                query.ValidatePassword(DefaultPassword);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.PwHasNoLowercaseLetter);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void ValidatePassword_UpperCase()
        {
            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());
            try
            {
                query.ValidatePassword(DefaultPassword);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.PwHasNoUppercaseLetter);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void ValidatePassword_Number()
        {
            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());
            try
            {
                query.ValidatePassword("TestTestTest@");
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.PwHasNoNumber);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void ValidatePassword_SpecialChars()
        {
            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());
            try
            {
                query.ValidatePassword("Testtest123");
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.PwHasNoSpecialChars);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void ValidatePassword_WhiteSpace()
        {
            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());
            try
            {
                query.ValidatePassword("Test test@123");
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.PwHasWhitespace);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void ValidatePassword_InvalidChars()
        {
            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());
            try
            {
                query.ValidatePassword("TestTest&123");
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.PwUsesInvalidChars);
                Assert.Equal(result.Code, ex.Code);
            }
        }
        #endregion

        #region Validate Username
        [Fact]
        public void ValidateUserName_Short()
        {
            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());

            string action() => query.ValidateUsername("Test");

            RequestException exception = Assert.Throws<RequestException>((Func<string>) action);
            RequestException result = new(ResultCodes.UnToShort);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void ValidateUserName_Short_Null()
        {
            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());

            string action() => query.ValidateUsername(null);

            RequestException exception = Assert.Throws<RequestException>((Func<string>) action);
            RequestException result = new(ResultCodes.UnToShort);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void ValidateUserName_InvalidChars()
        {
            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());

            string action() => query.ValidateUsername("Test#User");

            RequestException exception = Assert.Throws<RequestException>((Func<string>) action);
            RequestException result = new(ResultCodes.UnUsesInvalidChars);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void ValidateUserName_Unique()
        {
            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());

            string action() => query.ValidateUsername(DefaultUser);

            RequestException exception = Assert.Throws<RequestException>((Func<string>) action);
            RequestException result = new(ResultCodes.UsernameAlreadyExists);
            Assert.Equal(result.Code, exception.Code);
        }

        #endregion

        #region Validate Email
        [Fact]
        public void ValidateEmail_Invalid()
        {
            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());

            string action() => query.ValidateEmail(null);

            RequestException exception = Assert.Throws<RequestException>((Func<string>) action);
            RequestException result = new(ResultCodes.EmailInvalid);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void ValidateEmail_Invalid_WhiteSpace()
        {
            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());

            string action() => query.ValidateEmail(" ");

            RequestException exception = Assert.Throws<RequestException>((Func<string>) action);
            RequestException result = new(ResultCodes.EmailInvalid);
            Assert.Equal(result.Code, exception.Code);

        }

        [Fact]
        public void ValidateEmail_Invalid_NoAt()
        {
            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());

            string action() => query.ValidateEmail("schwammal55gmail.com");

            RequestException exception = Assert.Throws<RequestException>((Func<string>) action);
            RequestException result = new(ResultCodes.EmailInvalid);
            Assert.Equal(result.Code, exception.Code);

        }

        [Fact]
        public void ValidateEmail_Invalid_NoPoint()
        {
            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());

            string action() => query.ValidateEmail("schwammal55@gmailcom");

            RequestException exception = Assert.Throws<RequestException>((Func<string>) action);
            RequestException result = new(ResultCodes.EmailInvalid);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void ValidateEmail_AlreadyExists()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(DefaultUser, DefaultEmail, DefaultPassword);

            AuthQueries query = Helpers.ReturnAuthQuery(Guid.Empty.ToString());

            string action() => query.ValidateEmail(DefaultEmail);

            RequestException exception = Assert.Throws<RequestException>((Func<string>) action);
            RequestException result = new(ResultCodes.EmailAlreadyExists);
            Assert.Equal(result.Code, exception.Code);
        }
        #endregion
    }
}
