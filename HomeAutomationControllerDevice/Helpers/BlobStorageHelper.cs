using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;

namespace HomeAutomationControllerDevice.Helpers
{
    public class BlobStorageHelper
    {
        public BlobStorageHelper()
        {
            try
            {
                _storageAccount = CloudStorageAccount.Parse(_storageAccoutConnectionString);
                _blobClient = _storageAccount.CreateCloudBlobClient();
                _blobContainer = _blobClient.GetContainerReference("images");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred while initializing Azure Blob Storage (message: {ex.Message}).");
            }
        }

        public async Task<string> SaveImageToBlobAsync(string path)
        {
            var file = await StorageFile.GetFileFromPathAsync(path);
 
            try
            {
                _blockBlob = _blobContainer.GetBlockBlobReference(file.Name);
                await _blockBlob.UploadFromFileAsync(file);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"An error occurred while saving the image to Azure Blob (message: {e.Message}).");
            }
            return _blockBlob.Uri.OriginalString;
        }

        private string _storageAccountEndPoint = "https://iotworkshopstorage.blob.core.windows.net/";
        private CloudBlobClient _blobClient;
        private CloudStorageAccount _storageAccount;
        private CloudBlobContainer _blobContainer;
        private string _storageAccountName = "iotworkshopstorage";
        private string _storageAccountKey = "+xbREmJK8DtM4mTGyi/rgWzESso5D7uxVJvPWbNAPT0HUvcZBQ885lWgEjXJgegICpq4blmhjnLempnLH1lG/g==";
        private string _blobContainerName = "images";
        private const string _storageAccoutConnectionString = "DefaultEndpointsProtocol=https;AccountName=iotworkshopstorage;AccountKey=+xbREmJK8DtM4mTGyi/rgWzESso5D7uxVJvPWbNAPT0HUvcZBQ885lWgEjXJgegICpq4blmhjnLempnLH1lG/g==;EndpointSuffix=core.windows.net";
        private CloudBlockBlob _blockBlob;
    }

}