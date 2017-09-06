using Microsoft.Azure.Devices.Client;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace HomeAutomationControllerDevice.Helpers
{
    public class DeviceClientHelper
    {
        public DeviceClientHelper(string deviceId, string connectionString, TransportType protocol = TransportType.Amqp)
        {
            ConnectionString = connectionString;
            DeviceId = deviceId;
            _isConnected = false;
            _client = DeviceClient.CreateFromConnectionString(ConnectionString, protocol);
        }
        public async Task OpenConnectionAsync(TransportType protocol = TransportType.Amqp)
        {
            try
            {
                if ((_client != null) && _isConnected == false)
                {
                    await _client.OpenAsync();
                    _isConnected = true;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred while opening a connection to IoT Hub (message:{e.Message}).");
            }
        }

        public async Task SendDeviceTelemetryAsync(string data)
        {
            if (_isConnected)
            {
                try
                {
                    var message = new Message(Encoding.UTF8.GetBytes(data));
                    await _client.SendEventAsync(message);
                    Debug.WriteLine($"message sent: {data}");
                }
                catch (Exception ex)
                {
                    throw new Exception($"An error occurred while sending a telemetry IoT Hub (message:{ex.Message}).");
                }
            }
        }

        public async Task CloseConnectionAsync()
        {
            try
            {
                if (_isConnected)
                {
                    await _client.CloseAsync();
                    _isConnected = false;
                }
            }
            catch (Exception e)
            {
                throw new Exception($"An error occurred while closing a connection from IoT Hub (message:{e.Message}).");
            }

        }

        public DeviceClient _client;
        private bool _isConnected;
        public string ConnectionString { get; }
        public string DeviceId { get; }
    }
}
