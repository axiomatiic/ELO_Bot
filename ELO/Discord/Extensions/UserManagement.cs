namespace ELO.Discord.Extensions
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using global::Discord;

    using ELO.Discord.Context;
    using ELO.Handlers;
    using ELO.Models;

    public class UserManagement
    {
        public static GuildModel.Rank MaxRole(Context context, GuildModel.User user = null)
        {
            try
            {
                var maxRankPoints = context.Server.Ranks.Where(x => x.Threshold <= (user?.Stats.Points ?? context.Elo.User.Stats.Points)).Max(x => x.Threshold);
                var maxRank = context.Server.Ranks.FirstOrDefault(x => x.Threshold == maxRankPoints);
                maxRank.LossModifier = maxRank.LossModifier == 0 ? context.Server.Settings.Registration.DefaultLossModifier : maxRank.LossModifier;
                maxRank.WinModifier = maxRank.WinModifier == 0 ? context.Server.Settings.Registration.DefaultWinModifier : maxRank.WinModifier;
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
                if (user == null)
                {
                    user = context.Elo.User;
                }

                var maxRank = MaxRole(context, user);

                var serverRole = context.Guild.GetRole(maxRank.RoleID);
                if (serverRole != null)
                {
                    try
                    {
                        var gUser = context.Guild.GetUser(user.UserID);
                        if (gUser == null)
                        {
                            return;
                        }

                        if (gUser.Roles.Any(x => x.Id == serverRole.Id))
                        {
                            // Return if the user already has the role
                            return;
                        }

                        await gUser.AddRoleAsync(serverRole);
                    }
                    catch (Exception e)
                    {
                        // Role Unavailable OR user unable to receive role due to permissions
                        LogHandler.LogMessage(e.ToString(), LogSeverity.Error);
                    }
                }
            }
            catch (Exception e)
            {
                // No applicable roles
                LogHandler.LogMessage(e.ToString(), LogSeverity.Error);
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
                var gUser = context.Guild.GetUser(user.UserID);
                if (gUser == null)
                {
                    return;
                }

                if (gUser.Nickname == rename)
                {
                    return;
                }

                if (gUser.Id == context.Guild.OwnerId)
                {
                    return;
                }

                if (gUser.Roles.Max(x => x.Position) >= context.Guild.CurrentUser.Roles.Max(x => x.Position))
                {
                    return;
                }

                await gUser.ModifyAsync(x => x.Nickname = rename);
            }
            catch (Exception e)
            {
                // Error renaming user (permissions above bot.)
                LogHandler.LogMessage(e.ToString(), LogSeverity.Error);
            }
        }
    }
}