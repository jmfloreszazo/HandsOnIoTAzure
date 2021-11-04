using System;

namespace csharpConsoleApp.test
{
    public class TestB
    {
        private TestB()
        {
            var measurement = 7.5;

            if (measurement < 0.0)
            {
                Console.WriteLine($"Measured value is {measurement}; too low.");
            }
            else if (measurement > 15.0)
            {
                Console.WriteLine($"Measured value is {measurement}; too high.");
            }
            else if (double.IsNaN(measurement))
            {
                Console.WriteLine("Failed measurement.");
            }
            else
            {
                Console.WriteLine($"Measured value is {measurement}.");
            }
        }

    }
}