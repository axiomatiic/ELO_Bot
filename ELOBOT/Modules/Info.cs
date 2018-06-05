using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ELOBOT.Discord.Context;
using ELOBOT.Discord.Context.Interactive.Paginator;
using ELOBOT.Discord.Extensions;
using ELOBOT.Discord.Preconditions;
using ELOBOT.Models;

namespace ELOBOT.Modules
{
    [RequireContext(ContextType.Guild)]
    public class Info : Base
    {
        public enum LeaderboardSortMode
        {
            Win,
            Loss,
            Points,
            GamesPlayed,
            Kills,
            Deaths,
            Draws
        }

        [Command("Register")]
        public async Task Register()
        {
            await Register(Context.User.Username);
        }

        [Command("Register")]
        public async Task Register([Remainder] string name)
        {
            if (name.Length > 20)
            {
                throw new Exception("Name nust be equal to or less than 20 characters long");
            }

            var NewUser = new GuildModel.User
            {
                UserID = Context.User.Id,
                Username = name,
                Stats = new GuildModel.User.Score
                {
                    Points = Context.Server.Settings.Registration.RegistrationBonus
                }
            };

            if (Context.Elo.User != null)
            {
                NewUser.Stats = Context.Elo.User.Stats;
                NewUser.Banned = Context.Elo.User.Banned;
                Context.Server.Users.Remove(Context.Elo.User);
            }

            Context.Server.Users.Add(NewUser);

            if (NewUser.Stats.Points == Context.Server.Settings.Registration.RegistrationBonus && Context.Server.Ranks.FirstOrDefault(x => x.IsDefault)?.RoleID != null)
            {
                var RegisterRole = Context.Guild.GetRole(Context.Server.Ranks.FirstOrDefault(x => x.IsDefault).RoleID);
                if (RegisterRole != null)
                {
                    try
                    {
                        await (Context.User as IGuildUser).AddRoleAsync(RegisterRole);
                    }
                    catch
                    {
                        //User Permissions above the bot.
                    }
                }
            }
            else
            {
                await UserManagement.GiveMaxRole(Context, NewUser);
            }

            await UserManagement.UserRename(Context, NewUser);
            Context.Server.Save();

            await ReplyAsync(new EmbedBuilder
            {
                Title = $"Success, Registered as {name}",
                Description = Context.Server.Settings.Registration.Message
            });

            if (Context.Socket.Guild.GetRole(Context.Server.Ranks.FirstOrDefault(x => x.IsDefault)?.RoleID ?? 0) is IRole RegRole)
            {
                try
                {
                    await (Context.User as IGuildUser).AddRoleAsync(RegRole);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        [Command("Invite")]
        public async Task Invite()
        {
            await SimpleEmbedAsync($"You may invite the bot to your own server using the following URL: {BotInfo.GetInvite(Context)}");
        }

        [CustomPermissions]
        [Command("GetUser")]
        public async Task GetUser(IUser User = null)
        {
            if (User == null)
            {
                User = Context.User;
            }

            var ELOUser = Context.Server.Users.FirstOrDefault(x => x.UserID == User.Id);
            if (ELOUser == null)
            {
                throw new Exception("User is not registered");
            }

            var embed = new EmbedBuilder
                {
                    Color = Color.Blue,
                    Title = $"{ELOUser.Username} Profile"
                }
                .AddField("Points", ELOUser.Stats.Points.ToString())
                .AddField("Rank", Context.Socket.Guild.GetRole(UserManagement.MaxRole(Context, ELOUser).RoleID)?.Mention ?? "N/A")
                .AddField("Games", "**Games Played**\n" +
                                   $"{ELOUser.Stats.GamesPlayed}\n" +
                                   "**Wins**\n" +
                                   $"{ELOUser.Stats.Wins}\n" +
                                   "**Losses**\n" +
                                   $"{ELOUser.Stats.Losses}\n" +
                                   "**WLR**\n" +
                                   $"{(double) ELOUser.Stats.Wins / ELOUser.Stats.Losses}");

            if (Context.Server.Settings.GameSettings.useKD)
            {
                embed.AddField("K/D", "**Kills**\n" +
                                      $"{ELOUser.Stats.Kills}\n" +
                                      "**Deaths**\n" +
                                      $"{ELOUser.Stats.Deaths}\n" +
                                      "**KDR**\n" +
                                      $"{(double) ELOUser.Stats.Kills / ELOUser.Stats.Deaths}");
            }

            await ReplyAsync(embed);
        }

        [CustomPermissions]
        [Command("LeaderboardSort")]
        public async Task LSort()
        {
            await SimpleEmbedAsync("Leaderboard Sort Options:\n" +
                                   "Win\r\n" +
                                   "Loss\r\n" +
                                   "Points\r\n" +
                                   "GamesPlayed\r\n" +
                                   "Kills\r\n" +
                                   "Deaths\r\n" +
                                   "Draws");
        }

        [CustomPermissions]
        [Command("Leaderboard")]
        public async Task Leaderboard(LeaderboardSortMode Mode = LeaderboardSortMode.Points)
        {
            var rgx = new Regex("[^a-zA-Z0-9 -#]");
            List<string> userstrings;
            switch (Mode)
            {
                case LeaderboardSortMode.Points:
                {
                    var users = Context.Server.Users.OrderByDescending(x => x.Stats.Points).Where(x => Context.Socket.Guild.GetUser(x.UserID) != null).ToList();
                    userstrings = users.Select(x => $"`{$"#{users.IndexOf(x) + 1} - {rgx.Replace(x.Username, "")}".PadRight(40)}\u200B || Points: {x.Stats.Points}`").ToList();
                    break;
                }
                case LeaderboardSortMode.Win:
                {
                    var users = Context.Server.Users.OrderByDescending(x => x.Stats.Wins).Where(x => Context.Socket.Guild.GetUser(x.UserID) != null).ToList();
                    userstrings = users.Select(x => $"`{$"#{users.IndexOf(x) + 1} - {rgx.Replace(x.Username, "")}".PadRight(40)}\u200B || Wins: {x.Stats.Wins}`").ToList();
                    break;
                }
                case LeaderboardSortMode.Loss:
                {
                    var users = Context.Server.Users.OrderByDescending(x => x.Stats.Losses).Where(x => Context.Socket.Guild.GetUser(x.UserID) != null).ToList();
                    userstrings = users.Select(x => $"`{$"#{users.IndexOf(x) + 1} - {rgx.Replace(x.Username, "")}".PadRight(40)}\u200B || Losses: {x.Stats.Losses}`").ToList();
                    break;
                }
                case LeaderboardSortMode.Kills:
                {
                    var users = Context.Server.Users.OrderByDescending(x => x.Stats.Kills).Where(x => Context.Socket.Guild.GetUser(x.UserID) != null).ToList();
                    userstrings = users.Select(x => $"`{$"#{users.IndexOf(x) + 1} - {rgx.Replace(x.Username, "")}".PadRight(40)}\u200B || Kills: {x.Stats.Kills}`").ToList();
                    break;
                }
                case LeaderboardSortMode.Deaths:
                {
                    var users = Context.Server.Users.OrderByDescending(x => x.Stats.Deaths).Where(x => Context.Socket.Guild.GetUser(x.UserID) != null).ToList();
                    userstrings = users.Select(x => $"`{$"#{users.IndexOf(x) + 1} - {rgx.Replace(x.Username, "")}".PadRight(40)}\u200B || Deaths: {x.Stats.Deaths}`").ToList();
                    break;
                }
                case LeaderboardSortMode.Draws:
                {
                    var users = Context.Server.Users.OrderByDescending(x => x.Stats.Draws).Where(x => Context.Socket.Guild.GetUser(x.UserID) != null).ToList();
                    userstrings = users.Select(x => $"`{$"#{users.IndexOf(x) + 1} - {rgx.Replace(x.Username, "")}".PadRight(40)}\u200B || Draws: {x.Stats.Draws}`").ToList();
                    break;
                }
                case LeaderboardSortMode.GamesPlayed:
                {
                    var users = Context.Server.Users.OrderByDescending(x => x.Stats.GamesPlayed).Where(x => Context.Socket.Guild.GetUser(x.UserID) != null).ToList();
                    userstrings = users.Select(x => $"`{$"#{users.IndexOf(x) + 1} - {rgx.Replace(x.Username, "")}".PadRight(40)}\u200B || Games: {x.Stats.GamesPlayed}`").ToList();
                    break;
                }
                default:
                {
                    var users = Context.Server.Users.OrderByDescending(x => x.Stats.Points).Where(x => Context.Socket.Guild.GetUser(x.UserID) != null).ToList();
                    userstrings = users.Select(x => $"`{$"#{users.IndexOf(x) + 1} - {rgx.Replace(x.Username, "")}".PadRight(40)}\u200B || Points: {x.Stats.Points}`").ToList();
                    break;
                }
            }

            var pages = ListManagement.splitList(userstrings, 20).Select(x => new PaginatedMessage.Page
            {
                description = string.Join("\n", x)
            });
            var Pager = new PaginatedMessage
            {
                Title = "ELO Bot Leaderboard",
                Pages = pages,
                Color = Color.Blue
            };
            await PagedReplyAsync(Pager);
        }
    }
}