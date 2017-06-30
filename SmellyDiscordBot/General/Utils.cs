using Discord;
using Discord.Commands;
using SmellyDiscordBot.Bot;
using System.Threading.Tasks;

namespace SmellyDiscordBot
{
    public static class Utils
    {
        /// <summary>
        /// Fetch the user from the command event.
        /// </summary>
        /// <param name="e">The command event which was executed.</param>
        /// <returns>The nickname of the user if the user has one, otherwise returns the name.</returns>
        public static string FetchUserName(CommandEventArgs e)
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
                input[i] = e.Args[i].ToString();

            return input;
        }

        /// <summary>
        /// Sends a message showing how the command is supposed to be used.
        /// </summary>
        /// <param name="e">The command event which was executed.</param>
        /// <param name="commandname">The name of the command.</param>
        /// <param name="usage">The correct usage of the command.</param>
        /// <returns></returns>
        public static async Task InproperCommandUsageMessage(CommandEventArgs e, string commandName, string usageExample)
        {
            await e.Channel.SendMessage(string.Format("Inproper use of the command *{0}{1}*. It should look like this: *{2}*.", Properties.Default.prefix, commandName, usageExample));
        }

        /// <summary>
        /// Assigns a role to the user.
        /// </summary>
        /// <param name="e">The command event which was executed.</param>
        /// <returns>Gives the user the requested role.</returns>
        public static async Task AssignRole(CommandEventArgs e)
        {
            string[] input = Utils.ReturnInputParameterStringArray(e);
            for (int i = 0; i < input.Length; i++)
                foreach (Role r in e.Server.FindRoles(input[i]))
                {
                    System.Threading.Thread.Sleep(500);
                    await e.User.AddRoles(r);
                }

            await e.Channel.SendMessage(string.Format("The role(s) have been assigned, *{0}*.", Utils.FetchUserName(e)));
        }

        /// <summary>
        /// Removes a role from the user.
        /// </summary>
        /// <param name="e">The command event which was executed.</param>
        /// <returns>Removes the role from the user.</returns>
        public static async Task RemoveRole(CommandEventArgs e)
        {
            string[] input = Utils.ReturnInputParameterStringArray(e);

            for (int i = 0; i < input.Length; i++)
                foreach (Role r in e.Server.FindRoles(input[i]))
                {
                    System.Threading.Thread.Sleep(500);
                    await e.User.RemoveRoles(r);
                }

            await e.Channel.SendMessage(string.Format("The role(s) have been removed, *{0}*.", Utils.FetchUserName(e)));
        }
    }
}
