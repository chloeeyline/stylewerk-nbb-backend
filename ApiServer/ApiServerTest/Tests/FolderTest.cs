using StyleWerk.NBB.Database;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace ApiServerTest.Tests
{
    public class FolderTest
    {

        private Guid DefaultUserGuid = new("90865032-e4e8-4e2b-85e0-5db345f42a5b");
        private Guid OtherUserDefaultGuid = new("cd6c092d-0546-4f8b-b70c-352d2ca765a4");

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
            EntryFolderQueries query = ReturnQuery(user);

            Model_EntryFolders newFolderOtherUser = new(null, folderName, []);
            Model_EntryFolders responseOtherUser = query.Update(newFolderOtherUser);

            return responseOtherUser;
        }

        private static List<Model_EntryFolders> LoadFolders(string user)
        {
            EntryFolderQueries query = ReturnQuery(user);
            return query.List();
        }
        #endregion

        #region Update Folder Function
        [Fact]
        public void Update_CreateFolder()
        {
            EntryFolderQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_EntryFolders newFolder = new(null, $"TestFolder1", []);

            Model_EntryFolders response = query.Update(newFolder);

            Assert.IsType<Model_EntryFolders>(response);
        }

        [Fact]
        public void Update_MissingRight()
        {
            Model_EntryFolders responseOtherUser = CreateFolder("TestFolder2", OtherUserDefaultGuid.ToString());
            EntryFolderQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_EntryFolders folder = new(responseOtherUser.ID, "TestFolder3", []);

            Model_EntryFolders action() => query.Update(folder);

            RequestException exception = Assert.Throws<RequestException>((Func<Model_EntryFolders>)action);
            RequestException result = new(ResultCodes.MissingRight);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void Update_FolderNameExists()
        {
            Model_EntryFolders folderUser = CreateFolder("TestFolder3", DefaultUserGuid.ToString());
            EntryFolderQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");
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
            EntryFolderQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_EntryFolders folderOtherUser = CreateFolder("TestFolder6", OtherUserDefaultGuid.ToString());

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
            EntryFolderQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_EntryFolders folder = CreateFolder("TestFolder4", DefaultUserGuid.ToString());

            query.Remove(folder.ID);

            Assert.True(true);
        }

        #endregion

        #region ListFolder Function

        [Fact]
        public void ListFolder()
        {
            EntryFolderQueries query = ReturnQuery(DefaultUserGuid.ToString());
            CreateFolder("TestFolder5", DefaultUserGuid.ToString());
            CreateFolder("TestFolder6", DefaultUserGuid.ToString());
            List<Model_EntryFolders> folders = query.List();

            Assert.IsType<List<Model_EntryFolders>>(folders);
        }
        #endregion

        #region Reorder Function
        [Fact]
        public void Reorder()
        {
            EntryFolderQueries query = ReturnQuery(DefaultUserGuid.ToString());
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

            CreateFolder("TestFolder5", OtherUserDefaultGuid.ToString());
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
