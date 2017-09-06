using Newtonsoft.Json;

namespace HomeAutomationControllerDevice.Models
{
    public class DeviceElementValueModel
    {
        public string DeviceId;
        public ElementType ElementType { get; set; }
        public double SetValue { get; set; }
        public string ToJsonString(Formatting formatting) => JsonConvert.SerializeObject(this, formatting);
    }
}