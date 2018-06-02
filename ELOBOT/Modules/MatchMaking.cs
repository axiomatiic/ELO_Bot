using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ELOBOT.Discord.Context;
using ELOBOT.Discord.Preconditions;
using Raven.Client.Documents.Linq.Indexing;

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

        [Command("Pick")]
        public async Task PickUser(IGuildUser User)
        {
            if (!Context.Elo.Lobby.Game.IsPickingTeams)
            {
                throw new Exception("Lobby is not picking teams at the moment.");
            }

            if (Context.Elo.Lobby.Game.Team1.Captain != Context.User.Id && Context.Elo.Lobby.Game.Team2.Captain != Context.User.Id)
            {
                throw new Exception("User is not a captain");
            }

            if (!Context.Elo.Lobby.Game.QueuedPlayerIDs.Contains(User.Id))
            {
                throw new Exception("User is not pickable");
            }

            int TeamNext;
            if (Context.Elo.Lobby.Game.Team1.TurnToPick)
            {
                if (Context.User.Id != Context.Elo.Lobby.Game.Team1.Captain)
                {
                    throw new Exception("It is not your turn to pick.");
                }

                Context.Elo.Lobby.Game.Team1.Players.Add(User.Id);
                TeamNext = 2;
                Context.Elo.Lobby.Game.Team2.TurnToPick = true;
                Context.Elo.Lobby.Game.Team1.TurnToPick = false;
            }
            else
            {
                if (Context.User.Id != Context.Elo.Lobby.Game.Team2.Captain)
                {
                    throw new Exception("It is not your turn to pick.");
                }

                Context.Elo.Lobby.Game.Team2.Players.Add(User.Id);
                TeamNext = 1;
                Context.Elo.Lobby.Game.Team2.TurnToPick = false;
                Context.Elo.Lobby.Game.Team1.TurnToPick = true;
            }
            Context.Elo.Lobby.Game.QueuedPlayerIDs.Remove(User.Id);
            if (Context.Elo.Lobby.Game.QueuedPlayerIDs.Count == 0)
            {
                Context.Elo.Lobby.GamesPlayed++;
                await ReplyAsync("**Game has Started**\n" +
                                $"Team1: {string.Join(", ", Context.Elo.Lobby.Game.Team1.Players.Select(x => Context.Socket.Guild.GetUser(x)?.Mention).ToList())}\n" +
                                $"Team2: {string.Join(", ", Context.Elo.Lobby.Game.Team2.Players.Select(x => Context.Socket.Guild.GetUser(x)?.Mention).ToList())}\n" +
                                $"**Game #{Context.Elo.Lobby.GamesPlayed}**");
            }
            else
            {
                await SimpleEmbedAsync($"**Team1 Captain** {Context.Socket.Guild.GetUser(Context.Elo.Lobby.Game.Team1.Captain)?.Mention}\n" +
                                       $"**Team1:** {string.Join(", ", Context.Elo.Lobby.Game.Team1.Players.Select(x => Context.Socket.Guild.GetUser(x)?.Mention).ToList())}\n" +
                                       $"**Team2 Captain** {Context.Socket.Guild.GetUser(Context.Elo.Lobby.Game.Team2.Captain)?.Mention}\n" +
                                       $"**Team2:** {string.Join(", ", Context.Elo.Lobby.Game.Team2.Players.Select(x => Context.Socket.Guild.GetUser(x)?.Mention).ToList())}\n" +
                                       $"**Select Your Teams using `{Context.Prefix}pick <@user>`**\n" +
                                       $"**It is Captain {TeamNext}'s Turn to pick**\n" +
                                       "**Player Pool**\n" +
                                       $"{string.Join(" ", Context.Elo.Lobby.Game.QueuedPlayerIDs.Select(x => Context.Socket.Guild.GetUser(x)?.Mention))}");
            }
            Context.Server.Save();
        }
    }
}
