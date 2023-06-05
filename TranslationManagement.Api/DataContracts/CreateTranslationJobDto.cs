namespace TranslationManagement.Api.DataContracts
{
    public class CreateTranslationJobDto
    {
        public string CustomerName { get; set; }
        public string OriginalContent { get; set; }
        public string TranslatedContent { get; set; }
    }
}
