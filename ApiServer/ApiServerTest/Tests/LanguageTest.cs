using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Admin;
using StyleWerk.NBB.Database.User;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace ApiServerTest.Tests
{
    public class LanguageTest
    {
        private Guid DefaultUserGuid { get; set; }
        private Guid OtherUserDefaultGuid { get; set; }

        private readonly string Username = "TestUser";
        private readonly string Email = "chloe.hauer@lbs4.salzburg.at";
        private readonly string Password = "TestTest@123";

        private readonly string OtherUsername = "TestUser1";
        private readonly string OtherEmail = "florian.windisch@lbs4.salzburg.at";

        #region Add

        [Fact]
        public void Add()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Helpers.SetUserAdmin(DefaultUserGuid);

            Admin_Language result = Helpers.CreateLanguage(DefaultUserGuid.ToString(), "en", "English");
            Assert.NotNull(result);
        }

        [Fact]
        public void Add_DataInvalid()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Helpers.SetUserAdmin(DefaultUserGuid);

            try
            {
                LanguageQueries query = Helpers.ReturnLanguageQuery(DefaultUserGuid.ToString());

                Model_Language lang = new("En", "English", string.Empty);
                query.Update(lang);
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
                _ = Helpers.CreateLanguage(DefaultUserGuid.ToString(), "en", "English");
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
            Helpers.CreateLanguage(DefaultUserGuid.ToString(), "en", "English");

            try
            {
                _ = Helpers.CreateLanguage(DefaultUserGuid.ToString(), "gb", "English");
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.NameMustBeUnique);
                Assert.Equal(result.Code, ex.Code);
            }
        }
        #endregion

        #region Update

        [Fact]
        public void Update()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Helpers.SetUserAdmin(DefaultUserGuid);
            Admin_Language language = Helpers.CreateLanguage(DefaultUserGuid.ToString(), "en", "English");
            Helpers.CreateLanguage(DefaultUserGuid.ToString(), "en", "Australian");

            NbbContext context = NbbContext.Create();
            Admin_Language? lang = context.Admin_Language.FirstOrDefault(l => l.Code == language.Code);
            Assert.NotEqual(lang.Name, language.Name);
        }

        [Fact]
        public void Update_NameUnique()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Helpers.SetUserAdmin(DefaultUserGuid);
            Admin_Language language = Helpers.CreateLanguage(DefaultUserGuid.ToString(), "en", "English");
            Helpers.CreateLanguage(DefaultUserGuid.ToString(), "de", "Australian");

            try
            {
                Helpers.CreateLanguage(DefaultUserGuid.ToString(), "de", "English");
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.NameMustBeUnique);
                Assert.Equal(result.Code, ex.Code);
            }
        }
        #endregion

        #region Remove
        [Fact]
        public void Remove()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Helpers.SetUserAdmin(DefaultUserGuid);
            Admin_Language language = Helpers.CreateLanguage(DefaultUserGuid.ToString(), "en", "English");

            LanguageQueries query = Helpers.ReturnLanguageQuery(DefaultUserGuid.ToString());
            query.Remove(language.Code);

            NbbContext context = NbbContext.Create();
            Admin_Language? lang = context.Admin_Language.FirstOrDefault(l => l.Code == language.Code);
            Assert.Null(lang);
        }

        [Fact]
        public void Remove_DataInvalid()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Helpers.SetUserAdmin(DefaultUserGuid);

            LanguageQueries query = Helpers.ReturnLanguageQuery(DefaultUserGuid.ToString());
            try
            {
                query.Remove(string.Empty);
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

            LanguageQueries query = Helpers.ReturnLanguageQuery(DefaultUserGuid.ToString());
            try
            {
                query.Remove("xl");
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.NoDataFound);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void Remove_Admin()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Helpers.SetUserAdmin(DefaultUserGuid);
            Admin_Language language = Helpers.CreateLanguage(DefaultUserGuid.ToString(), "en", "English");

            NbbContext context = NbbContext.Create();
            User_Login user = context.User_Login.First(u => u.ID == DefaultUserGuid);
            user.Admin = false;
            context.SaveChanges();

            LanguageQueries query = Helpers.ReturnLanguageQuery(DefaultUserGuid.ToString());
            try
            {
                query.Remove(language.Code);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.UserMustBeAdmin);
                Assert.Equal(result.Code, ex.Code);
            }
        }
        #endregion

        #region Details
        [Fact]
        public void Details()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Helpers.SetUserAdmin(DefaultUserGuid);
            Admin_Language language = Helpers.CreateLanguage(DefaultUserGuid.ToString(), "en", "English");

            LanguageQueries query = Helpers.ReturnLanguageQuery(DefaultUserGuid.ToString());
            Model_Language details = query.Details(language.Code);

            Assert.NotNull(details);
            Assert.Equal(details.Name, language.Name);
        }

        [Fact]
        public void Details_DataInvalid()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Helpers.SetUserAdmin(DefaultUserGuid);

            LanguageQueries query = Helpers.ReturnLanguageQuery(DefaultUserGuid.ToString());
            try
            {
                Model_Language details = query.Details(string.Empty);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.DataIsInvalid);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void Details_NoDataFound()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Helpers.SetUserAdmin(DefaultUserGuid);

            LanguageQueries query = Helpers.ReturnLanguageQuery(DefaultUserGuid.ToString());
            try
            {
                Model_Language details = query.Details("xl");
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.NoDataFound);
                Assert.Equal(result.Code, ex.Code);
            }
        }
        #endregion

        #region List
        [Fact]
        public void List()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Helpers.SetUserAdmin(DefaultUserGuid);
            Admin_Language language = Helpers.CreateLanguage(DefaultUserGuid.ToString(), "en", "English");

            LanguageQueries query = Helpers.ReturnLanguageQuery(DefaultUserGuid.ToString());
            List<Model_Language> details = query.List();

            Assert.NotEmpty(details);
        }

        [Fact]
        public void List_Empty()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Helpers.SetUserAdmin(DefaultUserGuid);

            LanguageQueries query = Helpers.ReturnLanguageQuery(DefaultUserGuid.ToString());
            List<Model_Language> details = query.List();

            Assert.Empty(details);
        }
        #endregion

        #region Get
        [Fact]
        public void Get()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Helpers.SetUserAdmin(DefaultUserGuid);
            Admin_Language language = Helpers.CreateLanguage(DefaultUserGuid.ToString(), "en", "English");

            LanguageQueries query = Helpers.ReturnLanguageQuery(DefaultUserGuid.ToString());
            string details = query.Get(language.Code);

            Assert.NotNull(details);
        }

        [Fact]
        public void Get_DataInvalid()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Helpers.SetUserAdmin(DefaultUserGuid);

            LanguageQueries query = Helpers.ReturnLanguageQuery(DefaultUserGuid.ToString());
            try
            {
                string details = query.Get(string.Empty);

            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.DataIsInvalid);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void Get_NoDataFound()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Helpers.SetUserAdmin(DefaultUserGuid);

            LanguageQueries query = Helpers.ReturnLanguageQuery(DefaultUserGuid.ToString());
            try
            {
                string details = query.Get("xl");

            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.NoDataFound);
                Assert.Equal(result.Code, ex.Code);
            }
        }
        #endregion
    }
}
