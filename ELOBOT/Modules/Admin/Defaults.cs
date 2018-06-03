using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using ELOBOT.Discord.Context;
using ELOBOT.Discord.Preconditions;

namespace ELOBOT.Modules
{
    [CheckAdmin]
    public class Defaults : Base
    {
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
    }
}
