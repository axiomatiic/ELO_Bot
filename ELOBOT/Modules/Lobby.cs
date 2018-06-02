using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ELOBOT.Discord.Context;
using ELOBOT.Discord.Preconditions;
using ELOBOT.Handlers;
using ELOBOT.Models;
using Raven.Client.Documents.Linq.Indexing;

namespace ELOBOT.Modules
{
    [CheckRegistered]
    public class Lobby : Base
    {
        [Command("CreateLobby", RunMode = RunMode.Async)]
        public async Task CreateLobby()
        {
            if (Context.Elo.Lobby != null)
            {
                throw new Exception("Channel is already a lobby");
            }

            var lobby = new GuildModel.Lobby
            {
                ChannelID = Context.Channel.Id
            };

            var embed = await ReplyAsync(new EmbedBuilder
            {
                Description = "Please reply with the amount of players you would like **PER TEAM**",
                Color = Color.Blue
            });
            var next = await NextMessageAsync(true, true, TimeSpan.FromMinutes(1));
            if (int.TryParse(next.Content, out var PlayerCount))
            {
                if (PlayerCount <= 0)
                {
                    throw new Exception("Playercount must be a whole integer greater than 0, aborting the Lobby Setup.");
                }
                lobby.UserLimit = PlayerCount * 2;
                await next.DeleteAsync();
            }
            else
            {
                throw new Exception("Please reply with only a number, aborting the Lobby Setup.");
            }

           await  embed.ModifyAsync(x => x.Embed = new EmbedBuilder{Description = "Please reply with the team sorting mode you would like for this lobby:\n" +
                                                               "`CompleteRandom` __**Completely Random Team sorting**__\n" +
                                                               "All teams are chosen completely randomly\n\n" +
                                                               "`Captains` __**Captains Mode**__\n" +
                                                               "Two team captains are chosen, they each take turns picking players until teams are both filled.\n\n" +
                                                               "`SortByScore` __**Score Balance Mode**__\n" +
                                                               "Players will be automatically selected and teams will be balanced based on player scores",
               Color = Color.Blue
           }.Build());


            next = await NextMessageAsync(true, true, TimeSpan.FromMinutes(1));
            if (Enum.TryParse<GuildModel.Lobby._PickMode>(next.Content, out var PickMode))
            {
                lobby.PickMode = PickMode;
                await next.DeleteAsync();
            }
            else
            {
                throw new Exception("Please reply with only the sortmode, ie `Captains`, aborting the Lobby Setup.");
            }

            await embed.ModifyAsync(x => x.Embed = new EmbedBuilder
            {
                Description = "Please reply with a description of this lobby",
                Color = Color.Blue
            }.Build());

            next = await NextMessageAsync(true, true, TimeSpan.FromMinutes(1));
            lobby.Description = next.Content;
            await next.DeleteAsync();
            Context.Server.Lobbies.Add(lobby);
            Context.Server.Save();
            await embed.ModifyAsync(x => x.Embed = new EmbedBuilder
            {
                Title = "Success!",
                Description = "Lobby Created.\n" +
                              $"Players Per team: {lobby.UserLimit/2}\n" +
                              $"Total Players: {lobby.UserLimit}\n" +
                              $"Sort Mode: {lobby.PickMode.ToString()}\n" +
                              $"Channel: {Context.Channel.Name}\n" +
                              "Description:\n" +
                              $"{lobby.Description}",
                Color = Color.Green
            }.Build());
        }

        [CheckLobby]
        [Command("RemoveLobby")]
        public async Task RemoveLobby()
        {
            Context.Server.Lobbies.Remove(Context.Elo.Lobby);
            Context.Server.Save();
            await SimpleEmbedAsync("Success, Lobby has been removed.");
        }

        [CheckLobby]
        [Command("ClearQueue")]
        public async Task ClearQueue()
        {
            Context.Elo.Lobby.Game.Team1 = new GuildModel.Lobby.CurrentGame.Team();
            Context.Elo.Lobby.Game.Team2 = new GuildModel.Lobby.CurrentGame.Team();
            Context.Server.Save();
            await SimpleEmbedAsync("Queue has been cleared");
        }

        [CheckLobby]
        [Command("LobbySortMode")]
        public async Task LobbySortMode(GuildModel.Lobby._PickMode SortMode)
        {
            Context.Elo.Lobby.PickMode = SortMode;
            Context.Server.Save();

            await SimpleEmbedAsync("Success, lobby team sort mode has been modified to:\n" +
                                   $"{SortMode.ToString()}");
        }

