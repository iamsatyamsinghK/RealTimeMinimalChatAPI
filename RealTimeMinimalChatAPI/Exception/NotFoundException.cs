using System;
namespace RealTimeMinimalChatAPI.Exception
{
    public class NotFoundException : IOException
    {
        public NotFoundException(string message) : base(message)
        {
        }
    }
    public class BadRequestException : IOException
    {
        public BadRequestException() : base() { }
        public BadRequestException(string message) : base(message) { }
        public BadRequestException(string message, IOException innerException) : base(message, innerException) { }
    }
}
