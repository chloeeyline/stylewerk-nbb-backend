using StyleWerk.NBB.Authentication;
using StyleWerk.NBB.AWS;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.User;
using StyleWerk.NBB.Models;

namespace ApiServerTest.Tests
{
    public class UserTest
    {
        private readonly Guid UserGuid = new("90865032-e4e8-4e2b-85e0-5db345f42a5b");
        private static AuthQueries ReturnQuery(string userGuid)
        {
            NbbContext DB = NbbContext.Create();
            ApplicationUser user = DB.GetUser(new Guid(userGuid));

            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36";
            AuthQueries query = new(DB, user, userAgent, SecretData.GetData());
            return query;
        }

        [Fact]
        public void Register()
        {
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
            Model_Registration register = new("Test123Juliane", "juliane.krenek@lbs4.salzburg.at", "TestTest123@", "Juliane", "Krenek", UserGender.Female, 0);
            query.Registration(register);
            Assert.True(true);
        }

        [Fact]
        public void VerifyEmail()
        {
            NbbContext context = new();
            User_Login? user = context.User_Login.FirstOrDefault(u => u.ID == new Guid("90865032-e4e8-4e2b-85e0-5db345f42a5b"));
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
            query.VerifyEmail(user?.StatusToken);
            Assert.True(true);
        }

        [Fact]
        public void RequestPasswordReset()
        {
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
            query.RequestPasswordReset("schwammal55@gmail.com");
            Assert.True(true);
        }

        [Fact]
        public void ResetPassword()
        {
            AuthQueries query = ReturnQuery(UserGuid.ToString());
            string? token = query.RequestPasswordReset("schwammal55@gmail.com");
            Model_ResetPassword password = new(token, "TestTest@123");
            query.ResetPassword(password);
            Assert.True(true);
        }

        [Fact]
        public void Login_GetUser_UserLogin()
        {
            string passwordChange = "TestTest@123";
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
            string? token = query.RequestPasswordReset("schwammal55@gmail.com");
            Model_ResetPassword password = new(token, passwordChange);
            query.ResetPassword(password);

            Model_Login login = new("TestUser", passwordChange, true);
            User_Login user = query.GetUser(login);

            Assert.NotNull(user);
            Assert.IsType<User_Login>(user);
        }

        [Fact]
        public void Login_GetAccessToken_UserLogin()
        {
            string passwordChange = "TestTest@123";
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
            NbbContext context = NbbContext.Create();
            User_Login? dbUser = context.User_Login.FirstOrDefault(u => u.ID == UserGuid);
            dbUser.StatusCode = UserStatus.EmailChange;
            context.SaveChanges();

            string? statusToken = query.RequestPasswordReset("schwammal55@gmail.com");
            Model_ResetPassword password = new(statusToken, passwordChange);
            query.ResetPassword(password);

            Model_Login login = new("TestUser", passwordChange, true);
            User_Login user = query.GetUser(login);
            Model_Token token = query.GetAccessToken(user);

            Assert.NotNull(token);
            Assert.IsType<Model_Token>(token);
        }

        [Fact]
        public void Login_GetRefreshToken_UserLogin()
        {
            string passwordChange = "TestTest@123";
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
            NbbContext context = NbbContext.Create();
            User_Login? dbUser = context.User_Login.FirstOrDefault(u => u.ID == UserGuid);
            dbUser.StatusCode = UserStatus.EmailChange;
            context.SaveChanges();

            string? statusToken = query.RequestPasswordReset("schwammal55@gmail.com");
            Model_ResetPassword password = new(statusToken, passwordChange);
            query.ResetPassword(password);

            Model_Login login = new("TestUser", passwordChange, true);
            User_Login user = query.GetUser(login);
            Model_Token token = query.GetAccessToken(user);
            Model_Token refreshToken = query.GetRefreshToken(user.ID, true);

            Assert.NotNull(refreshToken);
            Assert.IsType<Model_Token>(refreshToken);
        }

        [Fact]
        public void Login_GetAuthenticationResult()
        {
            string passwordChange = "TestTest@123";
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
            NbbContext context = NbbContext.Create();
            User_Login? dbUser = context.User_Login.FirstOrDefault(u => u.ID == UserGuid);
            dbUser.StatusCode = UserStatus.EmailChange;
            context.SaveChanges();

            string? statusToken = query.RequestPasswordReset("schwammal55@gmail.com");
            Model_ResetPassword password = new(statusToken, passwordChange);
            query.ResetPassword(password);

            Model_Login login = new("TestUser", passwordChange, true);
            User_Login user = query.GetUser(login);
            Model_Token token = query.GetAccessToken(user);
            Model_Token refreshToken = query.GetRefreshToken(user.ID, true);

            AuthenticationResult result = query.GetAuthenticationResult(user.ID, token, refreshToken, true);
            Assert.NotNull(result);
            Assert.NotNull(result.AccessToken);
            Assert.NotNull(result.RefreshToken);
            Assert.NotNull(result.StatusCode);
            Assert.NotNull(result.Username);
        }

