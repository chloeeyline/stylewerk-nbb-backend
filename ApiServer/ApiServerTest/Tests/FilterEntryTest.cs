using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace ApiServerTest.Tests
{
    public class FilterEntryTest
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

            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> entrys = query.List(null, null, null, null, false, false, true, false);
            Assert.True(entrys.Count > 0);
        }

        [Fact]
        public void Filter_Public()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> entrys = query.List(null, null, null, null, true, false, false, false);
            Assert.True(entrys.Count == 0);
        }

        [Fact]
        public void Filter_Shared()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> entrys = query.List(null, null, null, null, false, true, false, false);
            Assert.True(entrys.Count == 0);
        }

        [Fact]
        public void Filter_Direct()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> entrys = query.List(null, null, null, null, false, false, false, true);
            Assert.True(entrys.Count > 0);
        }

        [Fact]
        public void Filter_ShareTypes()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> entrys = query.List(null, null, null, null, true, true, true, true);
            Assert.NotNull(entrys);
            Assert.True(entrys.Count > 0);
        }

        [Fact]
        public void Filter_Owned_Public()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> entrys = query.List(null, null, null, null, true, false, true, false);
            Assert.True(entrys.Count > 0);
        }
        #endregion

        #region Filter EntryName ShareTypes
        [Fact]
        public void Filter_EntryName_Owned()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> entries = query.List("Test", null, null, null, false, false, true, false);
            Assert.NotNull(entries);
            Assert.True(entries.Count > 0);
        }

        [Fact]
        public void Filter_EntryName_Public()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> entries = query.List("Test", null, null, null, true, false, false, false);
            Assert.True(entries.Count == 0);
        }

        [Fact]
        public void Filter_EntryName_Shared()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> entries = query.List("Test", null, null, null, false, true, false, false);
            Assert.True(entries.Count == 0);
        }

        [Fact]
        public void Filter_EntryName_Direct()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> entries = query.List("Test", null, null, null, false, false, false, true);
            Assert.True(entries.Count > 0);
        }

        [Fact]
        public void Filter_EntryName_ShareTypes()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> entries = query.List("Test", null, null, null, true, true, true, false);
            Assert.True(entries.Count > 0);
        }
        #endregion

        #region Filter UserName ShareTypes
        [Fact]
        public void Filter_UserName_Owned()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> entries = query.List(null, "Test", null, null, false, false, true, false);
            Assert.True(entries.Count > 0);
        }

        [Fact]
        public void Filter_UserName_Public()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> entries = query.List(null, "Test", null, null, true, false, false, false);
            Assert.True(entries.Count == 0);
        }

        [Fact]
        public void Filter_UserName_Shared()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> entries = query.List(null, "Test", null, null, false, true, false, false);
            Assert.True(entries.Count == 0);
        }

        [Fact]
        public void Filter_UserName_Direct()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> entries = query.List(null, "Test", null, null, false, false, false, true);
            Assert.True(entries.Count == 0);
        }

        [Fact]
        public void Filter_UserName_ShareTypes()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> entries = query.List(null, "Test", null, null, true, true, true, false);
            Assert.True(entries.Count > 0);
        }
        #endregion

        #region Filter TemplateName ShareTypes
        [Fact]
        public void Filter_TemplateName_Owned()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, "Test", null, false, false, true, false);
            Assert.NotNull(templates);
            Assert.True(templates.Count > 0);
        }

        [Fact]
        public void Filter_TemplateName_Public()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, "Test", null, true, false, false, false);
            Assert.True(templates.Count == 0);
        }

        [Fact]
        public void Filter_TemplateName_Shared()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, "Test", null, false, true, false, false);
            Assert.True(templates.Count == 0);
        }

        [Fact]
        public void Filter_TemplateName_Direct()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, "Test", null, false, false, false, true);
            Assert.True(templates.Count > 0);
        }

        [Fact]
        public void Filter_TemplateName_ShareTypes()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, "Test", null, true, true, true, false);
            Assert.True(templates.Count > 0);
        }
        #endregion

        #region Filter Tags ShareTypes
        [Fact]
        public void Filter_Tags_Owned()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, null, "Test", false, false, true, false);
            Assert.True(templates.Count > 0);
        }

        [Fact]
        public void Filter_Tags_Public()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, null, "Test", true, false, false, false);
            Assert.True(templates.Count == 0);
        }

        [Fact]
        public void Filter_Tags_Shared()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, null, "Test", false, true, false, false);
            Assert.True(templates.Count == 0);
        }

        [Fact]
        public void Filter_Tags_Direct()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, null, "Test", false, false, false, true);
            Assert.True(templates.Count > 0);
        }

        [Fact]
        public void Filter_Tags_ShareTypes()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, null, "Test", true, true, true, true);
            Assert.True(templates.Count > 0);
        }

        [Fact]
        public void Filter_Tags_Owned_Public()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, null, "Test", true, false, true, false);
            Assert.True(templates.Count > 0);
        }
        #endregion

        #region Mix
        [Fact]
        public void Filter_Name_Owned_Public()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List("Test", null, null, null, true, false, true, false);
            Assert.NotNull(templates);
            Assert.True(templates.Count > 0);
        }

        [Fact]
        public void Filter_Name_UserName_Tags()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List("Test", "Test", null, "Test", false, false, false, false);
            Assert.True(templates.Count > 0);
        }
        #endregion
    }
}
