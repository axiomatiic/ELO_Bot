using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ELOBOT.Discord.Context;
using ELOBOT.Discord.Preconditions;

namespace ELOBOT.Modules.Admin
{
    [CustomPermissions(true, false)]
    public class Stats : Base
    {
        [Command("ModifyPoints")]
        public async Task ModifyPoints(IUser user, int points)
        {
            var EUser = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (EUser == null)
            {
                throw new Exception("User is not registered");
            }

            EUser.Stats.Points += points;
            await SimpleEmbedAsync($"{user.Mention} Points Modified: {EUser.Stats.Points}");
            Context.Server.Save();
        }

        [Command("ModifyPoints")]
        public async Task ModifyPoints(int points, IUser user)
        {
            await ModifyPoints(user, points);
        }

        [Command("SetPoints")]
        public async Task SetPoints(IUser user, int points)
        {
            var EUser = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (EUser == null)
            {
                throw new Exception("User is not registered");
            }

            EUser.Stats.Points = points;
            await SimpleEmbedAsync($"{user.Mention} Points Set: {EUser.Stats.Points}");
            Context.Server.Save();
        }

        [Command("SetPoints")]
        public async Task SetPoints(int points, IUser user)
        {
            await SetPoints(user, points);
        }


        [Command("ModifyKills")]
        public async Task ModifyKills(IUser user, int Kills)
        {
            var EUser = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (EUser == null)
            {
                throw new Exception("User is not registered");
            }

            EUser.Stats.Kills += Kills;
            await SimpleEmbedAsync($"{user.Mention} Kills Modified: {EUser.Stats.Kills}");
            Context.Server.Save();
        }

        [Command("ModifyKills")]
        public async Task ModifyKills(int Kills, IUser user)
        {
            await ModifyKills(user, Kills);
        }

        [Command("SetKills")]
        public async Task SetKills(IUser user, int Kills)
        {
            var EUser = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (EUser == null)
            {
                throw new Exception("User is not registered");
            }

            EUser.Stats.Kills = Kills;
            await SimpleEmbedAsync($"{user.Mention} Kills Set: {EUser.Stats.Kills}");
            Context.Server.Save();
        }

        [Command("SetKills")]
        public async Task SetKills(int Kills, IUser user)
        {
            await SetKills(user, Kills);
        }

        [Command("ModifyDeaths")]
        public async Task ModifyDeaths(IUser user, int Deaths)
        {
            var EUser = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (EUser == null)
            {
                throw new Exception("User is not registered");
            }

            EUser.Stats.Deaths += Deaths;
            await SimpleEmbedAsync($"{user.Mention} Deaths Modified: {EUser.Stats.Deaths}");
            Context.Server.Save();
        }

        [Command("ModifyDeaths")]
        public async Task ModifyDeaths(int Deaths, IUser user)
        {
            await ModifyDeaths(user, Deaths);
        }


        [Command("SetDeaths")]
        public async Task SetDeaths(IUser user, int Deaths)
        {
            var EUser = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (EUser == null)
            {
                throw new Exception("User is not registered");
            }

            EUser.Stats.Deaths = Deaths;
            await SimpleEmbedAsync($"{user.Mention} Deaths Set: {EUser.Stats.Deaths}");
            Context.Server.Save();
        }

        [Command("SetDeaths")]
        public async Task SetDeaths(int Deaths, IUser user)
        {
            await SetDeaths(user, Deaths);
        }

        [Command("ModifyWins")]
        public async Task ModifyWins(IUser user, int Wins)
        {
            var EUser = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (EUser == null)
            {
                throw new Exception("User is not registered");
            }

            EUser.Stats.Wins += Wins;
            await SimpleEmbedAsync($"{user.Mention} Wins Modified: {EUser.Stats.Wins}");
            Context.Server.Save();
        }

        [Command("ModifyWins")]
        public async Task ModifyWins(int Wins, IUser user)
        {
            await ModifyWins(user, Wins);
        }

        [Command("SetWins")]
        public async Task SetWins(IUser user, int Wins)
        {
            var EUser = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (EUser == null)
            {
                throw new Exception("User is not registered");
            }

            EUser.Stats.Wins = Wins;
            await SimpleEmbedAsync($"{user.Mention} Wins Set: {EUser.Stats.Wins}");
            Context.Server.Save();
        }

        [Command("SetWins")]
        public async Task SetWins(int Wins, IUser user)
        {
            await SetWins(user, Wins);
        }

        [Command("ModifyLosses")]
        public async Task ModifyLosses(IUser user, int Losses)
        {
            var EUser = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (EUser == null)
            {
                throw new Exception("User is not registered");
            }

            EUser.Stats.Losses += Losses;
            await SimpleEmbedAsync($"{user.Mention} Losses Modified: {EUser.Stats.Losses}");
            Context.Server.Save();
        }

        [Command("ModifyLosses")]
        public async Task ModifyLosses(int Losses, IUser user)
        {
            await ModifyLosses(user, Losses);
        }

        [Command("SetLosses")]
        public async Task SetLosses(IUser user, int Losses)
        {
            var EUser = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (EUser == null)
            {
                throw new Exception("User is not registered");
            }

            EUser.Stats.Losses = Losses;
            await SimpleEmbedAsync($"{user.Mention} Losses Set: {EUser.Stats.Losses}");
            Context.Server.Save();
        }

        [Command("SetLosses")]
        public async Task SetLosses(int Losses, IUser user)
        {
            await ModifyLosses(user, Losses);
        }
    }
}
