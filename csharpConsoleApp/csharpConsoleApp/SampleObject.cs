using System.Data;

namespace csharpConsoleApp
{
    public class SampleObject
    {
        public int TestProperty
        {
            get
            {
                int test = 0;
                test = 1;
                return test;
            }
        }

        public SampleObject()
        {
            //...
        }

        public override string ToString()
        {
            return "I'm a sample object.";
        }
    }
}