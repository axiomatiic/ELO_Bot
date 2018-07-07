namespace ELO.Discord.Extensions
{
    using System.Linq;
    using System.Threading.Tasks;
    using global::Discord;

    using ELO.Discord.Context;
    using ELO.Models;

    public class UserManagement
    {
        public static GuildModel.Rank MaxRole(Context context, GuildModel.User user = null)
        {
            try
            {
                var maxRankPoints = context.Server.Ranks.Where(x => x.Threshold <= (user?.Stats.Points ?? context.Elo.User.Stats.Points)).Max(x => x.Threshold);
                var maxRank = context.Server.Ranks.FirstOrDefault(x => x.Threshold == maxRankPoints);
                return maxRank;
            }
            catch
            {
                return new GuildModel.Rank
                {
                    LossModifier = context.Server.Settings.Registration.DefaultLossModifier,
                    WinModifier = context.Server.Settings.Registration.DefaultWinModifier,
                    RoleID = 0,
                    Threshold = 0,
                    IsDefault = true
                };
            }
        }

        public static async Task GiveMaxRoleAsync(Context context, GuildModel.User user = null)
        {
            try
            {
                var maxRank = MaxRole(context, user);

                var serverRole = context.Guild.GetRole(maxRank.RoleID);
                if (serverRole != null)
                {
                    try
                    {
                        await (context.User as IGuildUser).AddRoleAsync(serverRole);
                    }
                    catch
                    {
                        // Role Unavailable OR user unable to receive role due to permissions
                    }
                }
            }
            catch
            {
                // No applicable roles
            }
        }

        public static async Task UserRenameAsync(Context context, GuildModel.User user = null)
        {
            if (user == null)
            {
                user = context.Elo.User;
            }

            var rename = context.Server.Settings.Registration.NameFormat.Replace("{score}", user.Stats.Points.ToString()).Replace("{username}", user.Username);

            try
            {
                await (context.User as IGuildUser).ModifyAsync(x => x.Nickname = rename);
            }
            catch
            {
                // Error renaming user (permissions above bot.)
            }
        }
    }
}