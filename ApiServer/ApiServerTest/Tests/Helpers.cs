﻿using StyleWerk.NBB.Authentication;
using StyleWerk.NBB.AWS;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Database.User;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace ApiServerTest.Tests
{
    public class Helpers
    {
        public static EntryFolderQueries ReturnFolderQuery(string userGuid)
        {
            NbbContext DB = NbbContext.Create();
            ApplicationUser user = DB.GetUser(new Guid(userGuid));
            EntryFolderQueries query = new(DB, user);
            return query;
        }

        public static TemplateQueries ReturnTemplateQuery(string userGuid)
        {
            NbbContext DB = NbbContext.Create();
            ApplicationUser user = DB.GetUser(new Guid(userGuid));

            TemplateQueries query = new(DB, user);
            return query;
        }

        public static EntryQueries ReturnEntryQuery(string userGuid)
        {
            NbbContext DB = NbbContext.Create();
            ApplicationUser user = DB.GetUser(new Guid(userGuid));

            EntryQueries query = new(DB, user);
            return query;
        }

        public static EditorQueries ReturnEditorQuery(string userGuid)
        {
            NbbContext DB = NbbContext.Create();
            ApplicationUser user = DB.GetUser(new Guid(userGuid));

            EditorQueries query = new(DB, user);
            return query;
        }

        public static void DeleteAll()
        {
            NbbContext context = NbbContext.Create();
            List<Structure_Entry_Folder> folders = [.. context.Structure_Entry_Folder];
            List<Structure_Template> templates = [.. context.Structure_Template];
            List<User_Login> usersLogin = [.. context.User_Login];
            List<User_Information> userInformations = [.. context.User_Information];
            List<Structure_Entry> entries = [.. context.Structure_Entry];

            if (folders.Count > 0)
                context.Structure_Entry_Folder.RemoveRange(folders);

            if (templates.Count > 0)
                context.Structure_Template.RemoveRange(templates);

            if (entries.Count > 0)
                context.Structure_Entry.RemoveRange(entries);

            if (usersLogin.Count > 0)
                context.User_Login.RemoveRange(usersLogin);

            if (userInformations.Count > 0)
                context.User_Information.RemoveRange(userInformations);

            context.SaveChanges();
        }

        public static Guid CreateUser(string userName, string email, string password)
        {
            NbbContext DB = NbbContext.Create();
            ApplicationUser user = DB.GetUser(Guid.NewGuid());

            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36";
            AuthQueries query = new(DB, user, userAgent, SecretData.GetData());

            Model_Registration register = new(userName, email, password, "Chloe", "Hauer", UserGender.Female, 0);
            query.Registration(register);

            Model_Login login = new(userName, password, true);
            User_Login myUser = query.GetUser(login);

            return myUser.ID;
        }

        public static Model_EntryFolders CreateFolder(string user, string folderName)
        {
            EntryFolderQueries query = ReturnFolderQuery(user);

            Model_EntryFolders folder = new(null, folderName, []);
            Model_EntryFolders result = query.Update(folder);

            return result;
        }

        public static Model_Editor CreateTemplate(string? templateName, string userGuid, Guid? updateTemplateId)
        {
            EditorQueries query = Helpers.ReturnEditorQuery(userGuid);
            Guid rowId = Guid.NewGuid();
            Guid templateId = Guid.NewGuid();
            Guid templateCellId = Guid.NewGuid();
            Template template;

            if (updateTemplateId != null)
            {
                template = new(updateTemplateId, templateName, "Test", "Test");
            }
            else
            {
                template = new(templateId, templateName, "Test", "Test");
            }

            TemplateCell templateCell = new(templateCellId, 1, false, false, null, null, null);
            TemplateRow templateRow = new(rowId, false, false, false);

            List<EntryCell> entryCells = [];
            EntryCell entryCell = new(Guid.NewGuid(), templateCellId, null, templateCell);
            entryCells.Add(entryCell);

            List<EntryRow> entryrows = [];
            EntryRow entryRow = new(Guid.NewGuid(), templateId, templateRow, entryCells);
            entryrows.Add(entryRow);


            Model_Editor newTemplate = new(null, null, null, null, null, false, template, entryrows);
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

            Model_Editor newEntry = new(Guid.NewGuid(), folderId, template.TemplateID, title, tags, false, template.Template, entryrows);
            Model_Editor result = query.UpdateEntry(newEntry);

            return result;
        }
    }
}