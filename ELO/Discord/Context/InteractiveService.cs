﻿namespace ELO.Discord.Context
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using global::Discord;
    using global::Discord.Addons.Interactive;
    using global::Discord.Commands;
    using global::Discord.WebSocket;

    /// <inheritdoc />
    /// <summary>
    ///     This is out own customized InteractiveService, giving support for DiscordShardedClient
    /// </summary>
    public class Interactive : IDisposable
    {
        private readonly Dictionary<ulong, IReactionCallback> _callbacks;
        private readonly TimeSpan _defaultTimeout;

        /// <summary>
        /// Sets our necessary things
        /// </summary>
        /// <param name="discord"></param>
        /// <param name="defaultTimeout"></param>
        public Interactive(DiscordShardedClient discord, TimeSpan? defaultTimeout = null)
        {
            Discord = discord;
            Discord.ReactionAdded += HandleReactionAsync;

            _callbacks = new Dictionary<ulong, IReactionCallback>();
            _defaultTimeout = defaultTimeout ?? TimeSpan.FromSeconds(15);
        }

        /// <summary>
        /// Gets the discordShardedclient
        /// </summary>
        public DiscordShardedClient Discord { get; }

        /// <summary>
        /// Disposes the reaction
        /// </summary>
        public void Dispose()
        {
            Discord.ReactionAdded -= HandleReactionAsync;
        }

        /// <summary>
        /// Waits for the next message
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fromSourceUser"></param>
        /// <param name="inSourceChannel"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public Task<SocketMessage> NextMessageAsync(SocketCommandContext context, bool fromSourceUser = true, bool inSourceChannel = true, TimeSpan? timeout = null)
        {
            var criterion = new Criteria<SocketMessage>();
            if (fromSourceUser)
                criterion.AddCriterion(new EnsureSourceUserCriterion());
            if (inSourceChannel)
                criterion.AddCriterion(new EnsureSourceChannelCriterion());
            return NextMessageAsync(context, criterion, timeout);
        }


        /// <summary>
        /// Waits for the next message to be sent
        /// </summary>
        /// <param name="context"></param>
        /// <param name="criterion"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public async Task<SocketMessage> NextMessageAsync(SocketCommandContext context, ICriterion<SocketMessage> criterion, TimeSpan? timeout = null)
        {
            timeout = timeout ?? _defaultTimeout;

            var eventTrigger = new TaskCompletionSource<SocketMessage>();

            async Task Handler(SocketMessage message)
            {
                var result = await criterion.JudgeAsync(context, message).ConfigureAwait(false);
                if (result)
                    eventTrigger.SetResult(message);
            }

            context.Client.MessageReceived += Handler;

            var trigger = eventTrigger.Task;
            var delay = Task.Delay(timeout.Value);
            var task = await Task.WhenAny(trigger, delay).ConfigureAwait(false);

            context.Client.MessageReceived -= Handler;

            if (task == trigger)
                return await trigger.ConfigureAwait(false);
            return null;
        }

        /// <summary>
        /// Sends a message with reaction callback
        /// </summary>
        /// <param name="context"></param>
        /// <param name="callbacks"></param>
        /// <param name="fromSourceUser"></param>
        /// <returns></returns>
        public async Task<IUserMessage> SendMessageWithReactionCallbacksAsync(SocketCommandContext context, ReactionCallbackData callbacks, bool fromSourceUser = true)
        {
            var criterion = new Criteria<SocketReaction>();
            if (fromSourceUser)
                criterion.AddCriterion(new EnsureReactionFromSourceUserCriterion());
            var callback = new InlineReactionCallback(new InteractiveService(Discord.GetShardFor(context.Guild)), context, callbacks, criterion);
            await callback.DisplayAsync().ConfigureAwait(false);
            return callback.Message;
        }

        /// <summary>
        /// Sends a message then deletes it
        /// </summary>
        /// <param name="context"></param>
        /// <param name="content"></param>
        /// <param name="isTTS"></param>
        /// <param name="embed"></param>
        /// <param name="timeout"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<IUserMessage> ReplyAndDeleteAsync(SocketCommandContext context, string content, bool isTTS = false, Embed embed = null, TimeSpan? timeout = null, RequestOptions options = null)
        {
            timeout = timeout ?? _defaultTimeout;
            var message = await context.Channel.SendMessageAsync(content, isTTS, embed, options).ConfigureAwait(false);
            _ = Task.Delay(timeout.Value)
                .ContinueWith(_ => message.DeleteAsync().ConfigureAwait(false))
                .ConfigureAwait(false);
            return message;
        }

        /// <summary>
        /// Sends a multi-pages message
        /// </summary>
        /// <param name="context"></param>
        /// <param name="pager"></param>
        /// <param name="Reactions"></param>
        /// <param name="criterion"></param>
        /// <returns></returns>
        public async Task<IUserMessage> SendPaginatedMessageAsync(SocketCommandContext context, PaginatedMessage pager, ReactionList Reactions, ICriterion<SocketReaction> criterion = null)
        {
            var callback = new PaginatedMessageCallback(new InteractiveService(Discord.GetShardFor(context.Guild)), context, pager, criterion);
            await callback.DisplayAsync(Reactions).ConfigureAwait(false);
            return callback.Message;
        }

        /// <summary>
        /// Adds reaction callback
        /// </summary>
        /// <param name="message"></param>
        /// <param name="callback"></param>
        public void AddReactionCallback(IMessage message, IReactionCallback callback)
        {
            _callbacks[message.Id] = callback;
        }

        /// <summary>
        /// Removes reaction callback
        /// </summary>
        /// <param name="message"></param>
        public void RemoveReactionCallback(IMessage message)
        {
            RemoveReactionCallback(message.Id);
        }

        /// <summary>
        /// Removes reaction callback
        /// </summary>
        /// <param name="id"></param>
        public void RemoveReactionCallback(ulong id)
        {
            _callbacks.Remove(id);
        }

        /// <summary>
        /// Clears all reaction callbacks
        /// </summary>
        public void ClearReactionCallbacks()
        {
            _callbacks.Clear();
        }

        private async Task HandleReactionAsync(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.UserId == Discord.CurrentUser.Id) return;
            if (!_callbacks.TryGetValue(message.Id, out var callback)) return;
            if (!await callback.Criterion.JudgeAsync(callback.Context, reaction).ConfigureAwait(false))
                return;
            switch (callback.RunMode)
            {
                case RunMode.Async:
                    _ = Task.Run(async () =>
                    {
                        if (await callback.HandleCallbackAsync(reaction).ConfigureAwait(false))
                            RemoveReactionCallback(message.Id);
                    });
                    break;
                default:
                    if (await callback.HandleCallbackAsync(reaction).ConfigureAwait(false))
                        RemoveReactionCallback(message.Id);
                    break;
            }
        }
    }
}