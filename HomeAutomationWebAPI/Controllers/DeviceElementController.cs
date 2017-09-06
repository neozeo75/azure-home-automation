using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HomeAutomationWebAPI.Helpers;
using HomeAutomationWebAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HomeAutomationWebAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/DeviceElement")]
    public class DeviceElementController : Controller
    {
        private readonly ServiceClientHelper _serviceClientHelper;

        public DeviceElementController()
        {
            _serviceClientHelper = new ServiceClientHelper("HostName=home-automation-iothub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=H22EJy8VsdA0jbzes+lDbXXtg0sC1PMIvphDXfYT0FM=");
        }

        [HttpPost]
        [Route("SetDeviceElementSwitch")]

        public async Task<object> SetDeviceElementSwitchStatus([FromBody] DeviceElementSwitchModel switchModel)
        {
            await _serviceClientHelper.OpenConnectionAsync();
            var result = await _serviceClientHelper.InvokeDeviceMethodAsync(
                switchModel.DeviceId,
                "SetDeviceElementSwitchStatus",
                switchModel.ToJsonString(formatting: Formatting.None));
            Debug.WriteLine($"SetDeviceElementSwitchStatus: status: {result.Status}, payload: {result.GetPayloadAsJson()}");
            await _serviceClientHelper.CloseConnectionAsync();

            var response = new
            {
                time = DateTime.Now.ToString("o"),
                statusCode = result.Status,
                result = result.GetPayloadAsJson()
            };

            return response;
        }

        [HttpPost]
        [Route("SetDeviceElementValue")]

        public async Task<object> SetDeviceElementValue([FromBody] DeviceElementValueModel valueModel)
        {
            await _serviceClientHelper.OpenConnectionAsync();
            var result = await _serviceClientHelper.InvokeDeviceMethodAsync(
                valueModel.DeviceId,
                "SetDeviceElementValue",
                valueModel.ToJsonString(formatting: Formatting.None));
            await _serviceClientHelper.CloseConnectionAsync();

            var response = new
            {
                time = DateTime.Now.ToString("o"),
                statusCode = result.Status,
                result = result.GetPayloadAsJson()
            };

            return response;
        }

        [HttpPost]
        [Route("CaptureImage")]

        public async Task<object> CaptureImage([FromBody] DeviceElementSwitchModel switchModel)
        {
            await _serviceClientHelper.OpenConnectionAsync();
            var result = await _serviceClientHelper.InvokeDeviceMethodAsync(
                switchModel.DeviceId,
                "CaptureImage",
                switchModel.ToJsonString(formatting: Formatting.None));
            await _serviceClientHelper.CloseConnectionAsync();
            var response = new
            {
                time = DateTime.Now.ToString("o"),
                statusCode = result.Status,
                result = result.GetPayloadAsJson()
            };

            return response;
        }

        [HttpGet]
        [Route("Temperature")]

        public async Task<object> ReadTemperature(string deviceId)
        {
            await _serviceClientHelper.OpenConnectionAsync();
            var result = await _serviceClientHelper.InvokeDeviceMethodAsync(
                deviceId,
                "OnTemerature",
                null);
            await _serviceClientHelper.CloseConnectionAsync();
            var response = new
            {
                time = DateTime.Now.ToString("o"),
                statusCode = result.Status,
                result = result.GetPayloadAsJson()
            };
            return response;
        }

        [HttpGet]
        [Route("Humidity")]

        public async Task<object> ReadHumidity(string deviceId)
        {
            await _serviceClientHelper.OpenConnectionAsync();
            var result = await _serviceClientHelper.InvokeDeviceMethodAsync(
                deviceId,
                "OnHumidity",
                null);
            await _serviceClientHelper.CloseConnectionAsync();
            var response = new
            {
                time = DateTime.Now.ToString("o"),
                statusCode = result.Status,
                result = result.GetPayloadAsJson()
            };
            return response;
        }

        [HttpGet]
        [Route("ReadDeviceStatus")]

        public async Task<object> ReadDeviceStatus(string deviceId)
        {
            await _serviceClientHelper.OpenConnectionAsync();
            var result = await _serviceClientHelper.InvokeDeviceMethodAsync(
                deviceId,
                "OnDeviceStatus",
                null);
            await _serviceClientHelper.CloseConnectionAsync();
            var response = new
            {
                time = DateTime.Now.ToString("o"),
                statusCode = result.Status,
                result = result.GetPayloadAsJson()
            };
            return response;
        }
    }
}