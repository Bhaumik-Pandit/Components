using System.IO;
using System.Threading.Tasks;

namespace AzureBlobConsole
{
    public interface IAzureHelper
    {
        /// <summary>
        /// Method to upload file on azure blob storage
        /// </summary>
        /// <param name="blobName">blobname</param>
        /// <param name="filePath">filePath</param>
        Task UploadAsync(string blobName, string filePath);

        /// <summary>
        /// Method to upload file on azure blob storage
        /// </summary>
        /// <param name="blobName">blobName</param>
        /// <param name="stream">stream</param>
        Task UploadAsync(string blobName, Stream stream);

        /// <summary>
        /// Method to download from azure blob storage
        /// </summary>
        /// <param name="blobName">blobName</param>
        /// <returns>MemoryStream</returns>
        Task<MemoryStream> DownloadAsync(string blobName);

        /// <summary>
        /// Method to download from azure blob storage
        /// </summary>
        /// <param name="blobName">blobName</param>
        /// <param name="filePath">filePath</param>
        Task DownloadAsync(string blobName, string filePath);
    }
}
