using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Share;
using StyleWerk.NBB.Database.User;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace ApiServerTest.Tests
{
    public class ShareGroupTest
    {
        private Guid DefaultUserGuid { get; set; }
        private Guid OtherUserDefaultGuid { get; set; }

        private const string Username = "TestUser";
        private const string Email = "chloe.hauer@lbs4.salzburg.at";
        private const string Password = "TestTest@123";
        private const string GroupName = "TestGroup";

        private const string OtherUsername = "TestUser1";
        private const string OtherEmail = "florian.windisch@lbs4.salzburg.at";

        #region Update
        [Fact]
        public void CreateGroup()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Group group = Helpers.CreateGroup(GroupName, DefaultUserGuid.ToString());

            NbbContext context = NbbContext.Create();
            Share_Group? dbGroup = context.Share_Group.FirstOrDefault(g => g.ID == group.ID);
            Assert.NotNull(dbGroup);
            Assert.Equal(GroupName, dbGroup.Name);
        }

        [Fact]
        public void CreateGroup_DataInvalid()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            ShareGroupQueries query = Helpers.ReturnShareGroupQuery(DefaultUserGuid.ToString());

            Model_Group action() => query.Update(null);

            RequestException exception = Assert.Throws<RequestException>((Func<Model_Group>)action);
            RequestException result = new(ResultCodes.DataIsInvalid);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void CreateGroup_NameUniquq()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateGroup(GroupName, DefaultUserGuid.ToString());

            Model_Group action() => Helpers.CreateGroup(GroupName, DefaultUserGuid.ToString());

            RequestException exception = Assert.Throws<RequestException>((Func<Model_Group>)action);
            RequestException result = new(ResultCodes.NameMustBeUnique);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void UpdateGroup()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Group group = Helpers.CreateGroup(GroupName, DefaultUserGuid.ToString());

            ShareGroupQueries query = Helpers.ReturnShareGroupQuery(DefaultUserGuid.ToString());
            Model_Group updateGroup = new(group.ID, "DefaultGroup", 3);

            Model_Group result = query.Update(updateGroup);

            NbbContext context = NbbContext.Create();
            Share_Group? dbGroup = context.Share_Group.FirstOrDefault(g => g.ID == group.ID);
            Assert.NotEqual(group.Name, result.Name);
        }

        [Fact]
        public void UpdateGroup_DontOwnData()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);

            Model_Group otherGroup = Helpers.CreateGroup(GroupName, OtherUserDefaultGuid.ToString());

            Model_Group update = new(otherGroup.ID, "DefaultGroup", 3);

            ShareGroupQueries query = Helpers.ReturnShareGroupQuery(DefaultUserGuid.ToString());
            Model_Group action() => query.Update(update);

            RequestException exception = Assert.Throws<RequestException>((Func<Model_Group>)action);
            RequestException result = new(ResultCodes.YouDontOwnTheData);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void UpdateGroup_NameUnique()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Model_Group group = Helpers.CreateGroup(GroupName, DefaultUserGuid.ToString());
            Model_Group group2 = Helpers.CreateGroup("Test", DefaultUserGuid.ToString());

            Model_Group update = new(group2.ID, GroupName, 3);

            ShareGroupQueries query = Helpers.ReturnShareGroupQuery(DefaultUserGuid.ToString());
            Model_Group action() => query.Update(update);

            RequestException exception = Assert.Throws<RequestException>((Func<Model_Group>)action);
            RequestException result = new(ResultCodes.NameMustBeUnique);
            Assert.Equal(result.Code, exception.Code);
        }
        #endregion

        #region Remove
        [Fact]
        public void Remove()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Model_Group group = Helpers.CreateGroup(GroupName, DefaultUserGuid.ToString());
            ShareGroupQueries query = Helpers.ReturnShareGroupQuery(DefaultUserGuid.ToString());
            query.Remove(group.ID);

            NbbContext context = NbbContext.Create();
            Share_Group? dbGroup = context.Share_Group.FirstOrDefault(g => g.ID == group.ID);
            Assert.Null(dbGroup);
        }

        [Fact]
        public void Remove_DataInvalid()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            ShareGroupQueries query = Helpers.ReturnShareGroupQuery(DefaultUserGuid.ToString());

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
            ShareGroupQueries query = Helpers.ReturnShareGroupQuery(DefaultUserGuid.ToString());

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

            Helpers.CreateGroup(GroupName, DefaultUserGuid.ToString());
            Model_Group otherGroup = Helpers.CreateGroup(GroupName, OtherUserDefaultGuid.ToString());
            ShareGroupQueries query = Helpers.ReturnShareGroupQuery(DefaultUserGuid.ToString());

            try
            {
                query.Remove(otherGroup.ID);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.YouDontOwnTheData);
                Assert.Equal(result.Code, ex.Code);
            }
        }
        #endregion

        #region UpdateUser
        [Fact]
        public void UpdateUser()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);

            Model_Group group = Helpers.CreateGroup(GroupName, DefaultUserGuid.ToString());
            Helpers.AddUserToGroup(OtherUsername, group.ID, Username, DefaultUserGuid.ToString());

            NbbContext context = NbbContext.Create();
            Share_GroupUser? groupUser = context.Share_GroupUser.FirstOrDefault(g => g.GroupID == group.ID);
            Assert.NotNull(groupUser);
        }

        [Fact]
        public void UpdateUser_DataInvalid()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Model_Group group = Helpers.CreateGroup(GroupName, DefaultUserGuid.ToString());

            try
            {
                Helpers.AddUserToGroup("", group.ID, Username, DefaultUserGuid.ToString());
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.DataIsInvalid);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void UpdateUser_NoDataFound()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Helpers.CreateGroup(GroupName, DefaultUserGuid.ToString());

            try
            {
                Helpers.AddUserToGroup(OtherUsername, Guid.NewGuid(), Username, DefaultUserGuid.ToString());
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.NoDataFound);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void UpdateUser_NoUserFound()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Model_Group group = Helpers.CreateGroup(GroupName, DefaultUserGuid.ToString());

            try
            {
                Helpers.AddUserToGroup("TEst", group.ID, Username, DefaultUserGuid.ToString());
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.NoUserFound);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void UpdateUser_CantShareWithYourself()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            Model_Group group = Helpers.CreateGroup(GroupName, DefaultUserGuid.ToString());

            try
            {
                Helpers.AddUserToGroup(Username, group.ID, Username, DefaultUserGuid.ToString());
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.CantShareWithYourself);
                Assert.Equal(result.Code, ex.Code);
            }
        }
        #endregion

        #region RemoveUser
        [Fact]
        public void RemoveUser()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);

            Model_Group group = Helpers.CreateGroup(GroupName, DefaultUserGuid.ToString());
            Helpers.AddUserToGroup(OtherUsername, group.ID, Username, DefaultUserGuid.ToString());
            ShareGroupQueries query = Helpers.ReturnShareGroupQuery(DefaultUserGuid.ToString());

            query.RemoveUser(OtherUsername, group.ID);

            NbbContext context = NbbContext.Create();
            Share_GroupUser? dbUser = context.Share_GroupUser.FirstOrDefault(g => g.GroupID == group.ID && g.UserID == OtherUserDefaultGuid);
            Assert.Null(dbUser);
        }

        [Fact]
        public void RemoveUser_DataInvalid()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);

            Model_Group group = Helpers.CreateGroup(GroupName, DefaultUserGuid.ToString());
            Helpers.AddUserToGroup(OtherUsername, group.ID, Username, DefaultUserGuid.ToString());
            ShareGroupQueries query = Helpers.ReturnShareGroupQuery(DefaultUserGuid.ToString());

            try
            {
                query.RemoveUser(OtherUsername, group.ID);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.DataIsInvalid);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void RemoveUser_NoDataFound()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);

            ShareGroupQueries query = Helpers.ReturnShareGroupQuery(DefaultUserGuid.ToString());

            try
            {
                query.RemoveUser(OtherUsername, Guid.NewGuid());
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.NoDataFound);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void RemoveUser_NoUserFound()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);

            Model_Group group = Helpers.CreateGroup(GroupName, DefaultUserGuid.ToString());
            Helpers.AddUserToGroup(OtherUsername, group.ID, Username, DefaultUserGuid.ToString());

            NbbContext context = NbbContext.Create();
            List<User_Login> usersLogin = [.. context.User_Login];
            List<User_Information> userInformations = [.. context.User_Information];
            if (usersLogin.Count > 0)
                context.User_Login.RemoveRange(usersLogin);

            if (userInformations.Count > 0)
                context.User_Information.RemoveRange(userInformations);
            ShareGroupQueries query = Helpers.ReturnShareGroupQuery(DefaultUserGuid.ToString());

            try
            {
                query.RemoveUser(OtherUsername, group.ID);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.NoUserFound);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void RemoveUser_NoDataFound_Item()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);

            Model_Group group = Helpers.CreateGroup(GroupName, DefaultUserGuid.ToString());
            ShareGroupQueries query = Helpers.ReturnShareGroupQuery(DefaultUserGuid.ToString());

            try
            {
                query.RemoveUser(OtherUsername, group.ID);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.NoDataFound);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void RemoveUser_DontOwnData()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);

            Model_Group group = Helpers.CreateGroup(GroupName, DefaultUserGuid.ToString());
            Helpers.AddUserToGroup(OtherUsername, group.ID, Username, DefaultUserGuid.ToString());

            ShareGroupQueries query = Helpers.ReturnShareGroupQuery(OtherUserDefaultGuid.ToString());

            try
            {
                query.RemoveUser(OtherUsername, group.ID);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.YouDontOwnTheData);
                Assert.Equal(result.Code, ex.Code);
            }
        }
        #endregion

        #region GetSharedToGroup

        [Fact]
        public void GetSharedToGroup()
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
        public void GetSharedToGroup_DataInvalid()
        {
            ShareGroupQueries query = Helpers.ReturnShareGroupQuery(DefaultUserGuid.ToString());
            List<Model_SharedToGroup> action() => query.GetSharedToGroup(Guid.Empty);

            RequestException exception = Assert.Throws<RequestException>((Func<List<Model_SharedToGroup>>)action);
            RequestException result = new(ResultCodes.DataIsInvalid);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void GetSharedToGroup_NoDataFound()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);

            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, "Test", template);
            Model_Group group = Helpers.CreateGroup(GroupName, DefaultUserGuid.ToString());
            Helpers.ShareWithGroup(entry.ID, group.ID, DefaultUserGuid, OtherUsername, ShareType.Entry);

            NbbContext context = NbbContext.Create();
            Share_Group? dbGroup = context.Share_Group.FirstOrDefault(g => g.ID == group.ID);
            context.Share_Group.Remove(dbGroup);
            context.SaveChanges();

            ShareGroupQueries query = Helpers.ReturnShareGroupQuery(DefaultUserGuid.ToString());
            List<Model_SharedToGroup> action() => query.GetSharedToGroup(group.ID);

            RequestException exception = Assert.Throws<RequestException>((Func<List<Model_SharedToGroup>>)action);
            RequestException result = new(ResultCodes.NoDataFound);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void GetSharedToGroup_DataInvalid_Name()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);

            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Model_Editor entry = Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, "Test", template);
            Model_Group group = Helpers.CreateGroup(GroupName, DefaultUserGuid.ToString());
            Helpers.ShareWithGroup(entry.ID, group.ID, DefaultUserGuid, OtherUsername, ShareType.Entry);

            NbbContext context = NbbContext.Create();
            StyleWerk.NBB.Database.Structure.Structure_Entry? dbEntry = context.Structure_Entry.FirstOrDefault(e => e.ID == entry.ID);
            context.Structure_Entry.Remove(dbEntry);
            context.SaveChanges();

            ShareGroupQueries query = Helpers.ReturnShareGroupQuery(DefaultUserGuid.ToString());
            List<Model_SharedToGroup> action() => query.GetSharedToGroup(group.ID);

            RequestException exception = Assert.Throws<RequestException>((Func<List<Model_SharedToGroup>>)action);
            RequestException result = new(ResultCodes.DataIsInvalid);
            Assert.Equal(result.Code, exception.Code);
        }
        #endregion

        #region Details
        [Fact]
        public void Details()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);

            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);
            Helpers.CreateEntry(DefaultUserGuid.ToString(), "TestEntry", null, "Test", template);
            Model_Group group = Helpers.CreateGroup(GroupName, DefaultUserGuid.ToString());
            Helpers.AddUserToGroup(OtherUsername, group.ID, Username, DefaultUserGuid.ToString());

            ShareGroupQueries query = Helpers.ReturnShareGroupQuery(DefaultUserGuid.ToString());
            List<string> details = query.Details(group.ID);
            Assert.NotEmpty(details);
        }

        [Fact]
        public void Details_DataInvalid()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            ShareGroupQueries query = Helpers.ReturnShareGroupQuery(DefaultUserGuid.ToString());
            List<Model_SharedToGroup> action() => query.GetSharedToGroup(Guid.Empty);

            RequestException exception = Assert.Throws<RequestException>((Func<List<Model_SharedToGroup>>)action);
            RequestException result = new(ResultCodes.DataIsInvalid);
            Assert.Equal(result.Code, exception.Code);
        }
        #endregion


        #region List

        [Fact]
        public void List_Empty()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            ShareGroupQueries query = Helpers.ReturnShareGroupQuery(DefaultUserGuid.ToString());
            List<Model_Group> groups = query.List();
            Assert.Empty(groups);
        }

        [Fact]
        public void List_Groups()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            Helpers.CreateGroup(GroupName, DefaultUserGuid.ToString());
            Helpers.CreateGroup("Default", DefaultUserGuid.ToString());

            ShareGroupQueries query = Helpers.ReturnShareGroupQuery(DefaultUserGuid.ToString());
            List<Model_Group> groups = query.List();
            Assert.NotEmpty(groups);
        }

        [Fact]
        public void List_UserGroup()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);
            Model_Group group = Helpers.CreateGroup(GroupName, DefaultUserGuid.ToString());
            Helpers.AddUserToGroup(OtherUsername, group.ID, Username, DefaultUserGuid.ToString());

            ShareGroupQueries query = Helpers.ReturnShareGroupQuery(OtherUserDefaultGuid.ToString());
            List<Model_Group> groups = query.List();
            Assert.Empty(groups);
        }
        #endregion
    }
}
