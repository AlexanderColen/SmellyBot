using Discord.Commands;
using SmellyDiscordBot.Gambling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace SmellyDiscordBot
{
    public class Casino
    {
        private List<Gambler> gamblers = new List<Gambler>();

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
        /// Fetches the gamblers and their cash from a file.
        /// </summary>
        public void FetchGamblers()
        {
            StreamReader sr = null;
            try
            {
                string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "gamblers.txt");
                sr = new StreamReader(filePath);

                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    string name = line.Substring(0, line.IndexOf(' '));
                    int cash = Int32.Parse(line.Substring(line.IndexOf(' '), line.Length));

                    Gambler g = new Gambler();
                    g.SetName(name);
                    g.SetCash(cash);

                    this.gamblers.Add(g);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (sr != null)
                {
                    sr.Close();
                }
            }
        }

        /// <summary>
        /// Returns the cash for a specific gambler.
        /// </summary>
        /// <param name="name">The name of the gambler.</param>
        /// <returns>The amount of cash of the gambler.
        /// Returns -1 if no gambler was found.</returns>
        public int GetCash(string name)
        {
            foreach (Gambler g in this.gamblers)
            {
                if (g.GetName() == name)
                {
                    return g.GetCash();
                }
            }

            return -1;
        }

        /// <summary>
        /// Writes all the gamblers and their cash to the file.
        /// </summary>
        public void WriteAllGamblers()
        {
            StreamWriter sw = null;
            try
            {
                string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "gamblers.txt");
                sw = new StreamWriter(filePath);

                foreach (Gambler g in this.gamblers)
                {
                    sw.Write(g.GetName() + " " + g.GetCash() + "\n");
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (sw != null)
                {
                    sw.Close();
                }
            }
        }

        /// <summary>
        /// Adds a new gambler to the list.
        /// </summary>
        /// <param name="name"></param>
        public bool AddNewGambler(string name)
        {
            foreach (Gambler g in this.gamblers)
            {
                if (g.GetName() == name)
                {
                    return false;
                }
            }

            Gambler newGambler = new Gambler();
            newGambler.SetName(name);
            newGambler.SetCash(500);

            gamblers.Add(newGambler);

            StreamWriter sw = null;

            try
            {
                string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "gamblers.txt");
                sw = new StreamWriter(filePath);
                sw.Write(newGambler.GetName() + " " + newGambler.GetCash() + "\n");
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            finally
            {
                if (sw != null)
                {
                    sw.Close();
                }
            }

            return true;
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
            string user = Utils.FetchUserName(e);
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
            try
            {
                if (Utils.ReturnInputParameterStringArray(e).Length >= 2)
                {
                    throw new UnusedParametersException("Too many parameters were given.");
                }

                var input = Utils.ReturnInputParameterStringArray(e)[0];
                var minimum = Convert.ToInt32(input.Substring(0, input.IndexOf("-")));
                var maximum = Convert.ToInt32(input.Remove(0, minimum.ToString().Length + 1));

                Random rand = new Random();

                var outcome = rand.Next(minimum, maximum);
                await e.Channel.SendMessage(string.Format("*{0}* rolled a **{1}**.", Utils.FetchUserName(e), outcome));
            }
            catch (Exception ex) when (ex is UnusedParametersException || ex is ArgumentException || ex is FormatException)
            {
                Console.WriteLine(ex.Message);
                await Utils.InproperCommandUsageMessage(e, "roll", "!roll <MINVALUE>-<MAXVALUE>");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await e.Channel.SendMessage("Something went wrong that shouldn't have went wrong...");
            }
        }
    }
}
