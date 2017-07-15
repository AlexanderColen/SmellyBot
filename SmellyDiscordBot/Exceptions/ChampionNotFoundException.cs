using System;

namespace SmellyDiscordBot
{
    [Serializable]
    class ChampionNotFoundException : Exception
    {
        public ChampionNotFoundException()
            : base() { }

        public ChampionNotFoundException(string message)
            : base(message) { }

        public ChampionNotFoundException(string format, params object[] args)
            : base(string.Format(format, args)) { }

        public ChampionNotFoundException(string message, Exception innerException)
            : base(message, innerException) { }

        public ChampionNotFoundException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException) { }
    }
}
