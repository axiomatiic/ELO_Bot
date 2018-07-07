namespace ELO.Modules.Admin
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using ELO.Discord.Context;
    using ELO.Discord.Preconditions;
    using ELO.Models;

    using global::Discord;
    using global::Discord.Commands;

    [CustomPermissions(true)]
    public class Rank : Base
    {
        [Command("AddRank")]
        public Task AddRankAsync(int points, IRole role)
        {
            return AddRankAsync(role, points);
        }

        [Command("AddRank")]
        public Task AddRankAsync(IRole role, int points)
        {
            if (Context.Server.Ranks.Any(x => x.RoleID == role.Id))
            {
                throw new Exception("This is already a rank");
            }

            var rank = new GuildModel.Rank
            {
                IsDefault = false,
                WinModifier = Context.Server.Settings.Registration.DefaultWinModifier,
                LossModifier = Context.Server.Settings.Registration.DefaultLossModifier,
                RoleID = role.Id,
                Threshold = points
            };
            Context.Server.Ranks.Add(rank);
            Context.Server.Save();
            return SimpleEmbedAsync("Rank added.");
        }

        [Command("DelRank")]
        public Task DelRankAsync(IRole role)
        {
            return DelRankAsync(role.Id);
        }

        [Command("DelRank")]
        public Task DelRankAsync(ulong roleId)
        {
            if (Context.Server.Ranks.All(x => x.RoleID != roleId))
            {
                throw new Exception("This is not a rank");
            }

            var rank = Context.Server.Ranks.FirstOrDefault(x => x.RoleID == roleId);
            if (rank.IsDefault)
            {
                throw new Exception("You cannot delete the default rank");
            }

            Context.Server.Ranks.Remove(rank);
            Context.Server.Save();
            return SimpleEmbedAsync("Rank Removed.");
        }

        [Command("WinModifier")]
        public Task WinModifierAsync(IRole role, int points)
        {
            if (Context.Server.Ranks.All(x => x.RoleID != role.Id))
            {
                throw new Exception("This is not a rank");
            }

            if (points <= 0)
            {
                throw new Exception("Point modifier must be a positive integer");
            }

            var rank = Context.Server.Ranks.FirstOrDefault(x => x.RoleID == role.Id);
            rank.WinModifier = points;
            Context.Server.Save();
            return SimpleEmbedAsync("Rank Win Modifier Updated.");
        }

        [Command("LossModifier")]
        public Task LossModifierAsync(IRole role, int points)
        {
            if (Context.Server.Ranks.All(x => x.RoleID != role.Id))
            {
                throw new Exception("This is not a rank");
            }

            points = Math.Abs(points);

            if (points == 0)
            {
                throw new Exception("Point modifier must be an integer");
            }

            var rank = Context.Server.Ranks.FirstOrDefault(x => x.RoleID == role.Id);
            rank.LossModifier = points;
            Context.Server.Save();
            return SimpleEmbedAsync("Rank Loss Modifier Updated.");
        }

        [Command("Ranks")]
        public Task ViewRanksAsync()
        {
            var list = Context.Server.Ranks
                .Select(x => new Tuple<GuildModel.Rank, IRole>(x, Context.Guild.GetRole(x.RoleID)))
                .Where(x => x.Item2 != null).OrderByDescending(x => x.Item1.Threshold).Select(
                    x =>
                        $"{x.Item1.Threshold} - {x.Item2.Mention} - W: {x.Item1.WinModifier} L: {x.Item1.LossModifier}").ToList();
            return SimpleEmbedAsync($"Ranks\n\n{string.Join("\n", list)}");
        }
    }
}