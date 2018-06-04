using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using ELOBOT.Discord.Context;
using ELOBOT.Discord.Preconditions;

namespace ELOBOT.Modules
{
    [CustomPermissions(true, false)]
    public class GameSettings : Base
    {
        [Command("AllowNegativeScore")]
        public async Task NegativeScore()
        {
            Context.Server.Settings.GameSettings.AllowNegativeScore = !Context.Server.Settings.GameSettings.AllowNegativeScore;
            Context.Server.Save();
            await SimpleEmbedAsync($"Negative Scores Allowed: {Context.Server.Settings.GameSettings.AllowNegativeScore}");
        }
        [Command("BlockMultiQueuing")]
        public async Task BlockMultiQueuing()
        {
            Context.Server.Settings.GameSettings.BlockMultiQueuing = !Context.Server.Settings.GameSettings.BlockMultiQueuing;
            Context.Server.Save();
            await SimpleEmbedAsync($"Multi Queuing Disabled: {Context.Server.Settings.GameSettings.BlockMultiQueuing}");
        }
        [Command("RemoveOnAFK")]
        public async Task RemoveOnAfk()
        {
            Context.Server.Settings.GameSettings.RemoveOnAfk = !Context.Server.Settings.GameSettings.RemoveOnAfk;
            Context.Server.Save();
            await SimpleEmbedAsync($"Players will be removed from queue on AFK: {Context.Server.Settings.GameSettings.RemoveOnAfk}");
        }

        [Command("AnnouncementsChannel")]
        public async Task AnnouncementsChannel()
        {
            Context.Server.Settings.GameSettings.AnnouncementsChannel = Context.Channel.Id;
            Context.Server.Save();
            await SimpleEmbedAsync($"Game announcements will now be posted to {Context.Channel.Name}");
        }
        [Command("DMAnnouncements")]
        public async Task DMAnnouncements()
        {
            Context.Server.Settings.GameSettings.DMAnnouncements = !Context.Server.Settings.GameSettings.DMAnnouncements;
            Context.Server.Save();
            await SimpleEmbedAsync($"Users will be DM'ed Announcements: {Context.Server.Settings.GameSettings.DMAnnouncements}");
        }
    }
}
