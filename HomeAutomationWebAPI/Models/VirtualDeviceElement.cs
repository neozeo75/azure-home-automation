using System;

namespace HomeAutomationWebAPI.Models
{
    public class VirtualDeviceElement
    {
        public VirtualDeviceElement(double maxPowerConsumption, ElementType elementType)
        {
            SetValue = 0.00;
            ElementType = elementType;
            _random = new Random();
            _powerLimit = maxPowerConsumption;
        }

        private double GetRandomDouble(double maximum, double minimum)
        {
            return _random.NextDouble() * (maximum - minimum) + minimum;
        }
        
        public DeviceElementModel ValueReadings()
        {
            var values = new DeviceElementModel
            {
                ElementType = ElementType,
                SwitchStatus = SwitchStatus,
                SetValue = SetValue,
                PowerConsumptionValue = _powerConsumptionValue
            };
            return values;
        }

        public void Switch(SwitchStatus switchStatus)
        {
            if (switchStatus == SwitchStatus.On) { SwitchStatus = SwitchStatus.On; }
            else
            {
                SwitchStatus = SwitchStatus.Off;
                SetValue = 0.00;
                PowerConsumptionValue = 0.00;
            }
        }

        public double PowerConsumptionValue
        {
            get
            {
                if (SwitchStatus != SwitchStatus.On) { _powerConsumptionValue = 0.00; }
                else
                {
                    _powerConsumptionValue = GetRandomDouble(SetValue + 0.5, SetValue - 0.5) * (_powerLimit / 100);
                    _powerConsumptionValue = _powerConsumptionValue > 0 ? _powerConsumptionValue : 0;
                }
                return _powerConsumptionValue;
            }
            set => _powerConsumptionValue = value;
        }

        public ElementType ElementType { get; set; }
        public double SetValue { get; set; }
        public SwitchStatus SwitchStatus { get; set; }
        private double _powerConsumptionValue;
        private readonly double _powerLimit;
        private readonly Random _random;
    }
}
