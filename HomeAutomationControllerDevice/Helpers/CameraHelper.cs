using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.System.Display;
using Windows.UI.Xaml.Controls;

namespace HomeAutomationControllerDevice.Helpers
{
    public class CameraHelper
    {
        public CameraHelper(CaptureElement captureElement)
        {
            _captureElement = captureElement;
        }

        public async Task<string> TakePhotoAsync()
        {
            string filePath = "";

            try
            {
                var picturesLibrary = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
                var guid = Guid.NewGuid().ToString();
                var captureFolder = picturesLibrary.SaveFolder ?? ApplicationData.Current.LocalFolder;
                var file = await captureFolder.CreateFileAsync($"img-{guid}.jpg", CreationCollisionOption.GenerateUniqueName);
                var stream = new InMemoryRandomAccessStream();
                await _mediaCapture.CapturePhotoToStreamAsync(_imageEncodingProperties, stream);
                await ReEncodeAndSavePhotoAsync(stream, file, PhotoOrientation.Normal);
                filePath = file.Path;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception when taking a photo: " + ex.ToString());
            }
            return filePath;
        }

        private async void InitializeCamera()
        {
            if (_isCameraInitialized) return;
            _displayRequest = new DisplayRequest();
            try
            {
                _mediaCapture = new MediaCapture();
                await _mediaCapture.InitializeAsync();
                _imageEncodingProperties = ImageEncodingProperties.CreateJpeg();
                _imageEncodingProperties.Width = 800;
                _imageEncodingProperties.Height = 600;
                _captureElement.Source = _mediaCapture;
                _displayRequest.RequestActive();
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
                _isCameraInitialized = true;

                if (_isCameraPreviewing) return;
                await _mediaCapture.StartPreviewAsync();
                _isCameraPreviewing = true;
            }
            catch (Exception ex)
            {
                _isCameraInitialized = false;
                Debug.WriteLine("MediaCapture initialization failed. {0}", ex.Message);
            }
        }

        public async Task StartCameraPreviewAsync()
        {
            if (_isCameraInitialized)
            {
                try
                {
                    await _mediaCapture.StartPreviewAsync();
                    _isCameraPreviewing = true;
                }
                catch (UnauthorizedAccessException)
                {
                    Debug.WriteLine("The app was denied access to the camera");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("MediaCapture initialization failed. {0}", ex.Message);
                }
            }
            else
            {
                try
                {
                    InitializeCamera();
                }
                catch (Exception ex)
                {
                    _isCameraInitialized = false;
                    Debug.WriteLine($"An error occurred while initializing camera / mediaCapture (message: {ex.Message}).");
                }
            }
        }

        public async Task StopCameraPreviewAsync()
        {
            try
            {
                await _mediaCapture.StopPreviewAsync();
                await CleanupCameraAsync();
                _isCameraPreviewing = false;
            }
            catch
            {
                Debug.WriteLine("The app was denied access to the camera");
            }
        }
        private async Task CleanupCameraAsync()
        {
            Debug.WriteLine("CleanupCameraAsync");
            if (_isCameraInitialized)
            {
                if (_isCameraPreviewing)
                {
                    await StopCameraPreviewAsync();
                }
                _isCameraInitialized = false;
            }

            if (_mediaCapture != null)
            {
                _mediaCapture.Dispose();
                _mediaCapture = null;
            }
        }

        private static async Task ReEncodeAndSavePhotoAsync(IRandomAccessStream stream, StorageFile file, PhotoOrientation photoOrientation)
        {
            using (var inputStream = stream)
            {
                var decoder = await BitmapDecoder.CreateAsync(inputStream);
                using (var outputStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var encoder = await BitmapEncoder.CreateForTranscodingAsync(outputStream, decoder);
                    var properties = new BitmapPropertySet {
                        {
                            "System.Photo.Orientation", new BitmapTypedValue(photoOrientation, PropertyType.UInt16)
                        }
                    };
                    await encoder.BitmapProperties.SetPropertiesAsync(properties);
                    await encoder.FlushAsync();
                }
            }
        }

        private bool _isCameraPreviewing;
        private bool _isCameraInitialized;
        private DisplayRequest _displayRequest;
        private ImageEncodingProperties _imageEncodingProperties;
        private MediaCapture _mediaCapture;
        private readonly CaptureElement _captureElement;
    }
}


