using Amazon.S3;
using Amazon.S3.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NugetDemo.Infrastructure
{
    public class AmazonS3BucketHelper
    {
        #region '---- Members ----'

        private static IAmazonS3 client;

        // Configuration keys should be added in the app.config/web.config file
        private static string accessKey = ConfigurationManager.AppSettings["AWSAccessKeyId"];
        private static string secretKey = ConfigurationManager.AppSettings["AWSSecretKey"];
        private static string myBucketUrl = ConfigurationManager.AppSettings["MyBucketUrl"];
        private static string serviceUrl = ConfigurationManager.AppSettings["ServiceUrl"];

        #endregion

        #region '---- Methods ----'

        /// <summary>
        /// Method to upload files, audios, videos on amazon storage.
        /// </summary>
        /// <param name="byteArray">byte array stream</param>
        /// <param name="fileName">file name string</param>
        /// <param name="contentType"><file type i.e. Audio Video </param>
        /// <param name="bucketName">bucketname</param>
        /// <returns>true or false</returns>
        public static bool UploadOnAmazon(byte[] byteArray, int contentType, string fileName, string bucketName = "mybucketname")
        {
            try
            {
                Stream fileStream = new MemoryStream(byteArray);
                string fileType = string.Empty;

                // Initializing config file object
                AmazonS3Config config = new AmazonS3Config();
                config.ServiceURL = serviceUrl;

                // Attachment type according to content Type sent in the request.
                switch (contentType)
                {
                    case 1:
                        fileType = "File";
                        break;
                    case 2:
                        fileType = "Audio";
                        break;
                    case 3:
                        fileType = "Video";
                        break;
                    case 4:
                        fileType = "Link";
                        break;
                    default:
                        fileType = "File";
                        break;
                }

                // Key name consists of the path of the folder where we want to save the attachments on amazon storage.
                string keyName = "/messagefiles/" + fileType + "/" + fileName;

                // Put request to store attachments
                using (client = Amazon.AWSClientFactory.CreateAmazonS3Client(accessKey, secretKey, config))
                {
                    PutObjectRequest request = new PutObjectRequest()
                    {
                        BucketName = bucketName,
                        CannedACL = S3CannedACL.PublicRead,
                        InputStream = fileStream,
                        Key = keyName,
                    };

                    client.PutObject(request);
                }

                fileStream.Close();

                return true;
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                // Catch amazon exception.
                //LogHelper.Error("UploadOnAmazon", amazonS3Exception);
                return false;
            }
            catch (Exception ex)
            {
                // Catch general exception.
                //LogHelper.Error("UploadOnAmazon", ex);
                return false;
            }
        }

        /// <summary>
        /// Download from amazon.
        /// </summary>
        /// <param name="destinationPath">destination path string</param>
        /// <param name="fileName">file name string</param>
        /// <param name="contentType"><file type i.e. Audio Video </param>
        /// <param name="bucketName">bucketname</param>
        /// <returns>true or false</returns>
        public static bool DownloadFromAmazon(string destinationPath, string fileName, int contentType, string bucketName = "mybucketname")
        {
            try
            {
                string fileType = string.Empty;

                AmazonS3Config config = new AmazonS3Config();
                config.ServiceURL = serviceUrl;

                // Attachment type according to content Type sent in the request.
                switch (contentType)
                {
                    case 1:
                        fileType = "File";
                        break;
                    case 2:
                        fileType = "Audio";
                        break;
                    case 3:
                        fileType = "Video";
                        break;
                    case 4:
                        fileType = "Link";
                        break;
                    default:
                        fileType = "File";
                        break;
                }

                client = Amazon.AWSClientFactory.CreateAmazonS3Client(
                    accessKey,
                    secretKey,
                    config
                    );

                // Key name consists of the path of the folder where we want to save the attachments on amazon storage.
                string keyName = "/messagefiles/" + fileType + "/" + fileName;

                using (client)
                {
                    GetObjectRequest request = new GetObjectRequest();
                    request.BucketName = bucketName;
                    request.Key = keyName;
                    using (GetObjectResponse response = client.GetObject(request))
                    {
                        response.WriteResponseStreamToFile(destinationPath);
                    }
                }

                return true;
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                // Catch amazon exception.
                //LogHelper.Error("DownloadFromAmazon", amazonS3Exception);
                return false;
            }
            catch (Exception ex)
            {
                // Catch general exception.
                //LogHelper.Error("DownloadFromAmazon", ex);
                return false;
            }

        }

        /// <summary>
        /// Method to get signed url for images stored in amazon S3 bucket.
        /// </summary>
        /// <param name="fileName">file name string</param>
        /// <param name="contentType"><file type i.e. Audio Video </param>
        /// <param name="bucketName">bucketname</param>
        /// <returns>signed url string</returns>
        public static string GetDownloadSignedURLFromAmazon(string fileName, int contentType, string bucketName = "mybucketname")
        {
            try
            {
                string signedURLToDownload = string.Empty;

                AmazonS3Config config = new AmazonS3Config();
                config.ServiceURL = serviceUrl;

                client = Amazon.AWSClientFactory.CreateAmazonS3Client(
                    accessKey,
                    secretKey,
                    config
                    );

                using (client)
                {
                    signedURLToDownload = GeneratePreSignedURL(fileName, contentType, bucketName);
                }

                return signedURLToDownload;
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                throw amazonS3Exception;
            }
        }

        /// <summary>
        /// Get Presigned URL from amazon to download attachments.
        /// The presigned URL generated can be accessed maximum up to 7 days, currently it the access limit is kept for 1 day. 
        /// </summary>
        /// <param name="filename">filename as string</param>
        /// <param name="contentType">content type as integer</param>
        /// <param name="bucketName">bucket name as string</param>
        /// <returns>string - presigned url to download attachment from amazon</returns>
        public static string GeneratePreSignedURL(string filename, int contentType, string bucketName)
        {
            string urlString = "";
            string fileType = string.Empty;

            // Attachment type according to content Type sent in the request.
            switch (contentType)
            {
                case 1:
                    fileType = "File";
                    break;
                case 2:
                    fileType = "Audio";
                    break;
                case 3:
                    fileType = "Video";
                    break;
                case 4:
                    fileType = "Link";
                    break;
                default:
                    fileType = "File";
                    break;
            }

            //// Key name consists of the path of the folder where we want to save the attachments on amazon storage.
            string objectKey = "/messagefiles/" + fileType + "/" + filename;

            GetPreSignedUrlRequest request1 = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = objectKey,
                Expires = DateTime.Now.AddDays(1) // NOTE : The presigned URL generated can be accessed maximum up to 7 days, currently it the access limit is kept for 1 day.
            };

            try
            {
                urlString = client.GetPreSignedURL(request1);
            }
            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null &&
                    (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId")
                    ||
                    amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    Console.WriteLine("Check the provided AWS Credentials.");
                    Console.WriteLine(
                    "To sign up for service, go to http://aws.amazon.com/s3");
                }
                else
                {
                    Console.WriteLine(
                     "Error occurred. Message:'{0}' when listing objects",
                     amazonS3Exception.Message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return urlString;
        }

        #endregion
    }
}
