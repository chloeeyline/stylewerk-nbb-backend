using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace ApiServerTest.Tests
{
    public class TemplateTest
    {
        private Guid DefaultUserGuid { get; set; }
        private Guid OtherUserDefaultGuid { get; set; }

        private string Username = "TestUser";
        private string Email = "chloe.hauer@lbs4.salzburg.at";
        private string Password = "TestTest@123";

        private string OtherUsername = "TestUser1";
        private string OtherEmail = "florian.windisch@lbs4.salzburg.at";

        #region Helpers


        private static Model_Template CreateTemplate(string templateName, string user)
        {
            TemplateQueries query = Helpers.ReturnTemplateQuery(user);
            Guid rowId = Guid.NewGuid();
            List<Model_TemplateCell> cells =
            [
                new Model_TemplateCell(Guid.NewGuid(), 1, false, false, "Test", "Test"),
                new Model_TemplateCell(Guid.NewGuid(), 1, false, false, "Test1", "Test"),
                new Model_TemplateCell(Guid.NewGuid(), 1, false, false, "Test2", "Test")
            ];

            List<Model_TemplateRow> rows =
            [
                new Model_TemplateRow(rowId, true, true, false, cells)
            ];

            Model_Template template = new(null, templateName, "TestDescription", "Test", rows);
            Model_Template result = query.Update(template);

            return result;
        }
        #endregion

        #region Update Function
        [Fact]
        public void Add()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Guid rowId = Guid.NewGuid();
            List<Model_TemplateCell> cells =
            [
                new Model_TemplateCell(Guid.NewGuid(), 1, false, false, "Test", "Test"),
                new Model_TemplateCell(Guid.NewGuid(), 1, false, false, "Test1", "Test"),
                new Model_TemplateCell(Guid.NewGuid(), 1, false, false, "Test2", "Test")
            ];

            List<Model_TemplateRow> rows =
            [
                new Model_TemplateRow(rowId, true, true, false, cells)
            ];

            Model_Template template = new(null, "TestTemplate", "TestDescription", "Test", rows);

            query.Update(template);

            Assert.True(true);
        }

        [Fact]
        public void AddDataInvalid()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_Template template = new(null, string.Empty, null, null, []);

            Model_Template action() => query.Update(template);

            RequestException exception = Assert.Throws<RequestException>((Func<Model_Template>)action);
            RequestException result = new(ResultCodes.DataIsInvalid);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void ChangeDontOwnData()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);

            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_Template templateOtherUser = CreateTemplate("TestTemplate2", OtherUserDefaultGuid.ToString());

            Model_Template action() => query.Update(templateOtherUser);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Template>)action);
            RequestException result = new(ResultCodes.YouDontOwnTheData);
            Assert.Equal(result.Code, ex.Code);
        }

        [Fact]
        public void Change()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_Template template = CreateTemplate("TestTemplate3", DefaultUserGuid.ToString());
            Model_Template changes = new(template.ID, "TestTemplate4", null, null, template.Items);

            query.Update(changes);

            Assert.True(true);
        }
        #endregion

        #region Remove Function
        [Fact]
        public void Remove()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_Template template = CreateTemplate("TestTemplate7", DefaultUserGuid.ToString());

            query.Remove(template.ID);

            Assert.True(true);
        }

        [Fact]
        public void RemoveDataInvalid()
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
        public void RemoveNoDataFound()
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
        public void RemoveDontOwnData()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);
            OtherUserDefaultGuid = Helpers.CreateUser(OtherUsername, OtherEmail, Password);

            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_Template templateOtherUser = CreateTemplate("TestTemplate6", OtherUserDefaultGuid.ToString());

            try
            {
                query.Remove(templateOtherUser.ID);
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
            Model_Template templateOtherUser = CreateTemplate("TestTemplate8", OtherUserDefaultGuid.ToString());

            query.Copy(templateOtherUser.ID);

            Assert.True(true);
        }

        [Fact]
        public void CopyDataInvalid()
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
        public void CopyNoDataFound()
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

        #region Details Function
        [Fact]
        public void Details()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            Model_Template template = CreateTemplate("TestTemplate10", DefaultUserGuid.ToString());
            Model_Template detailTemplate = query.Details(template.ID);
            Assert.True(detailTemplate.Items.Count > 0);
        }

        [Fact]
        public void DetailsDataIsInvalid()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            try
            {
                query.Details(null);
            }
            catch (RequestException ex)
            {
                RequestException result = new(ResultCodes.DataIsInvalid);
                Assert.Equal(result.Code, ex.Code);
            }
        }

        [Fact]
        public void DetailsNoDataFound()
        {
            Helpers.DeleteAll();
            DefaultUserGuid = Helpers.CreateUser(Username, Email, Password);

            TemplateQueries query = Helpers.ReturnTemplateQuery(DefaultUserGuid.ToString());
            try
            {
                query.Details(Guid.NewGuid());
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
