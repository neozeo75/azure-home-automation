using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;

namespace HomeAutomationWebAPI.Helpers
{
    public class ServiceClientHelper
    {
        public ServiceClientHelper(string connectionString)
        {
            _serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
        }

        public async Task OpenConnectionAsync()
        {
            if (!_isConnected)
            {
                try
                {
                    await _serviceClient.OpenAsync();
                    _isConnected = true;
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format($"An error occurred while opening connection to iot hub (message: {ex.Message})."));
                }
            }
        }

        public async Task<CloudToDeviceMethodResult> InvokeDeviceMethodAsync(string deviceId, string methodName, string payload)
        {
            var methodInvokation = new CloudToDeviceMethod(methodName).SetPayloadJson(payload);
            var result = new CloudToDeviceMethodResult() {Status = 200};
            try
            {
                result = await _serviceClient.InvokeDeviceMethodAsync(deviceId, methodInvokation);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"An error occurred while invoking direct method on a device (message: {e.Message}).");
            }
            return result;
        }

        public async Task CloseConnectionAsync()
        {
            if (_isConnected)
            {
                try
                {
                    await _serviceClient.CloseAsync();
                    _isConnected = false;
                }
                catch (Exception e)
                {
                    throw new Exception(string.Format($"An error occurred while closing connection from iot hub (message: {e.Message})."));
                }
            }
        }

        private readonly ServiceClient _serviceClient;
        private bool _isConnected = false;
    }
}
