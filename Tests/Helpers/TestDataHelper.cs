using DomainObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Helpers
{
    public class TestDataHelper
    {
        public static List<TranslationJob> GetFakeTranslationJobList()
        {
            return new List<TranslationJob>()
                            {
                                new TranslationJob
                                {
                                    Id = 1,
                                    CustomerName = "Foo",
                                    OriginalContent = "Bar",
                                    Price = 3,
                                    Status = JobStatuses.Inprogress,
                                    TranslatedContent = "sss"
                                }
                            };
        }

    }
}
