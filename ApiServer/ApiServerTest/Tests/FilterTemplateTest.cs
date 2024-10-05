using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace ApiServerTest.Tests
{
    public class FilterTemplateTest
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

        private void DeleteAll()
        {
            NbbContext context = NbbContext.Create();
            List<Structure_Template> templates = [.. context.Structure_Template];
            if (templates.Count > 0)
                context.RemoveRange(templates);
        }

        private static Model_Template CreateTemplate(string userGuid)
        {
            TemplateQueries query = ReturnQuery(userGuid);
            Guid rowId = Guid.NewGuid();
            List<Model_TemplateCell> cells =
            [
                new Model_TemplateCell(Guid.NewGuid(), 1, false, false, "Test", "Test"),
                new Model_TemplateCell(Guid.NewGuid(), 1, false, false, "Test1", "Test"),
                new Model_TemplateCell(Guid.NewGuid(), 1, false, false, "Test2", "Test")
            ];

            List<Model_TemplateRow> rows =
            [
                new Model_TemplateRow(rowId, true, true, false, cells)
            ];

            Model_Template template = new(null, "TestTemplate", "TestDescription", "Test", rows);
            Model_Template result = query.Update(template);

            return result;
        }
        #endregion

        #region Filter ShareTypes

        [Fact]
        public void Filter_Owned()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, null, false, false, true, false);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_Public()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, null, true, false, false, false);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void Filter_Shared()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, null, false, true, false, false);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void Filter_Direct()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, null, false, false, false, true);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void Filter_ShareTypes()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, null, true, true, true, true);
            Assert.NotNull(templates);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_Owned_Public()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, null, true, false, true, false);
            Assert.True(templates.Items.Count > 0);
        }
        #endregion

        #region Filter TemplateName ShareTypes

        [Fact]
        public void Filter_Name_Owned()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, "Test", null, null, null, false, false, true, false);
            Assert.NotNull(templates);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_Name_Public()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, "Test", null, null, null, true, false, false, false);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void Filter_Name_Shared()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, "Test", null, null, null, false, true, false, false);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void Filter_Name_Direct()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, "Test", null, null, null, false, false, false, true);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void Filter_Name_ShareTypes()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, "Test", null, null, null, true, true, true, false);
            Assert.True(templates.Items.Count > 0);
        }
        #endregion

        #region Filter Username ShareTypes

        [Fact]
        public void Filter_UserName_Owned()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, "Test", null, null, false, false, true, false);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_UserName_Public()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, "Test", null, null, true, false, false, false);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void Filter_UserName_Shared()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, "Test", null, null, false, true, false, false);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void Filter_UserName_Direct()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, "Test", null, null, false, false, false, true);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void Filter_UserName_ShareTypes()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, "Test", null, null, true, true, true, false);
            Assert.True(templates.Items.Count > 0);
        }

        #endregion

        #region Filter Description ShareTypes
        [Fact]
        public void Filter_Description_Owned()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, "Test", null, false, false, true, false);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_Description_Public()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, "Test", null, true, false, false, false);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void Filter_Description_Shared()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, "Test", null, false, true, false, false);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void Filter_Description_Direct()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, "Test", null, false, false, false, true);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void Filter_Description_ShareTypes()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, "Test", null, true, true, true, true);
            Assert.True(templates.Items.Count > 0);
        }
        #endregion

        #region Filter Tags ShareTypes
        [Fact]
        public void Filter_Tags_Owned()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, "Test", false, false, true, false);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_Tags_Public()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, "Test", true, false, false, false);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void Filter_Tags_Shared()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, "Test", false, true, false, false);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void Filter_Tags_Direct()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, "Test", false, false, false, true);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void Filter_Tags_ShareTypes()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, "Test", true, true, true, true);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_Tags_Owned_Public()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, null, null, null, "Test", true, false, true, false);
            Assert.True(templates.Items.Count > 0);
        }
        #endregion

        [Fact]
        public void Filter_Name_Owned_Public()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, "Test", null, null, null, true, false, true, false);
            Assert.NotNull(templates);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_Name_UserName_Description_Tags_Owned_Public_Shared()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, "Test", "Test", "Test", "Test", true, true, true, false);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void Filter_Name_UserName_Description_Tags()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, "Test", "Test", "Test", "Test", false, false, false, false);
            Assert.True(templates.Items.Count == 0);
        }

        [Fact]
        public void ListPaging()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(-1, 2, "Test", "Test", "Test", "Test", false, false, false, false);
            Assert.NotNull(templates);
        }

        [Fact]
        public void ListPerPage()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, -2, "Test", "Test", "Test", "Test", false, false, true, false);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void ListWhiteSpace()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, " ", null, null, null, false, false, true, false);
            Assert.True(templates.Items.Count > 0);
        }

        [Fact]
        public void ListEmptyString()
        {
            DeleteAll();
            CreateTemplate(DefaultUserGuid.ToString());
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_TemplatePaging templates = query.List(1, 2, "", null, null, null, false, false, true, false);
            Assert.True(templates.Items.Count > 0);
        }
    }
}
