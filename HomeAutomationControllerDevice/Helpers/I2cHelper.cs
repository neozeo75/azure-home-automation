using System;
using Windows.Devices.Enumeration;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using Windows.UI.Xaml;


namespace HomeAutomationControllerDevice.Helpers
{
    public sealed class I2cHelper
    {
        private DispatcherTimer timer;

        private const int FAN_PIN = 28;
        private GpioPin pin = null;
        private GpioPinValue pinValue;

        private const byte LEDCONTROL_ADDR = 0x33;
        private const byte LEDCONTROL_CONTROL = 0x00;
        private const byte LEDCONTROL_LIGHT = 0x47;
        private const byte LEDCONTROL_DARK = 0x41;
        private const byte LEDCONTROL_OFF = 0x00;
        private const byte LEDCONTROL_LED_BLINK = 0x40;
        private const string I2C_CONTROLLER_NAME = "I2C1";
        private I2cDevice I2CLight;
        private String Status = "OFF";

        #region Singletone

        private static I2cHelper mInstance = null;

        public static I2cHelper GetInstance()
        {
            if (mInstance == null)
            {
                mInstance = new I2cHelper();
            }
            return mInstance;
        }

        #endregion  // Singleton

        public I2cHelper()
        {
            InitI2CLight();
            InitGPIO();
        }

        public void Close()
        {
            I2CLight.Dispose();
        }

        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            if (gpio == null)
            {
                pin = null;
                throw new Exception("Init FAILED.");
            }

            pin = gpio.OpenPin(FAN_PIN);
            pinValue = GpioPinValue.Low;
            pin.Write(pinValue);
            pin.SetDriveMode(GpioPinDriveMode.Output);
        }

        private async void InitI2CLight()
        {
            try
            {
                string aqs = I2cDevice.GetDeviceSelector(I2C_CONTROLLER_NAME);
                var dis = await DeviceInformation.FindAllAsync(aqs);

                if (dis.Count == 0)
                {
                    return;
                }

                var settings = new I2cConnectionSettings(LEDCONTROL_ADDR);
                settings.BusSpeed = I2cBusSpeed.FastMode;
                settings.SharingMode = I2cSharingMode.Shared;
                I2CLight = await I2cDevice.FromIdAsync(dis[0].Id, settings);

                if (I2CLight == null)
                {
                    return;
                }

                byte[] WriteBuf_PowerControl = new byte[] {LEDCONTROL_CONTROL, 0x03};

                try
                {
                    I2CLight.Write(WriteBuf_PowerControl);
                }
                catch (Exception)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Init FAILED.");
            }
        }

        private ushort I2CRead16(byte addr)
        {

            byte[] address = new byte[] {(byte) (addr)};
            byte[] data = new byte[1];

            if (I2CLight == null)
                InitI2CLight();

            I2CLight.WriteRead(address, data);

            return (ushort) data[0];
        }

        private void setLed_Control(byte writingByte)
        {
            uint[] Data = new uint[2];

            Data[0] = I2CRead16(0x01);
            byte bByte = (byte) Data[0];

            byte[] WriteBuf_PowerControl = new byte[] {LEDCONTROL_CONTROL, writingByte};

            try
            {
                if (I2CLight == null)
                {
                    InitI2CLight();
                }

                I2CLight.Write(WriteBuf_PowerControl);
            }
            catch (Exception)
            {
                return;
            }
        }

        private void getLed_Control(byte writingByte)
        {

            uint[] Data = new uint[2];

            Data[0] = I2CRead16(0x01);
            byte bByte = (byte) Data[0];

            byte[] WriteBuf_PowerControl = new byte[] {LEDCONTROL_CONTROL, writingByte};

            try
            {
                I2CLight.Write(WriteBuf_PowerControl);
            }
            catch (Exception)
            {
                return;
            }

        }

        public string getLed()
        {

            //return Status;
            uint[] Data = new uint[2];
            Data[0] = I2CRead16(0x01);
            byte readByte = 0x00;
            byte bByte = (byte) Data[0];
            //readByte = (byte)(bByte & LEDCONTRO_LED_BLINK);
            if ((byte) (bByte & LEDCONTROL_LED_BLINK) == LEDCONTROL_LED_BLINK)
            {
                return "ON";
            }
            else return "OFF";
        }

        public void setLight()
        {

            setLed_Control(0x47);
        }

        public void setDark()
        {
            Status = "ON";
            setLed_Control(0x41);
        }

        public void setOFF()
        {
            setLed_Control(0x00);
        }


        public void SetFanOn()
        {
            pin.Write(GpioPinValue.High);
        }


        public void SetFanOff()
        {
            try
            {
                pin.Write(GpioPinValue.Low);
            }
            catch
            {

            }
        }
    }
}