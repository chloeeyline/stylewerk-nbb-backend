using StyleWerk.NBB.Database.Structure;

namespace StyleWerk.NBB.Queries.ViewModel
{
    public record TemplateViewModel
    {
        public string TemplateTitle { get; set; }
        public string TemplateDescription { get; set; }
        public bool IsPublic { get; set; }


        public TemplateViewModel(Structure_Template value)
        {
            TemplateTitle = value.Name;
            TemplateDescription = value.Description;
            IsPublic = value.IsPublic;
        }
    }
}
