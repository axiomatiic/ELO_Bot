using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using ELO_Bot.Preconditions;

namespace ELO_Bot.Commands.Admin
{
    /// <summary>
    ///     checks ensure that blacklisted commands are not run
    ///     and that these commands are only used by an admin
    /// </summary>
    [CheckBlacklist(true)]
    //[CheckAdmin]
    public class Lobby : InteractiveBase
    {
        [Command("CreateLobby", RunMode = RunMode.Async)]
        [Summary("CreateLobby")]
        [Remarks("Create A Lobby for the current channel")]
        public async Task CreateLobby1([Remainder] string overflow = null)
        {
            var server = Servers.ServerList.First(x => x.ServerId == Context.Guild.Id);
            var lobbyexists = server.Queue.FirstOrDefault(x => x.ChannelId == Context.Channel.Id);
            if (lobbyexists != null)
            {
                await ReplyAsync("This channel is already a lobby.");
                return;
            }

            var TotalPlayers = 0;
            Servers.Server.PickModes PlayerSortMode = 0;
            await ReplyAsync(
                "Please reply with the numbers of players you would like in this lobby. Ie. `10` will give you two teams of 5");
            var playercount = await NextMessageAsync(timeout: TimeSpan.FromMinutes(1d));
            if (int.TryParse(playercount.Content, out var i))
            {
                if (i % 2 != 0 || i < 2)
                {
                    await ReplyAsync("ERROR: Number must be even! Exiting Lobby Setup.");
                    return;
                }

                TotalPlayers = i;
            }
            else
            {
                await ReplyAsync("Error: Please reply with Just a number! Exiting Lobby Setup.");
                return;
            }

            await ReplyAsync("Please reply with the team sorting mode you would like for this lobby:\n" +
                             "`0` __**Completely Random Team sorting**__\n" +
                             "All teams are chosen completely randomly\n\n" +
                             "`1` __**Captains Mode**__\n" +
                             "Two team captains are chosen, they each take turns picking players until teams are both filled.\n\n" +
                             "`2` __**Score Balance Mode**__\n" +
                             "Players will be automatically selected and teams will be balanced based on player scores");
            var PickMode = await NextMessageAsync(timeout: TimeSpan.FromMinutes(1d));
            if (int.TryParse(PickMode.Content, out i))
            {
                if (i < 0 || i > 2)
                {
                    await ReplyAsync("Error: Please reply with `0`, `1`, or `2` Only! Exiting Lobby Setup.");
                    return;
                }
                PlayerSortMode = (Servers.Server.PickModes) i;
            }
            else
            {
                await ReplyAsync("Error: Please reply with Just a number! Exiting Lobby Setup.");
                return;
            }
            await ReplyAsync("Please specify a description for this lobby:\n" +
                             "ie. \"Ranked Gamemode, 5v5 ELITE Players Only!\"");
            var LobbyDesc = await NextMessageAsync(timeout: TimeSpan.FromMinutes(1d));

            var PickString = "";
            switch (PlayerSortMode)
            {
                case Servers.Server.PickModes.CompleteRandom:
                    PickString = "Random";
                    break;
                case Servers.Server.PickModes.Captains:
                    PickString = "Captains";
                    break;
                case Servers.Server.PickModes.SortByScore:
                    PickString = "Score Sort";
                    break;
            }

            server.Queue.Add(new Servers.Server.Q
            {
                ChannelId = Context.Channel.Id,
                ChannelGametype = LobbyDesc.Content,
                PickMode = PlayerSortMode,
                Users = new List<ulong>(),
                UserLimit = TotalPlayers
            });

            var embed = new EmbedBuilder
            {
                Title = "Success Lobby Created",
                Description = $"Lobby Name: {Context.Channel.Name}\n" +
                              $"Players: {TotalPlayers}\n" +
                              $"Pick Mode: {PickString}\n" +
                              $"Description:\n" +
                              $"{LobbyDesc.Content}",
                Color = Color.Blue
            };
            await ReplyAsync("", false, embed.Build());
        }

/*

        /// <summary>
        ///     creates a lobby
        ///     requires the user to provide the following:
        ///     Users Per Game
        ///     If teams are automatically chosen or chosen by team captains
        ///     Lobby Description
        /// </summary>
        /// <param name="lolsdvdv">ignored text, previously used in a different context</param>
        /// <returns></returns>
        [Command("Createlobby", RunMode = RunMode.Async)]
        [Summary("Createlobby")]
        [Remarks("Initialise a lobby in the current channel")]
        public async Task Createlobby([Remainder] string lolsdvdv = null)
        {
            var server = Servers.ServerList.First(x => x.ServerId == Context.Guild.Id);

            var embed = new EmbedBuilder();

            var lobbyexists = server.Queue.FirstOrDefault(x => x.ChannelId == Context.Channel.Id);
            if (lobbyexists == null)
            {
                await ReplyAsync(
                    "Please reply with a number for the amount of players you want for the lobby, ie. 10 gives two teams of 5");
                var n1 = await NextMessageAsync(timeout: TimeSpan.FromMinutes(1d));
                if (int.TryParse(n1.Content, out var i))
                    if (i % 2 != 0 || i < 2)
                    {
                        await ReplyAsync("ERROR: Number must be even!");
                    }
                    else
                    {
                        await ReplyAsync("Please reply:\n" +
                                         "`true` for captains to choose the players for each team\n" +
                                         "`false` for teams to automatically be chosen");
                        var n2 = await NextMessageAsync(timeout: TimeSpan.FromMinutes(1d));
                        if (bool.TryParse(n2.Content, out var captains))
                        {
                            await ReplyAsync("Please specify a description for this lobby:\n" +
                                             "ie. \"Ranked Gamemode, 5v5 ELITE Players Only!\"");
                            var n3 = await NextMessageAsync(timeout: TimeSpan.FromMinutes(1d));
                            if (i == 2)
                                captains = false;

                            var ser = Servers.ServerList.First(x => x.ServerId == Context.Guild.Id);
                            ser.Queue.Add(new Servers.Server.Q
                            {
                                ChannelId = Context.Channel.Id,
                                Users = new List<ulong>(),
                                UserLimit = i,
                                ChannelGametype = n3.Content,
                                Captains = captains
                            });


                            embed.AddField("LOBBY CREATED", $"**Lobby Name:** \n{Context.Channel.Name}\n" +
                                                            $"**PlayerLimit:** \n{i}\n" +
                                                            $"**Captains:** \n{captains}\n" +
                                                            "**GameMode Info:**\n" +
                                                            $"{n3.Content}");
                            await ReplyAsync("", false, embed.Build());
                        }
                        else
                        {
                            await ReplyAsync("ERROR: Invalid type specified.");
                        }
                    }
                else
                    await ReplyAsync("ERROR: Not an integer");
            }
            else
            {
                await ReplyAsync(
                    $"ERROR: Current channel is already a lobby OR Command timed out. {Context.User.Mention}");
            }
        }*/

