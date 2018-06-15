using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AzureBlobConsole
{
    public class AzureHelper
    {
        private const string storageAccountName = "<storageAccountName>";
        private const string storageKey = "<storageKey>";
        private const string storageConnectionString = "<storageConnectionString>";

        #region '---- Private Methods ----'

        /// <summary>
        /// Method to get cloud blob container
        /// </summary>
        /// <returns>CloudBlobContainer</returns>
        private async Task<CloudBlobContainer> GetContainerAsync()
        {
            //Account
            CloudStorageAccount storageAccount = new CloudStorageAccount(new StorageCredentials(storageAccountName, storageKey), false);

            //Client
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            //Container
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(storageAccountName);
            await blobContainer.CreateIfNotExistsAsync();

            return blobContainer;
        }

        /// <summary>
        /// Method to get cloud block blob
        /// </summary>
        /// <param name="blobName">blob name string</param>
        /// <returns>CloudBlockBlob</returns>
        private async Task<CloudBlockBlob> GetBlockBlobAsync(string blobName)
        {
            //Container
            CloudBlobContainer blobContainer = await GetContainerAsync();

            //Blob
            CloudBlockBlob blockBlob = blobContainer.GetBlockBlobReference(blobName);

            return blockBlob;
        }

        /// <summary>
        /// Method to get list of blobs available
        /// </summary>
        /// <param name="useFlatListing">true or false</param>
        /// <returns>List of AzureBlobItem</returns>
        private async Task<List<AzureBlobItem>> GetBlobListAsync(bool useFlatListing = true)
        {
            //Container
            CloudBlobContainer blobContainer = await GetContainerAsync();

            //List
            var list = new List<AzureBlobItem>();
            BlobContinuationToken token = null;
            do
            {
                BlobResultSegment resultSegment =
                    await blobContainer.ListBlobsSegmentedAsync("", useFlatListing,
                          new BlobListingDetails(), null, token, null, null);
                token = resultSegment.ContinuationToken;

                foreach (IListBlobItem item in resultSegment.Results)
                {
                    list.Add(new AzureBlobItem(item));
                }
            } while (token != null);

            return list.OrderBy(i => i.Folder).ThenBy(i => i.Name).ToList();
        }

        /// <summary>
        /// Method to get content type based on extension
        /// </summary>
        /// <param name="extenstion"></param>
        /// <returns>content type string</returns>
        private string GetFileContentTypes(string extension)
        {
            string ContentType = string.Empty;
            string Extension = extension.ToLower();

            switch (Extension)
            {
                case "pdf":
                    ContentType = "application/pdf";
                    break;
                case "txt":
                    ContentType = "text/plain";
                    break;
                case "bmp":
                    ContentType = "image/bmp";
                    break;
                case "gif":
                    ContentType = "image/gif";
                    break;
                case "png":
                    ContentType = "image/png";
                    break;
                case "jpg":
                    ContentType = "image/jpeg";
                    break;
                case "jpeg":
                    ContentType = "image/jpeg";
                    break;
                case "xls":
                    ContentType = "application/vnd.ms-excel";
                    break;
                case "xml":
                    ContentType = "text/xml";
                    break;
                default:
                    ContentType = "application/octet-stream";
                    break;

            }
            return ContentType;
        }

        #endregion

        #region '---- Public Methods ----'

        /// <summary>
        /// Method to upload file on azure blob storage
        /// </summary>
        /// <param name="blobName">blobname</param>
        /// <param name="filePath">filePath</param>
        public async Task UploadAsync(string blobName, string filePath)
        {
            //Blob
            CloudBlockBlob blockBlob = await GetBlockBlobAsync(string.Concat(blobName, Path.GetExtension(filePath)));

            //Upload
            using (var fileStream = File.Open(filePath, FileMode.Open))
            {
                fileStream.Position = 0;
                blockBlob.Properties.ContentType = GetFileContentTypes(Path.GetExtension(filePath));
                await blockBlob.UploadFromStreamAsync(fileStream);
            }
        }

        /// <summary>
        /// Method to upload file on azure blob storage
        /// </summary>
        /// <param name="blobName">blobName</param>
        /// <param name="stream">stream</param>
        public async Task UploadAsync(string blobName, Stream stream)
        {
            //Blob
            CloudBlockBlob blockBlob = await GetBlockBlobAsync(blobName);

            //Upload
            stream.Position = 0;
            await blockBlob.UploadFromStreamAsync(stream);
        }

        /// <summary>
        /// Method to download from azure blob storage
        /// </summary>
        /// <param name="blobName">blobName</param>
        /// <returns>MemoryStream</returns>
        public async Task<MemoryStream> DownloadAsync(string blobName)
        {
            //Blob
            CloudBlockBlob blockBlob = await GetBlockBlobAsync(blobName);

            //Download
            using (var stream = new MemoryStream())
            {
                await blockBlob.DownloadToStreamAsync(stream);
                return stream;
            }
        }

        /// <summary>
        /// Method to download from azure blob storage
        /// </summary>
        /// <param name="blobName">blobName</param>
        /// <param name="filePath">filePath</param>
        public async Task DownloadAsync(string blobName, string filePath)
        {
            //Blob
            CloudBlockBlob blockBlob = await GetBlockBlobAsync(string.Concat(blobName, Path.GetExtension(filePath)));

            blockBlob.Properties.ContentType = GetFileContentTypes(Path.GetExtension(filePath));

            //Download
            await blockBlob.DownloadToFileAsync(filePath, FileMode.Create);
        }

        #endregion
    }
}
