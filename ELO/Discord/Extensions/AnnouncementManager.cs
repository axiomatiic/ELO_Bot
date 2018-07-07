namespace ELO.Discord.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ELO.Discord.Context;
    using ELO.Models;

    using global::Discord;

    public class AnnouncementManager
    {
        public static EmbedFieldBuilder MapField(GuildModel.Lobby lobby)
        {
            if (!lobby.Maps.Any())
            {
                return null;
            }

            var map = lobby.Maps.OrderByDescending(x => new Random().Next()).First();
            return new EmbedFieldBuilder
            {
                Name = "Random Map",
                Value = map
            };
        }

        public static EmbedFieldBuilder HostField(Context context, GuildModel.Lobby lobby)
        {
            var ulongs = new List<ulong>();
            ulongs.AddRange(lobby.Game.Team1.Players);
            ulongs.AddRange(lobby.Game.Team2.Players);

            var ePlayers = ulongs.Select(x => context.Server.Users.FirstOrDefault(u => u.UserID == x)).Where(x => x != null).ToList();
            string player;
            switch (lobby.HostSelectionMode)
            {
                case GuildModel.Lobby.HostSelector.None:
                    return null;
                case GuildModel.Lobby.HostSelector.MostPoints:
                    player = context.Guild.GetUser(ePlayers.OrderByDescending(x => x.Stats.Points).FirstOrDefault().UserID)?.Mention;
                    break;
                case GuildModel.Lobby.HostSelector.MostWins:
                    player = context.Guild.GetUser(ePlayers.OrderByDescending(x => x.Stats.Wins).FirstOrDefault().UserID)?.Mention;
                    break;
                case GuildModel.Lobby.HostSelector.HighestWinLoss:
                    player = context.Guild.GetUser(ePlayers.OrderByDescending(x => (double)x.Stats.Wins / x.Stats.Losses).FirstOrDefault().UserID)?.Mention;
                    break;
                case GuildModel.Lobby.HostSelector.Random:
                    player = context.Guild.GetUser(ePlayers.OrderByDescending(x => new Random().Next()).FirstOrDefault().UserID)?.Mention;
                    break;
                default:
                    return null;
            }
            return new EmbedFieldBuilder
            {
                Name = "Selected Host",
                Value = player ?? "N/A"
            };
        }
    }
}
