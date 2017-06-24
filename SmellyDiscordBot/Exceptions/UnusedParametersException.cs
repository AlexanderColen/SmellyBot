using System;

namespace SmellyDiscordBot
{
    class UnusedParametersException : Exception
    {
        public UnusedParametersException()
            : base() { }

        public UnusedParametersException(string message)
            : base(message) { }

        public UnusedParametersException(string format, params object[] args)
            : base(string.Format(format, args)) { }

        public UnusedParametersException(string message, Exception innerException)
            : base(message, innerException) { }

        public UnusedParametersException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }
    }
}
