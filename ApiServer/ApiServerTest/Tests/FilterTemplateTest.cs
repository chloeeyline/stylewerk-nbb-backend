using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace ApiServerTest.Tests
{
    public class FilterTemplateTest
    {
        private Guid DefaultUserGuid { get; set; }
        private Guid OtherUserDefaultGuid { get; set; }

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
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, null, false, false, true, false);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_Public()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, null, true, false, false, false);
            Assert.Equal(templates.Items, []);
        }

        [Fact]
        public void Filter_Shared()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, null, false, true, false, false);
            Assert.Equal(templates.Items, []);
        }

        [Fact]
        public void Filter_Direct()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, null, false, false, false, true);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_ShareTypes()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, null, true, true, true, true);
            Assert.NotNull(templates);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_Owned_Public()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, null, true, false, true, false);
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
            Model_TemplatePaging templates = query.List(1, 2, "Test", null, null, null, false, false, true, false);
            Assert.NotNull(templates);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_Name_Public()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, "Test", null, null, null, true, false, false, false);
            Assert.Equal(templates.Items, []);
        }

        [Fact]
        public void Filter_Name_Shared()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, "Test", null, null, null, false, true, false, false);
            Assert.Equal(templates.Items, []);
        }

        [Fact]
        public void Filter_Name_Direct()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, "Test", null, null, null, false, false, false, true);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_Name_ShareTypes()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, "Test", null, null, null, true, true, true, false);
            Assert.True(templates.Items.Count > 0);
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
            Model_TemplatePaging templates = query.List(1, 2, null, "Test", null, null, false, false, true, false);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_UserName_Public()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, "Test", null, null, true, false, false, false);
            Assert.Equal(templates.Items, []);
        }

        [Fact]
        public void Filter_UserName_Shared()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, "Test", null, null, false, true, false, false);
            Assert.Equal(templates.Items, []);
        }

        [Fact]
        public void Filter_UserName_Direct()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, "Test", null, null, false, false, false, true);
            Assert.Equal(templates.Items, []);
        }

        [Fact]
        public void Filter_UserName_ShareTypes()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, "Test", null, null, true, true, true, false);
            Assert.True(templates.Items.Count > 0);
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
            Model_TemplatePaging templates = query.List(1, 2, null, null, "Test", null, false, false, true, false);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_Description_Public()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, "Test", null, true, false, false, false);
            Assert.Equal(templates.Items, []);
        }

        [Fact]
        public void Filter_Description_Shared()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, "Test", null, false, true, false, false);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void Filter_Description_Direct()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, "Test", null, false, false, false, true);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_Description_ShareTypes()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, "Test", null, true, true, true, true);
            Assert.True(templates.Items.Count > 0);
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
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, "Test", false, false, true, false);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_Tags_Public()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, "Test", true, false, false, false);
            Assert.Equal(templates.Items, []);
        }

        [Fact]
        public void Filter_Tags_Shared()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, "Test", false, true, false, false);
            Assert.Equal(templates.Items, []);
        }

        [Fact]
        public void Filter_Tags_Direct()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, "Test", false, false, false, true);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_Tags_ShareTypes()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, "Test", true, true, true, true);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_Tags_Owned_Public()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, "Test", true, false, true, false);
            Assert.True(templates.Items.Count > 0);
        }
        #endregion

        [Fact]
        public void Filter_Name_Owned_Public()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, "Test", null, null, null, true, false, true, false);
            Assert.NotNull(templates);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_Name_UserName_Description_Tags_Owned_Public_Shared()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, "Test", "Test", "Test", "Test", true, true, true, false);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_Name_UserName_Description_Tags()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, "Test", "Test", "Test", "Test", false, false, false, false);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void ListPaging()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(-1, 2, "Test", "Test", "Test", "Test", false, false, false, false);
            Assert.NotNull(templates);
        }

        [Fact]
        public void ListPerPage()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, -2, "Test", "Test", "Test", "Test", false, false, true, false);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void ListWhiteSpace()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, " ", null, null, null, false, false, true, false);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void ListEmptyString()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, "", null, null, null, false, false, true, false);
            Assert.True(templates.Items.Count > 0);
        }
    }
}
