using Newtonsoft.Json;

namespace HomeAutomationControllerDevice.Models
{
    public class DeviceElementSwitchModel
    {
        public string DeviceId;
        public ElementType ElementType { get; set; }
        public SwitchStatus SwitchStatus { get; set; }
        public string ToJsonString(Formatting formatting) => JsonConvert.SerializeObject(this, formatting);
    }
}