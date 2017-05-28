using Discord.Commands;
using System.Threading.Tasks;

namespace SmellyDiscordBot
{
    public static class General
    {
        /// <summary>
        /// Fetch the user from the command event.
        /// </summary>
        /// <param name="e">The command event which was executed.</param>
        /// <returns>The nickname of the user if the user has one, otherwise returns the name.</returns>
        public static string FetchUser(CommandEventArgs e)
        {
            return e.User.Nickname != null ? e.User.Nickname : e.User.Name;
        }

        /// <summary>
        /// Converts the added argument(s) to a string.
        /// </summary>
        /// <param name="e">The command event which was executed.</param>
        /// <returns>The parameters that were added to the command.</returns>
        public static string[] ReturnInputParameterStringArray(CommandEventArgs e)
        {
            string[] input = new string[e.Args.Length];

            for (int i = 0; i < e.Args.Length; i++)
            {
                input[i] = e.Args[i].ToString();
            }

            return input;
        }

        /// <summary>
        /// Sends a message showing how the command is supposed to be used.
        /// </summary>
        /// <param name="e">The command event which was executed.</param>
        /// <param name="commandname">The name of the command.</param>
        /// <param name="usage">The correct usage of the command.</param>
        /// <returns></returns>
        public static async Task InproperCommandUsageMessage(CommandEventArgs e, string commandname, string usage)
        {
            await e.Channel.SendMessage(string.Format("Inproper use of the command *!{0}*. It should look like this: *{1}*.", commandname, usage));
        }
    }
}
