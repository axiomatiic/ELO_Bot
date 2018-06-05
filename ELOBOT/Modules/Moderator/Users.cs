using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ELOBOT.Discord.Context;
using ELOBOT.Discord.Context.Interactive.Paginator;
using ELOBOT.Discord.Extensions;
using ELOBOT.Discord.Preconditions;
using ELOBOT.Models;

namespace ELOBOT.Modules.Moderator
{
    [CustomPermissions(true, true)]
    public class Users : Base
    {
        [Command("Ban")]
        public async Task Ban(IUser user, int hours, [Remainder] string reason = null)
        {
            var userprofile = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (userprofile == null)
            {
                throw new Exception("User is not registered");
            }

            if (reason == null || reason.Length > 200)
            {
                throw new Exception("Reason cannot be empty or greater than 200 characters long");
            }

            userprofile.Banned.Banned = true;
            userprofile.Banned.Moderator = Context.User.Id;
            userprofile.Banned.Reason = reason;
            userprofile.Banned.ExpiryTime = DateTime.UtcNow + TimeSpan.FromHours(hours);
            Context.Server.Save();
            await SimpleEmbedAsync($"{user.Mention} has been banned for {hours} hours by {Context.User.Mention}\n" +
                                   "**Reason**\n" +
                                   $"{reason}");
        }

        [Command("Unban")]
        public async Task UnBan(IUser user)
        {
            var userprofile = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (userprofile == null)
            {
                throw new Exception("User is not registered");
            }

            if (!userprofile.Banned.Banned)
            {
                throw new Exception("User is not banned");
            }

            await SimpleEmbedAsync($"{user.Mention} has been unbanned manually.\n" +
                                   "**Ban Info**\n" +
                                   $"Reason: {userprofile.Banned.Reason}\n" +
                                   $"Moderator: {Context.Socket.Guild.GetUser(userprofile.Banned.Moderator)?.Mention ?? $"[{userprofile.Banned.Moderator}]"}\n" +
                                   $"Expiry: {userprofile.Banned.ExpiryTime.ToString(CultureInfo.InvariantCulture)}");
            userprofile.Banned = new GuildModel.User.Ban();
            Context.Server.Save();
        }

        [Command("UnbanAll")]
        public async Task UnbanAll()
        {
            var modified = Context.Server.Users.Count(x => x.Banned.Banned);
            foreach (var user in Context.Server.Users.Where(x => x.Banned.Banned))
            {
                user.Banned = new GuildModel.User.Ban();
            }

            Context.Server.Save();
            await SimpleEmbedAsync($"Success, {modified} users have been unbanned.");
        }

        [Command("Bans")]
        public async Task Bans()
        {
            var pages = new List<PaginatedMessage.Page>();


            foreach (var bangroup in ListManagement.splitList(Context.Server.Users.Where(x => x.Banned.Banned).ToList(), 20))
            {
                var bansubgroup = ListManagement.splitList(bangroup, 5);
                var fields = bansubgroup.Select(x => new EmbedFieldBuilder
                {
                    Name = "Bans",
                    Value = string.Join("\n", x.Select(b => $"User: {b.Username} [{b.UserID}]\n" +
                                                            $"Mod: {Context.Socket.Guild.GetUser(b.Banned.Moderator)?.Mention ?? $"{b.Banned.Moderator}"}\n" +
                                                            $"Expires: {(b.Banned.ExpiryTime - DateTime.UtcNow).TotalMinutes} minutes\n" +
                                                            $"Reason: {b.Banned.Reason}"))
                }).ToList();
                pages.Add(new PaginatedMessage.Page
                {
                    Fields = fields
                });
            }

            foreach (var users in ListManagement.splitList(Context.Server.Users.Where(x => x.Banned.ExpiryTime < DateTime.UtcNow && x.Banned.Banned).ToList(), 5))
            {
                var userstrings = users.Select(b => new EmbedFieldBuilder
                {
                    Name = "Expired Bans",
                    Value = $"User: {b.Username} [{b.UserID}]\n" +
                            $"Mod: {Context.Socket.Guild.GetUser(b.Banned.Moderator)?.Mention ?? $"{b.Banned.Moderator}"}\n" +
                            $"Reason: {b.Banned.Reason}"
                }).ToList();

                pages.Add(new PaginatedMessage.Page
                {
                    Fields = userstrings
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
            await PagedReplyAsync(pager);
        }
    }
}