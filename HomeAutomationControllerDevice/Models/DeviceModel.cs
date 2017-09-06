using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HomeAutomationControllerDevice.Models
{
    public class DeviceModel
    {
        public string DeviceId { get; set; }
        public DateTime Time { get; set; }
        public Location Location { get; set; }
        public List<VirtualDeviceElement> Elements { get; set; }
        public string ToJsonString(Formatting formatting) => JsonConvert.SerializeObject(this, formatting);
    }
}