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

    [CustomPermissions(DefaultPermissionLevel.Administrators)]
    [Summary("Main server setup commands")]
    public class Setup : Base
    {
        [Command("RegistrationInfo", RunMode = RunMode.Async)]
        public Task SetupInfoAsync()
        {
            var r = Context.Server.Settings.Registration;
            return SimpleEmbedAsync(
                $"**AllowMultiRegistration:** {r.AllowMultiRegistration}\n" + $"**DefaultLossModifier:** {r.DefaultLossModifier}\n"
                                                      + $"**DefaultLossModifier:** {r.DefaultLossModifier}\n"
                                                      + $"**RegistrationBonus:** {r.RegistrationBonus}\n"
                                                      + $"**DeleteProfileOnLeave:** {r.DeleteProfileOnLeave}\n" 
                                                      + $"**NameFormat:** {r.NameFormat}\n"
                                                      + $"**Message:** \n{r.Message}\n");
        }

        [Command("ReadabilityInfo", RunMode = RunMode.Async)]
        public Task ReadabilityAsync()
        {
            var r = Context.Server.Settings.Readability;
            return SimpleEmbedAsync(
                $"**JoinLeaveErrors:** {r.JoinLeaveErrors}\n" + 
                $"**ReplyErrors:** {r.ReplyErrors}");
        }

        [Command("RegisterRole")]
        [Summary("Set the default role user's are given when registering")]
        public async Task RegisterRoleAsync(IRole role)
        {
            if (Context.Server.Ranks.Any(x => x.IsDefault))
            {
                Context.Server.Ranks = Context.Server.Ranks.Where(x => !x.IsDefault).ToList();
            }

            Context.Server.Ranks.Add(new GuildModel.Rank
            {
                IsDefault = true,
                LossModifier = 5,
                WinModifier = 10,
                RoleID = role.Id,
                Threshold = 0
            });
            await Context.Server.Save();

            await SimpleEmbedAsync("Success Default Registration Role has been set.");
        }

        [Command("RegisterMessage")]
        [Summary("Set the message that is displayed to users when registering")]
        public async Task RegisterMessageAsync([Remainder] string message)
        {
            Context.Server.Settings.Registration.Message = message;
            await Context.Server.Save();

            await SimpleEmbedAsync("Success Registration message has been set.");
        }

        [Command("RegisterPoints")]
        [Summary("Set the default points users are given when registering")]
        public async Task RegisterPointsAsync(int points = 0)
        {
            if (points < 0)
            {
                throw new Exception("Register Points must be a positive integer");
            }

            Context.Server.Settings.Registration.RegistrationBonus = points;
            await Context.Server.Save();

            await SimpleEmbedAsync($"Success, users who register for a first time will be given {points}");
        }

        [Command("NickNameFormat")]
        [Summary("Set a custom user nickname format")]
        public async Task NickFormatAsync([Remainder] string nicknameFormatting)
        {
            if (nicknameFormatting.Length > 32)
            {
                throw new Exception("Format must be shorter than 32 characters");
            }

            if (nicknameFormatting.Length - "{username}".Length > 12)
            {
                // Since discord limits username to 32 characters and out bot's registration limit is only 20 characters, there is a 12 character space to customize
                throw new Exception("Format Length too long, please shorten it.");
            }

            Context.Server.Settings.Registration.NameFormat = nicknameFormatting.ToLower();
            await SimpleEmbedAsync("Success Nickname Format has been set.");
            await Context.Server.Save();
        }

        [Command("NickNameFormat", RunMode = RunMode.Async)]
        [Summary("Info about setting a user's nickname format")]
        public Task NickFormatAsync()
        {
            return SimpleEmbedAsync("Set the server's nickname format. You can represent points using {score} and represent Name using {username}\n" +
                                    "ie. `NickNameFormat [{score}] - {username}`");
        }

        [Command("DefaultWinModifier")]
        [Summary("Set the default amount of points users are given when winning a match")]
        public async Task WinModifierAsync(int pointsToAdd = 10)
        {
            if (pointsToAdd <= 0)
            {
                throw new Exception("Win Modifier must be a positive integer");
            }

            Context.Server.Settings.Registration.DefaultWinModifier = pointsToAdd;
            await Context.Server.Save();

            await SimpleEmbedAsync($"Success Default Win Modifier is now: +{pointsToAdd}");
        }

        [Command("DefaultLossModifier")]
        [Summary("Set the default amount of points users lose wh")]
        public async Task LossModifierAsync(int pointsToRemove = 5)
        {
            Context.Server.Settings.Registration.DefaultLossModifier = Math.Abs(pointsToRemove);
            await Context.Server.Save();

            await SimpleEmbedAsync($"Success, Default Loss Modifier is now: -{pointsToRemove}");
        }

        [Command("ShowErrors")]
        [Summary("Toggle the response of error messages in chat")]
        public async Task ToggleErrorsAsync()
        {
            Context.Server.Settings.Readability.ReplyErrors = !Context.Server.Settings.Readability.ReplyErrors;
            await Context.Server.Save();

            await SimpleEmbedAsync($"Reply with errors: {Context.Server.Settings.Readability.ReplyErrors}");
        }

        [Command("JoinLeaveErrors")]
        [Summary("Toggle the response of error messages in chat when users try to join a queue they are already in and leave a queue they aren't in")]
        public async Task ToggleJLErrorsAsync()
        {
            Context.Server.Settings.Readability.JoinLeaveErrors = !Context.Server.Settings.Readability.JoinLeaveErrors;
            await Context.Server.Save();

            await SimpleEmbedAsync($"Reply with Join Leave: {Context.Server.Settings.Readability.JoinLeaveErrors}");
        }

        [Command("MultiRegister")]
        [Summary("toggle whether users can use the register command more than once")]
        public async Task ToggleMultiRegisterAsync()
        {
            Context.Server.Settings.Registration.AllowMultiRegistration = !Context.Server.Settings.Registration.AllowMultiRegistration;
            await Context.Server.Save();

            await SimpleEmbedAsync($"Allow multi registration: {Context.Server.Settings.Registration.AllowMultiRegistration}");
        }

        [Command("AutoDeleteOnLeave")]
        [Summary("toggle whether user profiles are auto-deleted when they leave the server")]
        public async Task ToggleAutoDeleteOnLeaveAsync()
        {
            Context.Server.Settings.Registration.DeleteProfileOnLeave = !Context.Server.Settings.Registration.DeleteProfileOnLeave;
            await Context.Server.Save();

            await SimpleEmbedAsync($"Auto Delete user profiles on leave: {Context.Server.Settings.Registration.DeleteProfileOnLeave}");
        }

        [Command("ResetLeaderboard")]
        [Summary("Reset all wins, losses, K/D, points and other user stats")]
        public async Task ResetLeaderboardAsync([Remainder]string confirm = null)
        {
            if (confirm?.ToLower() != "2ifh2")
            {
                await ReplyAsync("Please run the command using the following confirmation code in order to re-set the leaderboard: `2ifh2`");
                return;
            }

            foreach (var serverUser in Context.Server.Users)
            {
                serverUser.Stats = new GuildModel.User.Score();
            }

            await Context.Server.Save();

            await SimpleEmbedAsync("All user stats have been restored. \n" + 
                                    "NOTE: Nicknames and roles will not be re-set, player names and roles will be automatically updated when they play a new game, run the register command or have a game result");
        }
    }
}