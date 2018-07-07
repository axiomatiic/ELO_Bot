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
    public class Setup : Base
    {
        [Command("RegisterRole")]
        public Task RegisterRoleAsync(IRole role)
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
            Context.Server.Save();

            return SimpleEmbedAsync("Success Default Registration Role has been set.");
        }

        [Command("RegisterMessage")]
        public Task RegisterMessageAsync([Remainder] string message)
        {
            Context.Server.Settings.Registration.Message = message;
            Context.Server.Save();

            return SimpleEmbedAsync("Success Registration message has been set.");
        }

        [Command("RegisterPoints")]
        public Task RegisterPointsAsync(int points = 0)
        {
            if (points < 0)
            {
                throw new Exception("Register Points must be a positive integer");
            }

            Context.Server.Settings.Registration.RegistrationBonus = points;
            Context.Server.Save();

            return SimpleEmbedAsync($"Success, users who register for a first time will be given {points}");
        }

        [Command("NickNameFormat")]
        public async Task NickFormatAsync([Remainder] string message)
        {
            if (message.Length > 32)
            {
                throw new Exception("Format must be shorter than 32 characters");
            }

            if (message.Length - "{username}".Length > 12)
            {
                // Since discord limits username to 32 characters and out bot's registration limit is only 20 characters, there is a 12 character space to customize
                throw new Exception("Format Length too long, please shorten it.");
            }

            Context.Server.Settings.Registration.NameFormat = message.ToLower();
            await SimpleEmbedAsync("Success Nickname Format has been set.");
            Context.Server.Save();
        }

        [Command("NickNameFormat")]
        public Task NickFormatAsync()
        {
            return SimpleEmbedAsync("Set the server's nickname format. You can represent points using {score} and represent Name using {username}\n" +
                                    "ie. `NickNameFormat [{score}] - {username}`");
        }

        [Command("DefaultWinModifier")]
        public Task WinModifierAsync(int input = 10)
        {
            if (input <= 0)
            {
                throw new Exception("Win Modifier must be a positive integer");
            }

            Context.Server.Settings.Registration.DefaultWinModifier = input;
            Context.Server.Save();

            return SimpleEmbedAsync($"Success Default Win Modifier is now: +{input}");
        }

        [Command("DefaultLossModifier")]
        public Task LossModifierAsync(int input = 5)
        {
            Context.Server.Settings.Registration.DefaultLossModifier = Math.Abs(input);
            Context.Server.Save();

            return SimpleEmbedAsync($"Success, Default Loss Modifier is now: -{input}");
        }

        [Command("ReQueueDelay")]
        public Task ReQueueDelayAsync(int input = 0)
        {
            if (input < 0)
            {
                throw new Exception("Delay must be greater than or equal to zero");
            }

            Context.Server.Settings.GameSettings.ReQueueDelay = TimeSpan.FromMinutes(input);
            Context.Server.Save();

            return SimpleEmbedAsync($"Success, users must wait {input} minutes before re-queuing");
        }

        [Command("ShowKD")]
        public Task ShowKDAsync()
        {
            Context.Server.Settings.GameSettings.UseKd = !Context.Server.Settings.GameSettings.UseKd;
            Context.Server.Save();

            return SimpleEmbedAsync($"Show user KD: {Context.Server.Settings.GameSettings.UseKd}");
        }
    }
}