using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace ApiServerTest.Tests
{
    [Collection("Sequential")]
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
            _ = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> entrys = query.List(null, null, null, null, false);
            Assert.True(entrys.Count > 0);
        }

        [Fact]
        public void Filter_Public()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            _ = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> entrys = query.List(null, null, null, null, true);
            Assert.Empty(entrys);
        }

        [Fact]
        public void Filter_Shared()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            _ = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> entrys = query.List(null, null, null, null, false);
            Assert.Empty(entrys);
        }

        [Fact]
        public void Filter_Direct()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            _ = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> entrys = query.List(null, null, null, null, false);
            Assert.True(entrys.Count > 0);
        }

        [Fact]
        public void Filter_ShareTypes()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            _ = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> entrys = query.List(null, null, null, null, true);
            Assert.NotNull(entrys);
            Assert.True(entrys.Count > 0);
        }

        [Fact]
        public void Filter_Owned_Public()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            _ = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> entrys = query.List(null, null, null, null, true);
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
            _ = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> entries = query.List("Test", null, null, null, false);
            Assert.NotNull(entries);
            Assert.True(entries.Count > 0);
        }

        [Fact]
        public void Filter_EntryName_Public()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            _ = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> entries = query.List("Test", null, null, null, true);
            Assert.Empty(entries);
        }

        [Fact]
        public void Filter_EntryName_Shared()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            _ = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> entries = query.List("Test", null, null, null, false);
            Assert.Empty(entries);
        }

        [Fact]
        public void Filter_EntryName_Direct()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            _ = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> entries = query.List("Test", null, null, null, false);
            Assert.True(entries.Count > 0);
        }

        [Fact]
        public void Filter_EntryName_ShareTypes()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            _ = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> entries = query.List("Test", null, null, null, true);
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
            _ = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> entries = query.List(null, "Test", null, null, false);
            Assert.True(entries.Count > 0);
        }

        [Fact]
        public void Filter_UserName_Public()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            _ = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> entries = query.List(null, "Test", null, null, true);
            Assert.Empty(entries);
        }

        [Fact]
        public void Filter_UserName_Shared()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            _ = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> entries = query.List(null, "Test", null, null, false);
            Assert.Empty(entries);
        }

        [Fact]
        public void Filter_UserName_Direct()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            _ = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> entries = query.List(null, "Test", null, null, false);
            Assert.Empty(entries);
        }

        [Fact]
        public void Filter_UserName_ShareTypes()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            _ = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> entries = query.List(null, "Test", null, null, true);
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
            _ = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, "Test", null, false);
            Assert.NotNull(templates);
            Assert.True(templates.Count > 0);
        }

        [Fact]
        public void Filter_TemplateName_Public()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            _ = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, "Test", null, true);
            Assert.Empty(templates);
        }

        [Fact]
        public void Filter_TemplateName_Shared()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            _ = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, "Test", null, false);
            Assert.Empty(templates);
        }

        [Fact]
        public void Filter_TemplateName_Direct()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            _ = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, "Test", null, false);
            Assert.True(templates.Count > 0);
        }

        [Fact]
        public void Filter_TemplateName_ShareTypes()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            _ = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, "Test", null, true);
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
            _ = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, null, "Test", false);
            Assert.True(templates.Count > 0);
        }

        [Fact]
        public void Filter_Tags_Public()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            _ = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, null, "Test", true);
            Assert.Empty(templates);
        }

        [Fact]
        public void Filter_Tags_Shared()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            _ = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, null, "Test", false);
            Assert.Empty(templates);
        }

        [Fact]
        public void Filter_Tags_Direct()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            _ = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, null, "Test", false);
            Assert.True(templates.Count > 0);
        }

        [Fact]
        public void Filter_Tags_ShareTypes()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            _ = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, null, "Test", true);
            Assert.True(templates.Count > 0);
        }

        [Fact]
        public void Filter_Tags_Owned_Public()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            _ = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List(null, null, null, "Test", true);
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
            _ = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List("Test", null, null, null, true);
            Assert.NotNull(templates);
            Assert.True(templates.Count > 0);
        }

        [Fact]
        public void Filter_Name_UserName_Tags()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            _ = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntries", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            List<Model_EntryItem> templates = query.List("Test", "Test", null, "Test", false);
            Assert.True(templates.Count > 0);
        }
        #endregion
    }
}