        /// <summary>
        ///     removes the current channel from being used as a lobby if applicable
        /// </summary>
        /// <returns></returns>
        [Command("RemoveLobby")]
        [Summary("RemoveLobby")]
        [Remarks("Remove A Lobby")]
        public async Task ClearLobby()
        {
            var server = Servers.ServerList.First(x => x.ServerId == Context.Guild.Id);
            var q = server.Queue.FirstOrDefault(x => x.ChannelId == Context.Channel.Id);
            server.Queue.Remove(q);

            var removegames = server.Gamelist.Where(game => game.LobbyId == Context.Channel.Id).ToList();
            foreach (var game in removegames)
                if (server.Gamelist.Contains(game))
                    server.Gamelist.Remove(game);

            await ReplyAsync($"{Context.Channel.Name} is no longer a lobby!\n" +
                             "Previous games that took place in this lobby have been cleared from history.");
        }

        /// <summary>
        ///     removes all players form the current queue
        /// </summary>
        /// <returns></returns>
        [Command("ClearQueue")]
        [Summary("ClearQueue")]
        [Remarks("Clear All Players from The Queue")]
        public async Task ClearQueue()
        {
            var server = Servers.ServerList.First(x => x.ServerId == Context.Guild.Id);
            var q = server.Queue.FirstOrDefault(x => x.ChannelId == Context.Channel.Id);

            if (q == null)
            {
                await ReplyAsync("ERROR: Current Channel is not a lobby!");
                return;
            }

            q.Users = new List<ulong>();
            q.IsPickingTeams = false;
            q.Team1 = new List<ulong>();
            q.Team2 = new List<ulong>();
            q.T1Captain = 0;
            q.T2Captain = 0;

            await ReplyAsync($"{Context.Channel.Name}'s queue has been cleared!");
        }

