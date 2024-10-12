using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace ApiServerTest.Tests
{
    [Collection("Sequential")]

    public class TemplateTest
    {
        private Guid DefaultUserGuid { get; set; }
        private Guid OtherUserDefaultGuid { get; set; }

        private readonly string Username = "TestUser";
        private readonly string Email = "chloe.hauer@lbs4.salzburg.at";
        private readonly string Password = "TestTest@123";

        private readonly string OtherUsername = "TestUser1";
        private readonly string OtherEmail = "florian.windisch@lbs4.salzburg.at";

        #region Remove Function
        [Fact]
        public void Remove()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_Editor template = Helpers.CreateTemplate("TestTemplate", DefaultUserGuid.ToString(), null);

            query.Remove(template.TemplateID);

            Assert.True(true);
        }

        [Fact]
        public void Remove_DataInvalid()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());

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

            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
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

            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_Editor templateOtherUser = Helpers.CreateTemplate("TestTemplate", OtherUserDefaultGuid.ToString(), null);

            try
            {
                query.Remove(templateOtherUser.TemplateID);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.YouDontOwnTheData);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        #endregion

        #region Copy Function
        [Fact]
        public void Copy()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);

            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_Editor templateOtherUser = Helpers.CreateTemplate("TestTemplate", OtherUserDefaultGuid.ToString(), null);

            query.Copy(templateOtherUser.TemplateID);

            Structure_Template? template = Helpers.DB.Structure_Template.FirstOrDefault(t => t.UserID == DefaultUserGuid && t.Name == "TestTemplate (Kopie)");
            Assert.NotNull(template);
        }

        [Fact]
        public void Copy_DataInvalid()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            try
            {
                query.Copy(null);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.DataIsInvalid);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void Copy_NoDataFound()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            try
            {
                query.Copy(Guid.NewGuid());
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.NoDataFound);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        #endregion
    }
}
