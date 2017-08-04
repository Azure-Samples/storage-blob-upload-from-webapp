using ImageResizeWebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImageResizeWebApp.Helpers
{
    public static class StorageHelper
    {

        public static bool IsImage(IFormFile file)
        {
            if (file.ContentType.Contains("image"))
            {
                return true;
            }

            string[] formats = new string[] { ".jpg", ".png", ".gif", ".jpeg" }; // add more if u like...

            // linq from Henrik Stenbæk
            return formats.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }
        public static async Task<string> UploadFileToStorage(Stream fileStream, string fileName, AzureStorageConfig _storageConfig)
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
                    CloudBlobContainer container = blobClient.GetContainerReference(_storageConfig.ImageContainer);
                    await container.CreateIfNotExistsAsync();
                    // Retrieve reference to a blob named "myblob".
                    CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);
                    await blockBlob.UploadFromStreamAsync(fileStream);                    ;
                    return await Task.FromResult(fileName);
                }

            }

            return await Task.FromResult(string.Empty);
        }
        public static async Task<bool> CreateQueueItem(string imageInfo, AzureStorageConfig _storageConfig)
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
        public static async Task<List<string>> GetThumbNailUrls(AzureStorageConfig _storageConfig)
        {
            List<string> thumbnailUrls = new List<string>();
            try
            {
                StorageCredentials storageCredentials = new StorageCredentials(_storageConfig.AccountName, _storageConfig.AccountKey);
                CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);

                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(_storageConfig.ThumbnailContainer);               
                await container.CreateIfNotExistsAsync();
                await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
                // Loop over items within the container and output the length and URI.
                int i = 0;
                BlobContinuationToken continuationToken = null;
                BlobResultSegment resultSegment = null;


                //Call ListBlobsSegmentedAsync and enumerate the result segment returned, while the continuation token is non-null.
                //When the continuation token is null, the last page has been returned and execution can exit the loop.
                do
                {
                    //This overload allows control of the page size. You can return all remaining results by passing null for the maxResults parameter,
                    //or by calling a different overload.
                    resultSegment = await container.ListBlobsSegmentedAsync("", true, BlobListingDetails.All, 10, continuationToken, null, null);

                    foreach (var blobItem in resultSegment.Results)
                    {
                        thumbnailUrls.Add(blobItem.StorageUri.PrimaryUri.ToString());
                    }

                    //Get the continuation token.
                    continuationToken = resultSegment.ContinuationToken;
                }
                while (continuationToken != null);

                return await Task.FromResult(thumbnailUrls);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