        [Fact]
        public void RefreshToken_GetUser()
        {
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
            //Refreshtoken zuerst generieren müssen
            string passwordChange = "TestTest@123";
            NbbContext context = NbbContext.Create();
            User_Login? dbUser = context.User_Login.FirstOrDefault(u => u.ID == UserGuid);
            dbUser.StatusCode = UserStatus.EmailChange;
            context.SaveChanges();

            string? statusToken = query.RequestPasswordReset("schwammal55@gmail.com");
            Model_ResetPassword password = new(statusToken, passwordChange);
            query.ResetPassword(password);

            Model_Login login = new("TestUser", passwordChange, true);

            AuthenticationResult authResult = query.Login(login);
            //Get User
            Model_RefreshToken refreshToken = new(authResult.RefreshToken.ToString(), true);
            User_Login user = query.GetUser(refreshToken);

            Assert.NotNull(user);
            Assert.NotNull(user.Username);
        }

        #region Register Exceptions

        [Fact]
        public void RegisterDataInvalid()
        {
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
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

        #region VerifyEmail Exceptions

        [Fact]
        public void VerifyEmailDataInvalid()
        {
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
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
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
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
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
            NbbContext context = new();
            User_Login? user = context.User_Login.FirstOrDefault(u => u.ID == UserGuid);
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
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
            NbbContext context = new();
            User_Login? user = context.User_Login.FirstOrDefault(u => u.ID == UserGuid);
            long todayPlusTwo = new DateTimeOffset(DateTime.UtcNow).AddDays(2).ToUnixTimeMilliseconds();
            user.StatusTokenExpireTime = todayPlusTwo;
            context.SaveChanges();

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

        #region Login Exceptions

        [Fact]
        public void Login_GetUser_DataInvalid_Null()
        {
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
            Model_Login login = new(null, null, true);
            User_Login action() => query.GetUser(login);

            RequestException exception = Assert.Throws<RequestException>((Func<User_Login>) action);
            RequestException result = new(ResultCodes.DataIsInvalid);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void Login_GetUser_DataInvalid_WhiteSpace()
        {
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
            Model_Login login = new(" ", " ", true);
            User_Login action() => query.GetUser(login);

            RequestException exception = Assert.Throws<RequestException>((Func<User_Login>) action);
            RequestException result = new(ResultCodes.DataIsInvalid);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void Login_GetUser_NotFound_Username()
        {
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
            Model_Login login = new("Test", "TestTest@123", true);
            User_Login action() => query.GetUser(login);

            RequestException exception = Assert.Throws<RequestException>((Func<User_Login>) action);
            RequestException result = new(ResultCodes.NoUserFound);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void Login_GetUser_NotFound_Password()
        {
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
            Model_Login login = new("TestUser", "TestAdmin@123", true);
            User_Login action() => query.GetUser(login);

            RequestException exception = Assert.Throws<RequestException>((Func<User_Login>) action);
            RequestException result = new(ResultCodes.NoUserFound);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void Login_GetAccessToken_EmailNotVerified()
        {
            string passwordChange = "TestTest@123";
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
            NbbContext context = NbbContext.Create();
            User_Login? dbUser = context.User_Login.FirstOrDefault(u => u.ID == UserGuid);
            dbUser.StatusCode = UserStatus.EmailVerification;
            context.SaveChanges();

            string? token = query.RequestPasswordReset("schwammal55@gmail.com");
            Model_ResetPassword password = new(token, passwordChange);
            query.ResetPassword(password);

            Model_Login login = new("TestUser", passwordChange, true);
            User_Login user = query.GetUser(login);

            Model_Token action() => query.GetAccessToken(user);

            RequestException exception = Assert.Throws<RequestException>((Func<Model_Token>) action);
            RequestException result = new(ResultCodes.EmailIsNotVerified);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void Login_GetAccessToken_PasswordReset()
        {
            string passwordChange = "TestTest@123";
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
            NbbContext context = NbbContext.Create();
            User_Login? dbUser = context.User_Login.FirstOrDefault(u => u.ID == UserGuid);
            dbUser.StatusCode = UserStatus.PasswordReset;
            context.SaveChanges();

            string? token = query.RequestPasswordReset("schwammal55@gmail.com");
            Model_ResetPassword password = new(token, passwordChange);
            query.ResetPassword(password);

            Model_Login login = new("TestUser", passwordChange, true);
            User_Login user = query.GetUser(login);

            Model_Token action() => query.GetAccessToken(user);

            RequestException exception = Assert.Throws<RequestException>((Func<Model_Token>) action);
            RequestException result = new(ResultCodes.PasswordResetWasRequested);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void Login_GetAuthenticationResult_NoUserFound()
        {
            string passwordChange = "TestTest@123";
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
            NbbContext context = NbbContext.Create();
            User_Login? dbUser = context.User_Login.FirstOrDefault(u => u.ID == UserGuid);
            dbUser.StatusCode = UserStatus.EmailChange;
            context.SaveChanges();

            string? statusToken = query.RequestPasswordReset("schwammal55@gmail.com");
            Model_ResetPassword password = new(statusToken, passwordChange);
            query.ResetPassword(password);

            Model_Login login = new("TestUser", passwordChange, true);
            User_Login user = query.GetUser(login);
            Model_Token token = query.GetAccessToken(user);
            Model_Token refreshToken = query.GetRefreshToken(Guid.NewGuid(), true);

            Model_Token action() => query.GetRefreshToken(Guid.NewGuid(), true);

            RequestException exception = Assert.Throws<RequestException>((Func<Model_Token>) action);
            RequestException result = new(ResultCodes.NoUserFound);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void RefreshToken_Getuser_DataInvalid()
        {
            string passwordChange = "TestTest@123";
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
            NbbContext context = NbbContext.Create();
            User_Login? dbUser = context.User_Login.FirstOrDefault(u => u.ID == UserGuid);
            dbUser.StatusCode = UserStatus.EmailChange;
            context.SaveChanges();

            string? statusToken = query.RequestPasswordReset("schwammal55@gmail.com");
            Model_ResetPassword password = new(statusToken, passwordChange);
            query.ResetPassword(password);

            Model_RefreshToken token = new(" ", true);
            User_Login action() => query.GetUser(token);

            RequestException exception = Assert.Throws<RequestException>((Func<User_Login>) action);
            RequestException result = new(ResultCodes.NoUserFound);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void RefreshToken_Getuser_RefreshTokenNotFound()
        {
            string passwordChange = "TestTest@123";
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
            NbbContext context = NbbContext.Create();
            User_Login? dbUser = context.User_Login.FirstOrDefault(u => u.ID == UserGuid);
            dbUser.StatusCode = UserStatus.EmailChange;
            context.SaveChanges();

            string? statusToken = query.RequestPasswordReset("schwammal55@gmail.com");
            Model_ResetPassword password = new(statusToken, passwordChange);
            query.ResetPassword(password);

            Model_RefreshToken token = new("abcdefg", true);
            User_Login action() => query.GetUser(token);

            RequestException exception = Assert.Throws<RequestException>((Func<User_Login>) action);
            RequestException result = new(ResultCodes.RefreshTokenNotFound);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void RefreshToken_Getuser_RefreshTokenExpired()
        {
            string passwordChange = "TestTest@123";
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
            NbbContext context = NbbContext.Create();
            User_Login? dbUser = context.User_Login.FirstOrDefault(u => u.ID == UserGuid);
            long todayPlusTwo = new DateTimeOffset(DateTime.UtcNow).AddDays(2).ToUnixTimeMilliseconds();
            dbUser.StatusTokenExpireTime = todayPlusTwo;
            context.SaveChanges();

            string? statusToken = query.RequestPasswordReset("schwammal55@gmail.com");
            Model_ResetPassword password = new(statusToken, passwordChange);
            query.ResetPassword(password);

            Model_RefreshToken token = new(query.GetRefreshToken(UserGuid, true).Token, true);
            User_Login action() => query.GetUser(token);

            RequestException exception = Assert.Throws<RequestException>((Func<User_Login>) action);
            RequestException result = new(ResultCodes.RefreshTokenExpired);
            Assert.Equal(result.Code, exception.Code);
        }



        #endregion

        #region Validate Password

        [Fact]
        public void ValidatePassword_Short()
        {
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
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
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
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
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
            try
            {
                query.ValidatePassword("TESTTEST@123");
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
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
            try
            {
                query.ValidatePassword("testtest@123");
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
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
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
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
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
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
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
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
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

        #region RequestPasswordReset Exceptions

        [Fact]
        public void RequestPasswordReset_NoUserFound()
        {
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());

            try
            {
                query.RequestPasswordReset("schwammal55@gmail.com");
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
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
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
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
            NbbContext context = new();
            User_Login? user = context.User_Login.FirstOrDefault(u => u.ID == UserGuid);
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

        #endregion

        #region ResetPassword Exceptions
        [Fact]
        public void ResetPassword_DataInvalid()
        {
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
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
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
            Model_ResetPassword password = new("cd6c092d-0546-4f8b-b70c-352d2ca765a4", "TestTest@123");
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
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());
            string? token = query.RequestPasswordReset("schwammal55@gmail.com");
            Model_ResetPassword password = new(token, "TestTest@123");
            NbbContext context = NbbContext.Create();
            User_Login? user = context.User_Login.FirstOrDefault(u => u.ID == UserGuid);
            user.StatusCode = UserStatus.EmailVerification;
            context.SaveChanges();

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
            AuthQueries query = ReturnQuery(Guid.NewGuid().ToString());
            NbbContext context = NbbContext.Create();

            User_Login? user = context.User_Login.FirstOrDefault(u => u.ID == UserGuid);
            long todayPlusTwo = new DateTimeOffset(DateTime.UtcNow).AddDays(2).ToUnixTimeMilliseconds();
            user.StatusTokenExpireTime = todayPlusTwo;
            user.StatusCode = UserStatus.PasswordReset;
            context.SaveChanges();

            string? token = query.RequestPasswordReset("schwammal55@gmail.com");
            Model_ResetPassword password = new(token, "TestTest@123");

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

        #region Validate Username
        [Fact]
        public void ValidateUserName_Short()
        {
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());

            string action() => query.ValidateUsername("Test");

            RequestException exception = Assert.Throws<RequestException>((Func<string>) action);
            RequestException result = new(ResultCodes.UnToShort);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void ValidateUserName_Short_Null()
        {
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());

            string action() => query.ValidateUsername(null);

            RequestException exception = Assert.Throws<RequestException>((Func<string>) action);
            RequestException result = new(ResultCodes.UnToShort);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void ValidateUserName_InvalidChars()
        {
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());

            string action() => query.ValidateUsername("Test&User");

            RequestException exception = Assert.Throws<RequestException>((Func<string>) action);
            RequestException result = new(ResultCodes.UnUsesInvalidChars);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void ValidateUserName_Unique()
        {
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());

            string action() => query.ValidateUsername("TestUser");

            RequestException exception = Assert.Throws<RequestException>((Func<string>) action);
            RequestException result = new(ResultCodes.UsernameAlreadyExists);
            Assert.Equal(result.Code, exception.Code);
        }

        #endregion

        #region Validate Email
        [Fact]
        public void ValidateEmail_Invalid()
        {
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());

            string action() => query.ValidateEmail(null);

            RequestException exception = Assert.Throws<RequestException>((Func<string>) action);
            RequestException result = new(ResultCodes.EmailInvalid);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void ValidateEmail_Invalid_WhiteSpace()
        {
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());

            string action() => query.ValidateEmail(" ");

            RequestException exception = Assert.Throws<RequestException>((Func<string>) action);
            RequestException result = new(ResultCodes.EmailInvalid);
            Assert.Equal(result.Code, exception.Code);

        }

        [Fact]
        public void ValidateEmail_Invalid_NoAt()
        {
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());

            string action() => query.ValidateEmail("schwammal55gmail.com");

            RequestException exception = Assert.Throws<RequestException>((Func<string>) action);
            RequestException result = new(ResultCodes.EmailInvalid);
            Assert.Equal(result.Code, exception.Code);

        }

        [Fact]
        public void ValidateEmail_Invalid_NoPoint()
        {
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());

            string action() => query.ValidateEmail("schwammal55@gmailcom");

            RequestException exception = Assert.Throws<RequestException>((Func<string>) action);
            RequestException result = new(ResultCodes.EmailInvalid);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void ValidateEmail_AlreadyExists()
        {
            AuthQueries query = ReturnQuery(Guid.Empty.ToString());

            string action() => query.ValidateEmail("schwammal55@gmail.com");

            RequestException exception = Assert.Throws<RequestException>((Func<string>) action);
            RequestException result = new(ResultCodes.EmailAlreadyExists);
            Assert.Equal(result.Code, exception.Code);
        }
        #endregion

        #region SendMail Password
        [Fact]
        public void SendMail_PasswordReset()
        {

        }
        #endregion



    }
}
