using Discord.Commands;
using RiotSharp;
using SmellyDiscordBot.Exceptions;
using System;
using System.Threading.Tasks;

namespace SmellyDiscordBot.League
{
    public class LeagueStats
    {
        RiotApi api;

        /// <summary>
        /// Constructor for this class which fetches the API instance..
        /// </summary>
        /// <param name="apiKey">A developer key from https://developer.riotgames.com/ </param>
        public LeagueStats(string apiKey)
        {
            if (!"unknown".Equals(apiKey))
                api = RiotApi.GetInstance(apiKey);
        }

        /// <summary>
        /// Fetches the given Summoner's level from the RiotAPI for the given region.
        /// </summary>
        /// <param name="e">The command event which was executed.</param>
        /// <returns>A message in the channel with the given Summoner's name and level.</returns>
        public async Task GetSummonerLevel(CommandEventArgs e)
        {
            var input = Utils.ReturnInputParameterStringArray(e);
            string regionString = input[0];
            string summonerName = "";
            if (input.Length == 2)
            {
                summonerName = input[1];
            }
            else
            {
                summonerName = input[1];
                for (int i = 2; i < input.Length; i++)
                {
                    summonerName = String.Format("{0} {1}", summonerName, input[i]);
                }
            }
            try
            {
                var summoner = GetSummoner(regionString, summonerName);
                await e.Channel.SendMessage(string.Format("*{0}* is level **{1}**.", summoner.Name, summoner.Level));
            }
            catch (Exception ex) when (ex is RiotSharpException || ex is IndexOutOfRangeException)
            {
                await Utils.InproperCommandUsageMessage(e, "level", "level <REGION> <SUMMONERNAME>");
            }
        }

        /// <summary>
        /// Fetches the given Summoner's rank from the RiotAPI for the given region.
        /// </summary>
        /// <param name="e">The command event which was executed.</param>
        /// <returns>A message in the channel with the given Summoner's rank.</returns>
        public async Task GetSummonerRank(CommandEventArgs e)
        {
            var input = Utils.ReturnInputParameterStringArray(e);
            string regionString = input[0];
            string summonerName = "";
            if (input.Length == 2)
            {
                summonerName = input[1];
            }
            else
            {
                summonerName = input[1];
                for (int i = 2; i < input.Length; i++)
                {
                    summonerName = String.Format("{0} {1}", summonerName, input[i]);
                }
            }
            RiotSharp.SummonerEndpoint.Summoner summoner = null;
            try
            {
                summoner = GetSummoner(regionString, summonerName);
                foreach (RiotSharp.LeagueEndpoint.League league in summoner.GetLeagues())
                {
                    await e.Channel.SendMessage(string.Format("*{0}* is in **{1}** for {2}.", summoner.Name, league.Tier, league.Queue));
                }
            }
            catch (IndexOutOfRangeException)
            {
                await Utils.InproperCommandUsageMessage(e, "rank", "level <REGION> <SUMMONERNAME>");
            }
            catch (RiotSharpException)
            {
                await e.Channel.SendMessage(String.Format("*{0}* is **Unranked**.", summoner.Name));
            }
            catch (SummonerNotFoundException ex)
            {
                await e.Channel.SendMessage(ex.Message);
            }
        }

        /// <summary>
        /// Converts the string input to a RiotSharp.Region.
        /// </summary>
        private Region GetRegion(string input)
        {
            input = input.ToLower();
            Region region;
            #region Region Switch
            switch (input)
            {
                case "br":
                    region = Region.br;
                    break;
                case "eune":
                    region = Region.eune;
                    break;
                case "euw":
                    region = Region.euw;
                    break;
                case "jp":
                    region = Region.jp;
                    break;
                case "kr":
                    region = Region.kr;
                    break;
                case "lan":
                    region = Region.lan;
                    break;
                case "las":
                    region = Region.las;
                    break;
                case "na":
                    region = Region.na;
                    break;
                case "oce":
                    region = Region.oce;
                    break;
                case "ru":
                    region = Region.ru;
                    break;
                case "tr":
                    region = Region.tr;
                    break;
                default:
                    region = Region.global;
                    break;
            }
            #endregion
            return region;
        }

        /// <summary>
        /// Fetches a Summoner from the RiotAPI.
        /// </summary>
        /// <param name="region">The region this Summoner is part of.</param>
        /// <param name="summonerName">The name of the Summoner.</param>
        /// <returns>The Summoner with the given name within the given region.</returns>
        private RiotSharp.SummonerEndpoint.Summoner GetSummoner(string region, string summonerName)
        {
            RiotSharp.SummonerEndpoint.Summoner summoner = null;
            try
            {
                summoner = api.GetSummoner(GetRegion(region), summonerName);
            }
            catch (RiotSharpException)
            {
                throw new SummonerNotFoundException("This Summoner does not exist in this region.");
            }
            return summoner;
        }
    }
}
