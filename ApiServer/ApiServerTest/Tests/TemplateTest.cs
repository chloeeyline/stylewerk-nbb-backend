using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace ApiServerTest.Tests
{
    public class TemplateTest
    {
        private Guid DefaultUserGuid { get; set; }
        private Guid OtherUserDefaultGuid { get; set; }

        private readonly string Username = "TestUser";
        private readonly string Email = "chloe.hauer@lbs4.salzburg.at";
        private readonly string Password = "TestTest@123";

        private readonly string OtherUsername = "TestUser1";
        private readonly string OtherEmail = "florian.windisch@lbs4.salzburg.at";

        #region Update Function
        [Fact]
        public void Add()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            EditorQueries query = Helpers.ReturnEditorQuery(DefaultUserGuid.ToString());
            Model_Editor result = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);

            NbbContext context = NbbContext.Create();
            Structure_Template? dbTemplate = context.Structure_Template.FirstOrDefault(t => t.ID == result.TemplateID);
            Assert.NotNull(dbTemplate);
        }

        [Fact]
        public void Update()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor updateTemplate = Helpers.CreateTemplate("DefaultTemplate", DefaultUserGuid.ToString(), template.ID);

            NbbContext context = NbbContext.Create();
            Structure_Template dbTemplate = context.Structure_Template.First(t => t.ID == updateTemplate.TemplateID);
            Assert.NotEqual(template.Template?.Name, dbTemplate.Name);
        }

        [Fact]
        public void Add_DataInvalid()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Model_Editor action() => Helpers.CreateTemplate(string.Empty, DefaultUserGuid.ToString(), null);

            RequestException exception = Assert.Throws<RequestException>((Func<Model_Editor>) action);
            RequestException result = new(ResultCodes.DataIsInvalid);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void Add_NameUnique()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);

            Model_Editor action() => Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);

            RequestException exception = Assert.Throws<RequestException>((Func<Model_Editor>) action);
            RequestException result = new(ResultCodes.NameMustBeUnique);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void Update_DontOwnData()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);

            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor action() => Helpers.CreateTemplate("DefaultTemplate", OtherUserDefaultGuid.ToString(), template.TemplateID);

            RequestException exception = Assert.Throws<RequestException>((Func<Model_Editor>) action);
            RequestException result = new(ResultCodes.YouDontOwnTheData);
            Assert.Equal(result.Code, exception.Code);

        }

        [Fact]
        public void Update_NameUnique()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor template2 = Helpers.CreateTemplate("Test", DefaultUserGuid.ToString(), null);

            Model_Editor action() => Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), template2.TemplateID);

            RequestException exception = Assert.Throws<RequestException>((Func<Model_Editor>) action);
            RequestException result = new(ResultCodes.NameMustBeUnique);
            Assert.Equal(result.Code, exception.Code);
        }
        #endregion

        #region Remove Function
        [Fact]
        public void Remove()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);

            query.Remove(template.TemplateID);

            Assert.True(true);
        }

        [Fact]
        public void Remove_DataInvalid()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());

            try
            {
                query.Remove(null);
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

            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
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

        [Fact]
        public void Remove_DontOwnData()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);

            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_Editor templateOtherUser = Helpers.CreateTemplate("TestTemplate", OtherUserDefaultGuid.ToString(), null);

            try
            {
                query.Remove(templateOtherUser.TemplateID);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.YouDontOwnTheData);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        #endregion

        #region Copy Function
        [Fact]
        public void Copy()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);

            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_Editor templateOtherUser = Helpers.CreateTemplate("TestTemplate", OtherUserDefaultGuid.ToString(), null);

            query.Copy(templateOtherUser.TemplateID);

            NbbContext context = NbbContext.Create();
            Structure_Template? template = context.Structure_Template.FirstOrDefault(t => t.UserID == DefaultUserGuid && t.Name == "TestTemplate (Kopie)");
            Assert.NotNull(template);
        }

        [Fact]
        public void Copy_DataInvalid()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            try
            {
                query.Copy(null);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.DataIsInvalid);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void Copy_NoDataFound()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            try
            {
                query.Copy(Guid.NewGuid());
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
