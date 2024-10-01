using Microsoft.EntityFrameworkCore;
using StyleWerk.NBB.AWS;
using StyleWerk.NBB.Database;

namespace ApiServerTest.Tests
{
    public class UserTest
    {
        private static NbbContext CreateDbContext()
        {
            SecretData secretData = AmazonSecretsManager.GetData() ?? throw new Exception();
            string connectionString = secretData.GetConnectionString();

            DbContextOptionsBuilder<NbbContext> builder = new();
            builder.UseNpgsql(connectionString);

            return new NbbContext(builder.Options);
        }

        //private static AuthQueries ReturnQuery(string userGuid)
        //{
        //    NbbContext DB = CreateDbContext();

        //    ApplicationUser CurrentUser = new();
        //    Guid id = Guid.Parse(userGuid);
        //    User_Login? login = DB.User_Login.FirstOrDefault(s => s.ID == id);
        //    User_Information? information = DB.User_Information.FirstOrDefault(s => s.ID == id);
        //    string[] rights = [.. DB.User_Right.Where(s => s.ID == id).Select(s => s.Name)];
        //    CurrentUser = login is null || information is null ?
        //        new ApplicationUser() :
        //        new ApplicationUser(login, information, rights);

        //    string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36";
        //    SecretData secretData = AmazonSecretsManager.GetData() ?? throw new Exception();

        //    AuthQueries query = new(DB, CurrentUser, userAgent);
        //    return query;
        //}

        [Fact]
        public void Register()
        {

        }

        [Fact]
        public void Login()
        {

        }

        [Fact]
        public void ChangeEmail()
        {

        }

        [Fact]
        public void ChangePassword()
        {

        }
    }
}
