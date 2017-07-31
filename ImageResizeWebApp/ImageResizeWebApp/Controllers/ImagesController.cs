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

        [HttpGet("[action]")]
        public async Task<string> Storage()
        {
            try
            {
                if (_storageConfig != null)
                {
                    StorageCredentials storageCredentials = new StorageCredentials(_storageConfig.AccountName, _storageConfig.AccountKey);
                    CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, false);
                    if (storageAccount != null)
                    {
                        //Create the blob client object.
                        CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                        //Get a reference to a container to use for the sample code, and create it if it does not exist.
                        CloudBlobContainer container = blobClient.GetContainerReference(Guid.NewGuid().ToString().ToLower());
                        await container.CreateIfNotExistsAsync();
                        SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
                        sasConstraints.SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddHours(24);
                        sasConstraints.Permissions = SharedAccessBlobPermissions.List | SharedAccessBlobPermissions.Write;

                        //Generate the shared access signature on the container, setting the constraints directly on the signature.
                        string sasContainerToken = container.GetSharedAccessSignature(sasConstraints);

                        //Return the URI string for the container, including the SAS token.
                        return container.Uri + sasContainerToken;
                    }
                    else
                    {
                        return "sorry, can't retrieve your azure storage account";
                    }
                }
                else
                {
                    return "sorry, can't retrieve your azure storage details from appsettings.js";
                }
               
            }
            catch (Exception ex)
            {
                return ex.Message;
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