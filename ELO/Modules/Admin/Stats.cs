namespace ELO.Modules.Admin
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using ELO.Discord.Context;
    using ELO.Discord.Preconditions;

    using global::Discord;
    using global::Discord.Commands;

    [CustomPermissions(true)]
    public class Stats : Base
    {
        [Command("ModifyPoints")]
        public async Task ModifyPointsAsync(IUser user, int points)
        {
            var eUser = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (eUser == null)
            {
                throw new Exception("User is not registered");
            }

            eUser.Stats.Points += points;
            await SimpleEmbedAsync($"{user.Mention} Points Modified: {eUser.Stats.Points}");
            Context.Server.Save();
        }

        [Command("ModifyPoints")]
        public Task ModifyPointsAsync(int points, IUser user)
        {
            return ModifyPointsAsync(user, points);
        }

        [Command("SetPoints")]
        public async Task SetPointsAsync(IUser user, int points)
        {
            var eUser = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (eUser == null)
            {
                throw new Exception("User is not registered");
            }

            eUser.Stats.Points = points;
            await SimpleEmbedAsync($"{user.Mention} Points Set: {eUser.Stats.Points}");
            Context.Server.Save();
        }

        [Command("SetPoints")]
        public Task SetPointsAsync(int points, IUser user)
        {
            return SetPointsAsync(user, points);
        }

        [Command("ModifyKills")]
        public async Task ModifyKillsAsync(IUser user, int kills)
        {
            var eUser = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (eUser == null)
            {
                throw new Exception("User is not registered");
            }

            eUser.Stats.Kills += kills;
            await SimpleEmbedAsync($"{user.Mention} Kills Modified: {eUser.Stats.Kills}");
            Context.Server.Save();
        }

        [Command("ModifyKills")]
        public Task ModifyKillsAsync(int kills, IUser user)
        {
            return ModifyKillsAsync(user, kills);
        }

        [Command("SetKills")]
        public async Task SetKillsAsync(IUser user, int kills)
        {
            var eUser = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (eUser == null)
            {
                throw new Exception("User is not registered");
            }

            eUser.Stats.Kills = kills;
            await SimpleEmbedAsync($"{user.Mention} Kills Set: {eUser.Stats.Kills}");
            Context.Server.Save();
        }

        [Command("SetKills")]
        public Task SetKillsAsync(int kills, IUser user)
        {
            return SetKillsAsync(user, kills);
        }

        [Command("ModifyDeaths")]
        public async Task ModifyDeathsAsync(IUser user, int deaths)
        {
            var eUser = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (eUser == null)
            {
                throw new Exception("User is not registered");
            }

            eUser.Stats.Deaths += deaths;
            await SimpleEmbedAsync($"{user.Mention} Deaths Modified: {eUser.Stats.Deaths}");
            Context.Server.Save();
        }

        [Command("ModifyDeaths")]
        public Task ModifyDeathsAsync(int deaths, IUser user)
        {
            return ModifyDeathsAsync(user, deaths);
        }

        [Command("SetDeaths")]
        public async Task SetDeathsAsync(IUser user, int deaths)
        {
            var eUser = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (eUser == null)
            {
                throw new Exception("User is not registered");
            }

            eUser.Stats.Deaths = deaths;
            await SimpleEmbedAsync($"{user.Mention} Deaths Set: {eUser.Stats.Deaths}");
            Context.Server.Save();
        }

        [Command("SetDeaths")]
        public Task SetDeathsAsync(int deaths, IUser user)
        {
            return SetDeathsAsync(user, deaths);
        }

        [Command("ModifyWins")]
        public async Task ModifyWinsAsync(IUser user, int wins)
        {
            var eUser = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (eUser == null)
            {
                throw new Exception("User is not registered");
            }

            eUser.Stats.Wins += wins;
            await SimpleEmbedAsync($"{user.Mention} Wins Modified: {eUser.Stats.Wins}");
            Context.Server.Save();
        }

        [Command("ModifyWins")]
        public Task ModifyWinsAsync(int wins, IUser user)
        {
            return ModifyWinsAsync(user, wins);
        }

        [Command("SetWins")]
        public Task SetWinsAsync(IUser user, int wins)
        {
            var eUser = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (eUser == null)
            {
                throw new Exception("User is not registered");
            }

            eUser.Stats.Wins = wins;
            Context.Server.Save();

            return SimpleEmbedAsync($"{user.Mention} Wins Set: {eUser.Stats.Wins}");
        }

        [Command("SetWins")]
        public Task SetWinsAsync(int wins, IUser user)
        {
            return SetWinsAsync(user, wins);
        }

        [Command("ModifyLosses")]
        public Task ModifyLossesAsync(IUser user, int losses)
        {
            var eUser = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (eUser == null)
            {
                throw new Exception("User is not registered");
            }

            eUser.Stats.Losses += losses;
            Context.Server.Save();

            return SimpleEmbedAsync($"{user.Mention} Losses Modified: {eUser.Stats.Losses}");
        }

        [Command("ModifyLosses")]
        public Task ModifyLossesAsync(int losses, IUser user)
        {
            return ModifyLossesAsync(user, losses);
        }

        [Command("SetLosses")]
        public Task SetLossesAsync(IUser user, int losses)
        {
            var eUser = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (eUser == null)
            {
                throw new Exception("User is not registered");
            }

            eUser.Stats.Losses = losses;
            Context.Server.Save();

            return SimpleEmbedAsync($"{user.Mention} Losses Set: {eUser.Stats.Losses}");
        }

        [Command("SetLosses")]
        public Task SetLossesAsync(int losses, IUser user)
        {
            return ModifyLossesAsync(user, losses);
        }
    }
}
