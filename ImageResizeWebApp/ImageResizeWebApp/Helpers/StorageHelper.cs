using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using ImageResizeWebApp.Models;
using Microsoft.AspNetCore.Http;
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

        public static async Task<bool> UploadFileToStorage(Stream fileStream, string fileName,
                                                            AzureStorageConfig _storageConfig)
        {
            // Create a URI to the blob
            Uri blobUri = new Uri("https://" +
                                  _storageConfig.AccountName +
                                  ".blob.core.windows.net/" +
                                  _storageConfig.ImageContainer +
                                  "/" + fileName);

            // Create StorageSharedKeyCredentials object by reading
            // the values from the configuration (appsettings.json)
            StorageSharedKeyCredential storageCredentials =
                new StorageSharedKeyCredential(_storageConfig.AccountName, _storageConfig.AccountKey);

            // Create the blob client.
            BlobClient blobClient = new BlobClient(blobUri, storageCredentials);

            // Upload the file
            await blobClient.UploadAsync(fileStream);

            return await Task.FromResult(true);
        }

        public static async Task<List<string>> GetThumbNailUrls(AzureStorageConfig _storageConfig)
        {
            List<string> thumbnailUrls = new List<string>();

            // Create a URI to the storage account
            Uri accountUri = new Uri("https://" + _storageConfig.AccountName + ".blob.core.windows.net/");

            // Create BlobServiceClient from the account URI
            BlobServiceClient blobServiceClient = new BlobServiceClient(accountUri);

            // Get reference to the container
            BlobContainerClient container = blobServiceClient.GetBlobContainerClient(_storageConfig.ThumbnailContainer);

            if (container.Exists())
            {
                // Set the expiration time and permissions for the container.
                // In this case, the start time is specified as a few 
                // minutes in the past, to mitigate clock skew.
                // The shared access signature will be valid immediately.
                BlobSasBuilder sas = new BlobSasBuilder
                {
                    Resource = "c",
                    BlobContainerName = _storageConfig.ThumbnailContainer,
                    StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                    ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
                };

                sas.SetPermissions(BlobContainerSasPermissions.All);

                // Create StorageSharedKeyCredentials object by reading
                // the values from the configuration (appsettings.json)
                StorageSharedKeyCredential storageCredential =
                    new StorageSharedKeyCredential(_storageConfig.AccountName, _storageConfig.AccountKey);

                // Create a SAS URI to the storage account
                UriBuilder sasUri = new UriBuilder(accountUri);
                sasUri.Query = sas.ToSasQueryParameters(storageCredential).ToString();

                foreach (BlobItem blob in container.GetBlobs())
                {
                    // Create the URI using the SAS query token.
                    string sasBlobUri = container.Uri + "/" +
                                        blob.Name + sasUri.Query;

                    //Return the URI string for the container, including the SAS token.
                    thumbnailUrls.Add(sasBlobUri);
                }
            }
            return await Task.FromResult(thumbnailUrls);
        }
    }
}
