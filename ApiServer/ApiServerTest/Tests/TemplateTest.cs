using StyleWerk.NBB.Database;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace ApiServerTest.Tests
{
    public class TemplateTest
    {
        private Guid DefaultUserGuid = new("90865032-e4e8-4e2b-85e0-5db345f42a5b");
        private Guid OtherUserDefaultGuid = new("cd6c092d-0546-4f8b-b70c-352d2ca765a4");

        #region Helpers
        private static TemplateQueries ReturnQuery(string userGuid)
        {
            NbbContext DB = NbbContext.Create();
            ApplicationUser user = DB.GetUser(new Guid(userGuid));

            TemplateQueries query = new(DB, user);
            return query;
        }

        private static Model_Template CreateTemplate(string templateName, string user)
        {
            TemplateQueries query = ReturnQuery(user);
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
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
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
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_Template template = new(null, string.Empty, null, null, []);

            Model_Template action() => query.Update(template);

            RequestException exception = Assert.Throws<RequestException>((Func<Model_Template>)action);
            RequestException result = new(ResultCodes.DataIsInvalid);
            Assert.Equal(result.Code, exception.Code);
        }

        [Fact]
        public void ChangeDontOwnData()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_Template templateOtherUser = CreateTemplate("TestTemplate2", OtherUserDefaultGuid.ToString());

            Model_Template action() => query.Update(templateOtherUser);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Template>)action);
            RequestException result = new(ResultCodes.YouDontOwnTheData);
            Assert.Equal(result.Code, ex.Code);
        }

        [Fact]
        public void Change()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
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
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_Template template = CreateTemplate("TestTemplate7", DefaultUserGuid.ToString());

            query.Remove(template.ID);

            Assert.True(true);
        }

        [Fact]
        public void RemoveDataInvalid()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());

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
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
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
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
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
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_Template templateOtherUser = CreateTemplate("TestTemplate8", OtherUserDefaultGuid.ToString());

            query.Copy(templateOtherUser.ID);

            Assert.True(true);
        }

        [Fact]
        public void CopyDataInvalid()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
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
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
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
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
            Model_Template template = CreateTemplate("TestTemplate10", DefaultUserGuid.ToString());
            Model_Template detailTemplate = query.Details(template.ID);
            Assert.True(detailTemplate.Items.Count > 0);
        }

        [Fact]
        public void DetailsDataIsInvalid()
        {
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
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
            TemplateQueries query = ReturnQuery(DefaultUserGuid.ToString());
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