        [Command("PickMode")]
        [Summary("PickMode <mode number>")]
        [Remarks("Set the way teams are sorted")]
        public async Task PickMode(int mode = 999)
        {
            var server = Servers.ServerList.First(x => x.ServerId == Context.Guild.Id);
            var lobby = server.Queue.FirstOrDefault(x => x.ChannelId == Context.Channel.Id);
            if (lobby == null)
            {
                await ReplyAsync("Current channel is not a lobby!");
                return;
            }
            if (mode == 999)
            {
                await ReplyAsync($"Please use `{Config.Load().Prefix}pickmode <mode number>` with the team sorting mode you would like for this lobby:\n" +
                                 "`0` __**Completely Random Team sorting**__\n" +
                                 "All teams are chosen completely randomly\n\n" +
                                 "`1` __**Captains Mode**__\n" +
                                 "Two team captains are chosen, they each take turns picking players until teams are both filled.\n\n" +
                                 "`2` __**Score Balance Mode**__\n" +
                                 "Players will be automatically selected and teams will be balanced based on player scores");
                return;
            }

            if (mode < 0 || mode > 2)
            {
                await ReplyAsync("Invalid Mode!");
                return;
            }

            var PickString = "";
            switch ((Servers.Server.PickModes)mode)
            {
                case Servers.Server.PickModes.CompleteRandom:
                    PickString = "Random";
                    break;
                case Servers.Server.PickModes.Captains:
                    PickString = "Captains";
                    break;
                case Servers.Server.PickModes.SortByScore:
                    PickString = "Score Sort";
                    break;
            }
            var embed = new EmbedBuilder
            {
                Description = "Success! The Current Channel's Sorting Mode is now:\n" +
                              $"{PickString}"
            };

            lobby.PickMode = (Servers.Server.PickModes) mode;


            await ReplyAsync("", false, embed.Build());
        }

