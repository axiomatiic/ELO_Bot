namespace ELO.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using ELO.Discord.Context;
    using ELO.Discord.Extensions;
    using ELO.Discord.Preconditions;
    using ELO.Models;

    using global::Discord;
    using global::Discord.Addons.Interactive;
    using global::Discord.Commands;

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
        public Task RegisterAsync(IUser user)
        {
            if (user.Id == Context.User.Id)
            {
                return RegisterAsync(Context.User.Username);
            }

            throw new Exception("You cannot register by tagging another user");
        }

        [Command("Register")]
        public Task RegisterAsync()
        {
            return RegisterAsync(Context.User.Username);
        }

        [Command("Register")]
        public async Task RegisterAsync([Remainder] string name)
        {
            if (name.Length > 20)
            {
                throw new Exception("Name must be equal to or less than 20 characters long");
            }

            var newUser = new GuildModel.User
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
                newUser.Stats = Context.Elo.User.Stats;
                newUser.Banned = Context.Elo.User.Banned;
                Context.Server.Users.Remove(Context.Elo.User);
            }
            else
            {
                if (Context.Server.Users.Count > -1 && (Context.Server.Settings.Premium.Expiry < DateTime.UtcNow || !Context.Server.Settings.Premium.IsPremium))
                {
                    throw new Exception($"Premium is required to register more than 20 users. {ConfigModel.Load().PurchaseLink}\n"
                                        + $"Get the server owner to purchase a key and use the command `{Context.Prefix}Premium <key>`");
                }
            }

            Context.Server.Users.Add(newUser);

            if (newUser.Stats.Points == Context.Server.Settings.Registration.RegistrationBonus && Context.Server.Ranks.FirstOrDefault(x => x.IsDefault)?.RoleID != null)
            {
                var registerRole = Context.Guild.GetRole(Context.Server.Ranks.FirstOrDefault(x => x.IsDefault).RoleID);
                if (registerRole != null)
                {
                    try
                    {
                        await (Context.User as IGuildUser).AddRoleAsync(registerRole);
                    }
                    catch
                    {
                        // user Permissions above the bot.
                    }
                }
            }
            else
            {
                await UserManagement.GiveMaxRoleAsync(Context, newUser);
            }

            await UserManagement.UserRenameAsync(Context, newUser);
            Context.Server.Save();

            await ReplyAsync(new EmbedBuilder
            {
                Title = $"Success, Registered as {name}",
                Description = Context.Server.Settings.Registration.Message
            });

            if (Context.Guild.GetRole(Context.Server.Ranks.FirstOrDefault(x => x.IsDefault)?.RoleID ?? 0) is IRole RegRole)
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
        public Task InviteAsync()
        {
            return SimpleEmbedAsync($"You may invite the bot to your own server using the following URL: {BotInfo.GetInvite(Context)}");
        }

        [CustomPermissions]
        [Command("GetUser")]
        public Task GetUserAsync(IUser user = null)
        {
            if (user == null)
            {
                user = Context.User;
            }

            var eloProfile = Context.Server.Users.FirstOrDefault(x => x.UserID == user.Id);
            if (eloProfile == null)
            {
                throw new Exception("user is not registered");
            }

            var embed = new EmbedBuilder
                {
                    Color = Color.Blue,
                    Title = $"{eloProfile.Username} Profile"
                }
                .AddField("Points", eloProfile.Stats.Points.ToString())
                .AddField("Rank", Context.Guild.GetRole(UserManagement.MaxRole(Context, eloProfile).RoleID)?.Mention ?? "N/A")
                .AddField("Games", "**Games Played**\n" +
                                   $"{eloProfile.Stats.GamesPlayed}\n" +
                                   "**Wins**\n" +
                                   $"{eloProfile.Stats.Wins}\n" +
                                   "**Losses**\n" +
                                   $"{eloProfile.Stats.Losses}\n" +
                                   "**WLR**\n" +
                                   $"{(double) eloProfile.Stats.Wins / eloProfile.Stats.Losses}");

            if (Context.Server.Settings.GameSettings.UseKd)
            {
                embed.AddField("K/D", "**Kills**\n" +
                                      $"{eloProfile.Stats.Kills}\n" +
                                      "**Deaths**\n" +
                                      $"{eloProfile.Stats.Deaths}\n" +
                                      "**KDR**\n" +
                                      $"{(double) eloProfile.Stats.Kills / eloProfile.Stats.Deaths}");
            }

            return ReplyAsync(embed);
        }

        [CustomPermissions]
        [Command("LeaderBoardSort")]
        public Task LSortAsync()
        {
            return SimpleEmbedAsync("Leader board Sort Options:\n" +
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
        public Task LeaderboardAsync(LeaderboardSortMode mode = LeaderboardSortMode.Points)
        {
            var rgx = new Regex("[^a-zA-Z0-9 -#]");
            List<string> userStrings;
            switch (mode)
            {
                case LeaderboardSortMode.Points:
                {
                    var users = Context.Server.Users.OrderByDescending(x => x.Stats.Points).Where(x => Context.Guild.GetUser(x.UserID) != null).ToList();
                    userStrings = users.Select(x => $"`{$"#{users.IndexOf(x) + 1} - {rgx.Replace(x.Username, "")}".PadRight(40)}\u200B || Points: {x.Stats.Points}`").ToList();
                    break;
                }

                case LeaderboardSortMode.Win:
                {
                    var users = Context.Server.Users.OrderByDescending(x => x.Stats.Wins).Where(x => Context.Guild.GetUser(x.UserID) != null).ToList();
                    userStrings = users.Select(x => $"`{$"#{users.IndexOf(x) + 1} - {rgx.Replace(x.Username, "")}".PadRight(40)}\u200B || Wins: {x.Stats.Wins}`").ToList();
                    break;
                }

                case LeaderboardSortMode.Loss:
                {
                    var users = Context.Server.Users.OrderByDescending(x => x.Stats.Losses).Where(x => Context.Guild.GetUser(x.UserID) != null).ToList();
                    userStrings = users.Select(x => $"`{$"#{users.IndexOf(x) + 1} - {rgx.Replace(x.Username, "")}".PadRight(40)}\u200B || Losses: {x.Stats.Losses}`").ToList();
                    break;
                }

                case LeaderboardSortMode.Kills:
                {
                    var users = Context.Server.Users.OrderByDescending(x => x.Stats.Kills).Where(x => Context.Guild.GetUser(x.UserID) != null).ToList();
                    userStrings = users.Select(x => $"`{$"#{users.IndexOf(x) + 1} - {rgx.Replace(x.Username, "")}".PadRight(40)}\u200B || Kills: {x.Stats.Kills}`").ToList();
                    break;
                }

                case LeaderboardSortMode.Deaths:
                {
                    var users = Context.Server.Users.OrderByDescending(x => x.Stats.Deaths).Where(x => Context.Guild.GetUser(x.UserID) != null).ToList();
                    userStrings = users.Select(x => $"`{$"#{users.IndexOf(x) + 1} - {rgx.Replace(x.Username, "")}".PadRight(40)}\u200B || Deaths: {x.Stats.Deaths}`").ToList();
                    break;
                }

                case LeaderboardSortMode.Draws:
                {
                    var users = Context.Server.Users.OrderByDescending(x => x.Stats.Draws).Where(x => Context.Guild.GetUser(x.UserID) != null).ToList();
                    userStrings = users.Select(x => $"`{$"#{users.IndexOf(x) + 1} - {rgx.Replace(x.Username, "")}".PadRight(40)}\u200B || Draws: {x.Stats.Draws}`").ToList();
                    break;
                }

                case LeaderboardSortMode.GamesPlayed:
                {
                    var users = Context.Server.Users.OrderByDescending(x => x.Stats.GamesPlayed).Where(x => Context.Guild.GetUser(x.UserID) != null).ToList();
                    userStrings = users.Select(x => $"`{$"#{users.IndexOf(x) + 1} - {rgx.Replace(x.Username, "")}".PadRight(40)}\u200B || Games: {x.Stats.GamesPlayed}`").ToList();
                    break;
                }

                default:
                {
                    var users = Context.Server.Users.OrderByDescending(x => x.Stats.Points).Where(x => Context.Guild.GetUser(x.UserID) != null).ToList();
                    userStrings = users.Select(x => $"`{$"#{users.IndexOf(x) + 1} - {rgx.Replace(x.Username, "")}".PadRight(40)}\u200B || Points: {x.Stats.Points}`").ToList();
                    break;
                }
            }

            var pages = userStrings.SplitList(20).Select(x => new PaginatedMessage.Page
            {
                Description = string.Join("\n", x)
            });

            var pager = new PaginatedMessage
            {
                Title = "ELO Bot Leader board",
                Pages = pages,
                Color = Color.Blue
            };

            return PagedReplyAsync(pager, new ReactionList
                                              {
                                                  Forward = true,
                                                  Backward = true,
                                                  Trash = true
                                              });
        }
    }
}