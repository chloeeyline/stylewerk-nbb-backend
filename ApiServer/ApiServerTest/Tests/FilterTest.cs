using StyleWerk.NBB.Database;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace ApiServerTest.Tests
{
    public class FilterTest
    {
        private Guid DefaultUserGuid = new("90865032-e4e8-4e2b-85e0-5db345f42a5b");
        private Guid OtherUserDefaultGuid = new("cd6c092d-0546-4f8b-b70c-352d2ca765a4");

        #region Helpers
        private static TemplateQueries ReturnQuery(string userGuid)
        {
            NbbContext DB = NbbContext.Create();
            ApplicationUser user = DB.GetUser(new Guid(userGuid));

            TemplateQueries query = new(DB, user);
            return query;
        }
        #endregion

        #region Filter ShareTypes

        [Fact]
        public void Filter_Owned()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, null, false, false, true, false);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_Public()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, null, true, false, false, false);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void Filter_Shared()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, null, false, true, false, false);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void Filter_Direct()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, null, false, false, false, true);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void Filter_ShareTypes()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, null, true, true, true, true);
            Assert.NotNull(templates);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_Owned_Public()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, null, true, false, true, false);
            Assert.True(templates.Items.Count > 0);
        }
        #endregion

        #region Filter TemplateName ShareTypes

        [Fact]
        public void Filter_Name_Owned()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, "Test", null, null, null, false, false, true, false);
            Assert.NotNull(templates);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_Name_Public()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, "Test", null, null, null, true, false, false, false);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void Filter_Name_Shared()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, "Test", null, null, null, false, true, false, false);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void Filter_Name_Direct()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, "Test", null, null, null, false, false, false, true);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void Filter_Name_ShareTypes()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, "Test", null, null, null, true, true, true, false);
            Assert.True(templates.Items.Count > 0);
        }
        #endregion

        #region Filter Username ShareTypes

        [Fact]
        public void Filter_UserName_Owned()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, "Test", null, null, false, false, true, false);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_UserName_Public()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, "Test", null, null, true, false, false, false);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void Filter_UserName_Shared()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, "Test", null, null, false, true, false, false);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void Filter_UserName_Direct()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, "Test", null, null, false, false, false, true);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void Filter_UserName_ShareTypes()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, "Test", null, null, true, true, true, false);
            Assert.True(templates.Items.Count > 0);
        }

        #endregion

        #region Filter Description ShareTypes
        [Fact]
        public void Filter_Description_Owned()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, "Test", null, false, false, true, false);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_Description_Public()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, "Test", null, true, false, false, false);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void Filter_Description_Shared()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, "Test", null, false, true, false, false);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void Filter_Description_Direct()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, "Test", null, false, false, false, true);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void Filter_Description_ShareTypes()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, "Test", null, true, true, true, true);
            Assert.True(templates.Items.Count > 0);
        }
        #endregion

        #region Filter Tags ShareTypes
        [Fact]
        public void Filter_Tags_Owned()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, "Test", false, false, true, false);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_Tags_Public()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, "Test", true, false, false, false);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void Filter_Tags_Shared()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, "Test", false, true, false, false);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void Filter_Tags_Direct()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, "Test", false, false, false, true);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void Filter_Tags_ShareTypes()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, "Test", true, true, true, true);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_Tags_Owned_Public()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, "Test", true, false, true, false);
            Assert.True(templates.Items.Count > 0);
        }
        #endregion

        [Fact]
        public void Filter_Name_Owned_Public()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, "Test", null, null, null, true, false, true, false);
            Assert.NotNull(templates);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_Name_UserName_Description_Tags_Owned_Public_Shared()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, "Test", "Test", "Test", "Test", true, true, true, false);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_Name_UserName_Description_Tags()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, "Test", "Test", "Test", "Test", false, false, false, false);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void ListPaging()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(-1, 2, "Test", "Test", "Test", "Test", false, false, false, false);
            Assert.NotNull(templates);
        }

        [Fact]
        public void ListPerPage()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, -2, "Test", "Test", "Test", "Test", false, false, true, false);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void ListWhiteSpace()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, " ", null, null, null, false, false, true, false);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void ListEmptyString()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, "", null, null, null, false, false, true, false);
            Assert.True(templates.Items.Count > 0);
        }
    }
}
