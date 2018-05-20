using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace ELO_Bot.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class CheckAccessList : PreconditionAttribute
    {
        private readonly bool DefaultAdminModule;
        private readonly bool DefaultModModule;

        public CheckAccessList(bool AllowAdminPermission = false, bool allowAdministratorRole = false)
        {
            DefaultAdminModule = AllowAdminPermission;
            DefaultModModule = allowAdministratorRole;
        }

        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command,
            IServiceProvider prov)
        {
            try
            {
                var server = Servers.ServerList.First(x => x.ServerId == context.Guild.Id);
                var own = await context.Client.GetApplicationInfoAsync();


                
                if (own.Owner.Id == context.User.Id)
                    return await Task.FromResult(PreconditionResult.FromSuccess());
                if (context.Guild.OwnerId == context.User.Id)
                    return await Task.FromResult(PreconditionResult.FromSuccess());
                    
                var bl = server.moduleConfig.DisabledTypes;

                if (bl.Any())
                {
                    var blacklisted = false;
                    Servers.Server.ModuleConfig.DisabledType returntype = null;
                    foreach (var type in bl)
                    {
                        if (type.IsCommand)
                        {
                            if (string.Equals(type.Name, command.Name, StringComparison.CurrentCultureIgnoreCase))
                            {
                                blacklisted = true;
                            }
                        }
                        else
                        {
                            if (string.Equals(type.Name, command.Module.Name, StringComparison.CurrentCultureIgnoreCase))
                            {
                                blacklisted = true;
                            }
                        }

                        if (!blacklisted) continue;
                        returntype = type;
                        break;
                    }

                    if (returntype != null)
                    {
                        if (returntype.Setting.ServerOwnerOnly)
                        {
                            if (context.Guild.OwnerId == context.User.Id)
                            {
                                return await Task.FromResult(PreconditionResult.FromSuccess());
                            }

                            return await Task.FromResult(
                                PreconditionResult.FromError(
                                    "This Command/Module is server owner only!"));
                        }

                        if (returntype.Setting.AdminAllowed)
                        {
                            if (server.AdminRole != 0)
                                if (((IGuildUser)context.User).RoleIds.Contains(server.AdminRole))
                                    return await Task.FromResult(PreconditionResult.FromSuccess());
                        }
                        if (returntype.Setting.ModAllowed)
                        {
                            if (server.ModRole != 0)
                                if (((IGuildUser)context.User).RoleIds.Contains(server.ModRole))
                                    return await Task.FromResult(PreconditionResult.FromSuccess());
                        }

                        if (returntype.Setting.RegisteredAllowed)
                        {
                            if (server.UserList.Any(x => x.UserId == context.User.Id))
                            {
                                return await Task.FromResult(PreconditionResult.FromSuccess());
                            }
                        }
                        if (returntype.Setting.UnRegisteredAllowed)
                        {
                            return await Task.FromResult(PreconditionResult.FromSuccess());
                        }

                        if (DefaultAdminModule)
                        {
                            if (!bl.Any(x => string.Equals(x.Name, command.Module.Name, StringComparison.CurrentCultureIgnoreCase)) || !bl.Any(x => string.Equals(x.Name, command.Name, StringComparison.CurrentCultureIgnoreCase)))
                            {
                                if (server.AdminRole != 0)
                                    if (((IGuildUser)context.User).RoleIds.Contains(server.AdminRole))
                                        return await Task.FromResult(PreconditionResult.FromSuccess());

                                if (!(((IGuildUser)context.User).GuildPermissions.Administrator ||
                                      context.User.Id == context.Guild.OwnerId))
                                return await Task.FromResult(
                                    PreconditionResult.FromError(
                                        "This Command/Module requires admin permissions."));
                                return await Task.FromResult(PreconditionResult.FromSuccess());
                            }
                        }

                        if (DefaultModModule)
                        {
                            if (!bl.Any(x => string.Equals(x.Name, command.Module.Name, StringComparison.CurrentCultureIgnoreCase)) || !bl.Any(x => string.Equals(x.Name, command.Name, StringComparison.CurrentCultureIgnoreCase)))
                            {
                                if (server.ModRole != 0)
                                    if (((IGuildUser)context.User).RoleIds.Contains(server.ModRole))
                                        return await Task.FromResult(PreconditionResult.FromSuccess());

                                if (server.AdminRole != 0)
                                    if (((IGuildUser)context.User).RoleIds.Contains(server.AdminRole))
                                        return await Task.FromResult(PreconditionResult.FromSuccess());

                                if (!(((IGuildUser) context.User).GuildPermissions.Administrator ||
                                      context.User.Id == context.Guild.OwnerId))
                                return await Task.FromResult(
                                    PreconditionResult.FromError(
                                        "This Command/Module requires Moderator OR Admin permissions."));
                                return await Task.FromResult(PreconditionResult.FromSuccess());
                            }
                        }

                        return await Task.FromResult(PreconditionResult.FromError("This is a Blacklisted Command/Module."));
                    }
                    else
                    {
                        if (DefaultAdminModule)
                        {
                            if (!bl.Any(x => string.Equals(x.Name, command.Module.Name, StringComparison.CurrentCultureIgnoreCase)) || !bl.Any(x => string.Equals(x.Name, command.Name, StringComparison.CurrentCultureIgnoreCase)))
                            {
                                if (server.AdminRole != 0)
                                    if (((IGuildUser)context.User).RoleIds.Contains(server.AdminRole))
                                        return await Task.FromResult(PreconditionResult.FromSuccess());

                                if (!(((IGuildUser)context.User).GuildPermissions.Administrator ||
                                      context.User.Id == context.Guild.OwnerId))
                                return await Task.FromResult(
                                    PreconditionResult.FromError(
                                        "This Command/Module requires admin permissions."));
                                return await Task.FromResult(PreconditionResult.FromSuccess());
                            }
                        }

                        if (DefaultModModule)
                        {
                            if (!bl.Any(x => string.Equals(x.Name, command.Module.Name, StringComparison.CurrentCultureIgnoreCase)) || !bl.Any(x => string.Equals(x.Name, command.Name, StringComparison.CurrentCultureIgnoreCase)))
                            {
                                if (server.ModRole != 0)
                                    if (((IGuildUser)context.User).RoleIds.Contains(server.ModRole))
                                        return await Task.FromResult(PreconditionResult.FromSuccess());

                                if (server.AdminRole != 0)
                                    if (((IGuildUser)context.User).RoleIds.Contains(server.AdminRole))
                                        return await Task.FromResult(PreconditionResult.FromSuccess());

                                if (!(((IGuildUser) context.User).GuildPermissions.Administrator ||
                                      context.User.Id == context.Guild.OwnerId))
                                return await Task.FromResult(
                                    PreconditionResult.FromError(
                                        "This Command/Module requires Moderator OR Admin permissions."));
                                return await Task.FromResult(PreconditionResult.FromSuccess());
                            }
                        }
                    }
                }
                else
                {
                    if (DefaultAdminModule)
                    {
                        if (server.AdminRole != 0)
                            if (((IGuildUser)context.User).RoleIds.Contains(server.AdminRole))
                                return await Task.FromResult(PreconditionResult.FromSuccess());

                        if (!(((IGuildUser)context.User).GuildPermissions.Administrator ||
                              context.User.Id == context.Guild.OwnerId))
                            return await Task.FromResult(
                                PreconditionResult.FromError(
                                    "This Command/Module requires admin permissions."));

                        return await Task.FromResult(PreconditionResult.FromSuccess());
                    }

                    if (DefaultModModule)
                    {
                        if (server.ModRole != 0)
                            if (((IGuildUser)context.User).RoleIds.Contains(server.ModRole))
                                return await Task.FromResult(PreconditionResult.FromSuccess());

                        if (server.AdminRole != 0)
                            if (((IGuildUser)context.User).RoleIds.Contains(server.AdminRole))
                                return await Task.FromResult(PreconditionResult.FromSuccess());

                        if (!(((IGuildUser)context.User).GuildPermissions.Administrator ||
                              context.User.Id == context.Guild.OwnerId))
                            return await Task.FromResult(
                                PreconditionResult.FromError(
                                    "This Command/Module requires Moderator OR Admin permissions."));

                        return await Task.FromResult(PreconditionResult.FromSuccess());
                    }
                }


                return await Task.FromResult(PreconditionResult.FromSuccess());
            }
            catch
            {
                return await Task.FromResult(PreconditionResult.FromSuccess());
            }
        }
    }
}