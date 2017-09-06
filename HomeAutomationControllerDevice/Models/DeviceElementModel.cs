using Newtonsoft.Json;

namespace HomeAutomationControllerDevice.Models
{
    public class DeviceElementModel
    {
        public ElementType ElementType { get; set; }
        public SwitchStatus SwitchStatus { get; set; }
        public double SetValue { get; set; }
        public double PowerConsumptionValue { get; set; }
        public string ToJsonString(Formatting formatting) => JsonConvert.SerializeObject(this, formatting);
    }
}
