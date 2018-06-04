using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ELOBOT.Discord.Context;
using ELOBOT.Discord.Extensions;
using ELOBOT.Discord.Preconditions;
using ELOBOT.Models;

namespace ELOBOT.Modules
{
    [CustomPermissions(true, true)]
    public class Results : Base
    {
        [Command("Game", RunMode = RunMode.Async)]
        public async Task Game(IMessageChannel Lobby, int GameNumber, GuildModel.GameResult._Result Result)
        {
            if (Context.Server.Lobbies.All(x => x.ChannelID != Lobby.Id))
            {
                throw new Exception("Channel is not a lobby");
            }

            //var lobby = Context.Server.Lobbies.FirstOrDefault(x => x.ChannelID == Lobby.Id);
            var game = Context.Server.Results.FirstOrDefault(x => x.LobbyID == Lobby.Id && x.Gamenumber == GameNumber);
            if (game.Result != GuildModel.GameResult._Result.Undecided)
            {
                await SimpleEmbedAsync("This game's Result has already been set to:\n" +
                                       $"{game.Result.ToString()}\n" +
                                       "Please reply with `Continue` To Still modify the result and update scores\n" +
                                       "Or Reply with `Cancel` to cancel this command");
                var next = await NextMessageAsync(true, true, TimeSpan.FromMinutes(1));
                if (next.Content.ToLower() == "continue")
                {
                    await GameManagement.GameResult(Context, game, Result);
                }
                else
                {
                    await SimpleEmbedAsync("Cancelled command.");
                }
            }
            else
            {
                await GameManagement.GameResult(Context, game, Result);
            }
            
        }
    }
}
