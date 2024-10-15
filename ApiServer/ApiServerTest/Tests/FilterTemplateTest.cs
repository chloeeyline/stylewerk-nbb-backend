using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace ApiServerTest.Tests
{
    [Collection("Sequential")]
    public class FilterTemplateTest
    {
        private Guid DefaultUserGuid { get; set; }

        private const string Username = "TestUser";
        private const string Email = "chloe.hauer@lbs4.salzburg.at";
        private const string Password = "TestTest@123";

        #region Filter ShareTypes

        [Fact]
        public void Filter_Owned()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, null, false);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_Owned_Public()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, null, true);
            Assert.True(templates.Items.Count > 0);
        }
        #endregion

        #region Filter TemplateName ShareTypes

        [Fact]
        public void Filter_Name_Owned()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, "Test", null, null, null, false);
            Assert.NotNull(templates);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_Name_Owned_Public()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, "Test", null, null, null, true);
            Assert.NotEqual(templates.Items, []);
        }
        #endregion

        #region Filter Username ShareTypes

        [Fact]
        public void Filter_UserName_Owned()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, "Test", null, null, false);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_UserName_Owned_Public()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, "Test", null, null, true);
            Assert.NotEqual(templates.Items, []);
        }
        #endregion

        #region Filter Description ShareTypes
        [Fact]
        public void Filter_Description_Owned()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, "Test", null, false);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_Description_Owned_Public()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, "Test", null, true);
            Assert.NotEqual(templates.Items, []);
        }
        #endregion

        #region Filter Tags ShareTypes
        [Fact]
        public void Filter_Tags_Owned()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, "Test", false);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_Tags_Owned_Public()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, "Test", true);
            Assert.NotEqual(templates.Items, []);
        }

        #endregion


        [Fact]
        public void Filter_Name_UserName_Description_Tags_Owned_Public()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, "Test", "Test", "Test", "Test", true);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_Name_UserName_Description_Tags()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, "Test", "Test", "Test", "Test", false);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void ListPaging()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(-1, 2, "Test", "Test", "Test", "Test", false);
            Assert.NotNull(templates);
        }

        [Fact]
        public void ListPerPage()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, -2, "Test", "Test", "Test", "Test", false);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void ListWhiteSpace()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, " ", null, null, null, false);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void ListEmptyString()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, "", null, null, null, false);
            Assert.True(templates.Items.Count > 0);
        }
    }
}
