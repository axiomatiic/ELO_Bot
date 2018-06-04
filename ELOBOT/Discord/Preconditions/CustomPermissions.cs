using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ELOBOT.Handlers;
using ELOBOT.Models;

namespace ELOBOT.Discord.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class CustomPermissions : PreconditionAttribute
    {
        private readonly bool DefaultAdminModule;
        private readonly bool DefaultModModule;

        public CustomPermissions(bool DefaultAdmin = false, bool DefaultModerator = false)
        {
            DefaultAdminModule = DefaultAdmin;
            DefaultModModule = DefaultModerator;
        }
        
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.Channel is IDMChannel) return Task.FromResult(PreconditionResult.FromError("This is a guild command"));
            
            if (context.Client.GetApplicationInfoAsync().Result.Owner.Id == context.User.Id || context.Guild.OwnerId == context.User.Id)
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }
            
            var server = DatabaseHandler.GetGuild(context.Guild.Id);
            if (server.Users.All(u => u.UserID != context.User.Id))
            {
                return Task.FromResult(PreconditionResult.FromError("User is not registered"));
            }

            var guser = context.User as IGuildUser;
            //At this point, all users are registered, not the server owner and not the bot owner

            if (server.Settings.CustomPermissions.CustomisedPermission.Any())
            {
                var match = server.Settings.CustomPermissions.CustomisedPermission.FirstOrDefault(x => string.Equals(command.Name, x.Name, StringComparison.CurrentCultureIgnoreCase));
                if (match != null)
                {
                    switch (match.Setting)
                    {
                        case GuildModel.GuildSettings._CommandAccess.CustomPermission.accesstype.Registered:
                            return Task.FromResult(PreconditionResult.FromSuccess());
                        case GuildModel.GuildSettings._CommandAccess.CustomPermission.accesstype.Moderator when guser.RoleIds.Any(x => server.Settings.Moderation.ModRoles.Contains(x)):
                            return Task.FromResult(PreconditionResult.FromSuccess());
                        case GuildModel.GuildSettings._CommandAccess.CustomPermission.accesstype.Moderator when guser.RoleIds.Any(x => server.Settings.Moderation.AdminRoles.Contains(x)):
                            return Task.FromResult(PreconditionResult.FromSuccess());
                        case GuildModel.GuildSettings._CommandAccess.CustomPermission.accesstype.Moderator:
                            return Task.FromResult(PreconditionResult.FromError("This is a server Moderator only command"));
                        case GuildModel.GuildSettings._CommandAccess.CustomPermission.accesstype.Admin when guser.RoleIds.Any(x => server.Settings.Moderation.AdminRoles.Contains(x)):
                            return Task.FromResult(PreconditionResult.FromSuccess());
                        case GuildModel.GuildSettings._CommandAccess.CustomPermission.accesstype.Admin:
                            return Task.FromResult(PreconditionResult.FromError("This is a server Admin only command"));
                        case GuildModel.GuildSettings._CommandAccess.CustomPermission.accesstype.ServerOwner:
                            return Task.FromResult(PreconditionResult.FromError("This is a server Owner only command"));
                    }
                }
                else
                {
                    if (!DefaultModModule && !DefaultAdminModule) return Task.FromResult(PreconditionResult.FromSuccess());
                    if (guser.RoleIds.Any(x => server.Settings.Moderation.ModRoles.Contains(x)) || guser.RoleIds.Any(x => server.Settings.Moderation.AdminRoles.Contains(x)) && DefaultModModule)
                    {
                        return Task.FromResult(PreconditionResult.FromSuccess());
                    }

                    if (guser.RoleIds.Any(x => server.Settings.Moderation.AdminRoles.Contains(x)) && DefaultAdminModule)
                    {
                        return Task.FromResult(PreconditionResult.FromSuccess());
                    }

                    return Task.FromResult(PreconditionResult.FromError($"This command is {(DefaultModModule ? "Moderator+" : "")}{(DefaultAdminModule ? "Admin+" : "")} Only"));

                }
            }
            else
            {
                if (!DefaultModModule && !DefaultAdminModule) return Task.FromResult(PreconditionResult.FromSuccess());
                if (guser.RoleIds.Any(x => server.Settings.Moderation.ModRoles.Contains(x)) || guser.RoleIds.Any(x => server.Settings.Moderation.AdminRoles.Contains(x)) && DefaultModModule)
                {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }

                if (guser.RoleIds.Any(x => server.Settings.Moderation.AdminRoles.Contains(x)) && DefaultAdminModule)
                {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }

                return Task.FromResult(PreconditionResult.FromError($"This command is {(DefaultModModule ? "Moderator+ " : "")}{(DefaultAdminModule ? "Admin+" : "")} Only"));

            }
            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
