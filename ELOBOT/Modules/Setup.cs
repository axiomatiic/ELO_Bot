using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ELOBOT.Discord.Context;
using ELOBOT.Models;

namespace ELOBOT.Modules
{
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
    }
}
