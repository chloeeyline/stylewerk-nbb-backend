using StyleWerk.NBB.Database.Structure;
using System.Security.Cryptography.X509Certificates;

namespace StyleWerk.NBB.Queries.Dto
{
    public class EntryContentViewModel
    {
        public Guid EntryId { get; set; }

        public Guid CellId { get; set; }
        public string Metadata { get; set; }
        public int SortOrder { get; set; }
        public bool IsRequired { get; set; }
        public string Text { get; set; }

        public EntryContentViewModel(Structure_Entry_Cell value) 
        {
            EntryId = value.EntryID;
            CellId = value.CellID;
            Metadata = value.Metadata;
            SortOrder = value.O_Cell.SortOrder;
            Text = value.O_Cell.Text;
            IsRequired = value.O_Cell.IsRequiered;
        }
    }
}
