using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ImageResizeWebApp.Models;
using Microsoft.Extensions.Options;

using System.IO;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Auth;
using System.Net.Http;


namespace ImageResizeWebApp.Controllers
{
    [Route("api/[controller]")]
    public class ImagesController : Controller
    {
        private readonly AzureStorageConfig _storageConfig;

        public ImagesController(IOptions<AzureStorageConfig> config)
        {
            _storageConfig = config.Value;
        }

        [HttpGet("[action]/{containerName?}")]
        public async Task<IActionResult> Storage(string containerName)
        {

            try
            {
                if (_storageConfig != null)
                {
                    StorageCredentials storageCredentials = new StorageCredentials(_storageConfig.AccountName, _storageConfig.AccountKey);
                    CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials,true);
                    if (storageAccount != null)
                    {                  
                        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                        CloudBlobContainer container;

                        if (containerName != null)
                        {
                            container = blobClient.GetContainerReference(containerName);
                            bool containerExists = await container.ExistsAsync();
                            if(!containerExists)
                                return NotFound("can't find your storage container with name " + containerName);
                            
                        }
                        else
                        {
                            container = blobClient.GetContainerReference(Guid.NewGuid().ToString().ToLower());
                            await container.CreateIfNotExistsAsync();
                        }

                        if (container != null)
                        {
                            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
                            sasConstraints.SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddHours(24);
                            sasConstraints.Permissions = SharedAccessBlobPermissions.List | SharedAccessBlobPermissions.Write;

                            //Generate the shared access signature on the container, setting the constraints directly on the signature.
                            string sasContainerToken = container.GetSharedAccessSignature(sasConstraints);

                            //Return the URI string for the container, including the SAS token.
                            var blobContainer = new
                            {
                                storageUrl = container.Uri + sasContainerToken,
                               
                            };
                            return new ObjectResult(blobContainer) ;
                        }
                        else
                        {
                            return BadRequest("sorry, we can't create a new storage container"); 
                        }                
                        
                    }
                    else
                    {
                        return BadRequest("sorry, can't retrieve your azure storage account");
                    }
                }
                else
                {
                    return BadRequest("sorry, can't retrieve your azure storage details from appsettings.js");
                }
               
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
           
        }

        [HttpGet]
        public string List()
        {
            var val = _storageConfig;
            return "https://url";
        }



    }
}