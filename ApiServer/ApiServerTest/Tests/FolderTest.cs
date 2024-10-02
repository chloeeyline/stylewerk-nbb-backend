using StyleWerk.NBB.Database;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace ApiServerTest.Tests
{
    public class FolderTest
    {

        private static EntryFolderQueries ReturnQuery(string userGuid)
        {
            NbbContext DB = NbbContext.Create();
            ApplicationUser user = DB.GetUser(new Guid(userGuid));
            EntryFolderQueries query = new(DB, user);
            return query;
        }

        [Fact]
        public void AddFolder()
        {
            EntryFolderQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");
            Model_EntryFolders newFolder = new(null, $"TestFolder1", []);

            Model_EntryFolders response = query.Update(newFolder);

            Assert.IsType<Model_EntryFolders>(response);
        }

        private static Model_EntryFolders CreateFolderForAnotherUser(string folderName)
        {
            EntryFolderQueries query = ReturnQuery("6e4a61db-8c61-4594-a643-feae632caba2");

            Model_EntryFolders newFolderOtherUser = new(null, folderName, []);
            Model_EntryFolders responseOtherUser = query.Update(newFolderOtherUser);

            return responseOtherUser;
        }

        [Fact]
        public void MissingRight()
        {
            Model_EntryFolders responseOtherUser = CreateFolderForAnotherUser("TestFolder2");
            EntryFolderQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");
            Model_EntryFolders folder = new(responseOtherUser.ID, "TestFolder3", []);

            Model_EntryFolders action() => query.Update(folder);

            RequestException exception = Assert.Throws<RequestException>((Func<Model_EntryFolders>)action);
            RequestException result = new(ResultCodes.MissingRight);
            Assert.Equal(result.Code, exception.Code);
        }

        private static Model_EntryFolders CreateFolderForUser(string folderName)
        {
            EntryFolderQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");

            Model_EntryFolders newFolderUser = new(null, folderName, []);
            Model_EntryFolders responseUser = query.Update(newFolderUser);

            return responseUser;
        }

        [Fact]
        public void FolderNameExists()
        {
            Model_EntryFolders folderUser = CreateFolderForUser("TestFolder3");
            EntryFolderQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");
            Model_EntryFolders folder = new(folderUser.ID, "TestFolder1", []);

            Model_EntryFolders action() => query.Update(folder);

            RequestException exception = Assert.Throws<RequestException>((Func<Model_EntryFolders>)action);
            RequestException result = new(ResultCodes.NameMustBeUnique);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void DataInvalid()
        {
            EntryFolderQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");
            Model_EntryFolders newFolder = new(null, null, []);

            Model_EntryFolders action() => query.Update(newFolder);

            RequestException exception = Assert.Throws<RequestException>((Func<Model_EntryFolders>)action);
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

            query.Remove(folder.ID);

            Assert.True(true);
        }

        [Fact]
        public void ListFolder()
        {
            EntryFolderQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");

            List<Model_EntryFolders> folders = query.List();

            Assert.IsType<List<Model_EntryFolders>>(folders);
        }

        private static List<Model_EntryFolders> LoadFolders()
        {
            EntryFolderQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");
            return query.List();
        }

        [Fact]
        public void Reorder()
        {
            EntryFolderQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");
            List<Model_EntryFolders> folders = LoadFolders();
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
        public void ReorderNoDataFound()
        {
            List<Guid> guids = [];

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
        public void ReorderDontOwnData()
        {
            EntryFolderQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");
            EntryFolderQueries queryOtherUser = ReturnQuery("6e4a61db-8c61-4594-a643-feae632caba2");
            List<Guid> guidsOtherUser = [];

            CreateFolderForAnotherUser("TestFolder5");
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
