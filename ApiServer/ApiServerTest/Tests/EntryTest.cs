using Microsoft.EntityFrameworkCore;

using StyleWerk.NBB.AWS;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.User;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace StyleWerk.NBB.Tests
{
    public class EntryTest
    {
        private NbbContext CreateDbContext()
        {
            SecretData secretData = AmazonSecretsManager.GetData() ?? throw new Exception();
            string connectionString = secretData.GetConnectionString();

            DbContextOptionsBuilder<NbbContext> builder = new();
            builder.UseNpgsql(connectionString);

            return new NbbContext(builder.Options);
        }

        [Fact]
        public void AddFolder()
        {
            Database.NbbContext DB = CreateDbContext();
            ApplicationUser CurrentUser = new();
            Guid id = Guid.Parse("90865032-e4e8-4e2b-85e0-5db345f42a5b");
            User_Login? login = DB.User_Login.FirstOrDefault(s => s.ID == id);
            User_Information? information = DB.User_Information.FirstOrDefault(s => s.ID == id);
            string[] rights = [.. DB.User_Right.Where(s => s.ID == id).Select(s => s.Name)];
            CurrentUser = login is null || information is null ?
                new ApplicationUser() :
                new ApplicationUser(login, information, rights);

            EntryQueries query = new(DB, CurrentUser);
            //Model_EntryFolders newFolder = new(null, "TestFolder2", 1, new Model_EntryItem[0]);
            //Model_EntryFolders response = query.UpdateFolder(newFolder);

            //Assert.IsType<Model_EntryFolders>(response);
        }

        [Fact]
        public void ChangeFolderException()
        {
            Database.NbbContext DB = CreateDbContext();
            ApplicationUser CurrentUser = new();
            Guid id = Guid.Parse("90865032-e4e8-4e2b-85e0-5db345f42a5b");
            User_Login? login = DB.User_Login.FirstOrDefault(s => s.ID == id);
            User_Information? information = DB.User_Information.FirstOrDefault(s => s.ID == id);
            string[] rights = [.. DB.User_Right.Where(s => s.ID == id).Select(s => s.Name)];
            CurrentUser = login is null || information is null ?
                new ApplicationUser() :
                new ApplicationUser(login, information, rights);

            //EntryQueries query = new(DB, CurrentUser);
            //Model_EntryFolders newFolder = new(new Guid("b735f46a-a732-441f-9607-4c3a51f89434"), "TestFolder", 1, new Model_EntryItem[0]);
            //Func<Model_EntryFolders> action = () => query.UpdateFolder(newFolder);

            //RequestException exception = Assert.Throws<RequestException>(action);
            //RequestException result = new(ResultCodes.NameMustBeUnique);
            //Assert.Equal(result.Message, exception.Message);
        }

        [Fact]
        public void GetEntriesInFolder()
        {

        }

        [Fact]
        public void ChangeFolderSortOrder()
        {

        }

        [Fact]
        public void AddEntryToFolder()
        {

        }

        [Fact]
        public void RemoveEntryFromFolder()
        {

        }

        [Fact]
        public void DeleteFolder()
        {
            Database.NbbContext DB = CreateDbContext();
            ApplicationUser CurrentUser = new();
            Guid id = Guid.Parse("90865032-e4e8-4e2b-85e0-5db345f42a5b");
            User_Login? login = DB.User_Login.FirstOrDefault(s => s.ID == id);
            User_Information? information = DB.User_Information.FirstOrDefault(s => s.ID == id);
            string[] rights = [.. DB.User_Right.Where(s => s.ID == id).Select(s => s.Name)];
            CurrentUser = login is null || information is null ?
                new ApplicationUser() :
                new ApplicationUser(login, information, rights);

            EntryQueries query = new(DB, CurrentUser);
            try
            {
                //query.RemoveFolder(new Guid("b735f46a-a732-441f-9607-4c3a51f89431"));
            }
            catch (Exception ex)
            {
                RequestException result = new(ResultCodes.NoDataFound);
                Assert.Equal(result.Message, ex.Message);
            }
        }

        [Fact]
        public void AddEntry()
        {

        }

        [Fact]
        public void ChangeEntry()
        {

        }

        [Fact]
        public void AddTag()
        {

        }
        [Fact]
        public void RemoveTag()
        {

        }

        [Fact]
        public void ChangeTag()
        {

        }

        [Fact]
        public void GetEntriesWithoutFolder()
        {

        }

        [Fact]
        public void GetEntryPreview()
        {

        }
    }
}
