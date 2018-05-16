using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using ELO_Bot.Preconditions;
using Newtonsoft.Json;

namespace ELO_Bot.Commands.Admin
{
    [BotOwner]
    public class Owner : InteractiveBase
    {
        /// <summary>
        ///     Adds a list of keys to the premium list.
        ///     If there are duplicate keys, automatically remove them
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        [Command("addpremium")]
        [Summary("addpremium")]
        [Remarks("Bot Creator Command")]
        public async Task Addpremium(params string[] keys)
        {
            var i = 0;
            var duplicates = "Dupes:\n";
            if (CommandHandler.Keys == null)
            {
                CommandHandler.Keys = keys.ToList();
                await ReplyAsync("list replaced.");
                var obj1 = JsonConvert.SerializeObject(CommandHandler.Keys, Formatting.Indented);
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "setup/keys.json"), obj1);
                return;
            }
            foreach (var key in keys)
            {
                var dupe = false;
                foreach (var k in CommandHandler.Keys)
                    if (k == key)
                        dupe = true;
                if (!dupe)
                {
                    i++;
                    CommandHandler.Keys.Add(key); //NO DUPES
                }
                else
                {
                    duplicates += $"{key}\n";
                }
            }
            await ReplyAsync($"{keys.Length} Supplied\n" +
                             $"{i} Added\n" +
                             $"{duplicates}");
            var keyobject = JsonConvert.SerializeObject(CommandHandler.Keys, Formatting.Indented);
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "setup/keys.json"), keyobject);
        }

        /// <summary>
        ///     Announce message to all servers
        /// </summary>
        /// <param name="announcement"></param>
        /// <returns></returns>
        [Command("announce", RunMode = RunMode.Async)]
        [Summary("announce <announcement>")]
        [Remarks("Bot Creator Command")]
        public async Task Addpremium([Remainder]string announcement)
        {
            var embed = new EmbedBuilder();
            embed.AddField("IMPORTANT ANNOUNCEMENT FROM DEV", announcement);
            foreach (var guild in Context.Client.Guilds)
            {
                try
                {
                    foreach (var channel in ((SocketGuild)guild).TextChannels)
                    {
                        try
                        {
                            await channel.SendMessageAsync("", false, embed);
                            break;
                        }
                        catch
                        {
                            //
                        }
                    }
                }
                catch
                {
                    //
                }

            }

            await ReplyAsync("Complete");
        }


        [Command("GuildReset")]
        [Summary("GuildReset")]
        [Remarks("Reset the given guild config")]
        public async Task Help()
        {
            var oguild = Servers.ServerList.FirstOrDefault(x => x.ServerId == Context.Guild.Id);
            if (oguild == null)
            {
                return;
            }

            var nguild = new Servers.Server
            {
                IsPremium = oguild.IsPremium,
                Expiry = oguild.Expiry,
                ServerId = oguild.ServerId,
                PremiumKey = oguild.PremiumKey
            };

            Servers.ServerList.Remove(oguild);
            Servers.ServerList.Add(nguild);

            await ReplyAsync("Success");
        }

        /// <summary>
        ///     rename the bot using a the provided input
        /// </summary>
        /// <param name="name">the provided name for the bot</param>
        /// <returns></returns>
        [Command("botrename")]
        [Summary("botrename")]
        [Remarks("Bot Creator Command")]
        public async Task Help([Remainder] string name)
        {
            if (name.Length > 32)
                throw new Exception("Name length must be less than 32 characters");
            await Context.Client.CurrentUser.ModifyAsync(x => { x.Username = name; });
        }

        /// <summary>
        ///     backs up the current serverlist object to a permanent file in the backups folder.
        ///     also updates the serverlist file
        /// </summary>
        /// <returns></returns>
        [Command("backup")]
        [Summary("backup")]
        [Remarks("Backup the current state of the database")]
        public async Task Backup()
        {
            var contents = JsonConvert.SerializeObject(Servers.ServerList);
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "setup/serverlist.json"), contents);

            var time = $"{DateTime.UtcNow:dd - MM - yy HH.mm.ss}.txt";

            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, $"setup/backups/{time}"), contents);

            await ReplyAsync($"Backup has been saved to serverlist.json and {time}");
        }

        public class simpleserverobj
        {
            public string servername { get; set; }
            public int UserCount { get; set; }
            public string invite { get; set; }
        }

        [Command("Browse", RunMode = RunMode.Async)]
        [Summary("browse")]
        [Remarks("A list of servers")]
        public async Task Broswe([Remainder]string name = null)
        {

            var list = new List<simpleserverobj>();
            var Guilds = name == null ? Context.Client.Guilds.ToList() : Context.Client.Guilds.Where(x => x.Name.ToLower().Contains(name.ToLower())).ToList();

            var work = await ReplyAsync($"Working 0/{Guilds.Count}");
            var i = 0;
            foreach (var Guild in Guilds)
            {
                try
                {
                    foreach (var channel in Guild.Channels)
                        try
                        {
                            string inv;
                            if ((await Guild.GetInvitesAsync()).Count > 0)
                            {
                                var invs = await Guild.GetInvitesAsync();
                                inv = invs.FirstOrDefault(x => x.MaxAge == null)?.Url ??
                                      channel.CreateInviteAsync(null).Result.Url;
                            }
                            else
                            {
                                inv = channel.CreateInviteAsync(null).Result.Url;
                            }

                            list.Add(new simpleserverobj
                            {
                                invite = inv,
                                servername = Guild.Name,
                                UserCount = Guild.Users.Count
                            });
                            break;
                        }
                        catch
                        {
                            //
                        }
                }
                catch
                {
                    //
                }

                i++;
                var i1 = i;
                await work.ModifyAsync(x => x.Content = $"Working {i1}/{Guilds.Count}");
            }

          
            await ReplyAsync("Main Done");
            var newlist = list.OrderByDescending(x => x.UserCount)
                .Select(x => $"`{x.servername}`[{x.UserCount}] - {x.invite}\n");
            var stringlist = new List<string>();
            var shortstring = "";
            foreach (var line in newlist)
            {
                shortstring += line;
                if (shortstring.Split('\n').Length > 20)
                {
                    stringlist.Add(shortstring);
                    shortstring = "";
                }
            }
            stringlist.Add(shortstring);
            await ReplyAsync("Second Done");
            var msg = new PaginatedMessage
            {
                Pages = stringlist,
                Title = "Server Browser",
                Color = Color.Green
            };

            await PagedReplyAsync(msg);
        }

        [Command("VerifyPatreon")]
        [Summary("VerifyPatreon <@user>")]
        [Remarks("Varify a supporter of the top ELO Bot Patreon Tier")]
        public async Task VerifyPatreon(IUser user)
        {
            if (CommandHandler.VerifiedUsers == null)
                CommandHandler.VerifiedUsers = new List<ulong> {user.Id};
            else
                CommandHandler.VerifiedUsers.Add(user.Id);

            await ReplyAsync("User has been verified.");

            var verifiedusers = JsonConvert.SerializeObject(CommandHandler.VerifiedUsers);
            File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "setup/verified.json"), verifiedusers);
            await ReplyAsync("Object Saved");

            foreach (var server in Context.Client.Guilds)
                try
                {
                    var patreon = server.GetUser(user.Id);
                    if (patreon != null)
                    {
                        var serverobject = Servers.ServerList.First(x => x.ServerId == server.Id);
                        var userprofile = serverobject.UserList.First(x => x.UserId == user.Id);
                        if (serverobject.UsernameSelection == 1)
                            await patreon.ModifyAsync(x =>
                            {
                                x.Nickname = $"👑{userprofile.Points} ~ {userprofile.Username}";
                            });
                        else if (serverobject.UsernameSelection == 2)
                            await patreon.ModifyAsync(x =>
                            {
                                x.Nickname = $"👑[{userprofile.Points}] {userprofile.Username}";
                            });
                        else if (serverobject.UsernameSelection == 3)
                            await patreon.ModifyAsync(x => { x.Nickname = $"👑{userprofile.Username}"; });
                    }
                }
                catch
                {
                    //
                }
        }
    }
}