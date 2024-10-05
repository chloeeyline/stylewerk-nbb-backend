using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace ApiServerTest.Tests
{
    public class FilterEntryTest
    {
        private Guid DefaultUserGuid = new("90865032-e4e8-4e2b-85e0-5db345f42a5b");

        #region Helpers

        private static EntryQueries ReturnQueryEntry(string userGuid)
        {
            NbbContext DB = NbbContext.Create();
            ApplicationUser user = DB.GetUser(new Guid(userGuid));
            EntryQueries query = new(DB, user);
            return query;
        }

        private static TemplateQueries ReturnQueryTemplate(string userGuid)
        {
            NbbContext DB = NbbContext.Create();
            ApplicationUser user = DB.GetUser(new Guid(userGuid));
            TemplateQueries query = new(DB, user);
            return query;
        }

        private static EntryFolderQueries ReturnQueryFolder(string userGuid)
        {
            NbbContext DB = NbbContext.Create();
            ApplicationUser user = DB.GetUser(new Guid(userGuid));
            EntryFolderQueries query = new(DB, user);
            return query;
        }

        private static Model_Template CreateTemplate(string templateName, string userGuid)
        {
            TemplateQueries query = ReturnQueryTemplate(userGuid);
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

            Model_Template template = new(null, templateName, "TestDescription", "Test", rows);
            Model_Template result = query.Update(template);

            return result;
        }

        private static Model_Entry CreateEntry(string user, string title, string? tags, Guid? folderId, Model_Template template)
        {
            EntryQueries query = ReturnQueryEntry(user);
            Guid rowId = Guid.NewGuid();

            List<Model_EntryCell> cells =
            [
                new Model_EntryCell(Guid.NewGuid(), (Guid)template.ID, null, "Fourth Wing"),
                new Model_EntryCell(Guid.NewGuid(), (Guid)template.ID, null, "Iron Flame"),
                new Model_EntryCell(Guid.NewGuid(), (Guid)template.ID, null, "Onyx Storm")
            ];

            List<Model_EntryRow> rows =
            [
                new Model_EntryRow(rowId, (Guid)template.ID, 1, null, cells)
            ];

            Model_Entry entry = new(Guid.NewGuid(), folderId, (Guid)template.ID, title, tags, false, rows);
            Model_Entry result = query.Update(entry);

            return result;
        }

        private static void DeleteAll()
        {
            NbbContext context = NbbContext.Create();
            List<Structure_Template> templates = [.. context.Structure_Template];
            List<Structure_Entry> entries = [.. context.Structure_Entry];
            if (templates.Count > 0)
                context.Structure_Template.RemoveRange(templates);

            if (entries.Count > 0)
                context.Structure_Entry.RemoveRange(entries);

            context.SaveChanges();
        }

        #endregion

        #region Filter ShareTypes
        [Fact]
        public void Filter_Owned()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTemplate", DefaultUserGuid.ToString());
            CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, null, template);
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            List<Model_EntryItem> entrys = query.List(null, null, null, null, false, false, true, false);
            Assert.True(entrys.Count > 0);
        }

        [Fact]
        public void Filter_Public()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTemplate", DefaultUserGuid.ToString());
            CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, null, template);
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            List<Model_EntryItem> entrys = query.List(null, null, null, null, true, false, false, false);
            Assert.True(entrys.Count == 0);
        }

        [Fact]
        public void Filter_Shared()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTemplate", DefaultUserGuid.ToString());
            CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, null, template);
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            List<Model_EntryItem> entrys = query.List(null, null, null, null, false, true, false, false);
            Assert.True(entrys.Count == 0);
        }

        [Fact]
        public void Filter_Direct()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTemplate", DefaultUserGuid.ToString());
            CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, null, template);
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            List<Model_EntryItem> entrys = query.List(null, null, null, null, false, false, false, true);
            Assert.True(entrys.Count == 0);
        }

        [Fact]
        public void Filter_ShareTypes()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTemplate", DefaultUserGuid.ToString());
            CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, null, template);
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            List<Model_EntryItem> entrys = query.List(null, null, null, null, true, true, true, true);
            Assert.NotNull(entrys);
            Assert.True(entrys.Count > 0);
        }

        [Fact]
        public void Filter_Owned_Public()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTemplate", DefaultUserGuid.ToString());
            CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, null, template);
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            List<Model_EntryItem> entrys = query.List(null, null, null, null, true, false, true, false);
            Assert.True(entrys.Count > 0);
        }
        #endregion

        #region Filter EntryName ShareTypes
        [Fact]
        public void Filter_EntryName_Owned()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTempalte", DefaultUserGuid.ToString());
            CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, null, template);
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            List<Model_EntryItem> entries = query.List("Test", null, null, null, false, false, true, false);
            Assert.NotNull(entries);
            Assert.True(entries.Count > 0);
        }

        [Fact]
        public void Filter_EntryName_Public()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTempalte", DefaultUserGuid.ToString());
            CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, null, template);
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            List<Model_EntryItem> entries = query.List("Test", null, null, null, true, false, false, false);
            Assert.True(entries.Count == 0);
        }

        [Fact]
        public void Filter_EntryName_Shared()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTempalte", DefaultUserGuid.ToString());
            CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, null, template);
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            List<Model_EntryItem> entries = query.List("Test", null, null, null, false, true, false, false);
            Assert.True(entries.Count == 0);
        }

        [Fact]
        public void Filter_EntryName_Direct()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTempalte", DefaultUserGuid.ToString());
            CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, null, template);
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            List<Model_EntryItem> entries = query.List("Test", null, null, null, false, false, false, true);
            Assert.True(entries.Count == 0);
        }

        [Fact]
        public void Filter_EntryName_ShareTypes()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTempalte", DefaultUserGuid.ToString());
            CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, null, template);
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            List<Model_EntryItem> entries = query.List("Test", null, null, null, true, true, true, false);
            Assert.True(entries.Count > 0);
        }
        #endregion

        #region Filter UserName ShareTypes
        [Fact]
        public void Filter_UserName_Owned()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTempalte", DefaultUserGuid.ToString());
            CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, null, template);
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            List<Model_EntryItem> entries = query.List(null, "Test", null, null, false, false, true, false);
            Assert.True(entries.Count > 0);
        }

        [Fact]
        public void Filter_UserName_Public()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTempalte", DefaultUserGuid.ToString());
            CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, null, template);
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            List<Model_EntryItem> entries = query.List(null, "Test", null, null, true, false, false, false);
            Assert.True(entries.Count == 0);
        }

        [Fact]
        public void Filter_UserName_Shared()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTempalte", DefaultUserGuid.ToString());
            CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, null, template);
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            List<Model_EntryItem> entries = query.List(null, "Test", null, null, false, true, false, false);
            Assert.True(entries.Count == 0);
        }

        [Fact]
        public void Filter_UserName_Direct()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTempalte", DefaultUserGuid.ToString());
            CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, null, template);
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            List<Model_EntryItem> entries = query.List(null, "Test", null, null, false, false, false, true);
            Assert.True(entries.Count == 0);
        }

        [Fact]
        public void Filter_UserName_ShareTypes()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTempalte", DefaultUserGuid.ToString());
            CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, null, template);
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            List<Model_EntryItem> entries = query.List(null, "Test", null, null, true, true, true, false);
            Assert.True(entries.Count > 0);
        }
        #endregion

        #region Filter TemplateName ShareTypes
        [Fact]
        public void Filter_TemplateName_Owned()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTempalte", DefaultUserGuid.ToString());
            CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, null, template);
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, "Test", null, false, false, true, false);
            Assert.NotNull(templates);
            Assert.True(templates.Count > 0);
        }

        [Fact]
        public void Filter_TemplateName_Public()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTempalte", DefaultUserGuid.ToString());
            CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, null, template);
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, "Test", null, true, false, false, false);
            Assert.True(templates.Count == 0);
        }

        [Fact]
        public void Filter_TemplateName_Shared()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTempalte", DefaultUserGuid.ToString());
            CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, null, template);
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, "Test", null, false, true, false, false);
            Assert.True(templates.Count == 0);
        }

        [Fact]
        public void Filter_TemplateName_Direct()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTempalte", DefaultUserGuid.ToString());
            CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, null, template);
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, "Test", null, false, false, false, true);
            Assert.True(templates.Count == 0);
        }

        [Fact]
        public void Filter_TemplateName_ShareTypes()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTempalte", DefaultUserGuid.ToString());
            CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, null, template);
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, "Test", null, true, true, true, false);
            Assert.True(templates.Count > 0);
        }
        #endregion

        #region Filter Tags ShareTypes
        [Fact]
        public void Filter_Tags_Owned()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTempalte", DefaultUserGuid.ToString());
            CreateEntry(DefaultUserGuid.ToString(), "TestEntries", "Test", null, template);
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, null, "Test", false, false, true, false);
            Assert.True(templates.Count > 0);
        }

        [Fact]
        public void Filter_Tags_Public()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTempalte", DefaultUserGuid.ToString());
            CreateEntry(DefaultUserGuid.ToString(), "TestEntries", "Test", null, template);
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, null, "Test", true, false, false, false);
            Assert.True(templates.Count == 0);
        }

        [Fact]
        public void Filter_Tags_Shared()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTempalte", DefaultUserGuid.ToString());
            CreateEntry(DefaultUserGuid.ToString(), "TestEntries", "Test", null, template);
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, null, "Test", false, true, false, false);
            Assert.True(templates.Count == 0);
        }

        [Fact]
        public void Filter_Tags_Direct()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTempalte", DefaultUserGuid.ToString());
            CreateEntry(DefaultUserGuid.ToString(), "TestEntries", "Test", null, template);
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, null, "Test", false, false, false, true);
            Assert.True(templates.Count == 0);
        }

        [Fact]
        public void Filter_Tags_ShareTypes()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTempalte", DefaultUserGuid.ToString());
            CreateEntry(DefaultUserGuid.ToString(), "TestEntries", "Test", null, template);
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, null, "Test", true, true, true, true);
            Assert.True(templates.Count > 0);
        }

        [Fact]
        public void Filter_Tags_Owned_Public()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTempalte", DefaultUserGuid.ToString());
            CreateEntry(DefaultUserGuid.ToString(), "TestEntries", "Test", null, template);
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, null, "Test", true, false, true, false);
            Assert.True(templates.Count > 0);
        }
        #endregion

        #region Mix
        [Fact]
        public void Filter_Name_Owned_Public()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTempalte", DefaultUserGuid.ToString());
            CreateEntry(DefaultUserGuid.ToString(), "TestEntries", "Test", null, template);
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List("Test", null, null, null, true, false, true, false);
            Assert.NotNull(templates);
            Assert.True(templates.Count > 0);
        }

        [Fact]
        public void Filter_Name_UserName_Tags()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTempalte", DefaultUserGuid.ToString());
            CreateEntry(DefaultUserGuid.ToString(), "TestEntries", "Test", null, template);
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List("Test", "Test", null, "Test", false, false, false, false);
            Assert.True(templates.Count == 0);
        }
        #endregion
    }
}
