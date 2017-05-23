using Discord;
using Discord.Commands;
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

        private bool toggleUserEvents = true;
        private bool toggleChannelEvents = true;
        private bool toggleRoleEvents = true;

        string eventsChannel = "bot-testing";
        string welcomeChannel = "welcome";
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

            //TODO allow owner to customize the prefix with properties file.
            client.UsingCommands(input =>
            {
                input.PrefixChar = '!';
                input.AllowMentionPrefix = true;
            });

            commands = client.GetService<CommandService>();

            AddAllCommands();

            //TODO fetch the channel values from properties file.
            toggleEvents(eventType.user);
            toggleEvents(eventType.channel);
            toggleEvents(eventType.role);
            
            client.ExecuteAndWait(async () =>
            {
                await client.Connect("MzE0ODIwMjU5NDE4OTk2NzM2.DAYcvg.vDWGavl5N7hr7WRnvgGa14J6d3s", TokenType.Bot);
            });
        }

        /// <summary>
        /// Fakes a slot machine with emojis to the user.
        /// </summary>
        /// <param name="e">The command event which was executed.</param>
        /// <returns>A message in the channel that specifies which user tried to spin, 
        /// another one with the outcome of the spin, 
        /// and a final message that says something about the outcome.</returns>
        private async Task SlotMachine(CommandEventArgs e)
        {
            var message = FetchUser(e) + " tries their luck at the slot machine...";
            await e.Channel.SendMessage(message);

            try {
                Random rand = new Random(new Random().Next(10000));
                var enum1 = SmellyDiscordBot.SlotMachine.GetRandomOutcome(rand);
                var enum2 = SmellyDiscordBot.SlotMachine.GetRandomOutcome(rand);
                var enum3 = SmellyDiscordBot.SlotMachine.GetRandomOutcome(rand);
                await e.Channel.SendMessage(string.Format(":{0}: - :{1}: - :{2}:", enum1, enum2, enum3));

                //TODO do something with the outcome.
                if (enum1.Equals(enum2) && enum2.Equals(enum3))
                {
                    await e.Channel.SendMessage(string.Format("{0} has hit the jackpot!", FetchUser(e)));
                }
                else if (enum1.Equals(enum2) || enum2.Equals(enum3) || enum1.Equals(enum3))
                {
                    await e.Channel.SendMessage("So close, yet so far.");
                }
                else
                {
                    await e.Channel.SendMessage(string.Format("Better luck next time, {0}...", FetchUser(e)));
                }
            } catch (Exception ex)
            {
                Console.WriteLine(ex.GetBaseException());
            }
        }

        /// <summary>
        /// Rolls two random numbers between a minimum and maximum that was separated by a dash.
        /// </summary>
        /// <param name="e">The command event which was executed.</param>
        /// <returns>A message in the channel with a random number, taking into account the input and output.
        /// In case of a failed input, returns an error message that the command was wrongly used.</returns>
        private async Task Roll(CommandEventArgs e)
        {
            var input = ReturnInputParameterString(e);

            try {
                var minimum = Convert.ToInt32(input.Substring(0, input.IndexOf("-")));
                var maximum = Convert.ToInt32(input.Remove(0, minimum.ToString().Length + 1));

                Random rand = new Random();

                var outcome = rand.Next(minimum, maximum);
                await e.Channel.SendMessage(string.Format("{0} rolled a {1}.", FetchUser(e), outcome));
            } catch (Exception ex)
            {
                Console.WriteLine(ex.GetBaseException());
                await e.Channel.SendMessage("Inproper use of the command '!roll'. It should look like this: '!roll 1-5'.");
            }
        }

        /// <summary>
        /// Toggles a specific event on or off.
        /// </summary>
        /// <param name="e">The command event which was executed.</param>
        /// <returns>A message in the channel that the events have been changed.
        /// In case of a failed input, returns an error message that the command was wrongly used.</returns>
        private async Task toggleSpecificEvent(CommandEventArgs e)
        {
            try
            {
                string input = ReturnInputParameterString(e);

                string firstParameter = input.Substring(0, input.IndexOf(' '));
                input = input.TrimStart(firstParameter.ToCharArray());
                input = input.TrimStart(' ');
                string secondParameter = input.Substring(0, input.IndexOf(' '));
                input = input.TrimStart(secondParameter.ToCharArray());

                //Meaning that there are more given parameters than necessary.
                if (input.Length != 1)
                {
                    throw new ArgumentOutOfRangeException();
                }

                eventType eventtype = eventType.none;

                if (e.Command.Text.Contains("User"))
                {
                    eventtype = eventType.user;
                    toggleUserEvents = !toggleUserEvents;
                }
                else if (e.Command.Text.Contains("Channel"))
                {
                    eventtype = eventType.channel;
                    toggleChannelEvents = !toggleChannelEvents;
                }
                else if (e.Command.Text.Contains("Role"))
                {
                    eventtype = eventType.role;
                    toggleRoleEvents = !toggleRoleEvents;
                }
                welcomeChannel = firstParameter;
                eventsChannel = secondParameter;

                if (eventtype != eventType.none) {
                    await e.Channel.SendMessage(toggleEvents(eventtype));
                }
                else
                {
                    throw new ApplicationException();
                }
            } catch (ArgumentOutOfRangeException aor)
            {
                Console.WriteLine(aor.GetBaseException());
                await e.Channel.SendMessage("Inproper use of the command '!toggle<EVENT>'. It should look like this: '!toggle<EVENT> welcome server-updates'.");
            } catch (ApplicationException appex)
            {
                Console.WriteLine(appex.GetBaseException());
                await e.Channel.SendMessage("Something went wrong that shouldn't have went wrong...");
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
            #region Adding basic commands with responses
            #region Greetings
            AddCommand("Hello", "Hi!");
            AddCommand("hello", "Hi!");
            AddCommand("HELLO", "HI!");
            AddCommand("Hi", "Hello there, dokus");
            AddCommand("hi", "Hello there, dokus");
            AddCommand("HI", "Hello there, dokus");
            #endregion
            #region Love
            AddCommand("Love", "Alex :heart: Shivam");
            AddCommand("love", "Alex :heart: Shivam");
            AddCommand("LOVE", "Alex :heart: Shivam");
            #endregion
            #region Dokus
            AddCommand("Dokus", "Do you mean yourself?");
            AddCommand("dokus", "Do you mean yourself?");
            AddCommand("DOKUS", "No need to yell, dokus!");
            #endregion
            #endregion
            AddCommand("test", "<:chiya:299559728307109888>");
            #region Slot machine
            commands.CreateCommand("slots").Do(async (e) =>
            {
                await SlotMachine(e);
            });
            #endregion
            #region Disconnect command
            commands.CreateCommand("disconnect").Do(async (e) =>
            {
                await client.Disconnect();
            });
            #endregion
            #region Random roll
            commands.CreateCommand("roll").Parameter("message", ParameterType.Required).Do(async (e) =>
            {
                await Roll(e);
            });
            commands.CreateCommand("roll").Parameter("message", ParameterType.Multiple).Do(async (e) =>
            {
                await e.Channel.SendMessage("Inproper use of the command '!roll'. It should look like this: '!roll 1-5'.");
            });
            #endregion
            #region Toggle Events
            commands.CreateCommand("toggleAll").Do(async (e) =>
            {
                toggleUserEvents = !toggleUserEvents;
                toggleChannelEvents = !toggleChannelEvents;
                toggleRoleEvents = !toggleRoleEvents;
                await e.Channel.SendMessage(toggleEvents(eventType.user));
                await e.Channel.SendMessage(toggleEvents(eventType.channel));
                await e.Channel.SendMessage(toggleEvents(eventType.role));
            });
            commands.CreateCommand("toggleUser").Do(async (e) => 
            {
                toggleUserEvents = !toggleUserEvents;
                await e.Channel.SendMessage(toggleEvents(eventType.user));
            });
            commands.CreateCommand("toggleChannel").Do(async (e) =>
            {
                toggleChannelEvents = !toggleChannelEvents;
                await e.Channel.SendMessage(toggleEvents(eventType.channel));
            });
            commands.CreateCommand("toggleRole").Do(async (e) =>
            {
                toggleRoleEvents = !toggleRoleEvents;
                await e.Channel.SendMessage(toggleEvents(eventType.role));
            });
            commands.CreateCommand("toggleUser").Parameter("message", ParameterType.Multiple).Do(async (e) =>
            {
                await toggleSpecificEvent(e);
            });
            commands.CreateCommand("toggleUser").Parameter("message", ParameterType.Required).Do(async (e) =>
            {
                await e.Channel.SendMessage("Inproper use of the command '!toggleUser'. It should look like this: '!toggleUser welcome server-updates'.");
            });
            commands.CreateCommand("toggleChannel").Parameter("message", ParameterType.Multiple).Do(async (e) =>
            {
                await toggleSpecificEvent(e);
            });
            commands.CreateCommand("toggleChannel").Parameter("message", ParameterType.Required).Do(async (e) =>
            {
                await e.Channel.SendMessage("Inproper use of the command '!toggleChannel'. It should look like this: '!toggleChannel welcome server-updates'.");
            });
            commands.CreateCommand("toggleRole").Parameter("message", ParameterType.Multiple).Do(async (e) =>
            {
                await toggleSpecificEvent(e);
            });
            commands.CreateCommand("toggleRole").Parameter("message", ParameterType.Required).Do(async (e) =>
            {
                await e.Channel.SendMessage("Inproper use of the command '!toggleRole'. It should look like this: '!toggleRole welcome server-updates'.");
            });
            #endregion
        }

        /// <summary>
        /// Fetch the user from the command event.
        /// </summary>
        /// <param name="e">The command event which was executed.</param>
        /// <returns>The nickname of the user if the user has one, otherwise returns the name.</returns>
        private string FetchUser(CommandEventArgs e)
        {
            return e.User.Nickname != null ? e.User.Nickname : e.User.Name;
        }

        /// <summary>
        /// Converts the added argument(s) to a string.
        /// </summary>
        /// <param name="e">The command event which was executed.</param>
        /// <returns>The parameters that were added to the command.</returns>
        private string ReturnInputParameterString(CommandEventArgs e)
        {
            string input = "";

            for (int i = 0; i < e.Args.Length; i++)
            {
                input += e.Args[i] + " ";
            }

            return input;
        }

        /// <summary>
        /// Toggles events being sent to certains channels or not.
        /// </summary>
        /// <param name="eventType">The type of the events.</param>
        /// <param name="welcomeChannel">The channel where welcome messages should be posted.</param>
        /// <param name="eventsChannel">Channel where the rest should be posted.</param>
        private string toggleEvents(eventType eventType) 
        {
            switch (eventType)
            {
                case eventType.user:
                    #region Messages when user joins, gets banned/unbanned or leaves.
                    if (toggleUserEvents)
                    {
                        client.UserJoined -= OnUserJoined;
                        client.UserBanned -= OnUserBanned;
                        client.UserUnbanned -= OnUserUnbanned;
                        client.UserLeft -= OnUserLeft;
                    }
                    else
                    {
                        client.UserJoined -= OnUserJoined;
                        client.UserBanned -= OnUserBanned;
                        client.UserUnbanned -= OnUserUnbanned;
                        client.UserLeft -= OnUserLeft;
                    }
                    #endregion
                    return toggleUserEvents ? "User events have now been turned on." : "User events have now been turned off.";
                case eventType.channel:
                    #region Channel creation/destruction
                    if (toggleChannelEvents)
                    {
                        client.ChannelCreated -= OnChannelCreated;
                        client.ChannelDestroyed -= OnChannelCreated;
                    }
                    else
                    {
                        client.ChannelCreated -= OnChannelCreated;
                        client.ChannelDestroyed -= OnChannelCreated;
                    }
                    #endregion
                    return toggleChannelEvents ? "Channel events have now been turned on." : "Channel events have now been turned off.";
                case eventType.role:
                    #region Role creation/destruction
                    if (toggleRoleEvents)
                    {
                        client.RoleCreated -= OnRoleCreated;
                        client.RoleDeleted -= OnRoleDeleted;
                    }
                    else
                    {
                        client.RoleCreated -= OnRoleCreated;
                        client.RoleDeleted -= OnRoleDeleted;
                    }
                    #endregion
                    return toggleRoleEvents ? "Role events have now been turned on." : "Role events have now been turned off.";
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
            var channel = e.Server.FindChannels(welcomeChannel, ChannelType.Text).FirstOrDefault();

            await channel.SendMessage(string.Format("{0} has joined a channel!", e.User.Name));
        }

        /// <summary>
        /// Event handler for when a user gets banned from the server.
        /// </summary>
        /// <param name="sender">The object triggering the event.</param>
        /// <param name="e">The event arguments for this user.</param>
        public async void OnUserBanned(object sender, UserEventArgs e)
        {
            var channel = e.Server.FindChannels(eventsChannel, ChannelType.Text).FirstOrDefault();

            await channel.SendMessage(string.Format("{0} has been banned from the server!", e.User.Name));
        }

        /// <summary>
        /// Event handler for when a user gets unbanned from the server.
        /// </summary>
        /// <param name="sender">The object triggering the event.</param>
        /// <param name="e">The event arguments for this user.</param>
        public async void OnUserUnbanned(object sender, UserEventArgs e)
        {
            var channel = e.Server.FindChannels(eventsChannel, ChannelType.Text).FirstOrDefault();

            await channel.SendMessage(string.Format("{0} has been unbanned from the server!", e.User.Name));
        }

        /// <summary>
        /// Event handler for when a user leaves a channel or the server.
        /// </summary>
        /// <param name="sender">The object triggering the event.</param>
        /// <param name="e">The event arguments for this user.</param>
        public async void OnUserLeft(object sender, UserEventArgs e)
        {
            var channel = e.Server.FindChannels(eventsChannel, ChannelType.Text).FirstOrDefault();

            await channel.SendMessage(string.Format("{0} has left a channel!", e.User.Name));
        }

        /// <summary>
        /// Event handler for when a channel gets created.
        /// </summary>
        /// <param name="sender">The object triggering the event.</param>
        /// <param name="e">The event arguments for this channel.</param>
        public async void OnChannelCreated(object sender, ChannelEventArgs e)
        {
            var channel = e.Server.FindChannels(eventsChannel, ChannelType.Text).FirstOrDefault();

            await channel.SendMessage(string.Format("A new channel named '{0}' has been created!", e.Channel.Name));
        }

        /// <summary>
        /// Event handler for when a channel gets deleted.
        /// </summary>
        /// <param name="sender">The object triggering the event.</param>
        /// <param name="e">The event arguments for this channel.</param>
        public async void OnChannelDestroyed(object sender, ChannelEventArgs e)
        {
            var channel = e.Server.FindChannels(eventsChannel, ChannelType.Text).FirstOrDefault();

            await channel.SendMessage(string.Format("The channel named '{0}' has been deleted!", e.Channel.Name));
        }

        /// <summary>
        /// Event handler for when a role gets created.
        /// </summary>
        /// <param name="sender">The object triggering the event.</param>
        /// <param name="e">The event arguments for this role.</param>
        public async void OnRoleCreated(object sender, RoleEventArgs e)
        {
            var channel = e.Server.FindChannels(eventsChannel, ChannelType.Text).FirstOrDefault();

            await channel.SendMessage(string.Format("A new role named '{0}' has been created!", e.Role.Name));
        }

        /// <summary>
        /// Event handler for when a role gets deleted.
        /// </summary>
        /// <param name="sender">The object triggering the event.</param>
        /// <param name="e">The event arguments for this role.</param>
        public async void OnRoleDeleted(object sender, RoleEventArgs e)
        {
            var channel = e.Server.FindChannels(eventsChannel, ChannelType.Text).FirstOrDefault();

            await channel.SendMessage(string.Format("A role named '{0}' has been deleted!", e.Role.Name));
        }
        #endregion
    }
}
