using System.Linq;
using System.Threading.Tasks;
using Discord;
using ELOBOT.Models;

namespace ELOBOT.Discord.Extensions
{
    public class UserManagement
    {
        public static GuildModel.Rank MaxRole(Context.Context Context, GuildModel.User User = null)
        {
            try
            {
                var maxrankpoints = Context.Server.Ranks.Where(x => x.Threshhold <= (User?.Stats.Points ?? Context.Elo.User.Stats.Points)).Max(x => x.Threshhold);
                var maxrank = Context.Server.Ranks.FirstOrDefault(x => x.Threshhold == maxrankpoints);
                return maxrank;
            }
            catch
            {
                return new GuildModel.Rank
                {
                    LossModifier = Context.Server.Settings.Registration.DefaultLossModifier,
                    WinModifier = Context.Server.Settings.Registration.DefaultWinModifier,
                    RoleID = 0,
                    Threshhold = 0,
                    IsDefault = true
                };
            }
        }

        public static async Task GiveMaxRole(Context.Context context, GuildModel.User User = null)
        {
            try
            {
                var maxrank = MaxRole(context, User);

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