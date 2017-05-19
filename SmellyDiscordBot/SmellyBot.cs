using Discord;
using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SmellyDiscordBot
{
    public class SmellyBot
    {
        DiscordClient client;
        CommandService commands;

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

            //TODO allow owner to customize the prefix.
            client.UsingCommands(input =>
            {
                input.PrefixChar = '!';
                input.AllowMentionPrefix = true;
            });

            commands = client.GetService<CommandService>();

            #region Adding basic commands with responses
            #region Greetings
            addCommand("Hello", "Hi!");
            addCommand("hello", "Hi!");
            addCommand("HELLO", "HI!");
            addCommand("Hi", "Hello there, dokus");
            addCommand("hi", "Hello there, dokus");
            addCommand("HI", "Hello there, dokus");
            #endregion
            #region Love
            addCommand("Love", "Alex :heart: Shivam");
            addCommand("love", "Alex :heart: Shivam");
            addCommand("LOVE", "Alex :heart: Shivam");
            #endregion
            #region Dokus
            addCommand("Dokus", "Do you mean yourself?");
            addCommand("dokus", "Do you mean yourself?");
            addCommand("DOKUS", "No need to yell, dokus!");
            #endregion
            #endregion

            #region Slot machine
            commands.CreateCommand("slots").Do(async(e) =>
            {
                await SlotMachine(e);
            });
            #endregion

            #region Adding commands with parameters
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
            #endregion

            //TODO make this dynamic so it can be added to different channels when a command is called.
            #region Messages when user joins, gets banned/unbanned or leaves.
            client.UserJoined += async (s, e) => 
            {
                var channel = e.Server.FindChannels("welcome", ChannelType.Text).FirstOrDefault();

                await channel.SendMessage(string.Format("{0} has joined the channel!", e.User.Name));
            };
            client.UserBanned += async (s, e) =>
            {
                var channel = e.Server.FindChannels("server-updates", ChannelType.Text).FirstOrDefault();

                await channel.SendMessage(string.Format("{0} has been banned from the channel!", e.User.Name));
            };

            client.UserUnbanned += async (s, e) =>
            {
                var channel = e.Server.FindChannels("server-updates", ChannelType.Text).FirstOrDefault();

                await channel.SendMessage(string.Format("{0} has been unbanned from the channel!", e.User.Name));
            };

            client.UserLeft += async (s, e) =>
            {
                var channel = e.Server.FindChannels("server-updates", ChannelType.Text).FirstOrDefault();

                await channel.SendMessage(string.Format("{0} has left the channel!", e.User.Name));
            };
            #endregion
            //TODO make this dynamic so it can be added to different channels when a command is called.
            #region Channel creation/destruction
            client.ChannelCreated += async (s, e) =>
            {
                var channel = e.Server.FindChannels("server-updates", ChannelType.Text).FirstOrDefault();

                await channel.SendMessage(string.Format("A new channel named '{0}' has been created!", e.Channel.Name));
            };

            client.ChannelDestroyed += async (s, e) =>
            {
                var channel = e.Server.FindChannels("server-updates", ChannelType.Text).FirstOrDefault();

                await channel.SendMessage(string.Format("The channel named '{0}' has been deleted!", e.Channel.Name));
            };
            #endregion
            //TODO make this dynamic so it can be added to different channels when a command is called.
            #region Role creation/destruction
            client.RoleCreated += async (s, e) =>
            {
                var channel = e.Server.FindChannels("server-updates", ChannelType.Text).FirstOrDefault();

                await channel.SendMessage(string.Format("A new role named '{0}' has been created!", e.Role.Name));
            };

            client.RoleDeleted += async (s, e) =>
            {
                var channel = e.Server.FindChannels("server-updates", ChannelType.Text).FirstOrDefault();

                await channel.SendMessage(string.Format("A role named '{0}' has been deleted!", e.Role.Name));
            };
            #endregion

            client.ExecuteAndWait(async () =>
            {
                await client.Connect("MzE0ODIwMjU5NDE4OTk2NzM2.C_9uwQ.H0JUjD2ScOfSbOeJTyQmZH62IBE", TokenType.Bot);
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
            var message = fetchUser(e) + " tries their luck at the slot machine...";
            await e.Channel.SendMessage(message);

            try {
                Random rand = new Random(new Random().Next(10000));
                var enum1 = SlotMachineEnum.GetRandomOutcome(rand);
                var enum2 = SlotMachineEnum.GetRandomOutcome(rand);
                var enum3 = SlotMachineEnum.GetRandomOutcome(rand);
                await e.Channel.SendMessage(string.Format(":{0}: - :{1}: - :{2}:", enum1, enum2, enum3));

                //TODO do something with the outcome.
                if (enum1.Equals(enum2) && enum2.Equals(enum3))
                {
                    await e.Channel.SendMessage(string.Format("{0} has hit the jackpot!", fetchUser(e)));
                }
                else if (enum1.Equals(enum2) || enum2.Equals(enum3) || enum1.Equals(enum3))
                {
                    await e.Channel.SendMessage("So close, yet so far.");
                }
                else
                {
                    await e.Channel.SendMessage(string.Format("Better luck next time, {0}...", fetchUser(e)));
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
        /// In case of a failed input, returns a error message that the command was used wrongly.</returns>
        private async Task Roll(CommandEventArgs e)
        {
            var input = "";

            for (int i = 0; i < e.Args.Length; i++)
            {
                input += e.Args[i] + " ";
            }

            try {
                var minimum = Convert.ToInt32(input.Substring(0, input.IndexOf("-")));
                var maximum = Convert.ToInt32(input.Remove(0, minimum.ToString().Length + 1));

                Random rand = new Random();

                var outcome = rand.Next(minimum, maximum);
                await e.Channel.SendMessage(fetchUser(e) + " rolled a " + outcome);
            } catch (Exception ex)
            {
                Console.WriteLine(ex.GetBaseException());
                await e.Channel.SendMessage("Inproper use of the command '!roll'. It should look like this: '!roll 1-5'.");
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
        private void addCommand(string command, string response)
        {
            commands.CreateCommand(command).Do(async (e) =>
            {
                await e.Channel.SendMessage(response);
            });
        }

        /// <summary>
        /// Fetch the user from the command event.
        /// </summary>
        /// <param name="e">The command event which was executed.</param>
        /// <returns>The nickname of the user if the user has one, otherwise returns the name.</returns>
        private string fetchUser(CommandEventArgs e)
        {
            return e.User.Nickname != null ? e.User.Nickname : e.User.Name;
        }
    }
}
