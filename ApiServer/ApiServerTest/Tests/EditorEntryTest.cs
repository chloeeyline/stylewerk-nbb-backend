using StyleWerk.NBB.Database;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace ApiServerTest.Tests
{
    public class EditorEntryTest
    {
        private Guid DefaultUserGuid { get; set; }
        private Guid OtherUserDefaultGuid { get; set; }

        private const string Username = "TestUser";
        private const string Email = "chloe.hauer@lbs4.salzburg.at";
        private const string Password = "TestTest@123";

        private const string OtherUsername = "TestUser1";
        private const string OtherEmail = "florian.windisch@lbs4.salzburg.at";

        #region Update Entry Function
        [Fact]
        public void CreateEntry()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, "Test", template);

            NbbContext context = NbbContext.Create();
            StyleWerk.NBB.Database.Structure.Structure_Entry? dbEntry = context.Structure_Entry.FirstOrDefault(t => t.ID == entry.ID);
            Assert.NotNull(dbEntry);
        }

        [Fact]
        public void CreateEntry_DataInvalid()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor action() => Helpers.CreateEntry(DefaultUserGuid.ToString(), string.Empty, null, "Test", template);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Editor>)action);
            RequestException result = new(ResultCodes.DataIsInvalid);
            Assert.Equal(result.Code, ex.Code);
        }

        [Fact]
        public void UpdateEntry_Update()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            EditorQueries query = Helpers.ReturnEditorQuery(DefaultUserGuid.ToString());
            Model_Editor modelTemplate = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, "Test", modelTemplate);

            Model_Editor template = query.GetTemplate(modelTemplate.TemplateID);
            Model_Editor getEntry = query.GetEntry(entry.ID);

            Model_Editor uEntry = new(entry.ID, null, template.TemplateID, "DefaultTemplate", "Test", false, template.Template, getEntry.Items);
            Model_Editor updatedEntry = query.UpdateEntry(uEntry);

            Assert.NotEqual(entry.Name, updatedEntry.Name);
        }

        [Fact]
        public void UpdateEntry_Update_DataInvalid()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            EditorQueries query = Helpers.ReturnEditorQuery(DefaultUserGuid.ToString());
            Model_Editor modelTemplate = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, "Test", modelTemplate);

            Model_Editor template = query.GetTemplate(modelTemplate.TemplateID);
            Model_Editor getEntry = query.GetEntry(entry.ID);

            Model_Editor uEntry = new(entry.ID, null, template.TemplateID, null, "Test", false, template.Template, getEntry.Items);

            Model_Editor action() => query.UpdateEntry(uEntry);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Editor>)action);
            RequestException result = new(ResultCodes.DataIsInvalid);
            Assert.Equal(result.Code, ex.Code);
        }

        [Fact]
        public void UpdateEntry_UpdateEntry_NoDataFound()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            EditorQueries query = Helpers.ReturnEditorQuery(DefaultUserGuid.ToString());
            Model_Editor modelTemplate = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, "Test", modelTemplate);

            Model_Editor template = query.GetTemplate(modelTemplate.TemplateID);
            Model_Editor getEntry = query.GetEntry(entry.ID);

            Model_Editor uEntry = new(entry.ID, null, Guid.NewGuid(), "TestEntry", "Test", false, template.Template, getEntry.Items);

            Model_Editor action() => query.UpdateEntry(uEntry);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Editor>)action);
            RequestException result = new(ResultCodes.NoDataFound);
            Assert.Equal(result.Code, ex.Code);
        }

        [Fact]
        public void UpdateEntry_UpdateEntry_DontOwnData()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);

            EditorQueries query = Helpers.ReturnEditorQuery(OtherUserDefaultGuid.ToString());

            Model_Editor modelTemplate = Helpers.CreateTemplate("TestTemplate", OtherUserDefaultGuid.ToString(), null);
            Model_Editor modelTemplate2 = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, "Test", modelTemplate2);

            Model_Editor template = query.GetTemplate(modelTemplate.TemplateID);
            Model_Editor getEntry = query.GetEntry(entry.ID);

            Model_Editor uEntry = new(entry.ID, null, template.TemplateID, "DefaultTemplate", "Test", false, template.Template, getEntry.Items);

            Model_Editor action() => query.UpdateEntry(uEntry);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Editor>)action);
            RequestException result = new(ResultCodes.YouDontOwnTheData);
            Assert.Equal(result.Code, ex.Code);
        }

        [Fact]
        public void UpdateEntry_UpdateEntry_NoDataFound_Folder()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);

            EditorQueries query = Helpers.ReturnEditorQuery(DefaultUserGuid.ToString());
            Model_Editor modelTemplate = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, "Test", modelTemplate);

            Model_Editor template = query.GetTemplate(modelTemplate.TemplateID);
            Model_Editor getEntry = query.GetEntry(entry.ID);

            Model_Editor uEntry = new(entry.ID, Guid.NewGuid(), template.TemplateID, "DefaultTemplate", "Test", false, template.Template, getEntry.Items);

            Model_Editor action() => query.UpdateEntry(uEntry);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Editor>)action);
            RequestException result = new(ResultCodes.NoDataFound);
            Assert.Equal(result.Code, ex.Code);
        }

        [Fact]
        public void UpdateEntry_UpdateEntry_YouDontOwnData_Folder()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);

            EditorQueries query = Helpers.ReturnEditorQuery(DefaultUserGuid.ToString());
            Model_Editor modelTemplate = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, "Test", modelTemplate);
            Model_EntryFolders folder = Helpers.CreateFolder(OtherUserDefaultGuid.ToString(), "TestFolder");

            Model_Editor template = query.GetTemplate(modelTemplate.TemplateID);
            Model_Editor getEntry = query.GetEntry(entry.ID);

            Model_Editor uEntry = new(entry.ID, folder.ID, template.TemplateID, "DefaultTemplate", "Test", false, template.Template, getEntry.Items);

            Model_Editor action() => query.UpdateEntry(uEntry);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Editor>)action);
            RequestException result = new(ResultCodes.YouDontOwnTheData);
            Assert.Equal(result.Code, ex.Code);
        }

        [Fact]
        public void UpdateEntry_Create_NameUnique()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            EditorQueries query = Helpers.ReturnEditorQuery(DefaultUserGuid.ToString());
            Model_Editor modelTemplate = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, "Test", modelTemplate);

            Model_Editor template = query.GetTemplate(modelTemplate.TemplateID);
            Model_Editor getEntry = query.GetEntry(entry.ID);

            Model_Editor uEntry = new(null, null, template.TemplateID, "TestEntry", "Test", false, template.Template, getEntry.Items);

            Model_Editor action() => query.UpdateEntry(uEntry);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Editor>)action);
            RequestException result = new(ResultCodes.NameMustBeUnique);
            Assert.Equal(result.Code, ex.Code);
        }

        [Fact]
        public void UpdateEntry_Update_DontOwnData()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);

            EditorQueries query = Helpers.ReturnEditorQuery(DefaultUserGuid.ToString());
            Model_Editor modelTemplate = Helpers.CreateTemplate("TestTemplate", OtherUserDefaultGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(OtherUserDefaultGuid.ToString(), "TestEntry", null, "Test", modelTemplate);

            Model_Editor template = query.GetTemplate(modelTemplate.TemplateID);
            Model_Editor getEntry = query.GetEntry(entry.ID);

            Model_Editor uEntry = new(entry.ID, null, template.TemplateID, "DefaultTemplate", "Test", false, template.Template, getEntry.Items);

            Model_Editor action() => query.UpdateEntry(uEntry);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Editor>)action);
            RequestException result = new(ResultCodes.YouDontOwnTheData);
            Assert.Equal(result.Code, ex.Code);
        }

        [Fact]
        public void UpdateEntry_Update_NameUnique()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            EditorQueries query = Helpers.ReturnEditorQuery(DefaultUserGuid.ToString());
            Model_Editor modelTemplate = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, "Test", modelTemplate);
            Model_Editor entry2 = Helpers.CreateEntry(DefaultUserGuid.ToString(), "Test", null, "Test", modelTemplate);

            Model_Editor template = query.GetTemplate(modelTemplate.TemplateID);
            Model_Editor getEntry = query.GetEntry(entry2.ID);

            Model_Editor uEntry = new(entry2.ID, null, template.TemplateID, "TestEntry", "Test", false, template.Template, getEntry.Items);

            Model_Editor action() => query.UpdateEntry(uEntry);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Editor>)action);
            RequestException result = new(ResultCodes.NameMustBeUnique);
            Assert.Equal(result.Code, ex.Code);
        }
        #endregion
    }
}
