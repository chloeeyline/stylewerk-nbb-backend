using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Admin;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace ApiServerTest.Tests
{
    public class ColorTest
    {
        private Guid DefaultUserGuid { get; set; }
        private Guid OtherUserDefaultGuid { get; set; }

        private readonly string Username = "TestUser";
        private readonly string Email = "chloe.hauer@lbs4.salzburg.at";
        private readonly string Password = "TestTest@123";

        private readonly string OtherUsername = "TestUser1";
        private readonly string OtherEmail = "florian.windisch@lbs4.salzburg.at";

        #region add
        [Fact]
        public void Add()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Helpers.SetUserAdmin(DefaultUserGuid);

            Admin_ColorTheme result = Helpers.CreateColorTheme(DefaultUserGuid.ToString(), "TestTheme", Guid.Empty);
            Assert.NotNull(result);
        }

        [Fact]
        public void Add_DataInvalid()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Helpers.SetUserAdmin(DefaultUserGuid);

            Model_ColorTheme color = new(Guid.NewGuid(), "TestTheme", "Test", null);
            ColorThemeQueries query = Helpers.ReturnColorQuery(DefaultUserGuid.ToString());
            try
            {
                query.Update(color);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.DataIsInvalid);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void Add_UserMustBeAdmin()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            try
            {
                Helpers.CreateColorTheme(DefaultUserGuid.ToString(), "TestTheme", Guid.Empty);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.UserMustBeAdmin);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void Add_NameUnique()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Helpers.SetUserAdmin(DefaultUserGuid);
            Admin_ColorTheme theme = Helpers.CreateColorTheme(DefaultUserGuid.ToString(), "TestTheme", Guid.Empty);

            try
            {
                Helpers.CreateColorTheme(DefaultUserGuid.ToString(), "TestTheme", Guid.Empty);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.NameMustBeUnique);
                Assert.Equal(result.Code, ex.Code);
            }
        }
        #endregion

        #region update

        [Fact]
        public void Update()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Helpers.SetUserAdmin(DefaultUserGuid);
            Admin_ColorTheme theme = Helpers.CreateColorTheme(DefaultUserGuid.ToString(), "TestTheme", Guid.Empty);
            Admin_ColorTheme result = Helpers.CreateColorTheme(DefaultUserGuid.ToString(), "TestColor", theme.ID);

            NbbContext context = NbbContext.Create();
            Admin_ColorTheme? dbtheme = context.Admin_ColorTheme.FirstOrDefault(c => c.ID == result.ID);
            Assert.NotEqual(theme.Name, dbtheme.Name);
        }

        [Fact]
        public void Update_NameUnique()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Helpers.SetUserAdmin(DefaultUserGuid);
            Admin_ColorTheme theme = Helpers.CreateColorTheme(DefaultUserGuid.ToString(), "TestTheme", Guid.Empty);
            _ = Helpers.CreateColorTheme(DefaultUserGuid.ToString(), "DefaultTheme", Guid.Empty);

            try
            {
                _ = Helpers.CreateColorTheme(DefaultUserGuid.ToString(), "DefaultTheme", theme.ID);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.NameMustBeUnique);
                Assert.Equal(result.Code, ex.Code);
            }
        }
        #endregion

        #region remove
        [Fact]
        public void Remove()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Helpers.SetUserAdmin(DefaultUserGuid);
            Admin_ColorTheme theme = Helpers.CreateColorTheme(DefaultUserGuid.ToString(), "TestTheme", Guid.Empty);

            ColorThemeQueries query = Helpers.ReturnColorQuery(DefaultUserGuid.ToString());
            query.Remove(theme.ID);

            NbbContext context = NbbContext.Create();
            Admin_ColorTheme? dbTheme = context.Admin_ColorTheme.FirstOrDefault(t => t.ID == theme.ID);
            Assert.Null(dbTheme);
        }

        [Fact]
        public void Remove_DataInvalid()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Helpers.SetUserAdmin(DefaultUserGuid);

            ColorThemeQueries query = Helpers.ReturnColorQuery(DefaultUserGuid.ToString());

            try
            {
                query.Remove(Guid.Empty);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.DataIsInvalid);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void Remove_NoDataFound()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Helpers.SetUserAdmin(DefaultUserGuid);

            ColorThemeQueries query = Helpers.ReturnColorQuery(DefaultUserGuid.ToString());

            try
            {
                query.Remove(Guid.NewGuid());
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.NoDataFound);
                Assert.Equal(result.Code, ex.Code);
            }
        }
        #endregion

        #region details

        [Fact]
        public void Details()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Helpers.SetUserAdmin(DefaultUserGuid);
            Admin_ColorTheme theme = Helpers.CreateColorTheme(DefaultUserGuid.ToString(), "TestTheme", Guid.Empty);

            ColorThemeQueries query = Helpers.ReturnColorQuery(DefaultUserGuid.ToString());
            Model_ColorTheme result = query.Details(theme.ID);

            Assert.NotNull(result);
        }

        [Fact]
        public void Details_DataInvalid()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Helpers.SetUserAdmin(DefaultUserGuid);

            ColorThemeQueries query = Helpers.ReturnColorQuery(DefaultUserGuid.ToString());

            Model_ColorTheme action() => query.Details(Guid.Empty);

            RequestException exception = Assert.Throws<RequestException>((Func<Model_ColorTheme>)action);
            RequestException result = new(ResultCodes.DataIsInvalid);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void Detais_NoDataFound()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Helpers.SetUserAdmin(DefaultUserGuid);

            ColorThemeQueries query = Helpers.ReturnColorQuery(DefaultUserGuid.ToString());

            Model_ColorTheme action() => query.Details(Guid.NewGuid());

            RequestException exception = Assert.Throws<RequestException>((Func<Model_ColorTheme>)action);
            RequestException result = new(ResultCodes.NoDataFound);
            Assert.Equal(result.Code, exception.Code);
        }
        #endregion

        #region list

        [Fact]
        public void List()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Helpers.SetUserAdmin(DefaultUserGuid);
            Admin_ColorTheme theme = Helpers.CreateColorTheme(DefaultUserGuid.ToString(), "TestTheme", Guid.Empty);

            ColorThemeQueries query = Helpers.ReturnColorQuery(DefaultUserGuid.ToString());
            List<Model_ColorTheme> result = query.List();

            Assert.NotEmpty(result);
        }
        #endregion

        #region get

        [Fact]
        public void Get()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Helpers.SetUserAdmin(DefaultUserGuid);
            Admin_ColorTheme theme = Helpers.CreateColorTheme(DefaultUserGuid.ToString(), "TestTheme", Guid.Empty);
            ColorThemeQueries query = Helpers.ReturnColorQuery(DefaultUserGuid.ToString());

            string result = query.Get(theme.ID);
            Assert.NotNull(result);
        }

        [Fact]
        public void Get_DataInvalid()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Helpers.SetUserAdmin(DefaultUserGuid);
            Admin_ColorTheme theme = Helpers.CreateColorTheme(DefaultUserGuid.ToString(), "TestTheme", Guid.Empty);
            ColorThemeQueries query = Helpers.ReturnColorQuery(DefaultUserGuid.ToString());

            string action() => query.Get(Guid.Empty);

            RequestException exception = Assert.Throws<RequestException>((Func<string>)action);
            RequestException result = new(ResultCodes.DataIsInvalid);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void Get_NoDataFound()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Helpers.SetUserAdmin(DefaultUserGuid);
            ColorThemeQueries query = Helpers.ReturnColorQuery(DefaultUserGuid.ToString());

            string action() => query.Get(Guid.NewGuid());

            RequestException exception = Assert.Throws<RequestException>((Func<string>)action);
            RequestException result = new(ResultCodes.NoDataFound);
            Assert.Equal(result.Code, exception.Code);
        }
        #endregion
    }
}
