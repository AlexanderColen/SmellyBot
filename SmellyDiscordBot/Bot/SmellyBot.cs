using Discord;
using Discord.Commands;
using SmellyDiscordBot.Bot;
using SmellyDiscordBot.League;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SmellyDiscordBot
{
    public class SmellyBot
    {
        #region Fields
        private DiscordClient client;
        private CommandService commands;
        private LeagueStats stats = null;
        #endregion
        private enum eventType
        {
            user,
            channel,
            role,
            none
        }

        /// <summary>
        /// Constructor of SmellyBot. Connection with client, prefix and commands are set here.
        /// </summary>
        public SmellyBot()
        {
            client = new DiscordClient(input =>
            {
                input.LogLevel = LogSeverity.Info;
                input.LogHandler = Log;
            });
            
            client.UsingCommands(input =>
            {
                input.PrefixChar = Properties.Default.prefix;
                input.AllowMentionPrefix = true;
            });

            commands = client.GetService<CommandService>();

            AddAllCommands();
            
            ToggleEvents(eventType.user);
            ToggleEvents(eventType.channel);
            ToggleEvents(eventType.role);

            client.ExecuteAndWait(async () =>
            {
                await client.Connect(Properties.Default.botToken, TokenType.Bot);
                client.SetGame(String.Format("{0}help", Properties.Default.prefix));
            });
        }

        /// <summary>
        /// Toggles a specific event on or off.
        /// </summary>
        /// <param name="e">The command event which was executed.</param>
        /// <returns>A message in the channel that the events have been changed.
        /// In case of a failed input, returns an error message that the command was wrongly used.</returns>
        private async Task ToggleSpecificEvent(CommandEventArgs e)
        {
            try
            {
                string[] input = Utils.ReturnInputParameterStringArray(e);

                string firstParameter = input[0];
                string secondParameter = input[1];

                //Meaning that there are more given parameters than necessary.
                if (input.Length >= 3)
                    throw new UnusedParametersException("Too many parameters were given.");
                else if (input.Length <= 1)
                    throw new UnusedParametersException("Too few parameters were given.");

                eventType eventtype = eventType.none;

                if (e.Command.Text.Contains("user"))
                {
                    eventtype = eventType.user;
                    Properties.Default.userEvents = !Properties.Default.userEvents;
                }
                else if (e.Command.Text.Contains("channel"))
                {
                    eventtype = eventType.channel;
                    Properties.Default.channelEvents = !Properties.Default.channelEvents;
                }
                else if (e.Command.Text.Contains("role"))
                {
                    eventtype = eventType.role;
                    Properties.Default.roleEvents = !Properties.Default.roleEvents;
                }
                Properties.Default.eventsChannel = firstParameter;
                Properties.Default.welcomeChannel = secondParameter;

                if (eventtype != eventType.none)
                    await e.Channel.SendMessage(ToggleEvents(eventtype));
                else
                    throw new UnknownEventException("Event not found.");
            }
            catch (Exception ex) when (ex is UnusedParametersException || ex is IndexOutOfRangeException)
            {
                Console.WriteLine(ex.Message);
                await Utils.InproperCommandUsageMessage(e, "toggle<EVENT>", "!toggle<EVENT> <CHANNELNAME> <CHANNELNAME>");
            }
            catch (Exception uee)
            {
                Console.WriteLine(uee.Message);
                await e.Channel.SendMessage("Something went wrong that shouldn't have went wrong...");
            }
        }

        /// <summary>
        /// Adds a basic command with a response.
        /// </summary>
        /// <param name="e">The command event which was executed.</param>
        /// <returns>A message that shows if the command was successfully added or not.</returns>
        private async Task AddCommand(CommandEventArgs e)
        {
            string[] input = Utils.ReturnInputParameterStringArray(e);

            string command = input[0];
            string response = "";
            
            for (int i = 1; i<input.Length; i++)
                response += input[i] + " ";

            if (input.Length <= 1)
                throw new UnusedParametersException("Too few parameters were given.");

            foreach (Command c in commands.AllCommands)
                if (c.Text.Contains(command))
                    throw new DuplicateCommandException("Duplicate command attempted to be added.");

            try
            {
                AddCommand(command, response);
                await e.Channel.SendMessage(string.Format("Succesfully added the *{0}{1}* command!", Properties.Default.prefix, command));
            }
            catch (DuplicateCommandException dce)
            {
                Console.WriteLine(dce.Message);
                await e.Channel.SendMessage(dce.Message);
            }
            catch (UnusedParametersException upe)
            {
                Console.WriteLine(upe.Message);
                await Utils.InproperCommandUsageMessage(e, "addcommand", "!addcommand <NAME> <RESPONSE>");
            }
        }

        /// <summary>
        /// Logs the event in the console.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event that was executed.</param>
        private void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        /// <summary>
        /// Creates a basic command with a basic response.
        /// </summary>
        /// <param name="command">The command name that needs to be created.</param>
        /// <param name="response">The response that will be given when that command is called.</param>
        private void AddCommand(string command, string response)
        {
            commands.CreateCommand(command).Do(async (e) =>
            {
                await e.Channel.SendMessage(response);
            });
        }

        /// <summary>
        /// Adds all commands to the commandservice.
        /// </summary>
        private void AddAllCommands()
        {
            #region Basic Commands With Responses
            #region Dokus
            AddCommand("Dokus", "Did you mean yourself?");
            AddCommand("dokus", "Did you mean yourself?");
            AddCommand("DOKUS", "No need to yell, dokus!");
            #endregion
            #endregion
            #region Help Commands
            commands.CreateCommand("help").Do(async (e) => 
            {
                string serverName = "servers";
                if (e.Server != null)
                {
                    serverName = e.Server.Name;
                }
                string output = String.Format("Helpful commands for {0} in {1}. (NOTE: Lowercase is key!)", client.CurrentUser.Name, serverName)
                                +"``` \n"
                                + String.Format("{0}help", Properties.Default.prefix).PadRight(20) + "Displays this message again with commands. \n"
                                + String.Format("{0}gambling", Properties.Default.prefix).PadRight(20) + "Shows all the gambling commands. \n"
                                + String.Format("{0}league", Properties.Default.prefix).PadRight(20) + "Shows all the league commands. \n"
                                + String.Format("{0}roles", Properties.Default.prefix).PadRight(20) + "Shows all the roles commands. \n"
                                + String.Format("{0}admin", Properties.Default.prefix).PadRight(20) + "Shows all the admin commands. \n"
                                + "```";
                await e.User.SendMessage(output);
            });
            commands.CreateCommand("gambling").Do(async (e) =>
            {
                string output = "Commands regarding gambling. (NOTE: Lowercase is key!)"
                                + "``` \n"
                                + String.Format("{0}roll <min>-<max>", Properties.Default.prefix).PadRight(25) + "Rolls a random number between the given minimum & maximum.".PadRight(60) + String.Format("Example: {0}roll 1-100", Properties.Default.prefix) + "\n"
                                + String.Format("{0}slots", Properties.Default.prefix).PadRight(25) + "Generates a play on a slotmachine.".PadRight(60) + "\n"
                                + "```";
                await e.User.SendMessage(output);
            });
            //TODO Add explanation commands for League.
            commands.CreateCommand("roles").Do(async (e) =>
            {
                string output = "Commands regarding roles. (NOTE: Lowercase is key!)"
                                + "``` \n"
                                + String.Format("{0}assignrole <role1> <role2>", Properties.Default.prefix).PadRight(25) + "Assigns the requested role(s) to the user.".PadRight(60) + String.Format("Example: {0}assignrole Bot", Properties.Default.prefix) + "\n"
                                + String.Format("{0}removerole <role1> <role2>", Properties.Default.prefix).PadRight(25) + "Removes the mentioned role(s) from the user.".PadRight(60) + String.Format("Example: {0}removerole Bot", Properties.Default.prefix) + "\n"
                                + "```";
                await e.User.SendMessage(output);
            });
            //TODO Add explanation commands for Admins.
            #endregion
            #region Gambling
            #region Slot Machine
            commands.CreateCommand("slots").Do(async (e) =>
            {
                await Casino.Slots(e);
            });
            #endregion
            #region Random Roll
            commands.CreateCommand("roll").Parameter("message", ParameterType.Multiple).Do(async (e) =>
            {
                await Casino.Roll(e);
            });
            #endregion
            #endregion
            #region Admin Commands
            #region Toggle Events
            commands.CreateCommand("toggleall").Do(async (e) =>
            {
                if (e.User.ServerPermissions.Administrator)
                {
                    Properties.Default.userEvents = !Properties.Default.userEvents;
                    Properties.Default.channelEvents = !Properties.Default.channelEvents;
                    Properties.Default.roleEvents = !Properties.Default.roleEvents;
                    await e.Channel.SendMessage(ToggleEvents(eventType.user));
                    await e.Channel.SendMessage(ToggleEvents(eventType.channel));
                    await e.Channel.SendMessage(ToggleEvents(eventType.role));
                }
            });
            commands.CreateCommand("toggleuser").Do(async (e) =>
            {
                if (e.User.ServerPermissions.Administrator)
                {
                    Properties.Default.userEvents = !Properties.Default.userEvents;
                    await e.Channel.SendMessage(ToggleEvents(eventType.user));
                }
            });
            commands.CreateCommand("togglechannel").Do(async (e) =>
            {
                if (e.User.ServerPermissions.Administrator)
                {
                    Properties.Default.channelEvents = !Properties.Default.channelEvents;
                    await e.Channel.SendMessage(ToggleEvents(eventType.channel));
                }
            });
            commands.CreateCommand("togglerole").Do(async (e) =>
            {
                if (e.User.ServerPermissions.Administrator)
                {
                    Properties.Default.roleEvents = !Properties.Default.roleEvents;
                    await e.Channel.SendMessage(ToggleEvents(eventType.role));
                }
            });
            commands.CreateCommand("toggleuser").Parameter("message", ParameterType.Multiple).Do(async (e) =>
            {
                if (e.User.ServerPermissions.Administrator)
                    await ToggleSpecificEvent(e);
            });
            commands.CreateCommand("togglechannel").Parameter("message", ParameterType.Multiple).Do(async (e) =>
            {
                if (e.User.ServerPermissions.Administrator)
                    await ToggleSpecificEvent(e);
            });
            commands.CreateCommand("togglerole").Parameter("message", ParameterType.Multiple).Do(async (e) =>
            {
                if (e.User.ServerPermissions.Administrator)
                    await ToggleSpecificEvent(e);
            });
            #endregion
            #region Create Basic Command
            commands.CreateCommand("addcommand").Parameter("message", ParameterType.Multiple).Do(async (e) =>
            {
                if (e.User.ServerPermissions.Administrator)
                    await AddCommand(e);
            });
            #endregion
            #region Save changes to properties.
            commands.CreateCommand("save").Do(async (e) => {
                if (e.User.ServerPermissions.Administrator)
                {
                    Properties.Default.Save();
                    await e.Channel.SendMessage("The changes to the settings file were saved!");
                }
            });
            #endregion
            #region Disconnect Command
            commands.CreateCommand("disconnect").Do(async (e) =>
            {
                if (e.User.ServerPermissions.Administrator)
                {
                    await e.Channel.SendMessage(string.Format("{0} signing out.", client.CurrentUser.Name));
                    await client.Disconnect();
                }
            });
            #endregion
            #region Set SmellyBot's Game
            commands.CreateCommand("setgame").Parameter("message", ParameterType.Multiple).Do((e) => 
            {
                if (e.User.ServerPermissions.Administrator)
                {
                    string game = "";
                    foreach (string s in Utils.ReturnInputParameterStringArray(e))
                    {
                        game += s.PadRight(1);
                    }
                    client.SetGame(game);
                }
            });
            #endregion
            #endregion
            #region Request Role Addition/Removal
            commands.CreateCommand("assignrole").Parameter("message", ParameterType.Multiple).Do(async (e) =>
            {
                await Utils.AssignRole(e);
            });
            commands.CreateCommand("removerole").Parameter("message", ParameterType.Multiple).Do(async (e) =>
            {
                await Utils.RemoveRole(e);
            });
            #endregion
            #region League of Legends
            commands.CreateCommand("level").Parameter("message", ParameterType.Multiple).Do(async(e) => 
            {
                if (stats == null)
                    stats = new LeagueStats(Properties.Default.riotAPIkey);
                await stats.GetSummonerLevel(e);
            });
            commands.CreateCommand("rank").Parameter("message", ParameterType.Multiple).Do(async (e) =>
            {
                if (stats == null)
                    stats = new LeagueStats(Properties.Default.riotAPIkey);
                await stats.GetSummonerRank(e);
            });
            commands.CreateCommand("currentgame").Parameter("message", ParameterType.Multiple).Do(async (e) =>
            {
                if (stats == null)
                    stats = new LeagueStats(Properties.Default.riotAPIkey);
                await stats.GetCurrentGameStats(e);
            });
            commands.CreateCommand("status").Parameter("message", ParameterType.Required).Do(async (e) =>
            {
                if (stats == null)
                    stats = new LeagueStats(Properties.Default.riotAPIkey);
                await stats.GetLeagueStatus(e);
            });
            commands.CreateCommand("stats").Parameter("message", ParameterType.Multiple).Do(async(e) =>
            {
                if (stats == null)
                    stats = new LeagueStats(Properties.Default.riotAPIkey);
                await stats.GetChampionStats(e);
            });
            commands.CreateCommand("lore").Parameter("message", ParameterType.Required).Do(async (e) =>
            {
                if (stats == null)
                    stats = new LeagueStats(Properties.Default.riotAPIkey);
                await stats.GetChampionLore(e);
            });
            commands.CreateCommand("counter").Parameter("message", ParameterType.Required).Do(async (e) => 
            {
                if (stats == null)
                    stats = new LeagueStats(Properties.Default.riotAPIkey);
                await stats.GetChampionCounter(e);
            });
            commands.CreateCommand("howto").Parameter("message", ParameterType.Required).Do(async (e) =>
            {
                if (stats == null)
                    stats = new LeagueStats(Properties.Default.riotAPIkey);
                await stats.GetChampionTips(e);
            });
            #endregion
        }

        /// <summary>
        /// Toggles events being sent to certains channels or not.
        /// </summary>
        /// <param name="eventType">The type of the events.</param>
        /// <param name="welcomeChannel">The channel where welcome messages should be posted.</param>
        /// <param name="eventsChannel">Channel where the rest should be posted.</param>
        private string ToggleEvents(eventType eventType)
        {
            switch (eventType)
            {
                case eventType.user:
                    #region Messages when user joins, gets banned/unbanned or leaves.
                    if (Properties.Default.userEvents)
                    {
                        client.UserJoined += OnUserJoined;
                        client.UserBanned += OnUserBanned;
                        client.UserUnbanned += OnUserUnbanned;
                        client.UserLeft += OnUserLeft;
                    }
                    else
                    {
                        client.UserJoined -= OnUserJoined;
                        client.UserBanned -= OnUserBanned;
                        client.UserUnbanned -= OnUserUnbanned;
                        client.UserLeft -= OnUserLeft;
                    }
                    #endregion
                    return Properties.Default.userEvents ? "User events have now been turned on." : "User events have now been turned off.";
                case eventType.channel:
                    #region Channel creation/destruction
                    if (Properties.Default.channelEvents)
                    {
                        client.ChannelCreated += OnChannelCreated;
                        client.ChannelDestroyed += OnChannelCreated;
                    }
                    else
                    {
                        client.ChannelCreated -= OnChannelCreated;
                        client.ChannelDestroyed -= OnChannelCreated;
                    }
                    #endregion
                    return Properties.Default.channelEvents ? "Channel events have now been turned on." : "Channel events have now been turned off.";
                case eventType.role:
                    #region Role creation/destruction
                    if (Properties.Default.roleEvents)
                    {
                        client.RoleCreated += OnRoleCreated;
                        client.RoleDeleted += OnRoleDeleted;
                    }
                    else
                    {
                        client.RoleCreated -= OnRoleCreated;
                        client.RoleDeleted -= OnRoleDeleted;
                    }
                    #endregion
                    return Properties.Default.roleEvents ? "Role events have now been turned on." : "Role events have now been turned off.";
            }
            return "Something went wrong that shouldn't have went wrong...";
        }

        #region Event Handlers
        /// <summary>
        /// Event handler for when a user joins the server.
        /// </summary>
        /// <param name="sender">The object triggering the event.</param>
        /// <param name="e">The event arguments for this user.</param>
        public async void OnUserJoined(object sender, UserEventArgs e)
        {
            var channel = e.Server.FindChannels(Properties.Default.welcomeChannel, ChannelType.Text).FirstOrDefault();

            await channel.SendMessage(string.Format("{0} has joined a channel!", e.User.Name));

            string output = "Is it just me or does it smell in here? \n"
                            + "Oh wait... it is me... Erm... *cough* \n \n"
                            + String.Format("Hi, {0}! I am **{1}**, a Discord bot from **{2}**.", e.User.Nickname, client.CurrentUser.Name, e.Server.Name)
                            + "Type !help for a list of commands that can be used in the server.";
            await e.User.SendMessage(output);
        }

        /// <summary>
        /// Event handler for when a user gets banned from the server.
        /// </summary>
        /// <param name="sender">The object triggering the event.</param>
        /// <param name="e">The event arguments for this user.</param>
        public async void OnUserBanned(object sender, UserEventArgs e)
        {
            var channel = e.Server.FindChannels(Properties.Default.eventsChannel, ChannelType.Text).FirstOrDefault();

            await channel.SendMessage(string.Format("{0} has been banned from the server!", e.User.Name));
        }

        /// <summary>
        /// Event handler for when a user gets unbanned from the server.
        /// </summary>
        /// <param name="sender">The object triggering the event.</param>
        /// <param name="e">The event arguments for this user.</param>
        public async void OnUserUnbanned(object sender, UserEventArgs e)
        {
            var channel = e.Server.FindChannels(Properties.Default.eventsChannel, ChannelType.Text).FirstOrDefault();

            await channel.SendMessage(string.Format("{0} has been unbanned from the server!", e.User.Name));
        }

        /// <summary>
        /// Event handler for when a user leaves a channel or the server.
        /// </summary>
        /// <param name="sender">The object triggering the event.</param>
        /// <param name="e">The event arguments for this user.</param>
        public async void OnUserLeft(object sender, UserEventArgs e)
        {
            var channel = e.Server.FindChannels(Properties.Default.eventsChannel, ChannelType.Text).FirstOrDefault();

            await channel.SendMessage(string.Format("{0} has left a channel!", e.User.Name));
        }

        /// <summary>
        /// Event handler for when a channel gets created.
        /// </summary>
        /// <param name="sender">The object triggering the event.</param>
        /// <param name="e">The event arguments for this channel.</param>
        public async void OnChannelCreated(object sender, ChannelEventArgs e)
        {
            var channel = e.Server.FindChannels(Properties.Default.eventsChannel, ChannelType.Text).FirstOrDefault();

            await channel.SendMessage(string.Format("A new channel named '{0}' has been created!", e.Channel.Name));
        }

        /// <summary>
        /// Event handler for when a channel gets deleted.
        /// </summary>
        /// <param name="sender">The object triggering the event.</param>
        /// <param name="e">The event arguments for this channel.</param>
        public async void OnChannelDestroyed(object sender, ChannelEventArgs e)
        {
            var channel = e.Server.FindChannels(Properties.Default.eventsChannel, ChannelType.Text).FirstOrDefault();

            await channel.SendMessage(string.Format("The channel named '{0}' has been deleted!", e.Channel.Name));
        }

        /// <summary>
        /// Event handler for when a role gets created.
        /// </summary>
        /// <param name="sender">The object triggering the event.</param>
        /// <param name="e">The event arguments for this role.</param>
        public async void OnRoleCreated(object sender, RoleEventArgs e)
        {
            var channel = e.Server.FindChannels(Properties.Default.eventsChannel, ChannelType.Text).FirstOrDefault();

            await channel.SendMessage(string.Format("A new role named '{0}' has been created!", e.Role.Name));
        }

        /// <summary>
        /// Event handler for when a role gets deleted.
        /// </summary>
        /// <param name="sender">The object triggering the event.</param>
        /// <param name="e">The event arguments for this role.</param>
        public async void OnRoleDeleted(object sender, RoleEventArgs e)
        {
            var channel = e.Server.FindChannels(Properties.Default.eventsChannel, ChannelType.Text).FirstOrDefault();

            await channel.SendMessage(string.Format("A role named '{0}' has been deleted!", e.Role.Name));
        }
        #endregion
    }
}
