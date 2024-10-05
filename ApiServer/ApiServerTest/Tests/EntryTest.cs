using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace ApiServerTest.Tests
{
    public class EntryTest
    {
        private Guid DefaultUserGuid = new("90865032-e4e8-4e2b-85e0-5db345f42a5b");
        private Guid OtherUserDefaultGuid = new("cd6c092d-0546-4f8b-b70c-352d2ca765a4");

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

        private static Model_EntryFolders CreateFolder(string user, string folderName)
        {
            EntryFolderQueries query = ReturnQueryFolder(user);

            Model_EntryFolders folder = new(null, folderName, []);
            Model_EntryFolders result = query.Update(folder);

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

        #region UpdateEntry Function
        [Fact]
        public void UpdateEntry_CreateEntry()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTemplate", DefaultUserGuid.ToString());
            CreateEntry(DefaultUserGuid.ToString(), "TestEntry", "Test", null, template);
            Assert.True(true);
        }

        [Fact]
        public void UpdateEntry_UpdateEntry()
        {
            DeleteAll();
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            Model_Template template = CreateTemplate("TestTemplate", DefaultUserGuid.ToString());
            Model_Entry entry = CreateEntry(DefaultUserGuid.ToString(), "TestTemplate", "Test", null, template);

            Model_Entry updateEntry = new(entry.ID, null, (Guid)template.ID, "DefaultTemplate", "Test123", false, entry.Items);
            Model_Entry updatedEntry = query.Update(updateEntry);

            Assert.NotEqual(entry.Name, updatedEntry.Name);
            Assert.NotEqual(entry.Tags, updatedEntry.Tags);
        }

        [Fact]
        public void Update_Entry_DataInvalid()
        {
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());

            Model_Entry action() => query.Update(null);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Entry>)action);
            RequestException result = new(ResultCodes.DataIsInvalid);
            Assert.Equal(result.Code, ex.Code);
        }

        [Fact]
        public void Update_Entry_NoDataFound()
        {
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            Guid templateId = Guid.NewGuid();
            List<Model_EntryCell> cells =
            [
                new Model_EntryCell(Guid.NewGuid(),templateId, null, null)
            ];

            List<Model_EntryRow> rows =
            [
                new Model_EntryRow(Guid.NewGuid(), templateId,1, null, cells)
            ];

            DeleteAll();
            Model_Entry entry = new(Guid.NewGuid(), null, templateId, "TestTemplate", "Test", false, rows);
            Model_Entry action() => query.Update(entry);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Entry>)action);
            RequestException result = new(ResultCodes.NoDataFound);
            Assert.Equal(result.Code, ex.Code);
        }

        [Fact]
        public void UpdateEntry_YouDontOwnData_Template()
        {
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            DeleteAll();
            Model_Template template = CreateTemplate("TestTemplate", OtherUserDefaultGuid.ToString());
            Model_Entry entry = CreateEntry(OtherUserDefaultGuid.ToString(), "TestEntry", "Test", null, template);

            Model_Entry action() => query.Update(entry);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Entry>)action);
            RequestException result = new(ResultCodes.YouDontOwnTheData);
            Assert.Equal(result.Code, ex.Code);
        }

        [Fact]
        public void UpdateEntry_NoDataFound_Folder()
        {
            Guid folderId = Guid.NewGuid();
            DeleteAll();
            Model_Template template = CreateTemplate("TestTemplate", DefaultUserGuid.ToString());

            Model_Entry action() => CreateEntry(DefaultUserGuid.ToString(), "TestEntry", "Test", folderId, template);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Entry>)action);
            RequestException result = new(ResultCodes.NoDataFound);
            Assert.Equal(result.Code, ex.Code);
        }

        [Fact]
        public void UpdateEntry_YouDontOwnData_Folder()
        {
            DeleteAll();
            Model_EntryFolders folder = CreateFolder(OtherUserDefaultGuid.ToString(), "TestFolder");
            Model_Template template = CreateTemplate("TestTemplate", OtherUserDefaultGuid.ToString());

            Model_Entry action() => CreateEntry(DefaultUserGuid.ToString(), "TestEntry", "Test", folder.ID, template);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Entry>)action);
            RequestException result = new(ResultCodes.YouDontOwnTheData);
            Assert.Equal(result.Code, ex.Code);
        }

        [Fact]
        public void UpdateEntry_EntryNameUnique()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTemplate", DefaultUserGuid.ToString());
            Model_Entry entry = CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, null, template);

            Model_Entry action() => CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, null, template);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Entry>)action);
            RequestException result = new(ResultCodes.NameMustBeUnique);
            Assert.Equal(result.Code, ex.Code);
        }

        [Fact]
        public void UpdateEntry_YouDontOwnData_Entry()
        {
            DeleteAll();
            EntryQueries query = ReturnQueryEntry(OtherUserDefaultGuid.ToString());
            Model_Template template = CreateTemplate("TestTemplate", DefaultUserGuid.ToString());
            Model_Entry entry = CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, null, template);

            Model_Entry action() => query.Update(entry);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Entry>)action);
            RequestException result = new(ResultCodes.YouDontOwnTheData);
            Assert.Equal(result.Code, ex.Code);
        }

        [Fact]
        public void UpdateEntry_UpdateEntryName_EntryNameUnique()
        {
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            DeleteAll();
            Model_Template template = CreateTemplate("TestTemplate", DefaultUserGuid.ToString());
            Model_Entry entry = CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, null, template);
            Model_Entry entry2 = CreateEntry(DefaultUserGuid.ToString(), "TestEntry1", null, null, template);

            Model_Entry updateEntry = new(entry.ID, null, (Guid)template.ID, "TestEntry1", null, false, entry.Items);
            Model_Entry action() => query.Update(updateEntry);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Entry>)action);
            RequestException result = new(ResultCodes.NameMustBeUnique);
            Assert.Equal(result.Code, ex.Code);
        }

        [Fact]
        public void UpdateEntry_UpdateEntry_TemplateDoesntMatch()
        {
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            DeleteAll();
            Model_Template template = CreateTemplate("TestTemplate", DefaultUserGuid.ToString());
            Model_Template template2 = CreateTemplate("TestTemplate2", DefaultUserGuid.ToString());
            Model_Entry entry = CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, null, template);

            Model_Entry updateEntry = new(entry.ID, null, (Guid)template2.ID, "TestEntry1", null, false, entry.Items);
            Model_Entry action() => query.Update(updateEntry);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Entry>)action);
            RequestException result = new(ResultCodes.TemplateDoesntMatch);
            Assert.Equal(result.Code, ex.Code);
        }
        #endregion

        #region Remove Function

        [Fact]
        public void Remove()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTemplate", DefaultUserGuid.ToString());
            Model_Entry entry = CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, null, template);

            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            query.Remove(entry.ID);

            NbbContext context = NbbContext.Create();
            Structure_Entry? dbEntry = context.Structure_Entry.FirstOrDefault(e => e.ID == entry.ID);
            Assert.True(entry == null);
        }

        [Fact]
        public void Remove_DataInvalid()
        {
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());

            try
            {
                query.Remove(null);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.DataIsInvalid);
                Assert.True(result.Code == ex.Code);
            }
        }

        [Fact]
        public void Remove_NoDataFound()
        {
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            Guid entryId = Guid.NewGuid();

            try
            {
                query.Remove(entryId);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.DataIsInvalid);
                Assert.True(result.Code == ex.Code);
            }
        }

        [Fact]
        public void Remove_YouDontOwnData()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTemplate", DefaultUserGuid.ToString());
            Model_Entry entry = CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, null, template);
            EntryQueries query = ReturnQueryEntry(OtherUserDefaultGuid.ToString());

            try
            {
                query.Remove(entry.ID);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.DataIsInvalid);
                Assert.True(result.Code == ex.Code);
            }
        }
        #endregion

        #region Get Entry From Template

        [Fact]
        public void GetFromTemplate()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTemplate", DefaultUserGuid.ToString());
            Model_Entry entry = CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, null, template);
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            Model_Entry myEntry = query.GetFromTemplate(template.ID);
            Assert.Equal(entry.Name, myEntry.Name);
        }

        [Fact]
        public void GetFromTemplate_DataInvalid()
        {
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            Model_Entry action() => query.GetFromTemplate(null);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Entry>)action);
            RequestException result = new(ResultCodes.DataIsInvalid);
            Assert.Equal(result.Code, ex.Code);
        }

        [Fact]
        public void GetFromTempalte_NoDataFound()
        {
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            Guid templateId = Guid.NewGuid();
            Model_Entry action() => query.GetFromTemplate(templateId);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Entry>)action);
            RequestException result = new(ResultCodes.NoDataFound);
            Assert.Equal(result.Code, ex.Code);
        }

        [Fact]
        public void GetFromTemplate_YouDontOwnData()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTemplate", OtherUserDefaultGuid.ToString());
            Model_Entry entry = CreateEntry(OtherUserDefaultGuid.ToString(), "TestEntry", null, null, template);

            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            Model_Entry action() => query.GetFromTemplate(template.ID);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Entry>)action);
            RequestException result = new(ResultCodes.YouDontOwnTheData);
            Assert.Equal(result.Code, ex.Code);
        }
        #endregion

        #region Details Function

        [Fact]
        public void Details()
        {
            DeleteAll();
            Model_Template template = CreateTemplate("TestTemplate", OtherUserDefaultGuid.ToString());
            Model_Entry entry = CreateEntry(OtherUserDefaultGuid.ToString(), "TestEntry", null, null, template);

            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            Model_Entry details = query.Details(entry.ID);
            Assert.True(details.Name == entry.Name);
            Assert.True(details.Items.Count == entry.Items.Count);
        }

        [Fact]
        public void Details_DataInvalid()
        {
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            Model_Entry action() => query.Details(null);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Entry>)action);
            RequestException result = new(ResultCodes.DataIsInvalid);
            Assert.Equal(result.Code, ex.Code);

        }

        [Fact]
        public void Details_NoDataFound()
        {
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            Guid entryId = Guid.NewGuid();
            Model_Entry action() => query.Details(entryId);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Entry>)action);
            RequestException result = new(ResultCodes.NoDataFound);
            Assert.Equal(result.Code, ex.Code);
        }
        #endregion
    }
}
