using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using ELOBOT.Discord.Context.Interactive.Criteria;

namespace ELOBOT.Discord.Context.Interactive.Callbacks
{
    public interface IReactionCallback
    {
        RunMode RunMode { get; }
        ICriterion<SocketReaction> Criterion { get; }
        TimeSpan? Timeout { get; }
        SocketCommandContext Context { get; }

        Task<bool> HandleCallbackAsync(SocketReaction reaction);
    }
}