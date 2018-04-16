using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using ELO_Bot.PreConditions;

namespace ELO_Bot.Commands
{
    public class User : InteractiveBase
    {
        /// <summary>
        ///     sign up for ELO Bot
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [Command("Register")]
        [Ratelimit(3, 30d, Measure.Seconds)]
        [Summary("Register <username>")]
        [Remarks("registers the user in the server")]
        public async Task Register([Remainder] string username = null)
        {
            var embed = new EmbedBuilder();

            if (username == null)
            {
                embed.AddField("ERROR", "Please specify a name to be registered with");
                embed.WithColor(Color.Red);
                await ReplyAsync("", false, embed.Build());
                return;
            }

            if (username.Length > 20)
            {
                embed.AddField("ERROR", "Username Must be 20 characters or less");
                embed.WithColor(Color.Red);
                await ReplyAsync("", false, embed.Build());
                return;
            }

            var server = Servers.ServerList.First(x => x.ServerId == Context.Guild.Id);

            if (server.UserList.Count >= 20 && !server.IsPremium)
            {
                embed.AddField("ERROR",
                    "Free User limit has been hit. To upgrade the limit from 20 users to unlimited users, Purchase premium here: https://rocketr.net/buy/0e79a25902f5");
                embed.WithColor(Color.Red);
                await ReplyAsync("", false, embed.Build());
                return;
            }

            if (server.Expiry < DateTime.UtcNow)
            {
                embed.AddField("ERROR",
                    $"Premium for this server expired at {server.Expiry.ToString(CultureInfo.InvariantCulture)}. To renew, purchase premium here: https://rocketr.net/buy/0e79a25902f5");
                await ReplyAsync("", false, embed.Build());
                embed.WithColor(Color.Blue);
                return;
            }

            try
            {
                if (server.UserList.Any(member => member.UserId == Context.User.Id))
                {
                    var userprofile = server.UserList.FirstOrDefault(x => x.UserId == Context.User.Id);

                    if (userprofile == null)
                    {
                        await ReplyAsync("ERROR: User not registered!");
                        return;
                    }

                    if (!((IGuildUser)Context.User).RoleIds.Contains(server.RegisterRole) && server.RegisterRole != 0)
                        try
                        {
                            var serverrole = Context.Guild.GetRole(server.RegisterRole);
                            try
                            {
                                await ((IGuildUser)Context.User).AddRoleAsync(serverrole);
                            }
                            catch
                            {
                                embed.AddField("ERROR", "User Role Unable to be modified");
                            }
                        }
                        catch
                        {
                            embed.AddField("ERROR", "Register Role is Unavailable");
                        }

                    try
                    {
                        await UserRename(server.UsernameSelection, Context.User, username, userprofile.Points);


                        userprofile.Username = username;
                    }
                    catch
                    {
                        embed.AddField("ERROR", "Username Unable to be modified (Permissions are above the bot)");
                    }


                    embed.AddField("ERROR", "User is already registered, role and name have been updated accordingly");

                    embed.WithColor(Color.Red);

                    await ReplyAsync("", false, embed.Build());
                    return;
                }
            }
            catch
            {
                //
            }


            var user = new Servers.Server.User
            {
                UserId = Context.User.Id,
                Username = username,
                Points = server.registerpoints
            };

            server.UserList.Add(user);
            embed.AddField($"{Context.User.Username} registered as {username}", $"{server.Registermessage}");
            embed.WithColor(Color.Blue);
            try
            {
                await UserRename(server.UsernameSelection, Context.User, user.Username, server.registerpoints);
            }
            catch
            {
                embed.AddField("ERROR", "Username Unable to be modified (Permissions are above the bot)");
            }
            if (server.RegisterRole != 0)
                try
                {
                    var serverrole = Context.Guild.GetRole(server.RegisterRole);
                    try
                    {
                        await ((IGuildUser)Context.User).AddRoleAsync(serverrole);
                    }
                    catch
                    {
                        embed.AddField("ERROR", "User Role Unable to be modified");
                    }
                }
                catch
                {
                    embed.AddField("ERROR", "Register Role is Unavailable");
                }
            await ReplyAsync("", false, embed.Build());
        }

