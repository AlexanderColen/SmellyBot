using System;

namespace SmellyDiscordBot.Exceptions
{
    [Serializable]
    class RegionNotFoundException : Exception
    {
        public RegionNotFoundException()
            : base()
        { }

        public RegionNotFoundException(string message)
            : base(message)
        { }

        public RegionNotFoundException(string format, params object[] args)
            : base(string.Format(format, args))
        { }

        public RegionNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        { }

        public RegionNotFoundException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException)
        { }
    }
}
