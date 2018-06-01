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
                throw new Exception("Please reply with only the sortmode, ie `Captains`");
            }

            await embed.ModifyAsync(x => x.Embed = new EmbedBuilder
            {
                Description = "Please reply with a description of this lobby",
                Color = Color.Blue
            }.Build());

            next = await NextMessageAsync(true, true, TimeSpan.FromMinutes(1));
            lobby.Description = next.Content;

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

        [Command("RemoveLobby")]
        public async Task RemoveLobby()
        {
            if (Context.Elo.Lobby == null)
            {
                throw new Exception("This channel is not a lobby");
            }

            Context.Server.Lobbies.Remove(Context.Elo.Lobby);
            Context.Server.Save();
            await SimpleEmbedAsync("Success, Lobby has been removed.");
        }

        [Command("ClearQueue")]
        public async Task ClearQueue()
        {
            if (Context.Elo.Lobby == null)
            {
                throw new Exception("This channel is not a lobby");
            }

            Context.Elo.Lobby.Game.Team1 = new GuildModel.Lobby.CurrentGame.Team();
            Context.Elo.Lobby.Game.Team2 = new GuildModel.Lobby.CurrentGame.Team();
            Context.Server.Save();
            await SimpleEmbedAsync("Queue has been cleared");
        }

        [Command("CaptainSortMode")]
        public async Task CapSortMode(GuildModel.Lobby.CaptainSort SortMode)
        {
            if (Context.Elo.Lobby == null)
            {
                throw new Exception("This channel is not a lobby");
            }

            Context.Elo.Lobby.CaptainSortMode = SortMode;
            Context.Server.Save();

            await SimpleEmbedAsync("Success, captain sort mode has been modified to:\n" +
                                   $"{SortMode.ToString()}");
        }
        [Command("CaptainSortMode")]
        public async Task CapSortMode()
        {
            if (Context.Elo.Lobby == null)
            {
                throw new Exception("This channel is not a lobby");
            }

            await SimpleEmbedAsync($"Please use `{Context.Prefix}CapSortMode <mode>` with the captain selection mode you would like for this lobby:\n" +
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
    }
}
