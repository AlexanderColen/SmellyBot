using System;

namespace SmellyDiscordBot.Exceptions
{
    [Serializable]
    class InvalidAPITokenException : Exception
    {
        public InvalidAPITokenException()
            : base()
        { }

        public InvalidAPITokenException(string message)
            : base(message)
        { }

        public InvalidAPITokenException(string format, params object[] args)
            : base(string.Format(format, args))
        { }

        public InvalidAPITokenException(string message, Exception innerException)
            : base(message, innerException)
        { }

        public InvalidAPITokenException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException)
        { }
    }
}
