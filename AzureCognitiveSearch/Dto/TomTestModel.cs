using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AzureCognitiveSearch.Dto
{
    [SerializePropertyNamesAsCamelCase]
    public class TomTestModel
    {
        [Key]
        [IsFilterable]
        public string fileId { get; set; }
        [IsSearchable]
        public string fileText { get; set; }
        public string blobURL { get; set; }
        [IsSearchable]
        public string keyPhrases { get; set; }
    }
}