        [Command("CapSortMode")]
        [Summary("CapSortMode <mode number>")]
        [Remarks("Set the way captains are chosen (only needed for captains lobbies)")]
        public async Task CSortMode(int mode = 999)
        {
            var server = Servers.ServerList.First(x => x.ServerId == Context.Guild.Id);
            var lobby = server.Queue.FirstOrDefault(x => x.ChannelId == Context.Channel.Id);
            if (lobby == null)
            {
                await ReplyAsync("Current channel is not a lobby!");
                return;
            }
            if (mode == 999)
            {
                await ReplyAsync($"Please use `{Config.Load().Prefix}CapSortMode <mode number>` with the captain selection mode you would like for this lobby:\n" +
                                 "`0` __**Choose Two Players with Highest Wins**__\n" +
                                 "Selects the two players with the highest amount of Wins\n\n" +
                                 "`1` __**Choose Two Players with Highest Points**__\n" +
                                 "Selects the two players with the highest amount of Points\n\n" +
                                 "`2` __**Selects the two players with the highest Win/Loss Ratio**__\n" +
                                 "Selects the two players with the highest win/loss ratio\n\n" +
                                 "`3` __**Random**__\n" +
                                 "Selects Randomly\n\n" +
                                 "`4` __**Selects Random from top 4 Most Points**__\n" +
                                 "Selects Randomly from the top 4 highest ranked players based on points\n\n" +
                                 "`5` __**Selects Random from top 4 Most Wins**__\n" +
                                 "Selects Randomly from the top 4 highest ranked players based on wins\n\n" +
                                 "`6` __**Selects Random from top 4 Highest Win/Loss Ratio**__\n" +
                                 "Selects Randomly from the top 4 highest ranked players based on win/loss ratio");
                return;
            }
            
            if (mode < 0 || mode > 6)
            {
                await ReplyAsync("Invalid Mode!");
                return;
            }

            var PickString = "";
            switch ((Servers.Server.CaptainSortMode)mode)
            {
                case Servers.Server.CaptainSortMode.MostPoints:
                    PickString = "Most Points";
                    break;
                case Servers.Server.CaptainSortMode.MostWins:
                    PickString = "Most Wins";
                    break;
                case Servers.Server.CaptainSortMode.HighestWinLoss:
                    PickString = "Highest Win/Loss Ratio";
                    break;
                case Servers.Server.CaptainSortMode.Random:
                    PickString = "Random";
                    break;
                case Servers.Server.CaptainSortMode.RandomTop4MostPoints:
                    PickString = "Random Top4 Most Points";
                    break;
                case Servers.Server.CaptainSortMode.RandomTop4MostWins:
                    PickString = "Random Top4 Most Wins";
                    break;
                case Servers.Server.CaptainSortMode.RandomTop4HighestWinLoss:
                    PickString = "Random Top4 Highest Win/Loss Ratio";
                    break;
            }
            var embed = new EmbedBuilder
            {
                Description = "Success! The Current Channel's Captain Picking Mode is now:\n" +
                              $"{PickString}"
            };

            lobby.CaptainSortMode = (Servers.Server.CaptainSortMode)mode;


            await ReplyAsync("", false, embed.Build());
        }

        /// <summary>
        ///     adds a list of maps to the current lobby
        /// </summary>
        /// <param name="mapName">list of maps ie. map1 map2 map3...</param>
        /// <returns></returns>
        [Command("AddMap")]
        [Summary("AddMap <Map_0> <Map_1>")]
        [Remarks("Add A Map")]
        public async Task AddMap(params string[] mapName)
        {
            var embed = new EmbedBuilder();
            var server = Servers.ServerList.First(x => x.ServerId == Context.Guild.Id);
            var lobby = server.Queue.FirstOrDefault(x => x.ChannelId == Context.Channel.Id);
            if (lobby == null)
            {
                await ReplyAsync("Current channel is not a lobby!");
                return;
            }

            foreach (var map in mapName)
                if (!lobby.Maps.Contains(map))
                {
                    lobby.Maps.Add(map);
                    embed.Description += $"Map added {map}\n";
                }
                else
                {
                    embed.Description += $"Map Already Exists {map}\n";
                }

            await ReplyAsync("", false, embed.Build());
        }

        /// <summary>
        ///     remove a map from the maps list
        /// </summary>
        /// <param name="mapName">the name of the map you want to delete</param>
        /// <returns></returns>
        [Command("DelMap")]
        [Summary("DelMap <MapName>")]
        [Remarks("Delete A Map")]
        public async Task DeleteMap(string mapName)
        {
            var server = Servers.ServerList.First(x => x.ServerId == Context.Guild.Id);
            var lobby = server.Queue.FirstOrDefault(x => x.ChannelId == Context.Channel.Id);

            if (lobby == null)
            {
                await ReplyAsync("Current channel is not a lobby!");
                return;
            }

            if (lobby.Maps.Select(x => x.ToLower()).Contains(mapName.ToLower()))
            {
                lobby.Maps.Remove(mapName);
                await ReplyAsync($"Map Removed {mapName}");
            }
            else
            {
                await ReplyAsync($"Map doesnt exist {mapName}");
            }
        }

