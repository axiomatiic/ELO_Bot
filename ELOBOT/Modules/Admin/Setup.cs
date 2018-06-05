using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ELOBOT.Discord.Context;
using ELOBOT.Discord.Preconditions;
using ELOBOT.Models;

namespace ELOBOT.Modules
{
    [CustomPermissions(true, false)]
    public class Setup : Base
    {
        [Command("RegisterRole")]
        public async Task RegisterRole(IRole Role)
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
                RoleID = Role.Id,
                Threshhold = 0
            });
            await SimpleEmbedAsync("Success Default Registration Role has been set.");
            Context.Server.Save();
        }

        [Command("RegisterMessage")]
        public async Task RegisterMessage([Remainder]string Message)
        {
            Context.Server.Settings.Registration.Message = Message;
            await SimpleEmbedAsync("Success Registration message has been set.");
            Context.Server.Save();
        }

        [Command("RegisterPoints")]
        public async Task RegisterPoints(int Points = 0)
        {
            if (Points < 0)
            {
                throw new Exception("Register Points must be a positive integer");
            }
            Context.Server.Settings.Registration.RegistrationBonus = Points;
            await SimpleEmbedAsync($"Success, users who register for a first time will be given {Points}");
            Context.Server.Save();
        }

        [Command("NickNameFormat")]
        public async Task NickFormat([Remainder]string Message)
        {
            if (Message.Length > 32)
            {
                throw new Exception("Format must be shorter than 32 characters");
            }
            else if (Message.Length - "{username}".Length > 12)
            {
                throw new Exception("Format Length too long, please shorten it.");
            }
            Context.Server.Settings.Registration.NameFormat = Message.ToLower();
            await SimpleEmbedAsync("Success Nickname Format has been set.");
            Context.Server.Save();
        }
        [Command("NickNameFormat")]
        public async Task NickFormat()
        {
            await SimpleEmbedAsync("Set the server's nickname format. You can represent points using {score} and represent Name using {username}\n" +
                                   "ie. `NickNameFormat [{score}] - {username}`");
        }

        [Command("DefaultWinModifier")]
        public async Task WinModifier(int input = 10)
        {
            if (input <= 0)
            {
                throw new Exception("Win Modifier must be a positive integer");
            }
            Context.Server.Settings.Registration.DefaultWinModifier = input;
            Context.Server.Save();

            await SimpleEmbedAsync($"Succes Default Win Modifier is now: +{input}");
        }
        [Command("DefaultLossModifier")]
        public async Task LossModifier(int input = 5)
        {
            Context.Server.Settings.Registration.DefaultLossModifier = Math.Abs(input);
            Context.Server.Save();

            await SimpleEmbedAsync($"Succes Default Loss Modifier is now: -{input}");
        }

        [Command("ReQueueDelay")]
        public async Task RequeueDelay(int input = 0)
        {
            if (input < 0)
            {
                throw new Exception("Delay must be greater than or equal to zero");
            }
            Context.Server.Settings.GameSettings.ReQueueDelay = TimeSpan.FromMinutes(input);
            Context.Server.Save();

            await SimpleEmbedAsync($"Success, users must wait {input} minutes before re-queuing");
        }

        [Command("ShowKD")]
        public async Task ShowKD()
        {
            Context.Server.Settings.GameSettings.useKD = !Context.Server.Settings.GameSettings.useKD;
            Context.Server.Save();

            await SimpleEmbedAsync($"Show user KD: {Context.Server.Settings.GameSettings.useKD}");
        }
    }
}
