using System;

namespace HomeAutomationControllerDevice.Helpers
{
    public static class Dht22Helper
    {
        public static double ReadTemperature(double minimum = 26.5, double maximum = 28.5)
        {
            _random = new Random();
            return _random.NextDouble() * (maximum - minimum) + minimum;
        }
        public static double ReadHumidity(double minimum = 45.5, double maximum = 55.5)
        {
            _random = new Random();
            return _random.NextDouble() * (maximum - minimum) + minimum;
        }

        private static Random _random;
    }

}
