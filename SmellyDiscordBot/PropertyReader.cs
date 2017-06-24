using System.IO;

namespace SmellyDiscordBot
{
    public class PropertyReader
    {
        #region Fields
        private char prefix = '!';
        private bool userEventsOn = false;
        private bool channelEventsOn = false;
        private bool roleEventsOn = false;
        private string eventsChannel = "events";
        private string welcomeChannel = "welcome";
        #endregion
        #region Properties
        /// <summary>
        /// Getter for prefix.
        /// </summary>
        /// <returns></returns>
        public char GetPrefix()
        {
            return this.prefix;
        }
        /// <summary>
        /// Getter for userEventsOn.
        /// </summary>
        /// <returns></returns>
        public bool GetUserEvents()
        {
            return this.userEventsOn;
        }
        /// <summary>
        /// Setter for userEventsOn.
        /// </summary>
        /// <param name="value"></param>
        public void SetUserEvents(bool value)
        {
            this.userEventsOn = value;
        }
        /// <summary>
        /// Getter for channelEventsOn.
        /// </summary>
        /// <returns></returns>
        public bool GetChannelEvents()
        {
            return this.channelEventsOn;
        }
        /// <summary>
        /// Setter for channelEventsOn.
        /// </summary>
        /// <param name="value"></param>
        public void SetChannelEvents(bool value)
        {
            this.channelEventsOn = value;
        }
        /// <summary>
        /// Getter for roleEventsOn.
        /// </summary>
        /// <returns></returns>
        public bool GetRoleEvents()
        {
            return this.roleEventsOn;
        }
        /// <summary>
        /// Setter for roleEventsOn.
        /// </summary>
        /// <param name="value"></param>
        public void SetRoleEvents(bool value)
        {
            this.roleEventsOn = value;
        }
        /// <summary>
        /// Getter for eventsChannel;
        /// </summary>
        /// <returns></returns>
        public string GetEventsChannel()
        {
            return this.eventsChannel;
        }
        /// <summary>
        /// Setter for eventsChannel;
        /// </summary>
        /// <param name="channel"></param>
        public void SetEventsChannel(string channel)
        {
            this.eventsChannel = channel;
        }
        /// <summary>
        /// Getter for welcomeChannel.
        /// </summary>
        /// <returns></returns>
        public string GetWelcomeChannel()
        {
            return this.welcomeChannel;
        }
        /// <summary>
        /// Setter for welcomeChannel.
        /// </summary>
        /// <param name="channel"></param>
        public void SetWelcomeChannel(string channel)
        {
            this.welcomeChannel = channel;
        }
        #endregion

        /// <summary>
        /// Constructor for PropertyReader class.
        /// Reads the file and saves the found properties in variables.
        /// </summary>
        public PropertyReader(string filename)
        {
            using (StreamReader sr = new StreamReader(filename))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    //Find prefix
                    if (line.Contains("Prefix="))
                        this.prefix = line.Substring(7).ToCharArray()[0];
                    else if (line.Contains("User Events Toggle="))
                    {
                        if ("true".Equals(line.Substring(18)))
                            this.userEventsOn = true;
                    }
                    else if (line.Contains("Channel Events Toggle="))
                    {
                        if ("true".Equals(line.Substring(22)))
                            this.channelEventsOn = true;
                    }
                    else if (line.Contains("Role Events Toggle="))
                    {
                        if ("true".Equals(line.Substring(19)))
                            this.roleEventsOn = true;
                    }
                    else if (line.Contains("Welcome Channel="))
                        this.welcomeChannel = line.Substring(16);
                    else if (line.Contains("Events Channel="))
                        this.eventsChannel = line.Substring(15);
                }
            }
        }
    }
}
