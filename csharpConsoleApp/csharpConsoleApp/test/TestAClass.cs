using System;

namespace csharpConsoleApp.test
{
    public class TestA
    {
        private TestA()
        {
            var measurement = 7.5;

            switch (measurement)
            {
                case < 0.0:
                    Console.WriteLine($"Measured value is {measurement}; too low.");
                    break;

                case > 15.0:
                    Console.WriteLine($"Measured value is {measurement}; too high.");
                    break;

                case double.NaN:
                    Console.WriteLine("Failed measurement.");
                    break;

                default:
                    Console.WriteLine($"Measured value is {measurement}.");
                    break;
            }
        }

    }
}