using DomainObjects;

namespace TranslationManagement.Api.DataContracts
{
    public class UpdateJobStatusDto
    {
        public int TranslatorId { get; set; }
        public string Status { get; set; }
    }
}
