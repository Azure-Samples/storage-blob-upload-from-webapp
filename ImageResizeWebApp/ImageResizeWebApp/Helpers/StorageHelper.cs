using ImageResizeWebApp.Models;
using Microsoft.AspNetCore.Http;
using Azure.Storage;
using Azure.Storage.Blobs;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;

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

            string[] formats = new string[] { ".jpg", ".png", ".gif", ".jpeg" };

            return formats.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }

        public static async Task<bool> UploadFileToStorage(Stream fileStream, string fileName, AzureStorageConfig _storageConfig)
        {
            // Create a URI to the blob
            Uri uri = new Uri("https://" + _storageConfig.AccountName + "/.blob.core.windows.net/" + _storageConfig.ImageContainer + "/" + fileName);

            // Create storagecredentials object by reading the values from the configuration (appsettings.json)
            StorageSharedKeyCredential storageCredentials = new StorageSharedKeyCredential(_storageConfig.AccountName, _storageConfig.AccountKey);

            // Create cloudstorage account by passing the storagecredentials
            //CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);
            //BlobServiceClient blobServiceClient = new BlobServiceClient(uri);

            // Create the blob client.
            BlobClient blobClient = new BlobClient(uri, storageCredentials);

            // Get reference to the blob container by passing the name by reading the value from the configuration (appsettings.json)
            //CloudBlobContainer container = blobClient.GetContainerReference(_storageConfig.ImageContainer);
            //BlobContainerClient container = await blobServiceClient.CreateBlobContainerAsync(_storageConfig.ImageContainer);

            // Get the reference to the block blob from the container
            //CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);

            // Upload the file
            //await blockBlob.UploadFromStreamAsync(fileStream);
            await blobClient.UploadAsync(fileStream);

            return await Task.FromResult(true);
        }

        public static async Task<List<string>> GetThumbNailUrls(AzureStorageConfig _storageConfig)
        {
            List<string> thumbnailUrls = new List<string>();

            // Create a URI to the thumbnail container
            Uri uri = new Uri("https://" + _storageConfig.AccountName + "/.blob.core.windows.net/");

            // Create storagecredentials object by reading the values from the configuration (appsettings.json)
            //StorageSharedKeyCredential storageCredentials = new StorageSharedKeyCredential(_storageConfig.AccountName, _storageConfig.AccountKey);

            // Create cloudstorage account by passing the storagecredentials
            //CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);
            BlobServiceClient blobServiceClient = new BlobServiceClient(uri);

            // Create blob client
            //BlobClient blobClient = new BlobClient(uri, storageCredentials);

            // Get reference to the container
            //CloudBlobContainer container = blobClient.GetContainerReference(_storageConfig.ThumbnailContainer);
            BlobContainerClient container = await blobServiceClient.CreateBlobContainerAsync(_storageConfig.ThumbnailContainer);

            //BlobContinuationToken continuationToken = null;

            //BlobResultSegment resultSegment = null;

            //Call ListBlobsSegmentedAsync and enumerate the result segment returned, while the continuation token is non-null.
            //When the continuation token is null, the last page has been returned and execution can exit the loop.
            //do
            //{
            //    //This overload allows control of the page size. You can return all remaining results by passing null for the maxResults parameter,
            //    //or by calling a different overload.
            //    resultSegment = await container.ListBlobsSegmentedAsync("", true, BlobListingDetails.All, 10, continuationToken, null, null);

            //    foreach (var blobItem in resultSegment.Results)
            //    {
            //        thumbnailUrls.Add(blobItem.StorageUri.PrimaryUri.ToString());
            //    }

            //    //Get the continuation token.
            //    continuationToken = resultSegment.ContinuationToken;
            //}
            //while (continuationToken != null);

            foreach (BlobItem blobItem in container.GetBlobs())
            {
                string thumbnailUrl = "https://" + _storageConfig.AccountName + "/.blob.core.windows.net/" + _storageConfig.ThumbnailContainer + "/" + blobItem.Name;
                thumbnailUrls.Add(thumbnailUrl);
            }

            return await Task.FromResult(thumbnailUrls);
        }
    }
}
