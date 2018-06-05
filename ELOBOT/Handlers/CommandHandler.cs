using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using ELOBOT.Discord.Context;
using ELOBOT.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ELOBOT.Handlers
{
    public class CommandHandler
    {
        public static int Commands;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        public IServiceProvider Provider;

        public CommandHandler(IServiceProvider provider)
        {
            Provider = provider;
            Config = ConfigModel.Load();
            _client = Provider.GetService<DiscordSocketClient>();
            _commands = new CommandService();

            _client.MessageReceived += _client_MessageReceived;
            _client.Ready += Client_Ready;
            _client.JoinedGuild += _client_JoinedGuild;
            _client.GuildMemberUpdated += _client_GuildMemberUpdated;
        }

        public static ConfigModel Config { get; set; } = ConfigModel.Load();

        private async Task _client_GuildMemberUpdated(SocketGuildUser UserBefore, SocketGuildUser UserAfter)
        {
            if (UserBefore.Status != UserAfter.Status)
            {
                if (UserAfter.Status != UserStatus.Online)
                {
                    var guildobject = DatabaseHandler.GetGuild(UserBefore.Guild.Id);
                    if (guildobject.Settings.GameSettings.RemoveOnAfk)
                    {
                        var lobbymatches = guildobject.Lobbies.Where(x => x.Game.QueuedPlayerIDs.Contains(UserAfter.Id) || x.Game.Team1.Players.Contains(UserAfter.Id) || x.Game.Team2.Players.Contains(UserAfter.Id)).ToList();
                        if (lobbymatches.Any())
                        {
                            foreach (var lobby in lobbymatches)
                            {
                                var lchannel = _client.GetChannel(lobby.ChannelID) as ISocketMessageChannel;
                                if (lobby.Game.IsPickingTeams)
                                {
                                    await lchannel.SendMessageAsync($"{UserAfter.Mention} has gone {UserAfter.Status.ToString()}, but this lobby is currently picking teams. If they are inactive it is suggested that you clear the queue");
                                }
                                else
                                {
                                    lobby.Game.QueuedPlayerIDs.Remove(UserAfter.Id);
                                    await lchannel.SendMessageAsync($"{UserAfter.Mention} has gone {UserAfter.Status.ToString()} and has been automatically removed from the queue");
                                }
                            }

                            guildobject.Save();
                        }
                    }
                }
            }
        }

        private static Task _client_JoinedGuild(SocketGuild Guild)
        {
            if (DatabaseHandler.GetGuild(Guild.Id) == null)
            {
                DatabaseHandler.InsertGuildObject(new GuildModel
                {
                    ID = Guild.Id
                });
            }

            return Task.CompletedTask;
        }

        private async Task Client_Ready()
        {
            var application = await _client.GetApplicationInfoAsync();
            LogHandler.LogMessage($"Invite: https://discordapp.com/oauth2/authorize?client_id={application.Id}&scope=bot&permissions=2146958591");

            await DatabaseHandler.DatabaseInitialise(_client);

            var fullconfig = DatabaseHandler.GetFullConfig();
            var initialised = 0;

            foreach (var guild in _client.Guilds.Where(g => fullconfig.All(cfg => cfg.ID != g.Id)))
            {
                DatabaseHandler.InsertGuildObject(new GuildModel
                {
                    ID = guild.Id
                });
                initialised++;
            }

            LogHandler.LogMessage($"{initialised} Guilds Initialised");
        }

        private async Task _client_MessageReceived(SocketMessage SocketMessage)
        {
            //Check to ensure that we are only receiving valid messages from users only!
            if (!(SocketMessage is SocketUserMessage message)) return;
            var context = new Context(_client, message, Provider);
            if (context.User.IsBot) return;

            var argPos = 0;
            //Ensure that we filter out all messages that do not start with the bot prefix
            if (context.Channel is IDMChannel)
            {
                if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasStringPrefix(Config.Prefix, ref argPos))) return;
            }
            else
            {
                if (!(message.HasMentionPrefix(_client.CurrentUser, ref argPos) || message.HasStringPrefix(Config.Prefix, ref argPos) ||
                      context.Server.Settings.CustomPrefix != null && message.HasStringPrefix(context.Server.Settings.CustomPrefix, ref argPos)))
                {
                    return;
                }
            }

            if (context.Elo.User != null && context.Elo.User.Banned.Banned)
            {
                if (context.Elo.User.Banned.ExpiryTime < DateTime.UtcNow)
                {
                    await context.Channel.SendMessageAsync("", false, new EmbedBuilder
                    {
                        Description = $"{context.User.Mention} Your ban has expired.\n" +
                                      $"Reason: {context.Elo.User.Banned.Reason}\n" +
                                      $"Moderator: {context.Socket.Guild.GetUser(context.Elo.User.Banned.Moderator)?.Mention ?? $"[{context.Elo.User.Banned.Moderator}]"}",
                        Color = Color.DarkOrange
                    }.Build());
                    context.Elo.User.Banned = new GuildModel.User.Ban();
                    context.Server.Save();
                }
            }

            var result = await _commands.ExecuteAsync(context, argPos, Provider);
            if (result.IsSuccess)
            {
                LogHandler.LogMessage(context);
            }
            else
            {
                string ErrorMessage;
                if (result.Error == CommandError.UnknownCommand)
                {
                    ErrorMessage = "**Command:** N/A";
                }
                else
                {
                    var srch = _commands.Search(context, argPos);
                    var cmd = srch.Commands.FirstOrDefault();

                    ErrorMessage = $"**Command Name:** `{cmd.Command.Name}`\n" +
                                   $"**Summary:** `{cmd.Command?.Summary ?? "N/A"}`\n" +
                                   $"**Remarks:** `{cmd.Command?.Remarks ?? "N/A"}`\n" +
                                   $"**Aliases:** {(cmd.Command.Aliases.Any() ? string.Join(" ", cmd.Command.Aliases.Select(x => $"`{x}`")) : "N/A")}\n" +
                                   $"**Parameters:** {(cmd.Command.Parameters.Any() ? string.Join(" ", cmd.Command.Parameters.Select(x => x.IsOptional ? $" `<(Optional){x.Name}>` " : $" `<{x.Name}>` ")) : "N/A")}\n" +
                                   "**Error Reason**\n" +
                                   $"{result.ErrorReason}";
                }

                try
                {
                    await context.Channel.SendMessageAsync("", false, new EmbedBuilder
                    {
                        Title = $"{context.User.Username.ToUpper()} ERROR",
                        Description = ErrorMessage
                    }.Build());
                }
                catch
                {
                    //
                }

                LogHandler.LogMessage(context, result.ErrorReason, LogSeverity.Error);
            }
        }

        public async Task ConfigureAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }
    }
}