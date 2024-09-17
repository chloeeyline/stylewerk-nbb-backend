using StyleWerk.NBB.Database.Structure;

namespace StyleWerk.NBB.Queries.Dto
{
    public class EntryViewModel
    {
        public Guid Id { get; set; }
        public string EntryTitle { get; set; }
        public Guid TemplateId { get; set; }
        public string TemplateTitle { get; set; }
        public int FolderSortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }

        public EntryViewModel(Structure_Entry value)
        {
            Id = value.ID;
            EntryTitle = value.Name;
            TemplateTitle = value.O_Template.Name;
            TemplateId = value.TemplateID;
            CreatedAt = value.CreatedAt;
            LastUpdatedAt = value.LastUpdatedAt;
            FolderSortOrder = value.O_Folder.SortOrder;
        }
    }
}
