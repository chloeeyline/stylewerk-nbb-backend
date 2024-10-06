using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace ApiServerTest.Tests
{
    public class FolderTest
    {

        private Guid DefaultUserGuid { get; set; }
        private Guid OtherUserDefaultGuid { get; set; }

        private string Username = "TestUser";
        private string Email = "chloe.hauer@lbs4.salzburg.at";
        private string Password = "TestTest@123";

        private string OtherUsername = "TestUser1";
        private string OtherEmail = "juliane.krenek@lbs4.salzburg.at";

        #region Helpers

        private static Model_EntryFolders CreateFolder(string folderName, string user)
        {
            Model_EntryFolders newFolderOtherUser = new(Guid.NewGuid(), folderName, []);

            EntryFolderQueries query = Helpers.ReturnFolderQuery(user);
            Model_EntryFolders responseOtherUser = query.Update(newFolderOtherUser);

            return responseOtherUser;
        }

        private static List<Model_EntryFolders> LoadFolders(string user)
        {
            EntryFolderQueries query = Helpers.ReturnFolderQuery(user);
            return query.List();
        }
        #endregion

        #region Update Folder Function
        [Fact]
        public void Update_CreateFolder()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            EntryFolderQueries query = Helpers.ReturnFolderQuery(DefaultUserGuid.ToString());
            Model_EntryFolders newFolder = new(null, $"TestFolder", []);

            Model_EntryFolders response = query.Update(newFolder);

            Assert.IsType<Model_EntryFolders>(response);
        }

        [Fact]
        public void Update_MissingRight()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);

            Model_EntryFolders responseOtherUser = CreateFolder("TestFolder", OtherUserDefaultGuid.ToString());
            EntryFolderQueries query = Helpers.ReturnFolderQuery(DefaultUserGuid.ToString());
            Model_EntryFolders folder = new(responseOtherUser.ID, "TestFolder1", []);

            Model_EntryFolders action() => query.Update(folder);

            RequestException exception = Assert.Throws<RequestException>((Func<Model_EntryFolders>)action);
            RequestException result = new(ResultCodes.MissingRight);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void Update_FolderNameExists()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Model_EntryFolders folderUser = CreateFolder("TestFolder", DefaultUserGuid.ToString());
            Model_EntryFolders folder2 = CreateFolder("TestFolder1", DefaultUserGuid.ToString());
            EntryFolderQueries query = Helpers.ReturnFolderQuery(DefaultUserGuid.ToString());
            Model_EntryFolders folder = new(folderUser.ID, "TestFolder1", []);

            Model_EntryFolders action() => query.Update(folder);

            RequestException exception = Assert.Throws<RequestException>((Func<Model_EntryFolders>)action);
            RequestException result = new(ResultCodes.NameMustBeUnique);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void Update_DataInvalid()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            EntryFolderQueries query = Helpers.ReturnFolderQuery(DefaultUserGuid.ToString());
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
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            EntryFolderQueries query = Helpers.ReturnFolderQuery(DefaultUserGuid.ToString());

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
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            EntryFolderQueries query = Helpers.ReturnFolderQuery(DefaultUserGuid.ToString());

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
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);

            Model_EntryFolders folderOtherUser = CreateFolder("TestFolder", OtherUserDefaultGuid.ToString());

            try
            {
                EntryFolderQueries query = Helpers.ReturnFolderQuery(DefaultUserGuid.ToString());
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
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            EntryFolderQueries query = Helpers.ReturnFolderQuery(DefaultUserGuid.ToString());
            Model_EntryFolders folder = CreateFolder("TestFolder", DefaultUserGuid.ToString());

            query.Remove(folder.ID);

            Assert.True(true);
        }

        #endregion

        #region ListFolder Function

        [Fact]
        public void ListFolder()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            EntryFolderQueries query = Helpers.ReturnFolderQuery(DefaultUserGuid.ToString());
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
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            EntryFolderQueries query = Helpers.ReturnFolderQuery(DefaultUserGuid.ToString());
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
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            List<Guid> guids = [];

            for (int i = 0; i < 3; i++)
            {
                guids.Add(Guid.NewGuid());
            }

            EntryFolderQueries query = Helpers.ReturnFolderQuery(DefaultUserGuid.ToString());

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
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            EntryFolderQueries query = Helpers.ReturnFolderQuery(DefaultUserGuid.ToString());
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
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);

            EntryFolderQueries query = Helpers.ReturnFolderQuery(DefaultUserGuid.ToString());
            EntryFolderQueries queryOtherUser = Helpers.ReturnFolderQuery(OtherUserDefaultGuid.ToString());
            List<Guid> guidsOtherUser = [];

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
