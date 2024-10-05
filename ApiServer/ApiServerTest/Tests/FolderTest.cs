using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace ApiServerTest.Tests
{
    public class FolderTest
    {

        private Guid DefaultUserGuid = new("90865032-e4e8-4e2b-85e0-5db345f42a5b");
        private Guid OtherUserDefaultGuid = new("6e4a61db-8c61-4594-a643-feae632caba2");

        #region Helpers
        private static EntryFolderQueries ReturnQuery(string userGuid)
        {
            NbbContext DB = NbbContext.Create();
            ApplicationUser user = DB.GetUser(new Guid(userGuid));
            EntryFolderQueries query = new(DB, user);
            return query;
        }

        private static Model_EntryFolders CreateFolder(string folderName, string user)
        {
            Model_EntryFolders newFolderOtherUser = new(Guid.NewGuid(), folderName, []);

            EntryFolderQueries query = ReturnQuery(user);
            Model_EntryFolders responseOtherUser = query.Update(newFolderOtherUser);

            return responseOtherUser;
        }

        private static List<Model_EntryFolders> LoadFolders(string user)
        {
            EntryFolderQueries query = ReturnQuery(user);
            return query.List();
        }

        private void DeleteAll()
        {
            NbbContext context = NbbContext.Create();
            List<Structure_Entry_Folder> folders = [.. context.Structure_Entry_Folder];
            if (folders.Count > 0)
                context.Structure_Entry_Folder.RemoveRange(folders);

            context.SaveChanges();
        }
        #endregion

        #region Update Folder Function
        [Fact]
        public void Update_CreateFolder()
        {
            DeleteAll();
            EntryFolderQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_EntryFolders newFolder = new(null, $"TestFolder", []);

            Model_EntryFolders response = query.Update(newFolder);

            Assert.IsType<Model_EntryFolders>(response);
        }

        [Fact]
        public void Update_MissingRight()
        {
            DeleteAll();
            Model_EntryFolders responseOtherUser = CreateFolder("TestFolder", OtherUserDefaultGuid.ToString());
            EntryFolderQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_EntryFolders folder = new(responseOtherUser.ID, "TestFolder1", []);

            Model_EntryFolders action() => query.Update(folder);

            RequestException exception = Assert.Throws<RequestException>((Func<Model_EntryFolders>)action);
            RequestException result = new(ResultCodes.MissingRight);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void Update_FolderNameExists()
        {
            DeleteAll();
            Model_EntryFolders folderUser = CreateFolder("TestFolder", DefaultUserGuid.ToString());
            Model_EntryFolders folder2 = CreateFolder("TestFolder1", DefaultUserGuid.ToString());
            EntryFolderQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_EntryFolders folder = new(folderUser.ID, "TestFolder1", []);

            Model_EntryFolders action() => query.Update(folder);

            RequestException exception = Assert.Throws<RequestException>((Func<Model_EntryFolders>)action);
            RequestException result = new(ResultCodes.NameMustBeUnique);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void Update_DataInvalid()
        {
            EntryFolderQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_EntryFolders newFolder = new(null, null, []);

            Model_EntryFolders action() => query.Update(newFolder);

            RequestException exception = Assert.Throws<RequestException>((Func<Model_EntryFolders>)action);
            RequestException result = new(ResultCodes.DataIsInvalid);
            Assert.Equal(result.Code, exception.Code);
        }
        #endregion

        #region Remove Function
        [Fact]
        public void Remove_DataNoFound()
        {
            EntryFolderQueries query = ReturnQuery(DefaultUserGuid.ToString());

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
        public void Remove_DataInvalid()
        {
            EntryFolderQueries query = ReturnQuery(DefaultUserGuid.ToString());

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
        public void Remove_DontOwnData()
        {
            DeleteAll();
            Model_EntryFolders folderOtherUser = CreateFolder("TestFolder", OtherUserDefaultGuid.ToString());

            try
            {
                EntryFolderQueries query = ReturnQuery(DefaultUserGuid.ToString());
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
            DeleteAll();
            EntryFolderQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_EntryFolders folder = CreateFolder("TestFolder", DefaultUserGuid.ToString());

            query.Remove(folder.ID);

            Assert.True(true);
        }

        #endregion

        #region ListFolder Function

        [Fact]
        public void ListFolder()
        {
            DeleteAll();
            EntryFolderQueries query = ReturnQuery(DefaultUserGuid.ToString());
            CreateFolder("TestFolder", DefaultUserGuid.ToString());
            CreateFolder("TestFolder1", DefaultUserGuid.ToString());
            List<Model_EntryFolders> folders = query.List();

            Assert.IsType<List<Model_EntryFolders>>(folders);
        }
        #endregion

        #region Reorder Function
        [Fact]
        public void Reorder()
        {
            EntryFolderQueries query = ReturnQuery(DefaultUserGuid.ToString());
            DeleteAll();
            CreateFolder("TestFolder", DefaultUserGuid.ToString());
            CreateFolder("TestFolder1", DefaultUserGuid.ToString());
            CreateFolder("TestFolder2", DefaultUserGuid.ToString());
            List<Model_EntryFolders> folders = LoadFolders(DefaultUserGuid.ToString());
            List<Guid> guids = [];

            foreach (Model_EntryFolders folder in folders)
            {
                if (folder.ID != null)
                    guids.Add(folder.ID.Value);
            }

            query.Reorder(guids);

            Assert.True(true);
        }

        [Fact]
        public void Reorder_NoDataFound()
        {
            List<Guid> guids = [];

            for (int i = 0; i < 3; i++)
            {
                guids.Add(Guid.NewGuid());
            }

            EntryFolderQueries query = ReturnQuery(DefaultUserGuid.ToString());

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
        public void Reorder_DataInvalid()
        {
            EntryFolderQueries query = ReturnQuery(DefaultUserGuid.ToString());
            List<Guid> emptyGuids = [];

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
        public void Reorder_DontOwnData()
        {
            EntryFolderQueries query = ReturnQuery(DefaultUserGuid.ToString());
            EntryFolderQueries queryOtherUser = ReturnQuery(OtherUserDefaultGuid.ToString());
            List<Guid> guidsOtherUser = [];
            DeleteAll();
            CreateFolder("TestFolder", OtherUserDefaultGuid.ToString());
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
        #endregion
    }
}