        /// <summary>
        ///     get a user's profile and information
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [Command("GetUser")]
        [Summary("GetUser <@user>")]
        [Remarks("checks stats about a user")]
        public async Task GetUser(IUser user)
        {
            var embed = new EmbedBuilder();
            var server = Servers.ServerList.First(x => x.ServerId == Context.Guild.Id);
            var userlist = server.UserList;
            var orderlist = server.UserList.OrderByDescending(x => x.Points).ToList();
            foreach (var usr in userlist)
                if (usr.UserId == user.Id)
                {
                    if (server.showkd == false)
                    {
                        embed.AddField($"{usr.Username}", $"Points: {usr.Points}\n" +
                                                          $"Wins: {usr.Wins}\n" +
                                                          $"Losses: {usr.Losses}\n" +
                                                          $"Leaderboard Rank: {orderlist.FindIndex(x => x.UserId == user.Id)}");
                    }
                    else
                    {
                        embed.AddField($"{usr.Username}", $"Points: {usr.Points}\n" +
                                                          $"Wins: {usr.Wins}\n" +
                                                          $"Losses: {usr.Losses}\n" +
                                                          $"Kills: {usr.kills}\n" +
                                                          $"Deaths: {usr.deaths}\n" +
                                                          $"KD: {(double)usr.kills/(usr.deaths == 0 ? 1 : usr.deaths)}\n" +
                                                          $"Leaderboard Rank: {orderlist.FindIndex(x => x.UserId == user.Id)}");
                    }

                    await ReplyAsync("", false, embed.Build());
                    return;
                }

            await ReplyAsync("User Unavailable");
        }

        /// <summary>
        ///     list users in order of hihest points
        ///     most wins
        ///     or most losses
        /// </summary>
        [Command("Leaderboard")]
        [Summary("Leaderboard <wins, losses, points>")]
        [Remarks("Displays Rank Leaderboard (Top 20 )")]
        public async Task LeaderBoard([Remainder]string arg = "point")
        {

            var server = Servers.ServerList.First(x => x.ServerId == Context.Guild.Id);
            var page = new List<string>();
            var pagecontent = "";
            var i = 0;
            var total = 0;
            string argtype;
            List<Servers.Server.User> sorttype;
            if (arg.ToLower().Contains("win"))
            {
                argtype = "Leaderboard Wins";
                sorttype = server.UserList.OrderBy(x => x.Wins).Reverse().ToList();
                foreach (var user in sorttype)
                {
                    i++;
                    total++;
                    pagecontent += $"{total}. {user.Username} - {user.Wins}\n";
                    if (i >= 20)
                    {
                        page.Add(pagecontent);
                        pagecontent = "";
                        i = 0;
                    }
                }
            }
            else if (arg.ToLower().Contains("los"))
            {
                argtype = "Leaderboard Losses";
                sorttype = server.UserList.OrderBy(x => x.Losses).Reverse().ToList();
                foreach (var user in sorttype)
                {
                    i++;
                    total++;
                    pagecontent += $"{total}. {user.Username} - {user.Losses}\n";
                    if (i >= 20)
                    {
                        page.Add(pagecontent);
                        pagecontent = "";
                        i = 0;
                    }
                }
            }
            else if (arg.ToLower().Contains("kill"))
            {
                if (!server.showkd)
                {
                    await ReplyAsync("Server Does not have K/D Enabled");
                    return;
                }
                argtype = "Leaderboard Kills";
                sorttype = server.UserList.OrderBy(x => x.kills).Reverse().ToList();
                foreach (var user in sorttype)
                {
                    i++;
                    total++;
                    pagecontent += $"{total}. {user.Username} - {user.kills}\n";
                    if (i >= 20)
                    {
                        page.Add(pagecontent);
                        pagecontent = "";
                        i = 0;
                    }
                }
            }
            else if (arg.ToLower().Contains("death"))
            {
                if (!server.showkd)
                {
                    await ReplyAsync("Server Does not have K/D Enabled");
                    return;
                }
                argtype = "Leaderboard Deaths";
                sorttype = server.UserList.OrderBy(x => x.deaths).Reverse().ToList();
                foreach (var user in sorttype)
                {
                    i++;
                    total++;
                    pagecontent += $"{total}. {user.Username} - {user.deaths}\n";
                    if (i >= 20)
                    {
                        page.Add(pagecontent);
                        pagecontent = "";
                        i = 0;
                    }
                }
            }
            else if (arg.ToLower().Contains("kd"))
            {
                if (!server.showkd)
                {
                    await ReplyAsync("Server Does not have K/D Enabled");
                    return;
                }
                argtype = "Leaderboard K/D";
                sorttype = server.UserList.OrderBy(x => (double)x.kills/(x.deaths == 0 ? 1 : x.deaths)).Reverse().ToList();
                foreach (var user in sorttype)
                {
                    i++;
                    total++;
                    pagecontent += $"{total}. {user.Username} - {(double)user.kills / (user.deaths == 0 ? 1 : user.deaths)}\n";
                    if (i >= 20)
                    {
                        page.Add(pagecontent);
                        pagecontent = "";
                        i = 0;
                    }
                }
            }
            else
            {
                argtype = "Leaderboard Points";
                sorttype = server.UserList.OrderBy(x => x.Points).Reverse().ToList();
                foreach (var user in sorttype)
                {
                    i++;
                    total++;
                    pagecontent += $"{total}. {user.Username} - {user.Points}\n";
                    if (i >= 20)
                    {
                        page.Add(pagecontent);
                        pagecontent = "";
                        i = 0;
                    }
                }
            }

            page.Add(pagecontent);
            var msg = new PaginatedMessage
            {
                Title = $"{argtype} (Users = {sorttype.Count})",
                Pages = page,
                Color = new Color(114, 137, 218)
            };

            await PagedReplyAsync(msg);

        }

