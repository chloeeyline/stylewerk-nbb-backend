namespace StyleWerk.NBB.Dto
{
    public class EntryDto
    {
        public Guid? EntryId { get; set; }
        public Guid TemplateId { get; set; }
        public Guid UserId { get; set; }
        public Guid? FolderId { get; set; }
        public string EntryTitle { get; set; }
       
    }
}
