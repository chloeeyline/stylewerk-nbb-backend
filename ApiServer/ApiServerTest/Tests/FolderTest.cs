using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace ApiServerTest.Tests
{
    public class FolderTest
    {

        private Guid DefaultUserGuid { get; set; }
        private Guid OtherUserDefaultGuid { get; set; }

        private readonly string Username = "TestUser";
        private readonly string Email = "chloe.hauer@lbs4.salzburg.at";
        private readonly string Password = "TestTest@123";

        private readonly string OtherUsername = "TestUser1";
        private readonly string OtherEmail = "juliane.krenek@lbs4.salzburg.at";

        #region Helpers

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
            Model_EntryFolders newFolder = new(Guid.Empty, $"TestFolder", []);

            Model_EntryFolders response = query.Update(newFolder);

            Assert.IsType<Model_EntryFolders>(response);
        }

        [Fact]
        public void Update_MissingRight()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);

            Model_EntryFolders responseOtherUser = Helpers.CreateFolder(OtherUserDefaultGuid.ToString(), "TestFolder");
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

            Model_EntryFolders folderUser = Helpers.CreateFolder(DefaultUserGuid.ToString(), "TestFolder");
            Model_EntryFolders folder2 = Helpers.CreateFolder(DefaultUserGuid.ToString(), "TestFolder1");
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
            Model_EntryFolders newFolder = new(Guid.Empty, null, []);

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

            Model_EntryFolders folderOtherUser = Helpers.CreateFolder(OtherUserDefaultGuid.ToString(), "TestFolder");

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
            Model_EntryFolders folder = Helpers.CreateFolder(DefaultUserGuid.ToString(), "TestFolder");

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
            Helpers.CreateFolder(DefaultUserGuid.ToString(), "TestFolder");
            Helpers.CreateFolder(DefaultUserGuid.ToString(), "TestFolder2");
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
            Helpers.CreateFolder(DefaultUserGuid.ToString(), "TestFolder");
            Helpers.CreateFolder(DefaultUserGuid.ToString(), "TestFolder1");
            Helpers.CreateFolder(DefaultUserGuid.ToString(), "TestFolder2");
            List<Model_EntryFolders> folders = LoadFolders(DefaultUserGuid.ToString());
            List<Guid> guids = [];

            foreach (Model_EntryFolders folder in folders)
            {
                if (folder.ID != Guid.Empty)
                    guids.Add(folder.ID);
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

            Helpers.CreateFolder(OtherUserDefaultGuid.ToString(), "TestFolder");
            List<Model_EntryFolders> foldersOtherUser = queryOtherUser.List();
            foreach (Model_EntryFolders folder in foldersOtherUser)
            {
                if (folder.ID != Guid.Empty)
                    guidsOtherUser.Add(folder.ID);
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
