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

            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36";
            EntryQueries query = new(DB, user);
            return query;
        }

        private static TemplateQueries ReturnQueryTemplate(string userGuid)
        {
            NbbContext DB = NbbContext.Create();
            ApplicationUser user = DB.GetUser(new Guid(userGuid));

            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36";
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

        #endregion

        #region UpdateEntry Function
        [Fact]
        public void UpdateEntry_CreateEntry()
        {
            Model_Template template = CreateTemplate("DefaultTemplate1", DefaultUserGuid.ToString());
            CreateEntry(DefaultUserGuid.ToString(), "Lieblingsbücher", "Books", null, template);
            Assert.True(true);
        }

        [Fact]
        public void UpdateEntry_UpdateEntry()
        {
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            Model_Template template = CreateTemplate("DefaultTemplate2", DefaultUserGuid.ToString());
            Model_Entry entry = CreateEntry(DefaultUserGuid.ToString(), "Lieblingsfilme", "Movies", null, template);

            Model_Entry updateEntry = new(entry.ID, null, (Guid)template.ID, "Leselsite", "Books", false, entry.Items);
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

            Model_Entry entry = new(Guid.NewGuid(), null, templateId, "Lieder", "Music", false, rows);
            Model_Entry action() => query.Update(entry);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Entry>)action);
            RequestException result = new(ResultCodes.NoDataFound);
            Assert.Equal(result.Code, ex.Code);
        }

        [Fact]
        public void UpdateEntry_YouDontOwnData_Template()
        {
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            Model_Template template = CreateTemplate("DefaultTemplate3", OtherUserDefaultGuid.ToString());
            Model_Entry entry = CreateEntry(OtherUserDefaultGuid.ToString(), "Leseliste", "Bücher", null, template);

            Model_Entry action() => query.Update(entry);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Entry>)action);
            RequestException result = new(ResultCodes.YouDontOwnTheData);
            Assert.Equal(result.Code, ex.Code);
        }

        [Fact]
        public void UpdateEntry_NoDataFound_Folder()
        {
            Guid folderId = Guid.NewGuid();
            Model_Template template = CreateTemplate("DefaultTemplate4", DefaultUserGuid.ToString());

            Model_Entry action() => CreateEntry(DefaultUserGuid.ToString(), "Halligalli", "blabla", folderId, template);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Entry>)action);
            RequestException result = new(ResultCodes.NoDataFound);
            Assert.Equal(result.Code, ex.Code);
        }

        [Fact]
        public void UpdateEntry_YouDontOwnData_Folder()
        {
            Model_EntryFolders folder = CreateFolder(OtherUserDefaultGuid.ToString(), "DefaultFolder1");
            Model_Template template = CreateTemplate("DefaultTemplate5", OtherUserDefaultGuid.ToString());

            Model_Entry action() => CreateEntry(DefaultUserGuid.ToString(), "Halligalli", "blabla", folder.ID, template);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Entry>)action);
            RequestException result = new(ResultCodes.YouDontOwnTheData);
            Assert.Equal(result.Code, ex.Code);
        }

        [Fact]
        public void UpdateEntry_EntryNameUnique()
        {
            Model_Template template = CreateTemplate("DefaultTemplate6", DefaultUserGuid.ToString());
            Model_Entry entry = CreateEntry(DefaultUserGuid.ToString(), "TestEntry1", null, null, template);

            Model_Entry action() => CreateEntry(DefaultUserGuid.ToString(), "TestEntry1", null, null, template);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Entry>)action);
            RequestException result = new(ResultCodes.NameMustBeUnique);
            Assert.Equal(result.Code, ex.Code);
        }

        [Fact]
        public void UpdateEntry_YouDontOwnData_Entry()
        {
            EntryQueries query = ReturnQueryEntry(OtherUserDefaultGuid.ToString());
            Model_Template template = CreateTemplate("DefaultTemplate7", DefaultUserGuid.ToString());
            Model_Entry entry = CreateEntry(DefaultUserGuid.ToString(), "TestEntry2", null, null, template);

            Model_Entry action() => query.Update(entry);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Entry>)action);
            RequestException result = new(ResultCodes.YouDontOwnTheData);
            Assert.Equal(result.Code, ex.Code);
        }

        [Fact]
        public void UpdateEntry_UpdateEntryName_EntryNameUnique()
        {
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            Model_Template template = CreateTemplate("DefaultTemplate8", DefaultUserGuid.ToString());
            Model_Entry entry = CreateEntry(DefaultUserGuid.ToString(), "TestEntry3", null, null, template);
            Model_Entry entry2 = CreateEntry(DefaultUserGuid.ToString(), "TestEntry4", null, null, template);

            Model_Entry updateEntry = new(entry.ID, null, (Guid)template.ID, "TestEntry4", null, false, entry.Items);
            Model_Entry action() => query.Update(updateEntry);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Entry>)action);
            RequestException result = new(ResultCodes.NameMustBeUnique);
            Assert.Equal(result.Code, ex.Code);
        }

        [Fact]
        public void UpdateEntry_UpdateEntry_TemplateDoesntMatch()
        {
            EntryQueries query = ReturnQueryEntry(DefaultUserGuid.ToString());
            Model_Template template = CreateTemplate("DefaultTemplate9", DefaultUserGuid.ToString());
            Model_Template template2 = CreateTemplate("DefaultTemplate10", DefaultUserGuid.ToString());
            Model_Entry entry = CreateEntry(DefaultUserGuid.ToString(), "TestEntry5", null, null, template);

            Model_Entry updateEntry = new(entry.ID, null, (Guid)template2.ID, "TestEntry4", null, false, entry.Items);
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
            Model_Template template = CreateTemplate("DefaultTemplate11", DefaultUserGuid.ToString());
            Model_Entry entry = CreateEntry(DefaultUserGuid.ToString(), "TestEntry5", null, null, template);

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
            Model_Template template = CreateTemplate("DefaultTemplate12", DefaultUserGuid.ToString());
            Model_Entry entry = CreateEntry(DefaultUserGuid.ToString(), "TestEntry6", null, null, template);
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
            Model_Template template = CreateTemplate("DefaultTemplate13", DefaultUserGuid.ToString());
            Model_Entry entry = CreateEntry(DefaultUserGuid.ToString(), "TestEntry7", null, null, template);
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
            Model_Template template = CreateTemplate("DefaultTempalte14", OtherUserDefaultGuid.ToString());
            Model_Entry entry = CreateEntry(OtherUserDefaultGuid.ToString(), "TestEntry8", null, null, template);

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
            Model_Template template = CreateTemplate("DefaultTempalte15", OtherUserDefaultGuid.ToString());
            Model_Entry entry = CreateEntry(OtherUserDefaultGuid.ToString(), "TestEntry9", null, null, template);

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
