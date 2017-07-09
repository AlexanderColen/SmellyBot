using System;

namespace SmellyDiscordBot
{
    [Serializable]
    class DuplicateCommandException : Exception
    {
        public DuplicateCommandException()
            : base() { }

        public DuplicateCommandException(string message)
            : base(message) { }

        public DuplicateCommandException(string format, params object[] args)
            : base(string.Format(format, args)) { }

        public DuplicateCommandException(string message, Exception innerException)
            : base(message, innerException) { }

        public DuplicateCommandException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }
    }
}
