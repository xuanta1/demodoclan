using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AzureCognitiveSearch.Dto
{
    public class DtoSearchResponse
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public string FileText { get; set; }
        public string HighLightedText { get; set; }
    }
}
