using Microsoft.EntityFrameworkCore;
using StyleWerk.NBB.Database;
using StyleWerk.NBB.Queries.Dto;

namespace StyleWerk.NBB.Queries
{
    public class EntryQueries : IEntryQueries
    {
        private readonly NbbContext _context;

        public EntryQueries(NbbContext context) 
        { 
            _context = context;
        }

        public List<EntryViewModel> LoadEntriesByUserId(Guid userId)
        {
            var entries = _context.Structure_Entry
                .Include(f => f.O_Folder)
                .Where(u=> u.UserID == userId)
                .Select(e=> new EntryViewModel (e))
                .ToList();

            return entries;
        }

        public EntryContentViewModel LoadEntryByUserId(Guid userId) 
        {
            var entry = _context.Structure_Entry_Cell
                .Where(u => u.O_Entry.UserID == userId)
                .FirstOrDefault();
                
            return new EntryContentViewModel(entry);
        }
    }
}
