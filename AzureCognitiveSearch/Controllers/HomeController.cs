using AzureCognitiveSearch.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureCognitiveSearch.Controllers
{
    [Route("api/[controller]")]
    [SwaggerTag("Upload file")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IConfiguration configuration, ILogger<HomeController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("customIndex")]
        public async Task<IActionResult> UploadCustomIndexSearch(IFormFile file)
        {
            string searchServiceName = _configuration["SearchServiceName"];
            string adminApiKey = _configuration["SearchServiceAdminApiKey"];
            SearchServiceClient serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(adminApiKey));

            //var dataSource = DataSource.AzureBlobStorage("gemvietnam", "DefaultEndpointsProtocol=https;AccountName=gemvietnam;AccountKey=7sGhjk+07j5LklNzRGOiKu/GAixRH/ic/3XfKbjRMVzQ6YAJ1IqiKYi+ucpEv27gyvZQw0b6SpzStKzbUInlVg==;EndpointSuffix=core.windows.net", "gem-documents");
            ////create data source
            //if (serviceClient.DataSources.Exists(dataSource.Name))
            //{
            //    serviceClient.DataSources.Delete(dataSource.Name);
            //}
            //serviceClient.DataSources.Create(dataSource);

            // Create custom index
            var definition = new Microsoft.Azure.Search.Models.Index()
            {
                Name = "tomcustomindex",
                Fields = FieldBuilder.BuildForType<TomTestModel>()
            };
            //create Index
            if (!serviceClient.Indexes.Exists(definition.Name))
            {
                serviceClient.Indexes.Create(definition);
            }
            //var index = serviceClient.Indexes.Create(definition);

            if (CloudStorageAccount.TryParse("DefaultEndpointsProtocol=https;AccountName=gemvietnam;AccountKey=7sGhjk+07j5LklNzRGOiKu/GAixRH/ic/3XfKbjRMVzQ6YAJ1IqiKYi+ucpEv27gyvZQw0b6SpzStKzbUInlVg==;EndpointSuffix=core.windows.net", out CloudStorageAccount storageAccount))
            {
                try
                {
                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                    CloudBlobContainer container = blobClient.GetContainerReference("gem-documents");

                    await container.CreateIfNotExistsAsync();

                    var fileBlob = container.GetBlockBlobReference(file.FileName);

                    await fileBlob.UploadFromStreamAsync(file.OpenReadStream());

                    var tomIndexsList = new List<TomTestModel>();
                    tomIndexsList.Add(new TomTestModel
                    {
                        fileId = Guid.NewGuid().ToString(),
                        blobURL = fileBlob.Uri.ToString(),
                        fileText = fileBlob.DownloadTextAsync().Result,
                        keyPhrases = "key phrases",
                    });
                    var batch = IndexBatch.MergeOrUpload(tomIndexsList);
                    ISearchIndexClient indexClient = serviceClient.Indexes.GetClient("tomcustomindex");
                    indexClient.Documents.Index(batch);

                    return Ok(fileBlob.Uri);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }

            }


            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> SaveProfileFileAsync(IFormFile file)
        {
            var storageConnectionString = _configuration["AzureStorageConnectionString"];

            if (CloudStorageAccount.TryParse(storageConnectionString, out CloudStorageAccount storageAccount))
            {
                try
                {
                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                    CloudBlobContainer container = blobClient.GetContainerReference("mystorage");

                    await container.CreateIfNotExistsAsync();

                    var fileBlob = container.GetBlockBlobReference(file.FileName);

                    await fileBlob.UploadFromStreamAsync(file.OpenReadStream());

                    return Ok(fileBlob.Uri);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }

            }

            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpGet("download")]
        public async Task<IActionResult> DownLoadFileAsync(string fileName)
        {
            var storageConnectionString = _configuration["AzureStorageConnectionString"];

            if (CloudStorageAccount.TryParse(storageConnectionString, out CloudStorageAccount storageAccount))
            {
                try
                {
                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                    CloudBlobContainer container = blobClient.GetContainerReference("mystorage");

                    CloudBlockBlob blob = container.GetBlockBlobReference(fileName);

                    Stream blobStream = await blob.OpenReadAsync();

                    return File(blobStream, blob.Properties.ContentType, fileName);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }

            }

            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string keyword)
        {
            try
            {
                var interation = 6;
                _logger.LogDebug($"Debug {interation}");
                _logger.LogInformation($"Information {interation}");
                _logger.LogWarning($"Warning{ interation}");
                _logger.LogError($"Error {interation}");
                _logger.LogCritical($"Critical {interation}");

                try
                {
                    throw new NotImplementedException();
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }

                string searchServiceName = _configuration["SearchServiceName"];
                string key = _configuration["SearchServiceAdminApiKey"];
                string indexName = _configuration["IndexName"];

                SearchServiceClient serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(key));

                ISearchIndexClient indexClient = serviceClient.Indexes.GetClient(indexName);

                //SearchParameters searchParameters = new SearchParameters();
                //searchParameters.HighlightFields = new List<string> { "content" };
                //searchParameters.HighlightPostTag = "</b>";
                //searchParameters.HighlightPreTag = "<b>";

                //var result = await indexClient.Documents.SearchAsync(keyword, searchParameters);
                //List<SearchResponse> responses = new List<SearchResponse>();

                //foreach (var data in result.Results)
                //{
                //    SearchResponse response = new SearchResponse();
                //    var path = data.Document["metadata_storage_path"].ToString();
                //    path = path.Substring(0, path.Length - 1);

                //    var byteData = WebEncoders.Base64UrlDecode(path);
                //    response.FilePath = ASCIIEncoding.ASCII.GetString(byteData);
                //    response.FileName = Path.GetFileNameWithoutExtension(response.FilePath);
                //    response.FileText = data.Document["content"].ToString();
                //    if (data.Highlights != null)
                //    {
                //        foreach (var high in data.Highlights["content"].ToList())
                //        {
                //            response.HighLightedText += high;
                //        }
                //    }

                //    responses.Add(response);
                //}

                SearchParameters searchParameters = new SearchParameters();
                searchParameters.HighlightFields = new List<string> { "fileText" };
                searchParameters.HighlightPostTag = "</b>";
                searchParameters.HighlightPreTag = "<b>";

                var result = await indexClient.Documents.SearchAsync(keyword, searchParameters);
                List<SearchResponse> responses = new List<SearchResponse>();

                foreach (var data in result.Results)
                {
                    SearchResponse response = new SearchResponse();
                    var path = data.Document["blobURL"].ToString();
                    response.FilePath = path;
                    response.FileName = Path.GetFileNameWithoutExtension(response.FilePath);
                    response.FileText = data.Document["fileText"].ToString();
                    if (data.Highlights != null)
                    {
                        foreach (var high in data.Highlights["fileText"].ToList())
                        {
                            response.HighLightedText += high;
                        }
                    }

                    responses.Add(response);
                }

                return Ok(responses);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
