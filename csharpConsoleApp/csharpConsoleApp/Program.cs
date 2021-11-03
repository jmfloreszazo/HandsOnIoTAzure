using System;
using System.Collections.Generic;
using System.Linq;

namespace csharpConsoleApp
{
    class Program
    {
        private static IEnumerable<int> FindNumber(Predicate<int> predicate)
        {
            for (int i = 0; i <= 100; i++)
            {
                if (predicate(i))
                    yield return i;
            }
        }

        static void Main(string[] args)
        {

            IEnumerable<int> nums = FindNumber(n => n % 10 == 0);

            Console.Write(nums);

            int[] numbers = new[] { 1, 2, 3, 4, 5 };
            foreach (var n in numbers)
            {
                Console.Write("{0} ", n);
            }

            var anonymousObject = new
            {
                Id = 1,
                Name = "User One",
                GDPR = true
            };

            Console.WriteLine(anonymousObject);

            if (anonymousObject.GDPR)
            {
                Console.WriteLine("Save user in data base.");
            }

            var listSampeObjects = new List<SampleObject>();

            var test = from t in listSampeObjects
                       where t.TestProperty == 1 
                       orderby t.TestProperty 
                       select new { test = t.TestProperty };

        }
    }
}
