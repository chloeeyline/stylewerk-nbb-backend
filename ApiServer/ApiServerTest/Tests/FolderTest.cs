﻿using Microsoft.EntityFrameworkCore;
using StyleWerk.NBB.AWS;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.User;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;
using System.Diagnostics;

namespace ApiServerTest.Tests
{
    public class FolderTest
    {
        private static NbbContext CreateDbContext()
        {
            SecretData secretData = AmazonSecretsManager.GetData() ?? throw new Exception();
            string connectionString = secretData.GetConnectionString();

            DbContextOptionsBuilder<NbbContext> builder = new();
            builder.UseNpgsql(connectionString);

            return new NbbContext(builder.Options);
        }

        private static EntryFolderQueries ReturnQuery(string userGuid)
        {
            NbbContext DB = CreateDbContext();

            ApplicationUser CurrentUser = new();
            Guid id = Guid.Parse(userGuid);
            User_Login? login = DB.User_Login.FirstOrDefault(s => s.ID == id);
            User_Information? information = DB.User_Information.FirstOrDefault(s => s.ID == id);
            string[] rights = [.. DB.User_Right.Where(s => s.ID == id).Select(s => s.Name)];
            CurrentUser = login is null || information is null ?
                new ApplicationUser() :
                new ApplicationUser(login, information, rights);

            EntryFolderQueries query = new(DB, CurrentUser);
            return query;
        }

        [Fact]
        public void AddFolder()
        {
            EntryFolderQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");
            Model_EntryFolders newFolder = new(null, $"TestFolder1", new Model_EntryItem[0]);

            Model_EntryFolders response = query.Update(newFolder);

            Assert.IsType<Model_EntryFolders>(response);
        }

        private Model_EntryFolders CreateFolderForAnotherUser(string folderName)
        {
            EntryFolderQueries query = ReturnQuery("6e4a61db-8c61-4594-a643-feae632caba2");

            Model_EntryFolders newFolderOtherUser = new(null, folderName, new Model_EntryItem[0]);
            Model_EntryFolders responseOtherUser = query.Update(newFolderOtherUser);

            return responseOtherUser;
        }

        [Fact]
        public void MissingRight()
        {
            Model_EntryFolders responseOtherUser = CreateFolderForAnotherUser("TestFolder2");
            EntryFolderQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");
            Model_EntryFolders folder = new(responseOtherUser.ID, "TestFolder3", new Model_EntryItem[0]);

            Func<Model_EntryFolders> action = () => query.Update(folder);

            RequestException exception = Assert.Throws<RequestException>(action);
            RequestException result = new(ResultCodes.MissingRight);
            Assert.Equal(result.Code, exception.Code);
        }

        private Model_EntryFolders CreateFolderForUser(string folderName)
        {
            EntryFolderQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");

            Model_EntryFolders newFolderUser = new(null, folderName, new Model_EntryItem[0]);
            Model_EntryFolders responseUser = query.Update(newFolderUser);

            return responseUser;
        }

        [Fact]
        public void FolderNameExists()
        {
            Model_EntryFolders folderUser = CreateFolderForUser("TestFolder3");
            EntryFolderQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");
            Model_EntryFolders folder = new(folderUser.ID, "TestFolder1", new Model_EntryItem[0]);

            Func<Model_EntryFolders> action = () => query.Update(folder);

            RequestException exception = Assert.Throws<RequestException>(action);
            RequestException result = new(ResultCodes.NameMustBeUnique);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void DataInvalid()
        {
            EntryFolderQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");
            Model_EntryFolders newFolder = new(null, null, new Model_EntryItem[0]);

            Func<Model_EntryFolders> action = () => query.Update(newFolder);

            RequestException exception = Assert.Throws<RequestException>(action);
            RequestException result = new(ResultCodes.DataIsInvalid);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void RemoveDataNoFound()
        {
            EntryFolderQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");

            try
            {
                query.Remove(Guid.NewGuid());
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.NoDataFound);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void RemoveDataInvalid()
        {
            EntryFolderQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");

            try
            {
                query.Remove(null);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.DataIsInvalid);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void RemoveDontOwnData()
        {
            EntryFolderQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");
            Model_EntryFolders folderOtherUser = CreateFolderForAnotherUser("TestFolder6");

            try
            {
                query.Remove(folderOtherUser.ID);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.YouDontOwnTheData);
                Assert.Equal(result.Code, ex.Code);
            }

        }

        [Fact]
        public void Remove()
        {
            EntryFolderQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");
            Model_EntryFolders folder = CreateFolderForUser("TestFolder4");
            bool passed = false;

            query.Remove(folder.ID);

            passed = true;
            Assert.True(passed);
        }

        [Fact]
        public void ListFolder()
        {
            EntryFolderQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");

            List<Model_EntryFolders> folders = query.List();

            Assert.IsType<List<Model_EntryFolders>>(folders);
        }

        private List<Model_EntryFolders> LoadFolders()
        {
            EntryFolderQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");
            return query.List();
        }

        [Fact]
        public void Reorder()
        {
            EntryFolderQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");
            List<Model_EntryFolders> folders = LoadFolders();
            List<Guid> guids = new();
            bool passed = false;

            foreach (Model_EntryFolders folder in folders)
            {
                if (folder.ID != null)
                    guids.Add(folder.ID.Value);
            }

            try
            {
                query.Reorder(guids);
            }
            catch (RequestException ex)
            {
                Debug.WriteLine(ex.Code);
            }

            passed = true;
            Assert.True(passed);
        }

        [Fact]
        public void ReorderNoDataFound()
        {
            List<Guid> guids = new();

            for (int i = 0; i < 3; i++)
            {
                guids.Add(Guid.NewGuid());
            }

            EntryFolderQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");

            try
            {
                query.Reorder(guids);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.NoDataFound);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void ReorderDataInvalid()
        {
            EntryFolderQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");
            List<Guid> emptyGuids = new();

            try
            {
                query.Reorder(emptyGuids);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.DataIsInvalid);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void ReorderDontOwnData()
        {
            EntryFolderQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");
            EntryFolderQueries queryOtherUser = ReturnQuery("6e4a61db-8c61-4594-a643-feae632caba2");
            List<Guid> guidsOtherUser = new();

            Model_EntryFolders newFolderOtheruser = CreateFolderForAnotherUser("TestFolder5");
            List<Model_EntryFolders> foldersOtherUser = queryOtherUser.List();
            foreach (Model_EntryFolders folder in foldersOtherUser)
            {
                if (folder.ID != null)
                    guidsOtherUser.Add(folder.ID.Value);
            }

            try
            {
                query.Reorder(guidsOtherUser);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.YouDontOwnTheData);
                Assert.Equal(result.Code, ex.Code);
            }
        }
    }
}
