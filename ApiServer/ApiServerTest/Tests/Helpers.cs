using StyleWerk.NBB.Authentication;
using StyleWerk.NBB.AWS;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Database.Structure;
using StyleWerk.NBB.Database.User;
using StyleWerk.NBB.Models;
using StyleWerk.NBB.Queries;

namespace ApiServerTest.Tests
{
    public class Helpers
    {
        public static EntryFolderQueries ReturnFolderQuery(string userGuid)
        {
            NbbContext DB = NbbContext.Create();
            ApplicationUser user = DB.GetUser(new Guid(userGuid));
            EntryFolderQueries query = new(DB, user);
            return query;
        }

        public static void DeleteAll()
        {
            NbbContext context = NbbContext.Create();
            List<Structure_Entry_Folder> folders = [.. context.Structure_Entry_Folder];
            List<User_Login> usersLogin = [.. context.User_Login];
            List<User_Information> userInformations = [.. context.User_Information];
            if (folders.Count > 0)
                context.Structure_Entry_Folder.RemoveRange(folders);

            if (usersLogin.Count > 0)
                context.User_Login.RemoveRange(usersLogin);

            if (userInformations.Count > 0)
                context.User_Information.RemoveRange(userInformations);

            context.SaveChanges();
        }

        public static Guid CreateUser(string userName, string email, string password)
        {
            NbbContext DB = NbbContext.Create();
            ApplicationUser user = DB.GetUser(Guid.NewGuid());

            string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36";
            AuthQueries query = new(DB, user, userAgent, SecretData.GetData());

            Model_Registration register = new(userName, email, password, "Chloe", "Hauer", UserGender.Female, 0);
            string? newUser = query.Registration(register);

            Model_Login login = new(userName, password, true);
            User_Login myUser = query.GetUser(login);

            return myUser.ID;
        }
    }
}
