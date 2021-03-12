using AzureCognitiveSearch.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;
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

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
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
                string searchServiceName = _configuration["SearchServiceName"];
                string key = _configuration["SearchServiceAdminApiKey"];
                string indexName = _configuration["IndexName"];

                SearchServiceClient serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(key));

                ISearchIndexClient indexClient = serviceClient.Indexes.GetClient(indexName);

                SearchParameters searchParameters = new SearchParameters();
                searchParameters.HighlightFields = new List<string> { "content" };
                searchParameters.HighlightPostTag = "</b>";
                searchParameters.HighlightPreTag = "<b>";

                var result = await indexClient.Documents.SearchAsync(keyword, searchParameters);
                List<SearchResponse> responses = new List<SearchResponse>();

                foreach (var data in result.Results)
                {
                    SearchResponse response = new SearchResponse();
                    var path = data.Document["metadata_storage_path"].ToString();
                    path = path.Substring(0, path.Length - 1);

                    var byteData = WebEncoders.Base64UrlDecode(path);
                    response.FilePath = ASCIIEncoding.ASCII.GetString(byteData);
                    response.FileName = Path.GetFileNameWithoutExtension(response.FilePath);
                    response.FileText = data.Document["content"].ToString();
                    if (data.Highlights != null)
                    {
                        foreach (var high in data.Highlights["content"].ToList())
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
