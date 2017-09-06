using Microsoft.Azure.Devices.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using HomeAutomationControllerDevice.Helpers;
using HomeAutomationControllerDevice.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HomeAutomationControllerDevice
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly I2cHelper _ledcontrol;
        private readonly bool _isI2cIntialized = false;

        public const string DeviceId = "homehub-01";
        public const string Longitude = "0.00034";
        public const string Latitude = "13.22234";

        public MainPage()
        {
            this.InitializeComponent();
            this.InitializeApplicationAsync();
            try
            {
                _ledcontrol = new I2cHelper();
                _isI2cIntialized = true;
            }
            catch
            {
                _isI2cIntialized = false;
            }
        }

        public async void InitializeApplicationAsync()
        {
            _connectionStatusLED.Fill = _greyColorBrush;
            _cameraHelper = new CameraHelper(_captureElement);
            _blobStorageHepler = new BlobStorageHelper();

            _iothubclient = new DeviceClientHelper(DeviceId, String.Format($"HostName=home-automation-iothub.azure-devices.net;DeviceId={DeviceId};SharedAccessKey=ekgleg2ZT7hICYSY5wJCCZzOgp3RhdnIPt0W35Kojw0="));
            _iothubclient._client.SetConnectionStatusChangesHandler(OnConnectionStatusChagned);

            await _iothubclient._client.SetMethodHandlerAsync("SetDeviceElementSwitchStatus", OnSetDeviceElementSwitchStatus, null);
            await _iothubclient._client.SetMethodHandlerAsync("SetDeviceElementValue", OnSetDeviceElementValue, null);
            await _iothubclient._client.SetMethodHandlerAsync("CaptureImage", OnImageCapture, null);
            await _iothubclient._client.SetMethodHandlerAsync("OnTemerature", OnReadTemperature, null);
            await _iothubclient._client.SetMethodHandlerAsync("OnHumidity", OnReadHumidity, null);
            await _iothubclient._client.SetMethodHandlerAsync("OnWeatherForecast", OnReadWeatherForecast, null);
            await _iothubclient._client.SetMethodHandlerAsync("OnDeviceStatus", OnDeviceStatusAsync, null);
            await _iothubclient.OpenConnectionAsync();
            await _cameraHelper.StartCameraPreviewAsync();

            _timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 5) };
            _timer.Tick += OnTimerTicked;
            _timer.Start();

            _cooler = new VirtualDeviceElement(300, ElementType.Cooler);
            _heater = new VirtualDeviceElement(1800, ElementType.Heater);
            _light = new VirtualDeviceElement(100, ElementType.Light);

            _virtualElements = new List<VirtualDeviceElement>()
            {
                _cooler,
                _heater,
                _light,
            };
        }

        #region IOTHUB DIRECT METHODS
        private async Task<MethodResponse> OnSetDeviceElementSwitchStatus(MethodRequest request, object userContext)
        {
            var payload = request.DataAsJson;
            var command = JsonConvert.DeserializeObject<DeviceElementSwitchModel>(payload);
            var response = new MethodResponse(200);

            switch (command.ElementType)
            {
                case ElementType.Cooler:
                {
                    UpdateToggleSwitchAsync(_coolerToggleSwitch, command.SwitchStatus == SwitchStatus.On);
                    response = await Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(payload), 500));
                }
                    break;

                case ElementType.Heater:
                {
                    UpdateToggleSwitchAsync(_heaterToggleSwitch, command.SwitchStatus == SwitchStatus.On);
                    response = await Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(payload), 500));
                }
                    break;

                case ElementType.Light:
                {
                    UpdateToggleSwitchAsync(_lightToggleSwitch, command.SwitchStatus == SwitchStatus.On);
                    response = await Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(payload), 500));
                }
                    break;
            }
            return response;
        }

        private async Task<MethodResponse> OnSetDeviceElementValue(MethodRequest request, object userContext)
        {
            var payload = request.DataAsJson;
            var command = JsonConvert.DeserializeObject<DeviceElementValueModel>(payload);
            var response = new MethodResponse(200);

            switch (command.ElementType)
            {
                case ElementType.Cooler:
                {
                    UpdateValueSliderAsync(_coolerOutputSlider, command.SetValue);
                    response = await Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(payload), 500));
                }
                    break;
                case ElementType.Heater:
                {
                    UpdateValueSliderAsync(_heaterOutputSlider, command.SetValue);
                    response = await Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(payload), 500));
                }
                    break;
                case ElementType.Light:
                {
                    UpdateValueSliderAsync(_lightOutputSlider, command.SetValue);
                    response = await Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(payload), 500));
                }
                    break;
            }
            return response;
        }

        private async Task<MethodResponse> OnImageCapture(MethodRequest request, object userContext)
        {
            var payload = request.DataAsJson;
            var command = JsonConvert.DeserializeObject<DeviceElementSwitchModel>(payload);
            var response = new MethodResponse(200);

            if (command.ElementType == ElementType.Camera)
            {
                UpdateButtonStatusAsync(_takePhotoButton, false);
                try
                {
                    var blobUri = await CaptureImageAndSaveToBlobAsync();
                    var data = new
                    {
                        deviceId = DeviceId,
                        imageBlobUrl = blobUri
                    };
                    response = await Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)), 500));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"An error occured while capturing a photo from the camera (message: {ex.Message}).");
                    response = await Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(""), 200));
                }
                UpdateButtonStatusAsync(_takePhotoButton, true);
            }
            return response;
        }

        private async Task<MethodResponse> OnReadTemperature(MethodRequest request, object userContext)
        {
            var data = JsonConvert.SerializeObject(new
            {
                deviceId = DeviceId,
                value = _temperature
            });
            var response = await Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(data), 500));
            return response;
        }
        private async Task<MethodResponse> OnReadHumidity(MethodRequest request, object userContext)
        {
            var data = JsonConvert.SerializeObject(new
            {
                deviceId = DeviceId,
                value = _humidity
            });
            var response = await Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(data), 500));
            return response;
        }
        private async Task<MethodResponse> OnReadWeatherForecast(MethodRequest request, object userContext)
        {
            var data = JsonConvert.SerializeObject(new
            {
                deviceId = DeviceId,
                value = _weatherForecast
            });
            var response = await Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(data), 500));
            return response;
        }
        private Task<MethodResponse> OnDeviceStatusAsync(MethodRequest request, object userContext)
        {
            return Task.FromResult(new MethodResponse(Encoding.UTF8.GetBytes(DeviceElementTelemetry()), 500));
        }
        #endregion

        private async void OnTimerTicked(object sender, object e)
        {
            _temperature = Dht22Helper.ReadTemperature();
            _humidity = Dht22Helper.ReadHumidity();
            _weatherForecast = await GetWeatherForecast(_temperature, _humidity);
            _coolerPowerConsumed = _virtualElements.Find(device => device.ElementType == ElementType.Cooler).PowerConsumptionValue;
            _heaterPowerConsumed = _virtualElements.Find(device => device.ElementType == ElementType.Heater).PowerConsumptionValue;
            _lightPowerConsumed = _virtualElements.Find(device => device.ElementType == ElementType.Light).PowerConsumptionValue;
            _totalEnergyConsumedCurrent = _coolerPowerConsumed + _heaterPowerConsumed + _lightPowerConsumed;
            await UpdateUserInterfaceTextsAsync();

            if (_isConnectedToAzure)
            {
                await _iothubclient.SendDeviceTelemetryAsync(JsonConvert.SerializeObject(new
                {
                    deviceId = DeviceId,
                    latitude = Latitude,
                    longitude = Longitude,
                    time = DateTime.Now.ToString("o"),
                    temperature = _temperature,
                    humidity = _humidity,
                    cooler = _coolerPowerConsumed,
                    heater = _heaterPowerConsumed,
                    light = _lightPowerConsumed,
                    forecast = _weatherForecast,
                    totalPower = _totalEnergyConsumedCurrent
                }));
            }
        }
        private string DeviceElementTelemetry()
        {
            var elementModel = new DeviceModel()
            {
                DeviceId = DeviceId,
                Elements = _virtualElements,
                Humidity = _humidity,
                Temperature = _temperature,
                Forecast = _weatherForecast,
                Location = new Location()
                {
                    Latitude = Latitude,
                    Longitude = Longitude
                }
            };
            return JsonConvert.SerializeObject(elementModel);
        }

        private async void UpdateToggleSwitchAsync(ToggleSwitch toogleSwitch, bool isOn)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                toogleSwitch.IsOn = isOn;
            });
        }

        private async void UpdateButtonStatusAsync(Button button, bool isEnabled)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                button.IsEnabled = isEnabled;
            });
        }

        private async void UpdateValueSliderAsync(Slider valueSlider, double value)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                valueSlider.Value = value;
            });
        }
        private async void UpdateIndicatorElementsAsync(Ellipse ellipse, Brush value)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                ellipse.Fill = value;
            });
        }

        private async Task UpdateUserInterfaceTextsAsync()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                _currentTemperature.Text = string.Format("{0:0.00}", Math.Truncate(_temperature * 10) / 10);
                _currentHumidity.Text = string.Format("{0:0.00}", Math.Truncate(_humidity * 10) / 10);
                _forecast.Text = string.Format("{0:0.00}", Math.Truncate((_weatherForecast * 100) * 10) / 10);
                _coolerPowerConsumption.Text = string.Format("{0:0.00}", Math.Truncate(_coolerPowerConsumed * 10) / 10);
                _heaterPowerConsumption.Text = string.Format("{0:0.00}", Math.Truncate(_heaterPowerConsumed * 10) / 10);
                _lightPowerConsumption.Text = string.Format("{0:0.00}", Math.Truncate(_lightPowerConsumed * 10) / 10);
                _totalPowerConsumption.Text = string.Format("{0:0.00}", Math.Truncate(_totalEnergyConsumedCurrent * 10) / 10);
            });
        }

        private void OnConnectionStatusChagned(ConnectionStatus status, ConnectionStatusChangeReason reason)
        {
            if (status == ConnectionStatus.Connected)
            {
                UpdateIndicatorElementsAsync(_connectionStatusLED, _blueColorBrush);
                _isConnectedToAzure = true;

            }
            else if (status == ConnectionStatus.Disconnected_Retrying)
            {
                UpdateIndicatorElementsAsync(_connectionStatusLED, _redColorBrush);
                _isConnectedToAzure = false;
            }
            else
            {
                UpdateIndicatorElementsAsync(_connectionStatusLED, _greyColorBrush);
                _isConnectedToAzure = false;
            }
        }

        #region UI CONTROL EVENT HANDLERS
        private void OnLightOutputSliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            _light.SetValue = e.NewValue;
        }
        private void OnHeaterOutputSliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            _heater.SetValue = e.NewValue;
        }
        private void OnCoolerOutputSliderValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            _cooler.SetValue = e.NewValue;
        }
        private void OnLightSwitchToggled(object sender, RoutedEventArgs e)
        {

            if (((ToggleSwitch)sender).IsOn)
            {
                _light.Switch(SwitchStatus.On);
                if (_isI2cIntialized) _ledcontrol.setLight();
            }
            else
            {
                _light.Switch(SwitchStatus.Off);
                _light.SetValue = 0.00;
                _lightOutputSlider.Value = 0.00;
                if (_isI2cIntialized) _ledcontrol.setOFF();
            }
        }
        private void OnCoolerSwitchToggled(object sender, RoutedEventArgs e)
        {
            if (((ToggleSwitch)sender).IsOn)
            {
                _cooler.Switch(SwitchStatus.On);
                if (_isI2cIntialized) _ledcontrol.SetFanOn();
            }
            else
            {
                _cooler.Switch(SwitchStatus.Off);
                _cooler.SetValue = 0.00;
                _coolerOutputSlider.Value = 0.00;
                if (_isI2cIntialized) _ledcontrol.SetFanOff();
            }
        }
        private void OnHeaterSwitchToggled(object sender, RoutedEventArgs e)
        {
            if (((ToggleSwitch)sender).IsOn)
            {
                _heater.Switch(SwitchStatus.On);
            }
            else
            {
                _heater.Switch(SwitchStatus.Off);
                _heater.SetValue = 0.00;
                _heaterOutputSlider.Value = 0.00;
            }
        }
        #endregion

        private async void OnTakePhotoButtonClickedAsync(object sender, RoutedEventArgs e)
        {
            _takePhotoButton.IsEnabled = false;
            try
            {
                await CaptureImageAndSaveToBlobAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occured while capturing a photo from the camera (message: {ex.Message}).");
            }
            _takePhotoButton.IsEnabled = true;
        }

        private async Task<string> CaptureImageAndSaveToBlobAsync()
        {
            var blobAddress = "";
            try
            {
                var imagePath = await _cameraHelper.TakePhotoAsync();
                blobAddress = await _blobStorageHepler.SaveImageToBlobAsync(imagePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"An error occurred while capturing and saving an image (message: {ex.Message}).");
            }
            return blobAddress;
        }

        private async Task<double> GetWeatherForecast(double humidity, double temperature)
        {
            var forecast = 0.00;
            using (var client = new HttpClient())
            {
                var scoreRequest = new
                {
                    Inputs = new Dictionary<string, StringTable>() {
                        {
                            "input1",
                            new StringTable()
                            {
                                ColumnNames = new string[] {"temperature", "humidity"},
                                Values = new string[,] {  { ((int)temperature).ToString(), ((int)humidity).ToString()}, { ((int)temperature).ToString(), ((int)humidity).ToString() },  }
                            }
                        },
                    },
                    GlobalParameters = new Dictionary<string, string>()
                    {
                    }
                };
                const string apiKey = "n2k0hwKWwU2FDj5treQlT3bopWW+DxFCacO7dbl3VzmuE6ZXh0otOBIxhAZZPiE7tVl8Ygk8XCnkkFj5/mrRFQ=="; // Replace this with the API key for the web service
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                client.BaseAddress = new Uri("https://ussouthcentral.services.azureml.net/workspaces/8088888da3db41b3b5e5f66af6af79b9/services/7d3438c65dc349b3bd5e7fc293d0a3d4/execute?api-version=2.0&details=true");
                var response = await client.PostAsJsonAsync("", scoreRequest);

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var result = JObject.Parse(await response.Content.ReadAsStringAsync())["Results"]["output1"]["value"]["Values"][0][3];
                        if (!Double.TryParse(result.ToString(), out forecast)) _weatherForecast = 0.00;
                    }
                    catch (Exception)
                    {
                        forecast = 0.00;
                    }
                }
                else
                {
                    Debug.WriteLine(string.Format("The request failed with status code: {0}", response.StatusCode));
                    // Print the headers - they include the requert ID and the timestamp, which are useful for debugging the failure
                    Debug.WriteLine(response.Headers.ToString());
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine(responseContent);
                }
                return forecast;
            }
        }
    

    private bool _isConnectedToAzure = false;
        private double _temperature = 0.00;
        private double _humidity = 0.00;
        private double _weatherForecast = 0.00;

        private DeviceClientHelper _iothubclient;
        private CameraHelper _cameraHelper;
        private DispatcherTimer _timer;
        private List<VirtualDeviceElement> _virtualElements;
        private BlobStorageHelper _blobStorageHepler;

        private readonly SolidColorBrush _blueColorBrush = new SolidColorBrush() { Color = Color.FromArgb(255, 50, 150, 255) };
        private readonly SolidColorBrush _greyColorBrush = new SolidColorBrush() { Color = Color.FromArgb(255, 50, 50, 100) };
        private readonly SolidColorBrush _redColorBrush = new SolidColorBrush() { Color = Color.FromArgb(255, 255, 150, 50) };

        private VirtualDeviceElement _cooler;
        private VirtualDeviceElement _heater;
        private VirtualDeviceElement _light;

        private double _coolerPowerConsumed;
        private double _heaterPowerConsumed;
        private double _lightPowerConsumed;
        private double _totalEnergyConsumedCurrent;
    }

    public class StringTable
    {
        public string[] ColumnNames { get; set; }
        public string[,] Values { get; set; }
    }
}