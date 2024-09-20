using StyleWerk.NBB.Database;

namespace StyleWerk.NBB.Queries
{
    public class TemplateQueries
    {
        private readonly NbbContext _context;

        public TemplateQueries(NbbContext context)
        {
            _context = context;
        }
    }
}
