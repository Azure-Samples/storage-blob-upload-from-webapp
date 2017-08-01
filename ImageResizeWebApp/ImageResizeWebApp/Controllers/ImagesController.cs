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
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Auth;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace ImageResizeWebApp.Controllers
{
    [Route("api/[controller]")]
    public class ImagesController : Controller
    {
        private readonly AzureStorageConfig _storageConfig;
        private string imageInfo;

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


        [HttpPost("[action]")]
        public async Task<IActionResult> Upload(ICollection<IFormFile> files)
        {
            try
            {          
                bool isUploaded;
                bool isQueued;

                foreach (var formFile in files)
                {               
                    if (formFile.Length > 0 && IsImage(formFile))
                    {
                        using (Stream stream = formFile.OpenReadStream())
                        {
                           isUploaded =  await UploadFileToStorage(stream, formFile.FileName);
                        }

                        if (isUploaded)
                        {
                            isQueued = await CreateQueueItem(imageInfo);
                        }
                    }
                }           

                return new OkResult();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private bool IsImage(IFormFile file)
        {
            if (file.ContentType.Contains("image"))
            {
                return true;
            }

            string[] formats = new string[] { ".jpg", ".png", ".gif", ".jpeg" }; // add more if u like...

            // linq from Henrik Stenbæk
            return formats.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<bool> UploadFileToStorage(Stream fileStream, string fileName)
        {
            string containerName = Guid.NewGuid().ToString().ToLower();

            if (_storageConfig != null)
            {
                StorageCredentials storageCredentials = new StorageCredentials(_storageConfig.AccountName, _storageConfig.AccountKey);

                CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);

                if (storageAccount != null)
                {
                    // Create the blob client.
                    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                    // Retrieve reference to a previously created container.
                    CloudBlobContainer container = blobClient.GetContainerReference(containerName);

                    await container.CreateIfNotExistsAsync();

                    // Retrieve reference to a blob named "myblob".
                    CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);

                    await blockBlob.UploadFromStreamAsync(fileStream);
                    imageInfo = containerName + "," + fileName;                  

                    return await Task.FromResult(true);
                }
              
            }

            return await Task.FromResult(false);
        }

        private async Task<bool> CreateQueueItem(string imageInfo)
        {
            if (_storageConfig != null)
            {
                StorageCredentials storageCredentials = new StorageCredentials(_storageConfig.AccountName, _storageConfig.AccountKey);

                CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);

                if (storageAccount != null && imageInfo != string.Empty)
                {
                    // Create the queue client.
                    CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

                    // Retrieve a reference to a queue.
                    CloudQueue queue = queueClient.GetQueueReference(_storageConfig.QueueName);

                    // Create the queue if it doesn't already exist.
                    await queue.CreateIfNotExistsAsync();

                    // Create a message and add it to the queue.
                    CloudQueueMessage message = new CloudQueueMessage(imageInfo);

                    await queue.AddMessageAsync(message);

                    return await Task.FromResult(true);
                }

            }
            return await Task.FromResult(false);
        }



    }
}