using Discord;
using Discord.Commands;
using SmellyDiscordBot.Bot;
using SmellyDiscordBot.Exceptions;
using SmellyDiscordBot.League;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmellyDiscordBot
{
    public sealed class SmellyBot : IDisposable
    {
        #region Fields
        private DiscordClient client;
        private CommandService commands;
        private LeagueStats stats = null;
        private Casino casino = null;
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
                {
                    throw new UnusedParametersException("Too many parameters were given.");
                }
                else if (input.Length <= 1)
                {
                    throw new UnusedParametersException("Too few parameters were given.");
                }

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
                {
                    await e.Channel.SendMessage(ToggleEvents(eventtype));
                }
                else
                {
                    throw new UnknownEventException("Event not found.");
                }
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

            for (int i = 1; i < input.Length; i++)
            {
                response += input[i] + " ";
            }

            if (input.Length <= 1)
            {
                throw new UnusedParametersException("Too few parameters were given.");
            }

            foreach (Command c in commands.AllCommands)
            {
                if (c.Text.Contains(command))
                {
                    throw new DuplicateCommandException("Duplicate command attempted to be added.");
                }
            }

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
                                + "``` \n"
                                + "Command".PadRight(20) + "Description" + "\n" + "\n"
                                + String.Format("{0}help", Properties.Default.prefix).PadRight(20) + "Displays this message again with commands. \n"
                                + String.Format("{0}gambling", Properties.Default.prefix).PadRight(20) + "Shows all the gambling commands. \n"
                                + String.Format("{0}league", Properties.Default.prefix).PadRight(20) + "Shows all the league commands. \n"
                                + String.Format("{0}roles", Properties.Default.prefix).PadRight(20) + "Shows all the roles commands. \n"
                                + String.Format("{0}admin", Properties.Default.prefix).PadRight(20) + "Shows all the admin commands. \n"
                                + "```";
                await e.User.SendMessage(output);
            });

            commands.CreateCommand("admin").Do(async (e) =>
            {
                string output = "Commands regarding Admin, can only be used by people with administrator privileges. (NOTE: Lowercase is key!)"
                                + "``` \n"
                                + "Command".PadRight(35) + "Description".PadRight(85) + "Example" + "\n" + "\n"
                                + String.Format("{0}addcommand <command> <response>", Properties.Default.prefix).PadRight(35)
                                                + "Add a command to the current instance of SmellyBot, will be lost after reconnecting.".PadRight(85)
                                                + String.Format("Example: {0}command sayhi Hi!", Properties.Default.prefix) + "\n"
                                + String.Format("{0}save", Properties.Default.prefix).PadRight(35)
                                                + "Permanently saves the changes to bot settings.".PadRight(85) + "\n"
                                + String.Format("{0}disconnect", Properties.Default.prefix).PadRight(35)
                                                + "Disconnects SmellyBot from the server.".PadRight(85) + "\n"
                                + String.Format("{0}setgame <status>", Properties.Default.prefix).PadRight(35)
                                                + "Sets the status of this SmellyBot instance. Displays as: 'Playing <status>'".PadRight(85)
                                                + String.Format("Example: {0}setgame something", Properties.Default.prefix) + "\n"
                                + String.Format("{0}clear", Properties.Default.prefix).PadRight(35)
                                                + "Clears all of SmellyBot's messages in this channel.".PadRight(85) + "\n"
                                + String.Format("{0}clear", Properties.Default.prefix).PadRight(35)
                                                + "Clears every message in this channel.".PadRight(85) + "\n"
                                + String.Format("{0}toggleall", Properties.Default.prefix).PadRight(35)
                                                + "Toggles all events showing in chat. Turns them on if currently off and the opposite.".PadRight(85)
                                                + String.Format("Example: {0}toggleall", Properties.Default.prefix) + "\n"
                                + String.Format("{0}toggleuser <channel> <channel>", Properties.Default.prefix).PadRight(35)
                                                + "Toggles the user events. Optional parameters for channels.".PadRight(85)
                                                + String.Format("Example: {0}toggleuser announcements events", Properties.Default.prefix) + "\n"
                                + String.Format("{0}togglerole <channel> <channel>", Properties.Default.prefix).PadRight(35)
                                                + "Toggles the role events. Optional parameters for channels.".PadRight(85)
                                                + String.Format("Example: {0}togglerole announcements events", Properties.Default.prefix) + "\n"
                                + String.Format("{0}togglechannel <channel> <channel>", Properties.Default.prefix).PadRight(35)
                                                + "Toggles the channel events. Optional parameters for channels.".PadRight(85)
                                                + String.Format("Example: {0}togglechannel announcements events", Properties.Default.prefix) + "\n"
                                + "```";
                await e.User.SendMessage(output);
            });

            commands.CreateCommand("gambling").Do(async (e) =>
            {
                string output = "Commands regarding Gambling. (NOTE: Lowercase is key!)"
                                + "``` \n"
                                + "Command".PadRight(25) + "Description".PadRight(60) + "Example" + "\n" + "\n"
                                + String.Format("{0}roll <min>-<max>", Properties.Default.prefix).PadRight(25)
                                                + "Rolls a random number between the given minimum & maximum.".PadRight(60)
                                                + String.Format("Example: {0}roll 1-100", Properties.Default.prefix) + "\n"
                                + String.Format("{0}slots <betAmount>", Properties.Default.prefix).PadRight(25)
                                                + "Generates a play on a slotmachine.".PadRight(60)
                                                + String.Format("Example: {0}slots 25", Properties.Default.prefix) + "\n"
                                + String.Format("{0}startgambling", Properties.Default.prefix).PadRight(25)
                                                + "Registers an account at SmellyBank to start gambling.".PadRight(60) + "\n"
                                + String.Format("{0}checkcash", Properties.Default.prefix).PadRight(25)
                                                + "Checks the balance of your account at SmellyBank.".PadRight(60) + "\n"
                                + "```";
                await e.User.SendMessage(output);
            });

            commands.CreateCommand("league").Do(async (e) =>
            {
                string output = "Commands regarding League of Legends. (NOTE: Lowercase is key!)"
                                + "``` \n"
                                + "Command".PadRight(45) + "Description".PadRight(75) + "Example" + "\n" + "\n"
                                + String.Format("{0}level <region> <summoner>", Properties.Default.prefix).PadRight(45)
                                                + "Shows the level of the summoner in the region.".PadRight(75)
                                                + String.Format("Example: {0}level euw ChallengerJanna", Properties.Default.prefix) + "\n"
                                + String.Format("{0}rank <region> <summoner>", Properties.Default.prefix).PadRight(45)
                                                + "Shows the rank(s) of the summoner in the region.".PadRight(75)
                                                + String.Format("Example: {0}rank kr hide on bush", Properties.Default.prefix) + "\n"
                                + String.Format("{0}rankedstats <region> <champion> <summoner>", Properties.Default.prefix).PadRight(45)
                                                + "Shows ranked stats of the summoner in the region for a champion.".PadRight(75)
                                                + String.Format("Example: {0}rank kr hide on bush", Properties.Default.prefix) + "\n"
                                + String.Format("{0}currentgame <region> <summoner>", Properties.Default.prefix).PadRight(45)
                                                + "Displays the current game that the summoner in the region is playing.".PadRight(75)
                                                + String.Format("Example: {0}currentgame na xxRivenMaster98xx", Properties.Default.prefix) + "\n"
                                + String.Format("{0}status <region>", Properties.Default.prefix).PadRight(45)
                                                + "Checks the status of the requested region.".PadRight(75)
                                                + String.Format("Example: {0}status euw", Properties.Default.prefix) + "\n"
                                + String.Format("{0}stats <champion>", Properties.Default.prefix).PadRight(45)
                                                + "Fetches information for the champion.".PadRight(75)
                                                + String.Format("Example: {0}stats Nami", Properties.Default.prefix) + "\n"
                                + String.Format("{0}counter <champion>", Properties.Default.prefix).PadRight(45)
                                                + "Shows Riot's tips for playing against a certain champion.".PadRight(75)
                                                + String.Format("Example: {0}counter Draven", Properties.Default.prefix) + "\n"
                                + String.Format("{0}howto <champion>", Properties.Default.prefix).PadRight(45)
                                                + "Shows Riot's tips for playing a certain champion.".PadRight(75)
                                                + String.Format("Example: {0}howto Yasuo", Properties.Default.prefix) + "\n"
                                + String.Format("{0}lore <champion>", Properties.Default.prefix).PadRight(45)
                                                + "Displays the lore of a champion.".PadRight(75)
                                                + String.Format("Example: {0}lore Teemo", Properties.Default.prefix) + "\n"
                                + "```";
                await e.User.SendMessage(output);
            });

            commands.CreateCommand("roles").Do(async (e) =>
            {
                string output = "Commands regarding Roles. (NOTE: Lowercase is key!)"
                                + "``` \n"
                                + "Command".PadRight(30) + "Description".PadRight(50) + "Example" + "\n" + "\n"
                                + String.Format("{0}assignrole <role1> <role2>", Properties.Default.prefix).PadRight(30)
                                                + "Assigns the requested role(s) to the user.".PadRight(50)
                                                + String.Format("Example: {0}assignrole Bot", Properties.Default.prefix) + "\n"
                                + String.Format("{0}removerole <role1> <role2>", Properties.Default.prefix).PadRight(30)
                                                + "Removes the mentioned role(s) from the user.".PadRight(50)
                                                + String.Format("Example: {0}removerole Bot", Properties.Default.prefix) + "\n"
                                + "```";
                await e.User.SendMessage(output);
            });
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
                else
                {
                    await e.Channel.SendMessage("You do not have sufficient permissions to use this command.");
                }
            });

            commands.CreateCommand("toggleuser").Do(async (e) =>
            {
                if (e.User.ServerPermissions.Administrator)
                {
                    Properties.Default.userEvents = !Properties.Default.userEvents;
                    await e.Channel.SendMessage(ToggleEvents(eventType.user));
                }
                else
                {
                    await e.Channel.SendMessage("You do not have sufficient permissions to use this command.");
                }
            });

            commands.CreateCommand("togglechannel").Do(async (e) =>
            {
                if (e.User.ServerPermissions.Administrator)
                {
                    Properties.Default.channelEvents = !Properties.Default.channelEvents;
                    await e.Channel.SendMessage(ToggleEvents(eventType.channel));
                }
                else
                {
                    await e.Channel.SendMessage("You do not have sufficient permissions to use this command.");
                }
            });

            commands.CreateCommand("togglerole").Do(async (e) =>
            {
                if (e.User.ServerPermissions.Administrator)
                {
                    Properties.Default.roleEvents = !Properties.Default.roleEvents;
                    await e.Channel.SendMessage(ToggleEvents(eventType.role));
                }
                else
                {
                    await e.Channel.SendMessage("You do not have sufficient permissions to use this command.");
                }
            });

            commands.CreateCommand("toggleuser").Parameter("message", ParameterType.Multiple).Do(async (e) =>
            {
                if (e.User.ServerPermissions.Administrator)
                {
                    await ToggleSpecificEvent(e);
                }
                else
                {
                    await e.Channel.SendMessage("You do not have sufficient permissions to use this command.");
                }
            });

            commands.CreateCommand("togglechannel").Parameter("message", ParameterType.Multiple).Do(async (e) =>
            {
                if (e.User.ServerPermissions.Administrator)
                {
                    await ToggleSpecificEvent(e);
                }
                else
                {
                    await e.Channel.SendMessage("You do not have sufficient permissions to use this command.");
                }
            });

            commands.CreateCommand("togglerole").Parameter("message", ParameterType.Multiple).Do(async (e) =>
            {
                if (e.User.ServerPermissions.Administrator)
                {
                    await ToggleSpecificEvent(e);
                }
                else
                {
                    await e.Channel.SendMessage("You do not have sufficient permissions to use this command.");
                }
            });
            #endregion
            #region Create Basic Command
            commands.CreateCommand("addcommand").Parameter("message", ParameterType.Multiple).Do(async (e) =>
            {
                if (e.User.ServerPermissions.Administrator)
                {
                    await AddCommand(e);
                }
                else
                {
                    await e.Channel.SendMessage("You do not have sufficient permissions to use this command.");
                }
            });
            #endregion
            #region Save Changes To Properties.
            commands.CreateCommand("save").Do(async (e) =>
            {
                if (e.User.ServerPermissions.Administrator)
                {
                    Properties.Default.Save();
                    await e.Channel.SendMessage("The changes to the settings file were saved!");
                }
                else
                {
                    await e.Channel.SendMessage("You do not have sufficient permissions to use this command.");
                }
            });
            #endregion
            #region Disconnect Command
            commands.CreateCommand("disconnect").Do(async (e) =>
            {
                if (e.User.ServerPermissions.Administrator)
                {
                    this.casino.WriteAllGamblers();
                    await e.Channel.SendMessage(string.Format("{0} signing out.", client.CurrentUser.Name));
                    System.Threading.Thread.Sleep(500);
                    await client.Disconnect();
                    Dispose();
                }
                else
                {
                    await e.Channel.SendMessage("You do not have sufficient permissions to use this command.");
                }
            });
            #endregion
            #region Set SmellyBot's Game
            commands.CreateCommand("setgame").Parameter("message", ParameterType.Multiple).Do(async (e) =>
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
                else
                {
                    await e.Channel.SendMessage("You do not have sufficient permissions to use this command.");
                }
            });
            #endregion
            #region Delete Channel Messages
            commands.CreateCommand("clear").Do(async(e) => 
            {
                if (e.User.ServerPermissions.Administrator)
                {
                    List<Message> toRemoveMessages = new List<Message>();
                
                    foreach (Message m in await e.Channel.DownloadMessages())
                    {
                        //If SmellyBot sent the message, it will be added to the list. 
                        if (m.IsAuthor)
                        {
                            toRemoveMessages.Add(m);
                            continue;
                        }
                        //Also removes the messages that ask for clears.
                        else if (m.Text.Equals(String.Format("{0}clear", Properties.Default.prefix)))
                        {
                            toRemoveMessages.Add(m);
                        }
                    }

                    await e.Channel.DeleteMessages(toRemoveMessages.ToArray());
                }
                else
                {
                    await e.Channel.SendMessage("You do not have sufficient permissions to use this command.");
                }
            });
            commands.CreateCommand("clearall").Do(async (e) =>
            {
                if (e.User.ServerPermissions.Administrator)
                {
                    await e.Channel.DeleteMessages(await e.Channel.DownloadMessages());
                }
                else
                {
                    await e.Channel.SendMessage("You do not have sufficient permissions to use this command.");
                }
            });
            #endregion
            #endregion
            #region Gambling
            commands.CreateCommand("checkcash").Do(async (e) =>
            {
                if (this.casino == null)
                {
                    this.casino = new Casino();
                    this.casino.FetchGamblers();
                }

                Int64 cash = this.casino.GetCash(e.User.Name);

                if (cash != -1) {
                    await e.Channel.SendMessage(String.Format("{0} has a total of {1} SmellyBucks.", e.User.Nickname, cash));
                }
                else
                {
                    await e.Channel.SendMessage(String.Format("You don't have an account at SmellyBank yet. Please register one with {0}startgambling.", Properties.Default.prefix));
                }
            });

            commands.CreateCommand("startgambling").Do(async (e) =>
            {
                if (this.casino == null)
                {
                    this.casino = new Casino();
                    this.casino.FetchGamblers();
                }

                if (this.casino.AddNewGambler(e.User.Name))
                {
                    await e.Channel.SendMessage("You have succesfully registered at SmellyBank and have received your starting 500 SmellyBucks.");
                }
                else
                {
                    await e.Channel.SendMessage(String.Format("You already have an account at SmellyBank. You can use {0}checkcash to see your cash.", Properties.Default.prefix));
                }
            });

            commands.CreateCommand("leaderboard").Do(async (e) =>
            {
                if (this.casino == null)
                {
                    this.casino = new Casino();
                    this.casino.FetchGamblers();
                }

                await this.casino.Leaderboard(e);
            });
            #region Slot Machine
            commands.CreateCommand("slots").Parameter("message", ParameterType.Required).Do(async (e) =>
            {
                if (this.casino == null)
                {
                    this.casino = new Casino();
                    this.casino.FetchGamblers();
                }
                await this.casino.Slots(e);
            });
            #endregion
            #region Random Roll
            commands.CreateCommand("roll").Parameter("message", ParameterType.Multiple).Do(async (e) =>
            {
                await Casino.Roll(e);
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
            commands.CreateCommand("level").Parameter("message", ParameterType.Multiple).Do(async (e) =>
            {
                try
                {
                    if (stats == null)
                    {
                        stats = new LeagueStats(Properties.Default.riotAPIkey);
                    }

                    await stats.GetSummonerLevel(e);
                }
                catch (InvalidAPITokenException ex)
                {
                    await e.Channel.SendMessage(ex.Message);
                }
            });

            commands.CreateCommand("rank").Parameter("message", ParameterType.Multiple).Do(async (e) =>
            {
                try
                {
                    if (stats == null)
                    {
                        stats = new LeagueStats(Properties.Default.riotAPIkey);
                    }

                    await stats.GetSummonerRank(e);
                }
                catch (InvalidAPITokenException ex)
                {
                    await e.Channel.SendMessage(ex.Message);
                }
            });

            commands.CreateCommand("currentgame").Parameter("message", ParameterType.Multiple).Do(async (e) =>
            {
                try
                {
                    if (stats == null)
                    {
                        stats = new LeagueStats(Properties.Default.riotAPIkey);
                    }

                    await stats.GetCurrentGameStats(e);
                }
                catch (InvalidAPITokenException ex)
                {
                    await e.Channel.SendMessage(ex.Message);
                }
            });

            commands.CreateCommand("status").Parameter("message", ParameterType.Required).Do(async (e) =>
            {
                try
                {
                    if (stats == null)
                    {
                        stats = new LeagueStats(Properties.Default.riotAPIkey);
                    }

                    await stats.GetServerStatus(e);
                }
                catch (InvalidAPITokenException ex)
                {
                    await e.Channel.SendMessage(ex.Message);
                }
            });

            commands.CreateCommand("stats").Parameter("message", ParameterType.Multiple).Do(async (e) =>
            {

                try
                {
                    if (stats == null)
                    {
                        stats = new LeagueStats(Properties.Default.riotAPIkey);
                    }
                    await stats.GetChampionStats(e);
                }
                catch (InvalidAPITokenException ex)
                {
                    await e.Channel.SendMessage(ex.Message);
                }
            });

            commands.CreateCommand("counter").Parameter("message", ParameterType.Required).Do(async (e) =>
            {
                try
                {
                    if (stats == null)
                    {
                        stats = new LeagueStats(Properties.Default.riotAPIkey);
                    }

                    await stats.GetChampionCounter(e);
                }
                catch (InvalidAPITokenException ex)
                {
                    await e.Channel.SendMessage(ex.Message);
                }
            });

            commands.CreateCommand("howto").Parameter("message", ParameterType.Required).Do(async (e) =>
            {
                try
                {
                    if (stats == null)
                    {
                        stats = new LeagueStats(Properties.Default.riotAPIkey);
                    }

                    await stats.GetChampionTips(e);
                }
                catch (InvalidAPITokenException ex)
                {
                    await e.Channel.SendMessage(ex.Message);
                }
            });

            commands.CreateCommand("lore").Parameter("message", ParameterType.Required).Do(async (e) =>
            {
                try
                {
                    if (stats == null)
                    {
                        stats = new LeagueStats(Properties.Default.riotAPIkey);
                    }

                    await stats.GetChampionLore(e);
                }
                catch (InvalidAPITokenException ex)
                {
                    await e.Channel.SendMessage(ex.Message);
                }
            });

            commands.CreateCommand("rankedstats").Parameter("message", ParameterType.Multiple).Do(async (e) =>
            {
                try
                {
                    if (stats == null)
                    {
                        stats = new LeagueStats(Properties.Default.riotAPIkey);
                    }

                    await stats.GetRankedChampStats(e);
                }
                catch (InvalidAPITokenException ex)
                {
                    await e.Channel.SendMessage(ex.Message);
                }
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

        /// <summary>
        /// Dispose method from IDisposable interface.
        /// Closes and/or disposes instances.
        /// </summary>
        public void Dispose()
        {
            ((IDisposable) client).Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
