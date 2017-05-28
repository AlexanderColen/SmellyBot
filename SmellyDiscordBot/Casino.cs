using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace SmellyDiscordBot
{
    public static class Casino
    {
        enum Outcomes
        {
            heart,
            moneybag,
            ring,
            crown,
            frog,
            dolphin,
            zap,
            heartpulse,
            x
        }
        
        /// <summary>
        /// Gets a random enum.
        /// </summary>
        /// <param name="rand">The Random object.</param>
        /// <returns>A string of the enum outcome.</returns>
        public static string GetRandomOutcome(Random rand)
        {
            var enums = Enum.GetValues(typeof(Outcomes));
            return enums.GetValue(rand.Next(enums.Length)).ToString();
        }

        /// <summary>
        /// Fakes a slot machine with emojis to the user.
        /// </summary>
        /// <param name="e">The command event which was executed.</param>
        /// <returns>A message in the channel that specifies which user tried to spin, 
        /// another one with the outcome of the spin, 
        /// and a final message that says something about the outcome.</returns>
        public static async Task Slots(CommandEventArgs e)
        {
            string user = General.FetchUser(e);
            await e.Channel.SendMessage(string.Format("*{0}* tries their luck at the slot machine...", user));

            try
            {
                Random rand = new Random(new Random().Next(10000));
                var enum1 = GetRandomOutcome(rand);
                var enum2 = GetRandomOutcome(rand);
                var enum3 = GetRandomOutcome(rand);
                await e.Channel.SendMessage(string.Format(":{0}: - :{1}: - :{2}:", enum1, enum2, enum3));

                //TODO Do something with the outcome. (Possibly when betting is added?)
                if (enum1.Equals(enum2) && enum2.Equals(enum3))
                {
                    await e.Channel.SendMessage(string.Format("*{0}* has hit the jackpot!", user));
                }
                else if (enum1.Equals(enum2) || enum2.Equals(enum3) || enum1.Equals(enum3))
                {
                    await e.Channel.SendMessage("So close, yet so far.");
                }
                else
                {
                    await e.Channel.SendMessage(string.Format("Better luck next time, *{0}*...", user));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await e.Channel.SendMessage("Something went wrong that shouldn't have went wrong...");
            }
        }

        /// <summary>
        /// Rolls two random numbers between a minimum and maximum that was separated by a dash.
        /// </summary>
        /// <param name="e">The command event which was executed.</param>
        /// <returns>A message in the channel with a random number, taking into account the input and output.
        /// In case of a failed input, returns an error message that the command was wrongly used.</returns>
        public static async Task Roll(CommandEventArgs e)
        {
            if (General.ReturnInputParameterStringArray(e).Length >= 2)
            {
                throw new UnusedParametersException("Too many parameters were given.");
            }

            var input = General.ReturnInputParameterStringArray(e)[0];

            try
            {
                var minimum = Convert.ToInt32(input.Substring(0, input.IndexOf("-")));
                var maximum = Convert.ToInt32(input.Remove(0, minimum.ToString().Length + 1));

                Random rand = new Random();

                var outcome = rand.Next(minimum, maximum);
                await e.Channel.SendMessage(string.Format("*{0}* rolled a **{1}**.", General.FetchUser(e), outcome));
            }
            catch (Exception ex) when (ex is UnusedParametersException || ex is ArgumentException)
            {
                Console.WriteLine(ex.Message);
                await General.InproperCommandUsageMessage(e, "roll", "!roll <MINVALUE>-<MAXVALUE>");
            }
        }
    }
}
