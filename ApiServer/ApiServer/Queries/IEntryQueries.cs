using StyleWerk.NBB.Queries.Dto;

namespace StyleWerk.NBB.Queries
{
    public interface IEntryQueries
    {
        public List<EntryViewModel> LoadEntriesByUserId(Guid userId);
        public EntryContentViewModel LoadEntryByUserId(Guid userId);
    }
}
