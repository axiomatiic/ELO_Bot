using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace ELO_Bot.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class ServerOwner : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command,
            IServiceProvider prov)
        {
            var own = await context.Client.GetApplicationInfoAsync();
            if (own.Owner.Id == context.User.Id)
                return await Task.FromResult(PreconditionResult.FromSuccess());

            if (context.Guild.OwnerId == context.User.Id)
                return await Task.FromResult(PreconditionResult.FromSuccess());

            return await Task.FromResult(
                PreconditionResult.FromError(
                    "This Command can only be performed by the server owner"));
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class BotOwner : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command,
            IServiceProvider prov)
        {
            var own = await context.Client.GetApplicationInfoAsync();
            if (own.Owner.Id == context.User.Id || context.User.Id == 290763860179156993)
                return await Task.FromResult(PreconditionResult.FromSuccess());

            return await Task.FromResult(
                PreconditionResult.FromError(
                    "This Command can only be performed by the Bot Owner"));
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class CheckRegistered : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command,
            IServiceProvider prov)
        {
            var s1 = Servers.ServerList.First(x => x.ServerId == context.Guild.Id);

            try
            {
                if (s1.UserList.FirstOrDefault(x => x.UserId == context.User.Id) != null)
                    return await Task.FromResult(PreconditionResult.FromSuccess());

                return await Task.FromResult(
                    PreconditionResult.FromError(
                        "You are not registered, type `=register <name>` to begin"));
            }
            catch
            {
                return await Task.FromResult(
                    PreconditionResult.FromError(
                        "You are not registered, type `=register <name>` to begin"));
            }
        }
    }
}