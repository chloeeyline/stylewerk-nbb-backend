namespace StyleWerk.NBB.Dto
{
    public class TemplateDto
    {
        public Guid UserID { get; set; }
        public string TemplateName { get; set; }
        public string TemplateDescription { get; set; }
        public bool IsPublic { get; set; }

    }
}
