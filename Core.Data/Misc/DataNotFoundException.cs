using System;

namespace Core.Data.Misc
{
    public class DataNotFoundException : Exception
    {
        public DataNotFoundException()
        {
        }

        public DataNotFoundException(string message) : base(message)
        {
        }
    }
}
