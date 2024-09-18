using StyleWerk.NBB.Database;
using StyleWerk.NBB.Queries.ViewModel;

namespace StyleWerk.NBB.Queries
{
    public class TemplateQueries : ITemplateQueries
    {
        private readonly NbbContext _context;

        public TemplateQueries(NbbContext context)
        {
            _context = context;
        }

        public List<TemplateViewModel> GetTemplatesByUserId(Guid UserId)
        {
            var templates = _context.Structure_Template
                .Where(u => u.UserID == UserId)
                .Select(t => new TemplateViewModel(t))
                .ToList();

            return templates;
        }

        public List<TemplateViewModel> GetAllPublicTemplates()
        {
            var templates = _context.Structure_Template
                .Where(p=> p.IsPublic == true)
                .Select(t=> new TemplateViewModel(t))
                .ToList();

            return templates;
        }
    }
}