        /// <summary>
        ///     display ranks for the current server
        /// </summary>
        /// <returns></returns>
        [Command("ranks")]
        [Summary("ranks")]
        [Remarks("display all Ranked Roles")]
        public async Task List()
        {
            var embed = new EmbedBuilder();
            var server = Servers.ServerList.First(x => x.ServerId == Context.Guild.Id);
            var orderedlist = server.Ranks.OrderBy(x => x.Points).Reverse();
            var desc = "Points - Role (PPW/PPL)\n";
            foreach (var lev in orderedlist)
            {
                string rolename;
                try
                {
                    rolename = Context.Guild.GetRole(lev.RoleId).Name;
                }
                catch
                {
                    rolename = $"ERR: {lev.RoleId}";
                }

                var pwin = lev.WinModifier == 0 ? server.Winamount : lev.WinModifier;
                var ploss = lev.LossModifier == 0 ? server.Lossamount : lev.LossModifier;

                desc += $"`{lev.Points}` - {rolename} (+{pwin}/-{ploss})\n";
            }

            if (server.RegisterRole != 0)
            {
                string rolename;
                try
                {
                    rolename = Context.Guild.GetRole(server.RegisterRole).Name;
                }
                catch
                {
                    rolename = $"ERR: {server.RegisterRole}";
                }

                var pwin = server.Winamount;
                var ploss = server.Lossamount;

                desc += $"`0` - {rolename} (+{pwin}/-{ploss})\n";
            }

            desc += "\n\nPPW = Points per win\n" +
                    "PPL = Points per loss";

            embed.AddField("Ranks", desc);
            embed.WithColor(Color.Blue);
            await ReplyAsync("", false, embed.Build());
        }

        /// <summary>
        ///     generate an invite for the bot to join your own server
        /// </summary>
        /// <returns></returns>
        [Command("Invite")]
        [Summary("Invite")]
        [Remarks("Invite the Bot")]
        public async Task Invite()
        {
            await ReplyAsync(
                $"Invite ELO Bot Here: <https://discordapp.com/oauth2/authorize?client_id={Context.Client.CurrentUser.Id}&scope=bot&permissions=2146958591>\n" +
                $"Support Server: <{Config.Load().DiscordInvite}>\n" +
                "Developed By: PassiveModding");
        }

        public async Task UserRename(int usernameSelection, IUser user, string username, int userpoints)
        {
            //await UserRename(server.UsernameSelection, u, user.Username, user.Points);
            if (usernameSelection == 1)
            {
                await ((IGuildUser)user).ModifyAsync(x => { x.Nickname = $"{userpoints} ~ {username}"; });

                if (CommandHandler.VerifiedUsers != null)
                    if (CommandHandler.VerifiedUsers.Contains(Context.User.Id))
                        await ((IGuildUser)user).ModifyAsync(x => { x.Nickname = $"👑{userpoints} ~ {username}"; });
            }
            else if (usernameSelection == 2)
            {
                await ((IGuildUser)user).ModifyAsync(x => { x.Nickname = $"[{userpoints}] {username}"; });

                if (CommandHandler.VerifiedUsers != null)
                    if (CommandHandler.VerifiedUsers.Contains(Context.User.Id))
                        await ((IGuildUser)user).ModifyAsync(x => { x.Nickname = $"👑[{userpoints}] {username}"; });
            }
            else if (usernameSelection == 3)
            {
                await ((IGuildUser)user).ModifyAsync(x => { x.Nickname = $"{username}"; });

                if (CommandHandler.VerifiedUsers != null)
                    if (CommandHandler.VerifiedUsers.Contains(Context.User.Id))
                        await ((IGuildUser)user).ModifyAsync(x => { x.Nickname = $"👑{username}"; });
            }
        }
    }
}