using StyleWerk.NBB.Database;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace ApiServerTest.Tests
{
    public class TemplateTest
    {
        private static TemplateQueries ReturnQuery(string userGuid)
        {
            NbbContext DB = NbbContext.Create();
            ApplicationUser user = DB.GetUser(new Guid(userGuid));

            TemplateQueries query = new(DB, user);
            return query;
        }

        [Fact]
        public void Add()
        {
            TemplateQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");
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
            TemplateQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");
            Model_Template template = new(null, string.Empty, null, null, []);

            Model_Template action() => query.Update(template);

            RequestException exception = Assert.Throws<RequestException>((Func<Model_Template>)action);
            RequestException result = new(ResultCodes.DataIsInvalid);
            Assert.Equal(result.Code, exception.Code);
        }

        private static Model_Template CreateTemplateOtherUser(string templateName)
        {
            TemplateQueries query = ReturnQuery("6e4a61db-8c61-4594-a643-feae632caba2");
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

        [Fact]
        public void ChangeDontOwnData()
        {
            TemplateQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");
            Model_Template templateOtherUser = CreateTemplateOtherUser("TestTemplate2");

            Model_Template action() => query.Update(templateOtherUser);

            RequestException ex = Assert.Throws<RequestException>((Func<Model_Template>)action);
            RequestException result = new(ResultCodes.YouDontOwnTheData);
            Assert.Equal(result.Code, ex.Code);
        }

        private static Model_Template CreateTemplateUserOwn(string templateName)
        {
            TemplateQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");
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

        [Fact]
        public void Change()
        {
            TemplateQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");
            Model_Template template = CreateTemplateUserOwn("TestTemplate3");
            Model_Template changes = new(template.ID, "TestTemplate4", null, null, template.Items);

            query.Update(changes);

            Assert.True(true);
        }

        [Fact]
        public void Remove()
        {
            TemplateQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");
            Model_Template template = CreateTemplateUserOwn("TestTemplate7");

            query.Remove(template.ID);

            Assert.True(true);
        }

        [Fact]
        public void RemoveDataInvalid()
        {
            TemplateQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");

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
            TemplateQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");
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
            TemplateQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");
            Model_Template templateOtherUser = CreateTemplateOtherUser("TestTemplate6");

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

        [Fact]
        public void Copy()
        {
            TemplateQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");
            Model_Template templateOtherUser = CreateTemplateOtherUser("TestTemplate8");

            query.Copy(templateOtherUser.ID);

            Assert.True(true);
        }

        [Fact]
        public void CopyDataInvalid()
        {
            TemplateQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");
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
            TemplateQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");
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

        [Fact]
        public void Details()
        {
            TemplateQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");
            Model_Template template = CreateTemplateUserOwn("TestTemplate10");
            Model_Template detailTemplate = query.Details(template.ID);
            Assert.True(detailTemplate.Items.Count > 0);
        }

        [Fact]
        public void DetailsDataIsInvalid()
        {
            TemplateQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");
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
            TemplateQueries query = ReturnQuery("90865032-e4e8-4e2b-85e0-5db345f42a5b");
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
    }
}
