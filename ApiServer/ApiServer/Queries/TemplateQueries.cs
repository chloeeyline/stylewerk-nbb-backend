
using StyleWerk.NBB.Authentication;
using StyleWerk.NBB.Database;

namespace StyleWerk.NBB.Queries
{
    public class TemplateQueries
    {
        private readonly NbbContext _context;
        private readonly ApplicationUser _user;

        public TemplateQueries(NbbContext context, ApplicationUser user)
        {
            _user = user;
            _context = context;
        }


    }
}
