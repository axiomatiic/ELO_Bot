using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ELOBOT.Discord.Context;
using ELOBOT.Discord.Preconditions;
using ELOBOT.Handlers;
using ELOBOT.Models;

namespace ELOBOT.Modules
{
    [RequireContext(ContextType.Guild)]
    public class Info : Base
    {
        [Command("Register")]
        public async Task Register()
        {
            await Register(Context.User.Username);
        }
        [Command("Register")]
        public async Task Register([Remainder]string name)
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
                await Discord.Extensions.UserManagement.GiveMaxRole(Context, NewUser);
            }

            await Discord.Extensions.UserManagement.UserRename(Context, NewUser);
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
    }
}
