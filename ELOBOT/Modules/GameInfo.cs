using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using ELOBOT.Discord.Context;
using ELOBOT.Discord.Extensions;
using ELOBOT.Discord.Preconditions;
using ELOBOT.Models;

namespace ELOBOT.Modules
{
    [CustomPermissions]
    public class GameInfo : Base
    {
        [CheckLobby]
        [Command("Comment")]
        public async Task Comment(int gamenumber, [Remainder] string comment)
        {
            var game = Context.Server.Results.FirstOrDefault(x => x.LobbyID == Context.Channel.Id && x.Gamenumber == gamenumber);
            if (game == null)
            {
                throw new Exception("Invalid Game number specified");
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                throw new Exception("Comment cannot be empty");
            }

            game.Comments.Add(new GuildModel.GameResult.Comment
            {
                ID = game.Comments.Count,
                CommenterID = Context.User.Id,
                Content = comment
            });
            Context.Server.Save();
            await SimpleEmbedAsync("Your comment has been saved.");
        }

        [Command("GameList")]
        public async Task GameList(ITextChannel Channel = null)
        {
            var Lobby = Context.Elo.Lobby;
            if (Channel == null && Lobby == null)
            {
                throw new Exception("You must invoke this command in a lobby or provide a lobby");
            }

            if (Channel != null)
            {
                Lobby = Context.Server.Lobbies.FirstOrDefault(x => x.ChannelID == Channel.Id);
                if (Lobby == null)
                {
                    throw new Exception("The provided channel is not a lobby");
                }
            }

            var LobbyResults = Context.Server.Results.Where(x => x.LobbyID == Lobby.ChannelID).OrderByDescending(x => x.Gamenumber).Select(x => $"#{x.Gamenumber} - {x.Result.ToString()}").ToList();
            var split = ListManagement.splitList(LobbyResults, 20);
            var pages = new List<PaginatedMessage.Page>();
            foreach (var section in split)
            {
                pages.Add(new PaginatedMessage.Page
                {
                    Description = string.Join("\n", section)
                });
            }

            var pager = new PaginatedMessage
            {
                Pages = pages,
                Title = $"{Context.Socket.Guild.GetChannel(Lobby.ChannelID)?.Name} GameList",
                Color = Color.Blue
            };

            await PagedReplyAsync(pager, new ReactionList
            {
                Forward = true,
                Backward = true,
                Trash = true
            });
        }

        [CheckLobby]
        [Command("ShowGame")]
        public async Task ShowGame(int GameNumber)
        {
            await ShowGame(Context.Channel as ITextChannel, GameNumber);
        }

        [Command("ShowGame")]
        public async Task ShowGame(ITextChannel Channel, int GameNumber)
        {
            var Lobby = Context.Server.Lobbies.FirstOrDefault(x => x.ChannelID == Channel.Id);
            if (Lobby == null)
            {
                throw new Exception("Invalid lobby specified.");
            }

            var game = Context.Server.Results.FirstOrDefault(x => x.LobbyID == Lobby.ChannelID && GameNumber == x.Gamenumber);

            if (game == null)
            {
                throw new Exception("Invalid Game number specified");
            }

            if (game.Comments.Any())
            {
                var pages = new List<PaginatedMessage.Page>
                {
                    new PaginatedMessage.Page
                    {
                        Fields = new List<EmbedFieldBuilder>
                        {
                            new EmbedFieldBuilder
                            {
                                Name = "Result",
                                Value = game.Result.ToString()
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "Team 1",
                                Value = string.Join(", ", game.Team1.Select(x => Context.Socket.Guild.GetUser(x)?.Mention ?? $"{Context.Server.Users.FirstOrDefault(u => u.UserID == x)?.Username ?? $"[{x}]"}"))
                            },
                            new EmbedFieldBuilder
                            {
                                Name = "Team 2",
                                Value = string.Join(", ", game.Team2.Select(x => Context.Socket.Guild.GetUser(x)?.Mention ?? $"{Context.Server.Users.FirstOrDefault(u => u.UserID == x)?.Username ?? $"[{x}]"}"))
                            }
                        }
                    }
                };
                foreach (var commentgroup in ListManagement.splitList(game.Comments, 5))
                {
                    var fields = commentgroup.OrderByDescending(x => x.ID).Select(x => new EmbedFieldBuilder
                    {
                        Name = $"`#{x.ID}` {Context.Socket.Guild.GetUser(x.CommenterID)?.Nickname ?? Context.Server.Users.FirstOrDefault(u => u.UserID == x.CommenterID)?.Username ?? $"[{x.CommenterID}]"}",
                        Value = x.Content
                    }).ToList();

                    pages.Add(new PaginatedMessage.Page
                    {
                        Title = $"{Channel.Name} Game #{GameNumber} Comments",
                        Fields = fields
                    });
                }

                var pager = new PaginatedMessage
                {
                    Pages = pages,
                    Title = $"{Channel.Name} Game #{GameNumber}",
                    Color = Color.Blue
                };

                await PagedReplyAsync(pager, new ReactionList
                {
                    Forward = true,
                    Backward = true,
                    Trash = true
                });
            }
            else
            {
                var embed = new EmbedBuilder
                {
                    Title = $"{Channel.Name} Game #{GameNumber}",
                    Color = Color.Magenta
                };
                embed.AddField("Result", game.Result.ToString());
                embed.AddField("Team 1", string.Join(", ", game.Team1.Select(x => Context.Socket.Guild.GetUser(x)?.Mention ?? $"{Context.Server.Users.FirstOrDefault(u => u.UserID == x)?.Username ?? $"[{x}]"}")));
                embed.AddField("Team 2", string.Join(", ", game.Team2.Select(x => Context.Socket.Guild.GetUser(x)?.Mention ?? $"{Context.Server.Users.FirstOrDefault(u => u.UserID == x)?.Username ?? $"[{x}]"}")));
                await ReplyAsync(embed);
            }
        }
    }
}