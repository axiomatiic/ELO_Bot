namespace ELO.Modules.Admin
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using ELO.Discord.Context;
    using ELO.Discord.Extensions;
    using ELO.Discord.Preconditions;

    using global::Discord;
    using global::Discord.Commands;

    [CustomPermissions(true)]
    [Summary("Direct user stats modifications")]
    public class Stats : Base
    {
        [Command("ModifyPoints")]
        [Summary("Add or subtract points from a user")]
        public async Task ModifyPointsAsync(IUser user, int pointsToAddOrSubtract)
        {
            var eUser = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (eUser == null)
            {
                throw new Exception("User is not registered");
            }

            eUser.Stats.Points += pointsToAddOrSubtract;
            await SimpleEmbedAsync($"{user.Mention} Points Modified: {eUser.Stats.Points}");
            var nick = Task.Run(() => UserManagement.UserRenameAsync(Context, eUser));
            var role = Task.Run(() => UserManagement.GiveMaxRoleAsync(Context, eUser));
            Context.Server.Save();
        }

        [Command("ModifyPoints")]
        [Summary("Add or subtract points from a user")]
        public Task ModifyPointsAsync(int pointsToAddOrSubtract, IUser user)
        {
            return ModifyPointsAsync(user, pointsToAddOrSubtract);
        }

        [Command("SetPoints")]
        [Summary("Set the points of a user")]
        public async Task SetPointsAsync(IUser user, int points)
        {
            var eUser = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (eUser == null)
            {
                throw new Exception("User is not registered");
            }

            eUser.Stats.Points = points;
            await SimpleEmbedAsync($"{user.Mention} Points Set: {eUser.Stats.Points}");
            var nick = Task.Run(() => UserManagement.UserRenameAsync(Context, eUser));
            var role = Task.Run(() => UserManagement.GiveMaxRoleAsync(Context, eUser));
            Context.Server.Save();
        }

        [Command("SetPoints")]
        [Summary("Set the points of a user")]
        public Task SetPointsAsync(int points, IUser user)
        {
            return SetPointsAsync(user, points);
        }

        [Command("ModifyKills")]
        [Summary("Add or subtract kills from a user")]
        public async Task ModifyKillsAsync(IUser user, int killsToAddOrSubtract)
        {
            var eUser = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (eUser == null)
            {
                throw new Exception("User is not registered");
            }

            eUser.Stats.Kills += killsToAddOrSubtract;
            await SimpleEmbedAsync($"{user.Mention} Kills Modified: {eUser.Stats.Kills}");
            Context.Server.Save();
        }

        [Command("ModifyKills")]
        [Summary("Add or subtract kills from a user")]
        public Task ModifyKillsAsync(int killsToAddOrSubtract, IUser user)
        {
            return ModifyKillsAsync(user, killsToAddOrSubtract);
        }

        [Command("SetKills")]
        [Summary("Set the kills of a user")]
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
        [Summary("Set the kills of a user")]
        public Task SetKillsAsync(int kills, IUser user)
        {
            return SetKillsAsync(user, kills);
        }

        [Command("ModifyDeaths")]
        [Summary("Add or subtract Deaths from a user")]
        public async Task ModifyDeathsAsync(IUser user, int deathsToAddOrSubtract)
        {
            var eUser = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (eUser == null)
            {
                throw new Exception("User is not registered");
            }

            eUser.Stats.Deaths += deathsToAddOrSubtract;
            await SimpleEmbedAsync($"{user.Mention} Deaths Modified: {eUser.Stats.Deaths}");
            Context.Server.Save();
        }

        [Command("ModifyDeaths")]
        [Summary("Add or subtract Deaths from a user")]
        public Task ModifyDeathsAsync(int deathsToAddOrSubtract, IUser user)
        {
            return ModifyDeathsAsync(user, deathsToAddOrSubtract);
        }

        [Command("SetDeaths")]
        [Summary("Set the Deaths of a user")]
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
        [Summary("Set the Deaths of a user")]
        public Task SetDeathsAsync(int deaths, IUser user)
        {
            return SetDeathsAsync(user, deaths);
        }

        [Command("ModifyWins")]
        [Summary("Add or subtract Wins from a user")]
        public async Task ModifyWinsAsync(IUser user, int winsToAddOrSubtract)
        {
            var eUser = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (eUser == null)
            {
                throw new Exception("User is not registered");
            }

            eUser.Stats.Wins += winsToAddOrSubtract;
            await SimpleEmbedAsync($"{user.Mention} Wins Modified: {eUser.Stats.Wins}");
            Context.Server.Save();
        }

        [Command("ModifyWins")]
        [Summary("Add or subtract Wins from a user")]
        public Task ModifyWinsAsync(int winsToAddOrSubtract, IUser user)
        {
            return ModifyWinsAsync(user, winsToAddOrSubtract);
        }

        [Command("SetWins")]
        [Summary("Set the Wins of a user")]
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
        [Summary("Set the Wins of a user")]
        public Task SetWinsAsync(int wins, IUser user)
        {
            return SetWinsAsync(user, wins);
        }

        [Command("ModifyLosses")]
        [Summary("Add or subtract Losses from a user")]
        public Task ModifyLossesAsync(IUser user, int lossesToAddOrSubtract)
        {
            var eUser = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (eUser == null)
            {
                throw new Exception("User is not registered");
            }

            eUser.Stats.Losses += lossesToAddOrSubtract;
            Context.Server.Save();

            return SimpleEmbedAsync($"{user.Mention} Losses Modified: {eUser.Stats.Losses}");
        }

        [Command("ModifyLosses")]
        [Summary("Add or subtract Losses from a user")]
        public Task ModifyLossesAsync(int lossesToAddOrSubtract, IUser user)
        {
            return ModifyLossesAsync(user, lossesToAddOrSubtract);
        }

        [Command("SetLosses")]
        [Summary("Set the Losses of a user")]
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
        [Summary("Set the Losses of a user")]
        public Task SetLossesAsync(int losses, IUser user)
        {
            return ModifyLossesAsync(user, losses);
        }
    }
}
