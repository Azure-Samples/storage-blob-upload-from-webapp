using ImageResizeWebApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;

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

            string[] formats = new string[] { ".jpg", ".png", ".gif", ".jpeg" };

            return formats.Any(item => file.FileName.EndsWith(item, StringComparison.OrdinalIgnoreCase));
        }

        public static async Task<bool> UploadFileToStorage(Stream fileStream, string fileName, AzureStorageConfig _storageConfig)
        {
            // Create storagecredentials object by reading the values from the configuration (appsettings.json)
            StorageCredentials storageCredentials = new StorageCredentials(_storageConfig.AccountName, _storageConfig.AccountKey);

            // Create cloudstorage account by passing the storagecredentials
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Get reference to the blob container by passing the name by reading the value from the configuration (appsettings.json)
            CloudBlobContainer container = blobClient.GetContainerReference(_storageConfig.ImageContainer);

            // Get the reference to the block blob from the container
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(fileName);

            // Upload the file
            await blockBlob.UploadFromStreamAsync(fileStream);

            return await Task.FromResult(true);
        }


        public static async Task<List<string>> GetThumbNailUrls(AzureStorageConfig _storageConfig)
        {
            List<string> thumbnailUrls = new List<string>();

            // Create storagecredentials object by reading the values from the configuration (appsettings.json)
            StorageCredentials storageCredentials = new StorageCredentials(_storageConfig.AccountName, _storageConfig.AccountKey);

            // Create cloudstorage account by passing the storagecredentials
            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);

            // Create blob client
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Get reference to the container
            CloudBlobContainer container = blobClient.GetContainerReference(_storageConfig.ThumbnailContainer);

            // Create the container if it is not exists
            await container.CreateIfNotExistsAsync();

            // Set the permission of the container to public
            await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

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
                    CloudBlockBlob blob = blobItem as CloudBlockBlob;
                    //Set the expiry time and permissions for the blob.
                    //In this case, the start time is specified as a few minutes in the past, to mitigate clock skew.
                    //The shared access signature will be valid immediately.
                    SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();

                    sasConstraints.SharedAccessStartTime = DateTimeOffset.UtcNow.AddMinutes(-5);

                    sasConstraints.SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddHours(24);

                    sasConstraints.Permissions = SharedAccessBlobPermissions.Read;

                    //Generate the shared access signature on the blob, setting the constraints directly on the signature.
                    string sasBlobToken = blob.GetSharedAccessSignature(sasConstraints);

                    //Return the URI string for the container, including the SAS token.
                    thumbnailUrls.Add(blob.Uri + sasBlobToken);

                }

                //Get the continuation token.
                continuationToken = resultSegment.ContinuationToken;
            }

            while (continuationToken != null);

            return await Task.FromResult(thumbnailUrls);
        }

        public static async Task<Object> GetStorageURLWithSASToken(AzureStorageConfig _storageConfig)
        {
            StorageCredentials storageCredentials = new StorageCredentials(_storageConfig.AccountName, _storageConfig.AccountKey);

            CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer imageContainer = blobClient.GetContainerReference(_storageConfig.ImageContainer);

            await imageContainer.CreateIfNotExistsAsync();

            SharedAccessBlobPolicy sasContainerPolicy = new SharedAccessBlobPolicy();

            sasContainerPolicy.SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddHours(24);

            sasContainerPolicy.Permissions = SharedAccessBlobPermissions.List | SharedAccessBlobPermissions.Write;

            string sasContainerToken = imageContainer.GetSharedAccessSignature(sasContainerPolicy);

            var storageInfo = new
            {
                imageUploadUrl = imageContainer.Uri,

                imageUploadSASToken = sasContainerToken,
            };

            return await Task.FromResult(storageInfo);
        }
    }
}
