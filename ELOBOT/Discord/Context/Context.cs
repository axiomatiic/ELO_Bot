using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using ELOBOT.Handlers;
using ELOBOT.Models;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace ELOBOT.Discord.Context
{
    public abstract class Base : ModuleBase<Context>
    {
        public InteractiveService Interactive { get; set; }


        /// <summary>
        ///     Reply in the server. This is a shortcut for context.channel.sendmessageasync
        /// </summary>
        public async Task<IUserMessage> ReplyAsync(string Message, Embed Embed = null)
        {
            await Context.Channel.TriggerTypingAsync();
            return await base.ReplyAsync(Message, false, Embed);
        }

        /// <summary>
        ///     Shorthand for  replying with just an embed
        /// </summary>
        public async Task<IUserMessage> ReplyAsync(EmbedBuilder embed)
        {
            return await base.ReplyAsync("", false, embed.Build());
        }

        public async Task<IUserMessage> ReplyAsync(Embed embed)
        {
            return await base.ReplyAsync("", false, embed);
        }


        /// <summary>
        ///     Reply in the server and then delete after the provided delay.
        /// </summary>
        public async Task<IUserMessage> ReplyAndDeleteAsync(string Message, TimeSpan? Timeout = null)
        {
            Timeout = Timeout ?? TimeSpan.FromSeconds(5);
            var Msg = await ReplyAsync(Message).ConfigureAwait(false);
            _ = Task.Delay(Timeout.Value).ContinueWith(_ => Msg.DeleteAsync().ConfigureAwait(false)).ConfigureAwait(false);
            return Msg;
        }

        public Task<IUserMessage> InlineReactionReplyAsync(ReactionCallbackData data, bool fromSourceUser = true)
            => Interactive.SendMessageWithReactionCallbacksAsync(SocketContext(), data, fromSourceUser);

        public async Task<IUserMessage> SimpleEmbedAsync(string message)
        {
            var embed = new EmbedBuilder
            {
                Description = message,
                Color = Color.DarkOrange
            };
            return await base.ReplyAsync("", false, embed.Build());
        }

        private SocketCommandContext SocketContext()
        {
            return new SocketCommandContext(Context.Client as DiscordSocketClient, Context.Message as SocketUserMessage);
        }

        public Task<IUserMessage> PagedReplyAsync(PaginatedMessage pager, ReactionList Reactions, bool fromSourceUser = true)
        {
            var criterion = new Criteria<SocketReaction>();
            if (fromSourceUser)
            {
                criterion.AddCriterion(new EnsureReactionFromSourceUserCriterion());
            }

            return PagedReplyAsync(pager, criterion, Reactions);
        }

        public Task<IUserMessage> PagedReplyAsync(PaginatedMessage pager, ICriterion<SocketReaction> criterion, ReactionList Reactions)
        {
            return Interactive.SendPaginatedMessageAsync(SocketContext(), pager, Reactions, criterion);
        }

        public Task<SocketMessage> NextMessageAsync(ICriterion<SocketMessage> criterion, TimeSpan? timeout = null)
        {
            return Interactive.NextMessageAsync(SocketContext(), criterion, timeout);
        }

        public Task<SocketMessage> NextMessageAsync(bool fromSourceUser = true, bool inSourceChannel = true,
            TimeSpan? timeout = null)
        {
            return Interactive.NextMessageAsync(SocketContext(), fromSourceUser, inSourceChannel, timeout);
        }

        public Task<IUserMessage> ReplyAndDeleteAsync(string content, bool isTTS = false, Embed embed = null,
            TimeSpan? timeout = null, RequestOptions options = null)
        {
            return Interactive.ReplyAndDeleteAsync(SocketContext(), content, isTTS, embed, timeout, options);
        }
    }

    public class Context : ICommandContext
    {
        public Context(IDiscordClient ClientParam, IUserMessage MessageParam, IServiceProvider ServiceProvider)
        {
            Client = ClientParam;
            Message = MessageParam;
            User = MessageParam.Author;
            Channel = MessageParam.Channel;
            Guild = MessageParam.Channel is IDMChannel ? null : (MessageParam.Channel as IGuildChannel).Guild;

            //This is a shorthand conversion for our context, giving access to socket context stuff without the need to cast within out commands
            Socket = new SocketContext
            {
                Guild = Guild as SocketGuild,
                User = User as SocketUser,
                Client = Client as DiscordSocketClient,
                Message = Message as SocketUserMessage,
                Channel = Channel as ISocketMessageChannel
            };

            //These are our custom additions to the context, giving access to the server object and all server objects through Context.
            //Server = Channel is IDMChannel ? null : DatabaseHandler.GetGuild(Guild.Id);


            Session = ServiceProvider.GetRequiredService<IDocumentStore>().OpenSession();
            Server = Session.Load<GuildModel>(Guild.Id.ToString());
            Elo = new ServerContext
            {
                User = Server?.Users?.FirstOrDefault(x => x.UserID == User.Id),
                Lobby = Server?.Lobbies?.FirstOrDefault(x => x.ChannelID == Channel.Id)
            };
            Prefix = CommandHandler.Config.Prefix;
        }

        public GuildModel Server { get; }
        public IDocumentSession Session { get; }
        public SocketContext Socket { get; }
        public string Prefix { get; }
        public ServerContext Elo { get; set; }
        public IUser User { get; }
        public IGuild Guild { get; }
        public IDiscordClient Client { get; }
        public IUserMessage Message { get; }
        public IMessageChannel Channel { get; }

        public class ServerContext
        {
            public GuildModel.User User { get; set; }
            public GuildModel.Lobby Lobby { get; set; }
        }


        public class SocketContext
        {
            public SocketUser User { get; set; }
            public SocketGuild Guild { get; set; }
            public DiscordSocketClient Client { get; set; }
            public SocketUserMessage Message { get; set; }
            public ISocketMessageChannel Channel { get; set; }
        }
    }
}