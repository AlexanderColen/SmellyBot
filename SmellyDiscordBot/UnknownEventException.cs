using System;

namespace SmellyDiscordBot
{
    class UnknownEventException : Exception
    {
        public UnknownEventException()
            : base() { }

        public UnknownEventException(string message)
            : base(message) { }

        public UnknownEventException(string format, params object[] args)
            : base(string.Format(format, args)) { }

        public UnknownEventException(string message, Exception innerException)
            : base(message, innerException) { }

        public UnknownEventException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }
    }
}
