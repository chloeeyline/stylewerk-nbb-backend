using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace ApiServerTest.Tests
{
    [Collection("Sequential")]
    public class EditorTemplateTest
    {
        private Guid DefaultUserGuid { get; set; }
        private Guid OtherUserDefaultGuid { get; set; }

        private readonly string Username = "TestUser";
        private readonly string Email = "chloe.hauer@lbs4.salzburg.at";
        private readonly string Password = "TestTest@123";

        private readonly string OtherUsername = "TestUser1";
        private readonly string OtherEmail = "florian.windisch@lbs4.salzburg.at";

        #region Create
        [Fact]
        public void Add()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            EditorQueries query = Helpers.ReturnEditorQuery(DefaultUserGuid.ToString());
            Model_Editor result = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);

            Structure_Template? dbTemplate = Helpers.DB.Structure_Template.FirstOrDefault(t => t.ID == result.TemplateID);
            Assert.NotNull(dbTemplate);
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
        #endregion

        #region Update
        [Fact]
        public void Update()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor updateTemplate = Helpers.CreateTemplate("DefaultTemplate", DefaultUserGuid.ToString(), template.ID);

            Structure_Template dbTemplate = Helpers.DB.Structure_Template.First(t => t.ID == updateTemplate.TemplateID);
            Assert.NotEqual(template.Template?.Name, dbTemplate.Name);
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

        #region GetTemplate
        [Fact]
        public void GetTemplate()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            EditorQueries query = Helpers.ReturnEditorQuery(DefaultUserGuid.ToString());
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);

            Model_Editor result = query.GetTemplate(template.TemplateID);
            Assert.NotNull(result);
        }

        [Fact]
        public void GetTemplate_DataInvalid()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            EditorQueries query = Helpers.ReturnEditorQuery(DefaultUserGuid.ToString());

            Model_Editor action() => query.GetTemplate(null);

            RequestException exception = Assert.Throws<RequestException>((Func<Model_Editor>) action);
            RequestException result = new(ResultCodes.DataIsInvalid);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void GetTemplate_NoDataFound()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            EditorQueries query = Helpers.ReturnEditorQuery(DefaultUserGuid.ToString());

            Model_Editor action() => query.GetTemplate(Guid.NewGuid());

            RequestException exception = Assert.Throws<RequestException>((Func<Model_Editor>) action);
            RequestException result = new(ResultCodes.NoDataFound);
            Assert.Equal(result.Code, exception.Code);
        }

        #endregion
    }
}
