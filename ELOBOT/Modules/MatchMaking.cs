using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using ELOBOT.Discord.Context;
using ELOBOT.Discord.Preconditions;

namespace ELOBOT.Modules
{

    [CheckLobby]
    [CheckRegistered]
    public class MatchMaking : Base
    {
        [Command("Join")]
        public async Task JoinLobby()
        {
            if (!Context.Elo.Lobby.Game.QueuedPlayerIDs.Contains(Context.User.Id))
            {
                if (Context.Server.Settings.GameSettings.BlockMultiQueuing)
                {
                    if (Context.Server.Lobbies.Any(x => x.Game.QueuedPlayerIDs.Contains(Context.User.Id)))
                    {
                        throw new Exception("MultiQueuing is disabled by the server Admins");
                    }
                }

                Context.Elo.Lobby.Game.QueuedPlayerIDs.Add(Context.User.Id);
                Context.Server.Save();
                await SimpleEmbedAsync($"Success, Added to queue, [{Context.Elo.Lobby.Game.QueuedPlayerIDs.Count}/{Context.Elo.Lobby.UserLimit}]");
                if (Context.Elo.Lobby.UserLimit >= Context.Elo.Lobby.Game.QueuedPlayerIDs.Count)
                {
                    //Game is ready to be played
                    await Discord.Extensions.FullGame.FullQueue(Context);
                }
            }
        }
        [Command("Leave")]
        public async Task LeaveLobby()
        {
            if (Context.Elo.Lobby.Game.QueuedPlayerIDs.Contains(Context.User.Id))
            {
                Context.Elo.Lobby.Game.QueuedPlayerIDs.Remove(Context.User.Id);
                await SimpleEmbedAsync($"Success, Removed from queue, [{Context.Elo.Lobby.Game.QueuedPlayerIDs.Count}/{Context.Elo.Lobby.UserLimit}]");
                Context.Server.Save();
            }
        }

        [Command("Queue")]
        public async Task Queue()
        {
            var QueuedPlayers = Context.Elo.Lobby.Game.QueuedPlayerIDs.Select(p => Context.Socket.Guild.GetUser(p));
            await SimpleEmbedAsync($"**Player List**\n" +
                                   $"{string.Join("\n", QueuedPlayers.Select(x => x.Mention))}");
        }

        [Command("Lobby")]
        public async Task LobbyInfo()
        {
            await SimpleEmbedAsync("Lobby Created.\n" +
                                   $"Players Per team: {Context.Elo.Lobby.UserLimit / 2}\n" +
                                   $"Total Players: {Context.Elo.Lobby.UserLimit}\n" +
                                   $"Sort Mode: {Context.Elo.Lobby.PickMode.ToString()}\n" +
                                   $"Game Number: {Context.Elo.Lobby.GamesPlayed + 1}\n" +
                                   $"Channel: {Context.Channel.Name}\n" +
                                   "Description:\n" +
                                   $"{Context.Elo.Lobby.Description}");
        }
    }
}
