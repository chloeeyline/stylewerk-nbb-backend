using StyleWerk.NBB.Queries.ViewModel;

namespace StyleWerk.NBB.Queries
{
    public interface ITemplateQueries
    {
        List<TemplateViewModel> GetTemplatesByUserId(Guid UserId);
        public List<TemplateViewModel> GetAllPublicTemplates();
    }
}