using StyleWerk.NBB.Authentication;
using StyleWerk.NBB.AWS;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Admin;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Database.User;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace ApiServerTest.Tests
{
    public class Helpers
    {
        public readonly static NbbContext DB = NbbContext.Create(true);

        public static EntryFolderQueries ReturnFolderQuery(string userGuid)
        {
            ApplicationUser user = DB.GetUser(new Guid(userGuid));
            EntryFolderQueries query = new(DB, user);
            return query;
        }

        public static TemplateQueries ReturnTemplateQuery(string userGuid)
        {
            ApplicationUser user = DB.GetUser(new Guid(userGuid));

            TemplateQueries query = new(DB, user);
            return query;
        }

        public static EntryQueries ReturnEntryQuery(string userGuid)
        {
            ApplicationUser user = DB.GetUser(new Guid(userGuid));

            EntryQueries query = new(DB, user);
            return query;
        }

        public static EditorQueries ReturnEditorQuery(string userGuid)
        {
            ApplicationUser user = DB.GetUser(new Guid(userGuid));

            EditorQueries query = new(DB, user);
            return query;
        }

        public static AuthQueries ReturnAuthQuery(string userGuid)
        {
            ApplicationUser user = DB.GetUser(new Guid(userGuid));

            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36";
            AuthQueries query = new(DB, user, userAgent, SecretData.GetData(true));
            return query;
        }

        public static ColorThemeQueries ReturnColorQuery(string userGuid)
        {
            ApplicationUser user = DB.GetUser(new Guid(userGuid));

            ColorThemeQueries query = new(DB, user);
            return query;
        }

        public static LanguageQueries ReturnLanguageQuery(string userGuid)
        {
            ApplicationUser user = DB.GetUser(new Guid(userGuid));

            LanguageQueries query = new(DB, user);
            return query;
        }

        public static void DeleteAll()
        {
            List<Structure_Entry_Folder> folders = [.. DB.Structure_Entry_Folder];
            List<Structure_Template> templates = [.. DB.Structure_Template];
            List<User_Login> usersLogin = [.. DB.User_Login];
            List<User_Information> userInformations = [.. DB.User_Information];
            List<Structure_Entry> entries = [.. DB.Structure_Entry];
            List<Admin_ColorTheme> colors = [.. DB.Admin_ColorTheme];
            List<Admin_Language> langs = [.. DB.Admin_Language];

            if (folders.Count > 0)
                DB.Structure_Entry_Folder.RemoveRange(folders);

            if (templates.Count > 0)
                DB.Structure_Template.RemoveRange(templates);

            if (entries.Count > 0)
                DB.Structure_Entry.RemoveRange(entries);

            if (colors.Count > 0)
                DB.Admin_ColorTheme.RemoveRange(colors);

            if (langs.Count > 0)
                DB.Admin_Language.RemoveRange(langs);

            if (usersLogin.Count > 0)
                DB.User_Login.RemoveRange(usersLogin);

            if (userInformations.Count > 0)
                DB.User_Information.RemoveRange(userInformations);

            DB.SaveChanges();
        }

        public static Guid CreateUser(string userName, string email, string password)
        {
            ApplicationUser user = DB.GetUser(Guid.NewGuid());

            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36";
            AuthQueries query = new(DB, user, userAgent, SecretData.GetData(true));

            Model_Registration register = new(userName, email, password, "Chloe", "Hauer", UserGender.Female, 0);
            query.Registration(register, true);

            Model_Login login = new(userName, password, true);
            User_Login myUser = query.GetUser(login);

            return myUser.ID;
        }

        public static Admin_ColorTheme? CreateColorTheme(string userId, string themeName, Guid updateTheme)
        {
            ColorThemeQueries query = ReturnColorQuery(userId);

            if (updateTheme != Guid.Empty)
            {
                Model_ColorTheme color = new(updateTheme, themeName, "Test", "{\"foo\":\"bar\"}");
                query.Update(color);
            }
            else
            {
                Model_ColorTheme color = new(Guid.NewGuid(), themeName, "Test", "{\"foo\":\"bar\"}");
                query.Update(color);
            }

            Admin_ColorTheme? result = DB.Admin_ColorTheme.FirstOrDefault(c => c.Name == themeName);

            return result;
        }

        public static Admin_Language? CreateLanguage(string userId, string code, string langName)
        {
            LanguageQueries query = ReturnLanguageQuery(userId);

            Model_Language lang = new(code, langName, "{\"foo\":\"bar\"}");
            query.Update(lang);

            Admin_Language? result = DB.Admin_Language.FirstOrDefault(c => c.Code == code);

            return result;
        }

        public static Model_EntryFolders CreateFolder(string user, string folderName)
        {
            EntryFolderQueries query = ReturnFolderQuery(user);

            Model_EntryFolders folder = new(Guid.Empty, folderName, [], false, 0, null);
            Model_EntryFolders result = query.Update(folder);

            return result;
        }

        public static Model_Editor CreateTemplate(string? templateName, string userGuid, Guid? updateTemplateId)
        {
            EditorQueries query = ReturnEditorQuery(userGuid);
            Guid rowId = Guid.NewGuid();
            Guid templateId = Guid.NewGuid();
            Guid templateCellId = Guid.NewGuid();

            Template template = updateTemplateId.HasValue
                ? new(updateTemplateId.Value, templateName, false, "Test", "Test")
                : new(templateId, templateName, false, "Test", "Test");

            TemplateCell templateCell = new(templateCellId, 1, false, false, null, null, null);
            TemplateRow templateRow = new(rowId, false, false, false);

            List<EntryCell> entryCells = [];
            EntryCell entryCell = new(Guid.NewGuid(), templateCellId, null, templateCell);
            entryCells.Add(entryCell);

            List<EntryRow> entryrows = [];
            EntryRow entryRow = new(Guid.NewGuid(), templateId, templateRow, entryCells);
            entryrows.Add(entryRow);


            Model_Editor newTemplate = new(Guid.Empty, null, Guid.Empty, null, null, false, false, template, entryrows);
            Model_Editor result = query.UpdateTemplate(newTemplate);

            return result;
        }

        public static Model_Editor CreateEntry(string user, string title, Guid? folderId, string tags, Model_Editor template)
        {
            EditorQueries query = ReturnEditorQuery(user);

            List<EntryCell> entryCells = [];
            List<EntryRow> entryrows = [];
            foreach (EntryRow row in template.Items)
            {

                entryCells.Clear();
                foreach (EntryCell cell in row.Items)
                {
                    EntryCell temp = cell with { Data = "Test" };

                    entryCells.Add(temp);
                }

                EntryRow x = row with { Items = entryCells };
                entryrows.Add(x);

            }

            Model_Editor newEntry = new(Guid.NewGuid(), folderId, template.TemplateID, title, tags, false, false, template.Template, entryrows);
            Model_Editor result = query.UpdateEntry(newEntry);

            return result;
        }

        public static void SetUserTokensNull(Guid userGuid, string email)
        {
            User_Login user = DB.User_Login.First(u => u.ID == userGuid);
            user.StatusToken = null;
            user.StatusCode = null;
            user.StatusTokenExpireTime = null;
            user.Email = email;
            user.EmailNormalized = email.Normalize().ToLower();
            DB.SaveChanges();
        }

        public static void SetUserAdmin(Guid userId)
        {
            User_Login user = DB.User_Login.First(u => u.ID == userId);
            user.Admin = true;
            DB.SaveChanges();
        }
    }
}