        /// <summary>
        ///     rather than adding maps, set the entire list of maps for the current channel
        /// </summary>
        /// <param name="mapName">list of maps ie. map1 map2 map3...</param>
        /// <returns></returns>
        [Command("SetMaps")]
        [Summary("SetMaps <Map_0> <Map_1>")]
        [Remarks("Set all maps for the current lobby")]
        public async Task SetMaps(params string[] mapName)
        {
            var embed = new EmbedBuilder();
            var server = Servers.ServerList.First(x => x.ServerId == Context.Guild.Id);
            var lobby = server.Queue.FirstOrDefault(x => x.ChannelId == Context.Channel.Id);
            if (lobby == null)
            {
                await ReplyAsync("Current channel is not a lobby!");
                return;
            }

            embed.Title = $"{Context.Channel.Name}";

            foreach (var map in mapName)
                embed.Description += $"{map}\n";

            lobby.Maps = mapName.ToList();

            await ReplyAsync("", false, embed.Build());
        }

        [Command("NoPair")]
        [Summary("NoPair")]
        [Remarks("Disable the use of pairing features in the current queue")]
        public async Task NoPair()
        {
            var server = Servers.ServerList.First(x => x.ServerId == Context.Guild.Id);
            var lobby = server.Queue.FirstOrDefault(x => x.ChannelId == Context.Channel.Id);
            if (lobby == null)
            {
                await ReplyAsync("Current channel is not a lobby!");
                return;
            }

            lobby.NoPairs = !lobby.NoPairs;
            if (lobby.NoPairs)
            {
                await ReplyAsync("Pairs will no longer be available to this lobbn" +
                                 "NOTE: All pairs for the current lobby have now been deleted.");
                lobby.Pairs = new List<Servers.Server.Q.Buddy>();
            }
            else
            {
                await ReplyAsync("Users may pair up with a friend in this lobby now");
            }
        }

        /// <summary>
        ///     clears all maps for the current lobby
        /// </summary>
        /// <returns></returns>
        [Command("ClearMaps")]
        [Summary("ClearMaps")]
        [Remarks("Clear all maps for the current lobby")]
        public async Task ClearMap()
        {
            var server = Servers.ServerList.First(x => x.ServerId == Context.Guild.Id);
            var lobby = server.Queue.FirstOrDefault(x => x.ChannelId == Context.Channel.Id);

            if (lobby == null)
            {
                await ReplyAsync("Current channel is not a lobby!");
                return;
            }

            lobby.Maps = new List<string>();
            await ReplyAsync("Maps Cleared.");
        }

        [Command("GameList")]
        [Summary("GameList")]
        [Remarks("List previous games and their results")]
        public async Task GameList([Remainder] string lobby = null)
        {
            var server = Servers.ServerList.First(x => x.ServerId == Context.Guild.Id);

            IMessageChannel c = Context.Channel;

            if (lobby != null)
                c = Context.Guild.TextChannels.FirstOrDefault(x => x.Name.ToLower() == lobby.ToLower()) ??
                    Context.Channel;

            var games = server.Gamelist.Where(x => x.LobbyId == c.Id)
                .OrderByDescending(x => x.GameNumber);
            var gameresults = "";
            var pages = new List<string>();
            foreach (var game in games)
            {
                if (game.Cancelled)
                {
                    gameresults += $"{c.Name} {game.GameNumber} **Cancelled**\n";
                }
                else
                {
                    if (game.Result is true)
                        gameresults += $"{c.Name} {game.GameNumber} **Team1**\n";
                    else if (game.Result is false)
                        gameresults += $"{c.Name} {game.GameNumber} **Team2**\n";
                    else
                        gameresults += $"{c.Name} {game.GameNumber} **Undecided**\n";                    
                }

                var numLines = gameresults.Split('\n').Length;
                if (numLines > 20)
                {
                    pages.Add(gameresults);
                    gameresults = "";
                }
            }

            pages.Add(gameresults);

            var msg = new PaginatedMessage
            {
                Pages = pages,
                Title = "Previous Games List",
                Color = Color.Green
            };

            await PagedReplyAsync(msg);
        }
    }
}