        [CheckLobby]
        [Command("LobbySortMode")]
        public async Task LobbySortMode()
        {
            await SimpleEmbedAsync($"Please use command `{Context.Prefix}LobbySortMode <mode>` with the selection mode you would like for this lobby:\n" +
                              "`CompleteRandom` __**Completely Random Team sorting**__\n" +
                              "All teams are chosen completely randomly\n\n" +
                              "`Captains` __**Captains Mode**__\n" +
                              "Two team captains are chosen, they each take turns picking players until teams are both filled.\n\n" +
                              "`SortByScore` __**Score Balance Mode**__\n" +
                              "Players will be automatically selected and teams will be balanced based on player scores");
        }

        [CheckLobby]
        [Command("CaptainSortMode")]
        public async Task CapSortMode(GuildModel.Lobby.CaptainSort SortMode)
        {
            Context.Elo.Lobby.CaptainSortMode = SortMode;
            Context.Server.Save();

            await SimpleEmbedAsync("Success, captain sort mode has been modified to:\n" +
                                   $"{SortMode.ToString()}");
        }

        [CheckLobby]
        [Command("CaptainSortMode")]
        public async Task CapSortMode()
        {
            await SimpleEmbedAsync($"Please use command `{Context.Prefix}CapSortMode <mode>` with the captain selection mode you would like for this lobby:\n" +
                                   "`MostWins` __**Choose Two Players with Highest Wins**__\n" +
                                   "Selects the two players with the highest amount of Wins\n\n" +
                                   "`MostPoints` __**Choose Two Players with Highest Points**__\n" +
                                   "Selects the two players with the highest amount of Points\n\n" +
                                   "`HighestWinLoss` __**Selects the two players with the highest Win/Loss Ratio**__\n" +
                                   "Selects the two players with the highest win/loss ratio\n\n" +
                                   "`Random` __**Random**__\n" +
                                   "Selects Randomly\n\n" +
                                   "`RandomTop4MostPoints` __**Selects Random from top 4 Most Points**__\n" +
                                   "Selects Randomly from the top 4 highest ranked players based on points\n\n" +
                                   "`RandomTop4MostWins` __**Selects Random from top 4 Most Wins**__\n" +
                                   "Selects Randomly from the top 4 highest ranked players based on wins\n\n" +
                                   "`RandomTop4HighestWinLoss` __**Selects Random from top 4 Highest Win/Loss Ratio**__\n" +
                                   "Selects Randomly from the top 4 highest ranked players based on win/loss ratio");
        }

        [CheckLobby]
        [Command("AddMap")]
        public async Task AddMap([Remainder] string mapname)
        {
            if (!Context.Elo.Lobby.Maps.Contains(mapname))
            {
                Context.Elo.Lobby.Maps.Add(mapname);
                Context.Server.Save();
                await SimpleEmbedAsync("Success, Lobby Map list is now:\n" +
                                       $"{string.Join("\n", Context.Elo.Lobby.Maps)}");
            }
            else
            {
                throw new Exception("Map has already been added to the lobby");
            }
        }
        [CheckLobby]
        [Command("DelMap")]
        public async Task DelMap([Remainder] string mapname)
        {
            if (Context.Elo.Lobby.Maps.Contains(mapname))
            {
                Context.Elo.Lobby.Maps.Remove(mapname);
                Context.Server.Save();
                await SimpleEmbedAsync("Success, Lobby Map list is now:\n" +
                                       $"{string.Join("\n", Context.Elo.Lobby.Maps)}");
            }
            else
            {
                throw new Exception("Map is not in lobby");
            }
        }
        [CheckLobby]
        [Command("AddMaps")]
        public async Task AddMaps([Remainder] string maplist)
        {
            var maps = maplist.Split(",");
            if (!Context.Elo.Lobby.Maps.Any(x => maps.Contains(x)))
            {
                Context.Elo.Lobby.Maps.AddRange(maps);
                Context.Server.Save();
                await SimpleEmbedAsync("Success, Lobby Map list is now:\n" +
                                       $"{string.Join("\n", Context.Elo.Lobby.Maps)}");
            }
            else
            {
                throw new Exception("One of the provided maps is already in the lobby");
            }
        }
        [CheckLobby]
        [Command("ClearMaps")]
        public async Task ClearMaps()
        {
            Context.Elo.Lobby.Maps = new List<string>();
            Context.Server.Save();
            await SimpleEmbedAsync("Map List for this lobby has been reset.");
        }
    }
}
