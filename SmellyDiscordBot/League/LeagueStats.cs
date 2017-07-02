using Discord.Commands;
using RiotSharp;
using SmellyDiscordBot.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmellyDiscordBot.League
{
    public class LeagueStats
    {
        RiotApi api;
        StatusRiotApi statusApi;
        StaticRiotApi staticApi;

        /// <summary>
        /// Constructor for this class which fetches the API instance..
        /// </summary>
        /// <param name="apiKey">A developer key from https://developer.riotgames.com/ </param>
        public LeagueStats(string apiKey)
        {
            if (!"unknown".Equals(apiKey))
            {
                api = RiotApi.GetInstance(apiKey);
                staticApi = StaticRiotApi.GetInstance(apiKey);
            }
            statusApi = StatusRiotApi.GetInstance();
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
                summonerName = input[1];
            else
            {
                summonerName = input[1];
                for (int i = 2; i < input.Length; i++)
                    summonerName = String.Format("{0} {1}", summonerName, input[i]);
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
            catch (Exception ex) when (ex is SummonerNotFoundException || ex is RegionNotFoundException)
            {
                await e.Channel.SendMessage(ex.Message);
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
                summonerName = input[1];
            else
            {
                summonerName = input[1];
                for (int i = 2; i < input.Length; i++)
                    summonerName = String.Format("{0} {1}", summonerName, input[i]);
            }

            RiotSharp.SummonerEndpoint.Summoner summoner = null;

            try
            {
                summoner = GetSummoner(regionString, summonerName);

                string output = String.Format("Ranked statistics for *{0}*: \n ```", summoner.Name);

                foreach (RiotSharp.LeagueEndpoint.League league in summoner.GetLeagues())
                    output += string.Format("\n {0} - {1} - {2}", 
                                                league.Tier.ToString().PadRight(10), 
                                                league.Name.ToString().PadRight(25), 
                                                league.Queue);
                output += "```";

                await e.Channel.SendMessage(output);
            }
            catch (IndexOutOfRangeException)
            {
                await Utils.InproperCommandUsageMessage(e, "rank", "rank <REGION> <SUMMONERNAME>");
            }
            catch (RiotSharpException)
            {
                await e.Channel.SendMessage(String.Format("*{0}* is **Unranked**.", summoner.Name));
            }
            catch (Exception ex) when (ex is SummonerNotFoundException || ex is RegionNotFoundException)
            {
                await e.Channel.SendMessage(ex.Message);
            }
        }

        /// <summary>
        /// Fetches the given Summoner's current game from the RiotAPI for the given region.
        /// </summary>
        /// <param name="e">The command event which was executed.</param>
        /// <returns>A message in the channel with information about the current game of the Summoner in the region.</returns>
        public async Task GetCurrentGameStats(CommandEventArgs e)
        {
            var input = Utils.ReturnInputParameterStringArray(e);
            string regionString = input[0];
            string summonerName = input[1];

            try
            {
                var summoner = GetSummoner(regionString, summonerName);
                var currentGame = api.GetCurrentGame(GetPlatform(regionString), summoner.Id);

                List<RiotSharp.CurrentGameEndpoint.Participant> blueTeam = new List<RiotSharp.CurrentGameEndpoint.Participant>();
                List<RiotSharp.CurrentGameEndpoint.Participant> redTeam = new List<RiotSharp.CurrentGameEndpoint.Participant>();
                foreach (RiotSharp.CurrentGameEndpoint.Participant player in currentGame.Participants)
                {
                    if (player.TeamId == 100)
                        blueTeam.Add(player);
                    else
                        redTeam.Add(player);
                }

                //Fill up empty slots.
                if (currentGame.Participants.Count < 10)
                {
                    int emptySlots = 10;
                    emptySlots -= redTeam.Count;
                    emptySlots -= blueTeam.Count;

                    for (int i = 0; i < emptySlots / 2; i++)
                    {
                        redTeam.Add(new RiotSharp.CurrentGameEndpoint.Participant());
                        blueTeam.Add(new RiotSharp.CurrentGameEndpoint.Participant());
                    }
                }

                int minutes = (Int32) currentGame.GameLength / 60;
                int seconds = (Int32) currentGame.GameLength % 60;

                //Information to display.
                string output = String.Format("*{0}* is currently in a game of **{1}** on {2}.", summoner.Name, currentGame.GameMode, currentGame.MapType) + "\n"
                                               + String.Format("This match started at *{0}* and has been going on for **{1}:{2}** (+ ~ 5 minutes).",
                                                    currentGame.GameStartTime.ToShortTimeString(),
                                                    minutes.ToString().PadLeft(2, '0'),
                                                    seconds.ToString().PadLeft(2, '0'),
                                                    currentGame.GameLength) + "\n"
                                               + "```"
                                               + "Blue Team".PadRight(20) + " - " + "Red Team".PadLeft(20) + "\n"
                                               + "".PadRight(43, '~') + "\n"
                                               + blueTeam[0].SummonerName.ToString().PadRight(20) + " - " + redTeam[0].SummonerName.PadLeft(20) + "\n"
                                               + blueTeam[1].SummonerName.ToString().PadRight(20) + " - " + redTeam[1].SummonerName.PadLeft(20) + "\n"
                                               + blueTeam[2].SummonerName.ToString().PadRight(20) + " - " + redTeam[2].SummonerName.PadLeft(20) + "\n"
                                               + blueTeam[3].SummonerName.ToString().PadRight(20) + " - " + redTeam[3].SummonerName.PadLeft(20) + "\n"
                                               + blueTeam[4].SummonerName.ToString().PadRight(20) + " - " + redTeam[4].SummonerName.PadLeft(20) + "\n"
                                               + "```";
                await e.Channel.SendMessage(output);
            }
            catch (RiotSharpException)
            {
                await e.Channel.SendMessage("This Summoner is currently not in a game.");
            }
            catch (IndexOutOfRangeException)
            {
                await Utils.InproperCommandUsageMessage(e, "currentgame", "currentgame <REGION> <SUMMONERNAME>");
            }
            catch (Exception ex) when (ex is SummonerNotFoundException || ex is RegionNotFoundException)
            {
                await e.Channel.SendMessage(ex.Message);
            }
        }

        /// <summary>
        /// Checks the server status of the requested region.
        /// </summary>
        /// <param name="e">The command event which was executed.</param>
        /// <returns>A message in the channel with information regarding the server status for the requested region.</returns>
        public async Task GetLeagueStatus(CommandEventArgs e)
        {
            var input = Utils.ReturnInputParameterStringArray(e);
            string regionString = input[0];

            try
            {
                string output = "Checking status of: **" + GetRegion(regionString) + "** server. \n ```";
                var shardStatuses = statusApi.GetShardStatus(GetRegion(regionString));
                foreach (var service in shardStatuses.Services)
                {
                    output += String.Format("Status of {0}: {1}. ({2} incidents happened)", service.Name, service.Status, service.Incidents.Count) + "\n";
                }
                output += "```";
                await e.Channel.SendMessage(output);
            }
            catch (RiotSharpException)
            {
                await Utils.InproperCommandUsageMessage(e, "status", "status <REGION>");
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
                    throw new RegionNotFoundException("This region does not exist (yet).");
            }
            #endregion
            return region;
        }

        /// <summary>
        /// Converts the string input to a RiotSharp.Platform.
        /// </summary>
        private Platform GetPlatform(string input)
        {
            input = input.ToLower();
            Platform platform;
            #region Platform Switch
            switch (input)
            {
                case "br":
                    platform = Platform.BR1;
                    break;
                case "eune":
                    platform = Platform.EUN1;
                    break;
                case "euw":
                    platform = Platform.EUW1;
                    break;
                case "jp":
                    throw new RegionNotFoundException("This region does not exist in the API (yet).");
                case "kr":
                    platform = Platform.KR;
                    break;
                case "lan":
                    platform = Platform.LA1;
                    break;
                case "las":
                    platform = Platform.LA2;
                    break;
                case "na":
                    platform = Platform.NA1;
                    break;
                case "oce":
                    platform = Platform.OC1;
                    break;
                case "ru":
                    platform = Platform.RU;
                    break;
                case "tr":
                    platform = Platform.TR1;
                    break;
                default:
                    throw new RegionNotFoundException("This region does not exist (yet).");
            }
            #endregion
            return platform;
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
