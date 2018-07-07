namespace ELO.Modules.Moderator
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    using ELO.Discord.Context;
    using ELO.Discord.Extensions;
    using ELO.Discord.Preconditions;
    using ELO.Models;

    using global::Discord;
    using global::Discord.Addons.Interactive;
    using global::Discord.Commands;

    [CustomPermissions(true, true)]
    public class Users : Base
    {
        [Command("DelUser")]
        [Summary("Deletes the specified user's profile")]
        public Task DeleteUserAsync(IUser user)
        {
            var profile = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (profile == null)
            {
                throw new Exception("User is not registered");
            }

            Context.Server.Users.Remove(profile);

            Context.Server.Save();
            return SimpleEmbedAsync($"Success {user.Mention}'s profile has been deleted.");
        }

        [Command("Rename")]
        [Summary("Rename the specified user")]
        public Task RenameAsync(IUser user, string nickname)
        {
            var profile = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (profile == null)
            {
                throw new Exception("User is not registered");
            }

            if (nickname == null || nickname.Length > 20)
            {
                throw new Exception("Name cannot be empty or greater than 20 characters long");
            }

            profile.Username = nickname;
            Context.Server.Save();
            return SimpleEmbedAsync($"Success {user.Mention} renamed to {nickname}");
        }

        [Command("Ban")]
        [Summary("Ban the specified user for the given amount of hours")]
        public Task BanAsync(IUser user, int hours, [Remainder] string reason = null)
        {
            var profile = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (profile == null)
            {
                throw new Exception("User is not registered");
            }

            if (reason == null || reason.Length > 200)
            {
                throw new Exception("Reason cannot be empty or greater than 200 characters long");
            }

            profile.Banned.Banned = true;
            profile.Banned.Moderator = Context.User.Id;
            profile.Banned.Reason = reason;
            profile.Banned.ExpiryTime = DateTime.UtcNow + TimeSpan.FromHours(hours);
            Context.Server.Save();
            return SimpleEmbedAsync($"{user.Mention} has been banned for {hours} hours by {Context.User.Mention}\n" +
                                    "**Reason**\n" +
                                    $"{reason}");
        }

        [Command("Unban")]
        [Summary("Unban the specified user")]
        public async Task UnBanAsync(IUser user)
        {
            var profile = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (profile == null)
            {
                throw new Exception("User is not registered");
            }

            if (!profile.Banned.Banned)
            {
                throw new Exception("User is not banned");
            }

            await SimpleEmbedAsync($"{user.Mention} has been unbanned manually.\n" +
                                   "**Ban Info**\n" +
                                   $"Reason: {profile.Banned.Reason}\n" +
                                   $"Moderator: {Context.Guild.GetUser(profile.Banned.Moderator)?.Mention ?? $"[{profile.Banned.Moderator}]"}\n" +
                                   $"Expiry: {profile.Banned.ExpiryTime.ToString(CultureInfo.InvariantCulture)}");
            profile.Banned = new GuildModel.User.Ban();
            Context.Server.Save();
        }

        [Command("Unban")]
        [Summary("Unban a user via ID")]
        public async Task UnBanAsync(ulong user)
        {
            var profile = Context.Server.Users.FirstOrDefault(x => x.UserID == user);
            if (profile == null)
            {
                throw new Exception("User is not registered");
            }

            if (!profile.Banned.Banned)
            {
                throw new Exception("User is not banned");
            }

            await SimpleEmbedAsync($"{profile.Username} [{user}] has been unbanned manually.\n" +
                                   "**Ban Info**\n" +
                                   $"Reason: {profile.Banned.Reason}\n" +
                                   $"Moderator: {Context.Guild.GetUser(profile.Banned.Moderator)?.Mention ?? $"[{profile.Banned.Moderator}]"}\n" +
                                   $"Expiry: {profile.Banned.ExpiryTime.ToString(CultureInfo.InvariantCulture)}");
            profile.Banned = new GuildModel.User.Ban();
            Context.Server.Save();
        }

        [Command("UnbanAll")]
        [Summary("Unban a user via User ID")]
        public Task UnbanAllAsync()
        {
            var modified = Context.Server.Users.Count(x => x.Banned.Banned);
            foreach (var user in Context.Server.Users.Where(x => x.Banned.Banned))
            {
                user.Banned = new GuildModel.User.Ban();
            }

            Context.Server.Save();
            return SimpleEmbedAsync($"Success, {modified} users have been unbanned.");
        }

        [Command("Bans")]
        [Summary("Shows all bans")]
        public Task BansAsync()
        {
            var pages = new List<PaginatedMessage.Page>();

            foreach (var banGroup in Context.Server.Users.Where(x => x.Banned.Banned).ToList().SplitList(20))
            {
                var splitList = banGroup.SplitList(5);
                var fields = splitList.Select(x => new EmbedFieldBuilder
                {
                    Name = "Bans",
                    Value = string.Join("\n", x.Select(b => $"User: {b.Username} [{b.UserID}]\n" +
                                                            $"Mod: {Context.Guild.GetUser(b.Banned.Moderator)?.Mention ?? $"{b.Banned.Moderator}"}\n" +
                                                            $"Expires: {(b.Banned.ExpiryTime - DateTime.UtcNow).TotalMinutes} minutes\n" +
                                                            $"Reason: {b.Banned.Reason}"))
                }).ToList();
                pages.Add(new PaginatedMessage.Page
                {
                    Fields = fields
                });
            }

            foreach (var users in Context.Server.Users.Where(x => x.Banned.ExpiryTime < DateTime.UtcNow && x.Banned.Banned).ToList().SplitList(5))
            {
                var userStrings = users.Select(b => new EmbedFieldBuilder
                {
                    Name = "Expired Bans",
                    Value = $"User: {b.Username} [{b.UserID}]\n" +
                            $"Mod: {Context.Guild.GetUser(b.Banned.Moderator)?.Mention ?? $"{b.Banned.Moderator}"}\n" +
                            $"Reason: {b.Banned.Reason}"
                }).ToList();

                pages.Add(new PaginatedMessage.Page
                {
                    Fields = userStrings
                });

                foreach (var user in users)
                {
                    Context.Server.Users.FirstOrDefault(x => x.UserID == user.UserID).Banned = new GuildModel.User.Ban();
                }
            }

            Context.Server.Save();
            var pager = new PaginatedMessage
            {
                Pages = pages,
                Title = "Bans"
            };

            return PagedReplyAsync(pager, new ReactionList
                                              {
                                                  Forward = true,
                                                  Backward = true, Trash = true
                                              });
        }
    }
}