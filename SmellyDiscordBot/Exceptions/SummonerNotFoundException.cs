using System;

namespace SmellyDiscordBot.Exceptions
{
    class SummonerNotFoundException : Exception
    {
        public SummonerNotFoundException()
            : base() { }

        public SummonerNotFoundException(string message)
            : base(message) { }

        public SummonerNotFoundException(string format, params object[] args)
            : base(string.Format(format, args)) { }

        public SummonerNotFoundException(string message, Exception innerException)
            : base(message, innerException) { }

        public SummonerNotFoundException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }
    }
}
