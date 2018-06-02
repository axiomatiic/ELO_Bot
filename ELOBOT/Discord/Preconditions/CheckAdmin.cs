using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace ELOBOT.Discord.Preconditions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CheckAdmin : PreconditionAttribute
    {
        /// <summary>
        ///     This will check wether or not a user has permissions to use a command/module
        /// </summary>
        /// <param name="context"></param>
        /// <param name="command"></param>
        /// <param name="services"></param>
        /// <returns></returns>
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var EloContext = new Context.Context(context.Client, context.Message, services);
            if (context.Guild.OwnerId == context.User.Id)
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }

            if (context.Client.GetApplicationInfoAsync().Result.Owner.Id == context.User.Id)
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }

            if ((context.User as IGuildUser).RoleIds.Any(x => EloContext.Server.Settings.Moderation.AdminRoles.Contains(x)))
            {
                return Task.FromResult(PreconditionResult.FromSuccess());
            }

            return Task.FromResult(PreconditionResult.FromError("User is Not an Admin!"));
        }
    }
}
