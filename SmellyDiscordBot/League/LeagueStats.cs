using Discord.Commands;
using RiotSharp;
using RiotSharp.CurrentGameEndpoint;
using RiotSharp.StaticDataEndpoint;
using RiotSharp.SummonerEndpoint;
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
                Console.WriteLine(ex.Message);
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

            Summoner summoner = null;

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
            catch (RiotSharpException ex)
            {
                Console.WriteLine(ex.Message);
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
                var currentGame = api.GetCurrentGame(GetPlatform(regionString), summoner.Id);
                string[] summoners = new string[10];
                Region region = GetRegion(regionString);

                List<Participant> blueTeam = new List<Participant>();
                List<Participant> redTeam = new List<Participant>();
                for (int i = 0; i < currentGame.Participants.Count; i++)
                {
                    if (currentGame.Participants[i].TeamId == 100)
                        blueTeam.Add(currentGame.Participants[i]);
                    else if (currentGame.Participants[i].TeamId == 200)
                        redTeam.Add(currentGame.Participants[i]);

                    string champion = "";
                    if (currentGame.Participants[i].ChampionId != 0)
                    {
                        champion = " (" + staticApi.GetChampion(region, (Int32)currentGame.Participants[i].ChampionId).Name + ")";
                    }
                    summoners[i] = currentGame.Participants[i].SummonerName.PadRight(20) + champion;
                }

                //Fill up empty slots.
                if (currentGame.Participants.Count < 10)
                {
                    int emptySlots = 10;
                    emptySlots -= redTeam.Count;
                    emptySlots -= blueTeam.Count;
                    Participant emptyParticipant = new Participant();

                    for (int i = 0; i < emptySlots / 2; i++)
                    {
                        redTeam.Add(emptyParticipant);
                        blueTeam.Add(emptyParticipant);
                    }
                }

                int minutes = (Int32) currentGame.GameLength / 60;
                int seconds = (Int32) currentGame.GameLength % 60;

                //Information to display.
                string output = String.Format("*{0}* is currently in a **{1}** game on {2}. ({3})", summoner.Name, currentGame.GameMode, currentGame.MapType, currentGame.GameQueueType) + "\n"
                                               + String.Format("This match started at *{0}* and has been going on for **{1}:{2}** (+ ~ 5 minutes).",
                                                    currentGame.GameStartTime.ToShortTimeString(),
                                                    minutes.ToString().PadLeft(2, '0'),
                                                    seconds.ToString().PadLeft(2, '0'),
                                                    currentGame.GameLength) + "\n"
                                               + "```"
                                               + "".PadLeft(13) + "Blue Team".PadRight(22) + " - " + "".PadLeft(13) + "Red Team".PadRight(22) + "\n"
                                               + "".PadRight(75, '~') + "\n";
                
                for (int i = 0; i < currentGame.Participants.Count / 2; i++)
                {
                    output += summoners[i].PadRight(35);
                    output += " - ";
                    output += summoners[i + 5].PadRight(35);
                    output += " \n";
                }
                                                
                output += "```";
                await e.Channel.SendMessage(output);
            }
            catch (RiotSharpException ex)
            {
                Console.WriteLine(ex.Message);
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
                Region region = GetRegion(regionString);
                string output = "Checking status of: **" + region + "** server. \n ```";
                var shardStatuses = statusApi.GetShardStatus(region);
                shardStatuses = statusApi.GetShardStatus(region);
                foreach (var service in shardStatuses.Services)
                {
                    output += String.Format("Status of {0}: {1}. ({2} incidents happened)", service.Name, service.Status, service.Incidents.Count) + "\n";
                }
                output += "```";
                await e.Channel.SendMessage(output);
            }
            catch (RiotSharpException ex)
            {
                Console.WriteLine(ex.Message);
                await e.Channel.SendMessage("Something went wrong that shouldn't have gone wrong...");
            }
            catch (RegionNotFoundException ex)
            {
                await e.Channel.SendMessage(ex.Message);
            }
        }

        /// <summary>
        /// Fetches a specific champion's info from Riot API.
        /// </summary>
        /// <param name="e">The command event which was executed.</param>
        /// <returns>A message in the channel with information regarding the requested champion.</returns>
        public async Task GetChampionStats(CommandEventArgs e)
        {
            var input = Utils.ReturnInputParameterStringArray(e);
            string championname = input[0].ToLower();

            try
            {
                ChampionStatic champ = GetChampion(Region.na, championname);
                string output = String.Format("Riot's ratings for **{0}**, *{1}*", champ.Name, champ.Title) + "\n \n";

                //Riot Ratings
                output += "*Tags:* ";
                foreach (var tag in champ.Tags)
                {
                    output += tag + " ";
                }
                output += "\n \n";
                output += "*Attack*".PadRight(15);
                for (int i = 0; i < champ.Info.Attack; i++)
                {
                    output += ":crossed_swords: ";
                }
                output += "\n";

                output += "*Defence*".PadRight(15);
                for (int i = 0; i < champ.Info.Defense; i++)
                {
                    output += ":shield: ";
                }
                output += "\n";

                output += "*Magic*".PadRight(15);
                for (int i = 0; i < champ.Info.Defense; i++)
                {
                    output += ":sparkles: ";
                }
                output += "\n";

                output += "*Difficulty*".PadRight(15);
                for (int i = 0; i < champ.Info.Difficulty; i++)
                {
                    output += ":star: ";
                }
                output += "\n";

                //Stats
                output += "\n **Base Stats**";
                output += "```" + "\n";
                //Health
                output += String.Format("Health: {0} (+{1} Health/5s)", 
                            champ.Stats.Hp.ToString(System.Globalization.CultureInfo.InvariantCulture), 
                            champ.Stats.HpRegen.ToString(System.Globalization.CultureInfo.InvariantCulture)).PadRight(40)
                        + String.Format(" - Growth/Level: {0}  (+{1} Health/5s)", 
                            champ.Stats.HpPerLevel.ToString(System.Globalization.CultureInfo.InvariantCulture), 
                            champ.Stats.HpRegenPerLevel.ToString(System.Globalization.CultureInfo.InvariantCulture)) + "\n";
                //Resource
                output += String.Format("{0}: {1} (+{2} {0}/5s)",
                            champ.Partype,
                            champ.Stats.Mp.ToString(System.Globalization.CultureInfo.InvariantCulture),
                            champ.Stats.MpRegen.ToString(System.Globalization.CultureInfo.InvariantCulture)).PadRight(40)
                        + String.Format(" - Growth/Level: {0}  (+{1} {2}/5s)", 
                            champ.Stats.MpPerLevel.ToString(System.Globalization.CultureInfo.InvariantCulture),
                            champ.Stats.MpRegenPerLevel.ToString(System.Globalization.CultureInfo.InvariantCulture) ,
                            champ.Partype) + "\n";
                output += "\n";
                //Defences
                output += String.Format("Armor: {0}", champ.Stats.Armor).PadRight(40)
                        + String.Format(" - Growth/Level: {0}", champ.Stats.ArmorPerLevel.ToString(System.Globalization.CultureInfo.InvariantCulture)) + "\n";
                output += String.Format("Magic Resist: {0}", champ.Stats.SpellBlock).PadRight(40)
                        + String.Format(" - Growth/Level: {0}", champ.Stats.SpellBlockPerLevel.ToString(System.Globalization.CultureInfo.InvariantCulture)) + "\n";
                output += "\n";
                //Attack
                output += String.Format("Attack Damage: {0}", champ.Stats.AttackDamage).PadRight(40)
                        + String.Format(" - Growth/Level: {0}", champ.Stats.AttackDamagePerLevel) + "\n";
                output += String.Format("Attack Speed: {0}", champ.Stats.AttackSpeedOffset).PadRight(40)
                        + String.Format(" - Growth/Level: {0}", champ.Stats.AttackSpeedPerLevel) + "\n";
                output += "\n";
                output += String.Format("Attack Range: {0}", champ.Stats.AttackRange) + "\n";
                output += String.Format("Movement Speed: {0}", champ.Stats.MoveSpeed) + "\n";
                output += "```";

                output += String.Format("**Passive**: *{0}* - {1}", champ.Passive.Name, champ.Passive.SanitizedDescription);
                await e.Channel.SendMessage(output);
            }
            catch (RiotSharpException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (IndexOutOfRangeException)
            {
                await Utils.InproperCommandUsageMessage(e, "champion", "chmapion <REGION> <CHAMPIONNAME>");
            }
            catch (NullReferenceException)
            {
                await e.Channel.SendMessage("Champion not found.");
            }
        }

        /// <summary>
        /// Fetches tips to counter a specific champion from Riot API.
        /// </summary>
        /// <param name="e">The command event which was executed.</param>
        /// <returns>A message in the channel with information regarding the ways to counter the champion.</returns>
        public async Task GetChampionCounter(CommandEventArgs e)
        {
            var input = Utils.ReturnInputParameterStringArray(e);
            string championname = input[0].ToLower();
            try
            {
                ChampionStatic champ = GetChampion(Region.na, championname);
                string output = String.Format("Tips for playing against **{0}**:", champ.Name);
                output += "```" + "\n";
                foreach (string tip in champ.EnemyTips)
                {
                    output += tip + "\n";
                }
                output += "```";
                await e.Channel.SendMessage(output);
            }
            catch (RiotSharpException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (IndexOutOfRangeException)
            {
                await Utils.InproperCommandUsageMessage(e, "counter", "counter <CHAMPIONNAME>");
            }
            catch (NullReferenceException)
            {
                await e.Channel.SendMessage("Champion not found.");
            }
        }

        /// <summary>
        /// Fetches tips to play a specific champion from Riot API.
        /// </summary>
        /// <param name="e">The command event which was executed.</param>
        /// <returns>A message in the channel with information regarding the ways to play the champion.</returns>
        public async Task GetChampionTips(CommandEventArgs e)
        {
            var input = Utils.ReturnInputParameterStringArray(e);
            string championname = input[0].ToLower();
            try
            {
                ChampionStatic champ = GetChampion(Region.na, championname);
                string output = String.Format("Tips for playing **{0}**:", champ.Name);
                output += "```" + "\n";
                foreach (string tip in champ.AllyTips)
                {
                    output += tip + "\n";
                }
                output += "```";
                await e.Channel.SendMessage(output);
            }
            catch (RiotSharpException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (IndexOutOfRangeException)
            {
                await Utils.InproperCommandUsageMessage(e, "counter", "counter <CHAMPIONNAME>");
            }
            catch (NullReferenceException)
            {
                await e.Channel.SendMessage("Champion not found.");
            }
        }

        /// <summary>
        /// Fetches the lore for a specific champion.
        /// </summary>
        /// <param name="e">The command event which was executed.</param>
        /// <returns>A message in the channel with lore regarding the requested champion.</returns>
        public async Task GetChampionLore(CommandEventArgs e)
        {
            var input = Utils.ReturnInputParameterStringArray(e);
            string championname = input[0].ToLower();
            try
            {
                var champ = GetChampion(Region.na, championname);
                string output = String.Format("Champion lore for **{0}**, *{1}*. \n \n", champ.Name, champ.Title);
                output += champ.Lore;
                output = output.Replace("<br>", " \n");
                if (output.Length > 2000)
                {
                    await e.Channel.SendMessage(output.Substring(0, 2000));
                    await e.Channel.SendMessage(output.Substring(2000));
                }
                else
                    await e.Channel.SendMessage(output);
            }
            catch (RiotSharpException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (IndexOutOfRangeException)
            {
                await Utils.InproperCommandUsageMessage(e, "champion", "chmapion <REGION> <CHAMPIONNAME>");
            }
            catch (NullReferenceException)
            {
                await e.Channel.SendMessage("Champion not found.");
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
        private Summoner GetSummoner(string region, string summonerName)
        {
            Summoner summoner = null;
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

        /// <summary>
        /// Fetches a Champion from the static RiotAPI.
        /// </summary>
        /// <param name="region">The region where the information should be fetched from.</param>
        /// <param name="championName">The name of the champion.</param>
        /// <returns></returns>
        private ChampionStatic GetChampion(Region region, string championName)
        {
            if ("tf".Equals(championName.ToLower()))
                championName = "Twisted Fate";
            else if ("asol".Equals(championName.ToLower()))
                championName = "Aurelion Sol";
            var champions = staticApi.GetChampions(region, ChampionData.all).Champions.Values;
            foreach (var champ in champions)
            {
                if (champ.Name.ToLower().Contains(championName.ToLower()))
                {
                    return champ;
                }
            }
            return null;
        }
    }
}
