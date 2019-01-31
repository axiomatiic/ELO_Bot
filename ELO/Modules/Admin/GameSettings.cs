namespace ELO.Modules.Admin
{
    using System;
    using System.Threading.Tasks;

    using ELO.Discord.Context;
    using ELO.Discord.Preconditions;

    using global::Discord.Commands;

    [CustomPermissions(DefaultPermissionLevel.Administrators)]
    [Summary("Game setup settings")]
    public class GameSettings : Base
    {
        [Command("GameSettings", RunMode = RunMode.Async)]
        [Summary("GameSettings module settings")]
        public Task GameSettingsAsync()
        {
            var g = Context.Server.Settings.GameSettings;
            return SimpleEmbedAsync(
                $"**AllowNegativeScore:** {g.AllowNegativeScore}\n" + $"**DMAnnouncements:** {g.DMAnnouncements}\n"
                                                                + $"**RemoveOnAfk:** {g.RemoveOnAfk}\n"
                                                                + $"**BlockMultiQueuing:** {g.BlockMultiQueuing}\n"
                                                                + $"**AllowUserSubmissions (GameResult Command):** {g.AllowUserSubmissions}\n"
                                                                + $"**AnnouncementsChannel:** {Context.Guild.GetChannel(g.AnnouncementsChannel)?.Name ?? "N/A"}\n"
                                                                + $"**ReQueueDelay:** {g.ReQueueDelay.TotalMinutes} Minutes\n"
                                                                + $"**UseKd:** {g.UseKd}\n");
        }

        [Command("AllowNegativeScore")]
        [Summary("Toggle the ability to use negative scores")]
        public async Task NegativeScoreAsync()
        {
            Context.Server.Settings.GameSettings.AllowNegativeScore = !Context.Server.Settings.GameSettings.AllowNegativeScore;
            await Context.Server.Save();
            await SimpleEmbedAsync($"Negative Scores Allowed: {Context.Server.Settings.GameSettings.AllowNegativeScore}");
        }

        [Command("BlockMultiQueuing")]
        [Summary("Toggle whether users are allowed in multiple queues at the same time")]
        public async Task BlockMultiQueuingAsync()
        {
            Context.Server.Settings.GameSettings.BlockMultiQueuing = !Context.Server.Settings.GameSettings.BlockMultiQueuing;
            await Context.Server.Save();
            await SimpleEmbedAsync($"Multi Queuing Disabled: {Context.Server.Settings.GameSettings.BlockMultiQueuing}");
        }

        [Command("RemoveOnAFK")]
        [Summary("Toggle the auto-removal of users from queue when they go afk")]
        public async Task RemoveOnAfkAsync()
        {
            Context.Server.Settings.GameSettings.RemoveOnAfk = !Context.Server.Settings.GameSettings.RemoveOnAfk;
            await Context.Server.Save();
            await SimpleEmbedAsync($"Players will be removed from queue on AFK: {Context.Server.Settings.GameSettings.RemoveOnAfk}");
        }

        [Command("AnnouncementsChannel")]
        [Summary("Ser the current channel as the announcements channel")]
        public async Task AnnouncementsChannelAsync()
        {
            Context.Server.Settings.GameSettings.AnnouncementsChannel = Context.Channel.Id;
            await Context.Server.Save();
            await SimpleEmbedAsync($"Game announcements will now be posted to {Context.Channel.Name}");
        }

        [Command("DMAnnouncements")]
        [Summary("Toggle whether to dm users announcements")]
        public async Task DMAnnouncementsAsync()
        {
            Context.Server.Settings.GameSettings.DMAnnouncements = !Context.Server.Settings.GameSettings.DMAnnouncements;
            await Context.Server.Save();
            await SimpleEmbedAsync($"Users will be DM'ed Announcements: {Context.Server.Settings.GameSettings.DMAnnouncements}");
        }

        [Command("ReQueueDelay")]
        [Summary("Set the amount of time users must wait between games")]
        public async Task ReQueueDelayAsync(int minutes = 0)
        {
            if (minutes < 0)
            {
                throw new Exception("Delay must be greater than or equal to zero");
            }

            Context.Server.Settings.GameSettings.ReQueueDelay = TimeSpan.FromMinutes(minutes);
            await Context.Server.Save();

            await SimpleEmbedAsync($"Success, users must wait {minutes} minutes before re-queuing");
        }

        [Command("ShowKD")]
        [Summary("Toggle the use of K/D ratio in the server")]
        public async Task ShowKDAsync()
        {
            Context.Server.Settings.GameSettings.UseKd = !Context.Server.Settings.GameSettings.UseKd;
            await Context.Server.Save();

            await SimpleEmbedAsync($"Show user KD: {Context.Server.Settings.GameSettings.UseKd}");
        }

        [Command("UserGameResults")]
        [Summary("Toggle whether users are able to submit their own game results")]
        public async Task UserGameResultsAsync()
        {
            Context.Server.Settings.GameSettings.AllowUserSubmissions = !Context.Server.Settings.GameSettings.AllowUserSubmissions;
            await Context.Server.Save();

            await SimpleEmbedAsync($"Users are able to set game result: {Context.Server.Settings.GameSettings.AllowUserSubmissions}\n" + 
                                    "This requires a player from both teams to submit the same game result.\n");
        }
    }
}