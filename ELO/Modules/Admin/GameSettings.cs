namespace ELO.Modules.Admin
{
    using System.Threading.Tasks;

    using ELO.Discord.Context;
    using ELO.Discord.Preconditions;

    using global::Discord.Commands;

    [CustomPermissions(true)]
    public class GameSettings : Base
    {
        [Command("AllowNegativeScore")]
        [Summary("Toggle the ability to use negative scores")]
        public Task NegativeScoreAsync()
        {
            Context.Server.Settings.GameSettings.AllowNegativeScore = !Context.Server.Settings.GameSettings.AllowNegativeScore;
            Context.Server.Save();
            return SimpleEmbedAsync($"Negative Scores Allowed: {Context.Server.Settings.GameSettings.AllowNegativeScore}");
        }

        [Command("BlockMultiQueuing")]
        [Summary("Toggle whether users are allowed in multiple queues at the same time")]
        public Task BlockMultiQueuingAsync()
        {
            Context.Server.Settings.GameSettings.BlockMultiQueuing = !Context.Server.Settings.GameSettings.BlockMultiQueuing;
            Context.Server.Save();
            return SimpleEmbedAsync($"Multi Queuing Disabled: {Context.Server.Settings.GameSettings.BlockMultiQueuing}");
        }

        [Command("RemoveOnAFK")]
        [Summary("Toggle the auto-removal of users from queue when they go afk")]
        public Task RemoveOnAfkAsync()
        {
            Context.Server.Settings.GameSettings.RemoveOnAfk = !Context.Server.Settings.GameSettings.RemoveOnAfk;
            Context.Server.Save();
            return SimpleEmbedAsync($"Players will be removed from queue on AFK: {Context.Server.Settings.GameSettings.RemoveOnAfk}");
        }

        [Command("AnnouncementsChannel")]
        [Summary("Ser the current channel as the announcements channel")]
        public Task AnnouncementsChannelAsync()
        {
            Context.Server.Settings.GameSettings.AnnouncementsChannel = Context.Channel.Id;
            Context.Server.Save();
            return SimpleEmbedAsync($"Game announcements will now be posted to {Context.Channel.Name}");
        }

        [Command("DMAnnouncements")]
        [Summary("Toggle whether to dm users announcements")]
        public Task DMAnnouncementsAsync()
        {
            Context.Server.Settings.GameSettings.DMAnnouncements = !Context.Server.Settings.GameSettings.DMAnnouncements;
            Context.Server.Save();
            return SimpleEmbedAsync($"Users will be DM'ed Announcements: {Context.Server.Settings.GameSettings.DMAnnouncements}");
        }
    }
}