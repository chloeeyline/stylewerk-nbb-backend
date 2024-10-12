using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace ApiServerTest.Tests
{
    [Collection("Sequential")]
    public class EntryTest
    {
        private Guid DefaultUserGuid { get; set; }
        private Guid OtherUserDefaultGuid { get; set; }

        private const string Username = "TestUser";
        private const string Email = "chloe.hauer@lbs4.salzburg.at";
        private const string Password = "TestTest@123";

        #region Remove Function

        [Fact]
        public void Remove()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, "Test", template);

            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            query.Remove(entry.ID);

            Structure_Entry? dbEntry = Helpers.DB.Structure_Entry.FirstOrDefault(e => e.ID == entry.ID);
            Assert.Null(dbEntry);
        }

        [Fact]
        public void Remove_DataInvalid()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());

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
        public void Remove_NoDataFound()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            EntryQueries query = Helpers.ReturnEntryQuery(DefaultUserGuid.ToString());
            Guid entryId = Guid.NewGuid();

            try
            {
                query.Remove(entryId);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.NoDataFound);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void Remove_YouDontOwnData()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, "Test", template);
            EntryQueries query = Helpers.ReturnEntryQuery(OtherUserDefaultGuid.ToString());

            try
            {
                query.Remove(entry.ID);
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
