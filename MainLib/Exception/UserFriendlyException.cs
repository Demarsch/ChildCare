using System;

namespace Core
{
    /// <summary>
    /// Represents an exception that contains user-friendly message
    /// </summary>
    public class UserFriendlyException : Exception
    {
        public UserFriendlyException()
        {
        }

        public UserFriendlyException(string message) : base(message)
        {
        }

        public UserFriendlyException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
