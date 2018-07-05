using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discord;
using ELOBOT.Handlers;
using ELOBOT.Models;

namespace ELOBOT.Discord.Extensions
{
    public class AnnouncementManager
    {
        public static EmbedFieldBuilder MapField(GuildModel.Lobby Lobby)
        {
            if (!Lobby.Maps.Any()) return null;
            var map = Lobby.Maps.OrderByDescending(x => new Random().Next()).First();
            return new EmbedFieldBuilder
            {
                Name = "Random Map",
                Value = map
            };
        }

        public static EmbedFieldBuilder HostField(Context.Context Context, GuildModel.Lobby Lobby)
        {
            var allplayerIDs = new List<ulong>();
            allplayerIDs.AddRange(Lobby.Game.Team1.Players);
            allplayerIDs.AddRange(Lobby.Game.Team2.Players);

            var EPlayers = allplayerIDs.Select(x => Context.Server.Users.FirstOrDefault(u => u.UserID == x)).Where(x => x != null).ToList();
            string Player;
            switch (Lobby.HostSelectionMode)
            {
                case GuildModel.Lobby.HostSelector.None:
                    return null;
                case GuildModel.Lobby.HostSelector.MostPoints:
                    Player = Context.Socket.Guild.GetUser(EPlayers.OrderByDescending(x => x.Stats.Points).FirstOrDefault().UserID)?.Mention;
                    break;
                case GuildModel.Lobby.HostSelector.MostWins:
                    Player = Context.Socket.Guild.GetUser(EPlayers.OrderByDescending(x => x.Stats.Wins).FirstOrDefault().UserID)?.Mention;
                    break;
                case GuildModel.Lobby.HostSelector.HighestWinLoss:
                    Player = Context.Socket.Guild.GetUser(EPlayers.OrderByDescending(x => (double)x.Stats.Wins/x.Stats.Losses).FirstOrDefault().UserID)?.Mention;
                    break;
                case GuildModel.Lobby.HostSelector.Random:
                    Player = Context.Socket.Guild.GetUser(EPlayers.OrderByDescending(x => new Random().Next()).FirstOrDefault().UserID)?.Mention;
                    break;
                default:
                    return null;
            }
            return new EmbedFieldBuilder
            {
                Name = "Selected Host",
                Value = Player ?? "N/A"
            };
        }
    }
}
