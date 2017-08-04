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
using ImageResizeWebApp.Helpers;

namespace ImageResizeWebApp.Controllers
{
    [Route("api/[controller]")]
    public class ImagesController : Controller
    {
        // make sure that appsettings.json is filled with the necessary details of the azure storage
        private readonly AzureStorageConfig _storageConfig;

        public ImagesController(IOptions<AzureStorageConfig> config)
        {
            _storageConfig = config.Value;
        }

        // this API action is for workstream #2 and not yet used / implemented fully
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

        // POST /api/images/upload
        [HttpPost("[action]")]
        public async Task<IActionResult> Upload(ICollection<IFormFile> files)
        {
            string uploadedfileName = string.Empty;
            bool isQueued = false;
            try
            {          
               
                if (files.Count == 0)
                    return BadRequest("No files received from the upload");

                if(_storageConfig.AccountKey == string.Empty || _storageConfig.AccountName == string.Empty)
                    return BadRequest("sorry, can't retrieve your azure storage details from appsettings.js, make sure that you add azure storage details there");

                if (_storageConfig.ImageContainer == string.Empty)
                    return BadRequest("Please provide a name for your image container in the azure blob storage");


                foreach (var formFile in files)
                {
                    if (StorageHelper.IsImage(formFile))
                    {
                        if (formFile.Length > 0)
                        {
                            using (Stream stream = formFile.OpenReadStream())
                            {
                                uploadedfileName = await StorageHelper.UploadFileToStorage(stream, formFile.FileName,_storageConfig);
                            }

                            if (uploadedfileName != string.Empty)
                            {
                                isQueued = await StorageHelper.CreateQueueItem(uploadedfileName, _storageConfig);
                            }
                        }
                    }
                    else
                    {
                        return new UnsupportedMediaTypeResult();                        
                    }                   
                }

                if (uploadedfileName != string.Empty)
                {
                    if(_storageConfig.ThumbnailContainer != string.Empty)
                        return new AcceptedAtActionResult("GetThumbNails", "Images", null, null);
                    else
                        return new AcceptedResult();
                }                    
                else
                    return BadRequest("Look like the image couldnt upload to the storage");


            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // GET /api/images/thumbnails
        [HttpGet("thumbnails")]
        public async Task<IActionResult> GetThumbNails(string containerName)
        {
            try
            {
                if (_storageConfig != null)
                {
                    List<string> thumbnailUrls = await StorageHelper.GetThumbNailUrls(_storageConfig);
                    return new ObjectResult(thumbnailUrls);
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

    }
}