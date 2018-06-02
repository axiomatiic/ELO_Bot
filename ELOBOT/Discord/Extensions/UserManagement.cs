using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using ELOBOT.Handlers;
using ELOBOT.Models;

namespace ELOBOT.Discord.Extensions
{
    public class UserManagement
    {
        public static async Task GiveMaxRole(Context.Context context, GuildModel.User User = null)
        {
            try
            {
                var maxrankpoints = context.Server.Ranks.Where(x => x.Threshhold <= (User?.Stats.Points ?? context.Elo.User.Stats.Points)).Max(x => x.Threshhold);
                var maxrank = context.Server.Ranks.FirstOrDefault(x => x.Threshhold == maxrankpoints);
                if (maxrank != null)
                {
                    var serverrole = context.Guild.GetRole(maxrank.RoleID);
                    if (serverrole != null)
                    {
                        try
                        {
                            await (context.User as IGuildUser).AddRoleAsync(serverrole);
                        }
                        catch
                        {
                            //Role Unavailable OR user unable to receive role due to permissions
                        }
                    }
                }
            }
            catch
            {
                //No applicable roles
            }
        }

        public static async Task UserRename(Context.Context context, GuildModel.User User = null)
        {
            if (User == null)
            {
                User = context.Elo.User;
            }

            var rename = context.Server.Settings.Registration.NameFormat.Replace("{score}", User.Stats.Points.ToString()).Replace("{username}", User.Username);

            try
            {
                await (context.User as IGuildUser).ModifyAsync(x => x.Nickname = rename);
            }
            catch
            {
                //Error renaming user (permissions above bot.)
            }
        }
    }
}
