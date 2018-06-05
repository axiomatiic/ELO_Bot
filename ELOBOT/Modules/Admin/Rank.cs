using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ELOBOT.Discord.Context;
using ELOBOT.Discord.Preconditions;
using ELOBOT.Models;

namespace ELOBOT.Modules.Admin
{
    [CustomPermissions(true)]
    public class Rank : Base
    {
        [Command("AddRank")]
        public async Task AddRank(int Points, IRole Role)
        {
            await AddRank(Role, Points);
        }

        [Command("AddRank")]
        public async Task AddRank(IRole Role, int Points)
        {
            if (Context.Server.Ranks.Any(x => x.RoleID == Role.Id))
            {
                throw new Exception("This is already a rank");
            }

            var Rank = new GuildModel.Rank
            {
                IsDefault = false,
                WinModifier = Context.Server.Settings.Registration.DefaultWinModifier,
                LossModifier = Context.Server.Settings.Registration.DefaultLossModifier,
                RoleID = Role.Id,
                Threshhold = Points
            };
            Context.Server.Ranks.Add(Rank);
            Context.Server.Save();
            await SimpleEmbedAsync("Rank added.");
        }

        [Command("DelRank")]
        public async Task DelRank(IRole Role)
        {
            await DelRank(Role.Id);
        }

        [Command("DelRank")]
        public async Task DelRank(ulong RoleID)
        {
            if (Context.Server.Ranks.All(x => x.RoleID != RoleID))
            {
                throw new Exception("This is not a rank");
            }

            var Rank = Context.Server.Ranks.FirstOrDefault(x => x.RoleID == RoleID);
            if (Rank.IsDefault)
            {
                throw new Exception("You cannot delete the default rank");
            }

            Context.Server.Ranks.Remove(Rank);
            Context.Server.Save();
            await SimpleEmbedAsync("Rank Removed.");
        }

        [Command("WinModifier")]
        public async Task WinModifier(IRole Role, int Points)
        {
            if (Context.Server.Ranks.All(x => x.RoleID != Role.Id))
            {
                throw new Exception("This is not a rank");
            }

            if (Points <= 0)
            {
                throw new Exception("Point modifier must be a positive integer");
            }

            var Rank = Context.Server.Ranks.FirstOrDefault(x => x.RoleID == Role.Id);
            Rank.WinModifier = Points;
            Context.Server.Save();
            await SimpleEmbedAsync("Rank Win Modifier Updated.");
        }

        [Command("LossModifier")]
        public async Task LossModifier(IRole Role, int Points)
        {
            if (Context.Server.Ranks.All(x => x.RoleID != Role.Id))
            {
                throw new Exception("This is not a rank");
            }

            Points = Math.Abs(Points);

            if (Points == 0)
            {
                throw new Exception("Point modifier must be an integer");
            }

            var Rank = Context.Server.Ranks.FirstOrDefault(x => x.RoleID == Role.Id);
            Rank.LossModifier = Points;
            Context.Server.Save();
            await SimpleEmbedAsync("Rank Loss Modifier Updated.");
        }
    }
}