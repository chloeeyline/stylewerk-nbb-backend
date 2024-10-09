using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace ApiServerTest.Tests
{
    public class ShareTest
    {
        private Guid DefaultUserGuid { get; set; }
        private Guid OtherUserDefaultGuid { get; set; }

        private const string Username = "TestUser";
        private const string Email = "chloe.hauer@lbs4.salzburg.at";
        private const string Password = "TestTest@123";

        private const string OtherUsername = "TestUser1";
        private const string OtherEmail = "florian.windisch@lbs4.salzburg.at";

        private const string GroupName = "TestGroup";

        [Fact]
        public void Share_DataInvalid()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            ShareQueries query = Helpers.ReturnShareQuery(DefaultUserGuid.ToString());
            Model_ShareItem item = new(Guid.Empty, Guid.Empty, string.Empty, ShareVisibility.None, ShareType.Template, string.Empty);
            item = null;

            try
            {
                query.Update(item);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.DataIsInvalid);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void Share_DontOwnData()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);

            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, "Test", template);

            ShareQueries query = Helpers.ReturnShareQuery(OtherUserDefaultGuid.ToString());
            Model_ShareItem item = new(Guid.Empty, entry.ID, OtherUsername, ShareVisibility.None, ShareType.Entry, DefaultUserGuid.ToString());

            try
            {
                query.Update(item);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.YouDontOwnTheData);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        #region Share Group
        [Fact]
        public void Share_Group()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);

            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, "Test", template);
            Model_Group group = Helpers.CreateGroup(GroupName, DefaultUserGuid.ToString());
            Helpers.ShareWithGroup(entry.ID, group.ID, DefaultUserGuid, OtherUsername, ShareType.Entry);

            ShareGroupQueries query = Helpers.ReturnShareGroupQuery(DefaultUserGuid.ToString());
            List<Model_SharedToGroup> items = query.GetSharedToGroup(group.ID);

            Assert.NotEmpty(items);
        }

        [Fact]
        public void Share_Group_NoDataFound()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);

            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, "Test", template);
            try
            {
                Helpers.ShareWithGroup(entry.ID, Guid.NewGuid(), DefaultUserGuid, OtherUsername, ShareType.Entry);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.NoDataFound);
                Assert.Equal(result.Code, ex.Code);
            }
        }
        #endregion

        #region Share Direct
        [Fact]
        public void Share_Direct()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);

            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_ShareItem item = new(Guid.Empty, template.TemplateID, Username, ShareVisibility.Directly, ShareType.Template, OtherUsername);

            ShareQueries query = Helpers.ReturnShareQuery(DefaultUserGuid.ToString());
            query.Update(item);

            NbbContext context = NbbContext.Create();
            Share_Item? dbItem = context.Share_Item.FirstOrDefault(i => i.ItemID == template.TemplateID);
            Assert.NotNull(dbItem);
        }

        [Fact]
        public void Share_Direct_NoDataFound()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_ShareItem item = new(Guid.Empty, template.TemplateID, Username, ShareVisibility.Directly, ShareType.Template, "Test");

            try
            {
                ShareQueries query = Helpers.ReturnShareQuery(DefaultUserGuid.ToString());
                query.Update(item);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.NoDataFound);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void Share_Direct_CantShareYourself()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_ShareItem item = new(Guid.Empty, template.TemplateID, Username, ShareVisibility.Directly, ShareType.Template, Username);

            try
            {
                ShareQueries query = Helpers.ReturnShareQuery(DefaultUserGuid.ToString());
                query.Update(item);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.CantShareWithYourself);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void Share_NoDirect_NoGroup()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_ShareItem item = new(Guid.Empty, template.TemplateID, Username, ShareVisibility.None, ShareType.Template, Username);

            try
            {
                ShareQueries query = Helpers.ReturnShareQuery(DefaultUserGuid.ToString());
                query.Update(item);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.DataIsInvalid);
                Assert.Equal(result.Code, ex.Code);
            }
        }
        #endregion

        #region Share Public
        [Fact]
        public void Share_Public()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_ShareItem item = new(Guid.Empty, template.TemplateID, Username, ShareVisibility.Public, ShareType.Template, null);

            ShareQueries query = Helpers.ReturnShareQuery(DefaultUserGuid.ToString());
            query.Update(item);

            NbbContext context = NbbContext.Create();
            Share_Item? dbItem = context.Share_Item.FirstOrDefault(i => i.ItemID == template.TemplateID);
            Assert.NotNull(dbItem);
        }
        #endregion

        #region Remove
        [Fact]
        public void Remove()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);

            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_ShareItem item = new(Guid.Empty, template.TemplateID, Username, ShareVisibility.Directly, ShareType.Template, OtherUsername);

            ShareQueries query = Helpers.ReturnShareQuery(DefaultUserGuid.ToString());
            query.Update(item);

            NbbContext context = NbbContext.Create();
            Share_Item? dbItem = context.Share_Item.FirstOrDefault(i => i.ItemID == template.TemplateID);

            query.Remove(dbItem.ID);

            context = NbbContext.Create();
            Share_Item? rItem = context.Share_Item.FirstOrDefault(i => i.ItemID == template.TemplateID);
            Assert.Null(rItem);
        }

        [Fact]
        public void Remove_DataInvalid()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            ShareQueries query = Helpers.ReturnShareQuery(DefaultUserGuid.ToString());

            try
            {
                query.Remove(Guid.Empty);
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
            ShareQueries query = Helpers.ReturnShareQuery(DefaultUserGuid.ToString());

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
        public void Remove_DontOwnData()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);

            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_ShareItem item = new(Guid.Empty, template.TemplateID, Username, ShareVisibility.Directly, ShareType.Template, OtherUsername);

            ShareQueries query = Helpers.ReturnShareQuery(DefaultUserGuid.ToString());
            query.Update(item);

            NbbContext context = NbbContext.Create();
            Share_Item? dbItem = context.Share_Item.FirstOrDefault(i => i.ItemID == template.TemplateID);
            ShareQueries query2 = Helpers.ReturnShareQuery(OtherUserDefaultGuid.ToString());

            try
            {
                query2.Remove(dbItem.ID);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.YouDontOwnTheData);
                Assert.Equal(result.Code, ex.Code);
            }
        }
        #endregion

        #region List
        [Fact]
        public void List()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);

            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_ShareItem item = new(Guid.Empty, template.TemplateID, Username, ShareVisibility.Directly, ShareType.Template, OtherUsername);

            ShareQueries query = Helpers.ReturnShareQuery(DefaultUserGuid.ToString());
            query.Update(item);

            List<Model_ShareItem> result = query.List(template.TemplateID, ShareType.Template);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void List_DataInvalid()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            ShareQueries query = Helpers.ReturnShareQuery(DefaultUserGuid.ToString());

            List<Model_ShareItem> action() => query.List(Guid.Empty, ShareType.Template);

            RequestException exception = Assert.Throws<RequestException>((Func<List<Model_ShareItem>>)action);
            RequestException result = new(ResultCodes.DataIsInvalid);
            Assert.Equal(result.Code, exception.Code);
        }

        #endregion

    }
}
