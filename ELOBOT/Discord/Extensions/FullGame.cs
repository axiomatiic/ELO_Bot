using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using ELOBOT.Handlers;
using ELOBOT.Models;

namespace ELOBOT.Discord.Extensions
{
    public class FullGame
    {
        public static async Task FullQueue(Context.Context Context)
        {
            if (Context.Elo.Lobby.Game.QueuedPlayerIDs.Count >= Context.Elo.Lobby.UserLimit)
            {
                var users = Context.Elo.Lobby.Game.QueuedPlayerIDs.Select(qu => Context.Socket.Guild.GetUser(qu)).ToList();
                if (users.Any(x => x == null))
                {
                    Context.Elo.Lobby.Game = new GuildModel.Lobby.CurrentGame();
                    Context.Server.Save();
                    await Context.Channel.SendMessageAsync("Game Aborted, Missing Player in queue");
                    return;
                }

                if (Context.Elo.Lobby.PickMode == GuildModel.Lobby._PickMode.CompleteRandom)
                {
                    var Shuffled = Context.Elo.Lobby.Game.QueuedPlayerIDs.OrderBy(x => new Random().Next()).ToList();
                    Context.Elo.Lobby.Game.Team1.Players = Shuffled.Take(Context.Elo.Lobby.UserLimit / 2).ToList();
                    Context.Elo.Lobby.Game.Team2.Players = Shuffled.Skip(Context.Elo.Lobby.UserLimit / 2).Take(Context.Elo.Lobby.UserLimit / 2).ToList();
                }
                else if (Context.Elo.Lobby.PickMode == GuildModel.Lobby._PickMode.SortByScore)
                {
                    var Ordered = Context.Elo.Lobby.Game.QueuedPlayerIDs.Select(x => Context.Server.Users.First(u => x == u.UserID)).OrderByDescending(x => x.Stats.Points).ToList();
                    foreach (var user in Ordered)
                    {
                        if (Context.Elo.Lobby.Game.Team1.Players.Count > Context.Elo.Lobby.Game.Team2.Players.Count)
                            Context.Elo.Lobby.Game.Team2.Players.Add(user.UserID);
                        else
                            Context.Elo.Lobby.Game.Team1.Players.Add(user.UserID);
                    }
                }
                else if (Context.Elo.Lobby.PickMode == GuildModel.Lobby._PickMode.Captains)
                {
                    IUser cap1;
                    IUser cap2;
                    var userlist = Context.Elo.Lobby.Game.QueuedPlayerIDs.Select(x => Context.Server.Users.First(u => u.UserID == x)).ToList();
                    var rnd = new Random();
                    if (Context.Elo.Lobby.CaptainSortMode == GuildModel.Lobby.CaptainSort.MostPoints)
                    {
                        var CSorted = userlist.OrderByDescending(x => x.Stats.Points).ToList();
                        cap1 = await Context.Guild.GetUserAsync(CSorted[0].UserID);
                        cap2 = await Context.Guild.GetUserAsync(CSorted[1].UserID);
                    }
                    else if (Context.Elo.Lobby.CaptainSortMode == GuildModel.Lobby.CaptainSort.MostWins)
                    {
                        var CSorted = userlist.OrderByDescending(x => x.Stats.Wins).ToList();
                        cap1 = await Context.Guild.GetUserAsync(CSorted[0].UserID);
                        cap2 = await Context.Guild.GetUserAsync(CSorted[1].UserID);
                    }
                    else if (Context.Elo.Lobby.CaptainSortMode == GuildModel.Lobby.CaptainSort.HighestWinLoss)
                    {
                        var CSorted = userlist.OrderByDescending(x => (double) x.Stats.Wins / (x.Stats.Losses == 0 ? 1 : x.Stats.Losses)).ToList();
                        cap1 = await Context.Guild.GetUserAsync(CSorted[0].UserID);
                        cap2 = await Context.Guild.GetUserAsync(CSorted[1].UserID);
                    }
                    else if (Context.Elo.Lobby.CaptainSortMode == GuildModel.Lobby.CaptainSort.Random)
                    {
                        var CSorted = userlist.OrderByDescending(x => rnd.Next()).ToList();
                        cap1 = await Context.Guild.GetUserAsync(CSorted[0].UserID);
                        cap2 = await Context.Guild.GetUserAsync(CSorted[1].UserID);
                    }
                    else if (Context.Elo.Lobby.CaptainSortMode == GuildModel.Lobby.CaptainSort.RandomTop4MostPoints)
                    {
                        if (userlist.Count >= 4)
                        {
                            var CSorted = userlist.OrderByDescending(x => x.Stats.Points).Take(4).OrderByDescending(x => rnd.Next()).ToList();
                            cap1 = await Context.Guild.GetUserAsync(CSorted[0].UserID);
                            cap2 = await Context.Guild.GetUserAsync(CSorted[1].UserID);
                        }
                        else
                        {
                            var CSorted = userlist.OrderByDescending(x => x.Stats.Points).ToList();
                            cap1 = await Context.Guild.GetUserAsync(CSorted[0].UserID);
                            cap2 = await Context.Guild.GetUserAsync(CSorted[1].UserID);
                        }
                    }
                    else if (Context.Elo.Lobby.CaptainSortMode == GuildModel.Lobby.CaptainSort.RandomTop4MostWins)
                    {
                        if (userlist.Count >= 4)
                        {
                            var CSorted = userlist.OrderByDescending(x => x.Stats.Wins).Take(4).OrderByDescending(x => rnd.Next()).ToList();
                            cap1 = await Context.Guild.GetUserAsync(CSorted[0].UserID);
                            cap2 = await Context.Guild.GetUserAsync(CSorted[1].UserID);
                        }
                        else
                        {
                            var CSorted = userlist.OrderByDescending(x => x.Stats.Wins).ToList();
                            cap1 = await Context.Guild.GetUserAsync(CSorted[0].UserID);
                            cap2 = await Context.Guild.GetUserAsync(CSorted[1].UserID);
                        }
                    }
                    else if (Context.Elo.Lobby.CaptainSortMode == GuildModel.Lobby.CaptainSort.RandomTop4HighestWinLoss)
                    {
                        if (userlist.Count >= 4)
                        {
                            var CSorted = userlist.OrderByDescending(x => (double) x.Stats.Wins / (x.Stats.Losses == 0 ? 1 : x.Stats.Losses)).Take(4).OrderByDescending(x => rnd.Next()).ToList();
                            cap1 = await Context.Guild.GetUserAsync(CSorted[0].UserID);
                            cap2 = await Context.Guild.GetUserAsync(CSorted[1].UserID);
                        }
                        else
                        {
                            var CSorted = userlist.OrderByDescending(x => (double) x.Stats.Wins / (x.Stats.Losses == 0 ? 1 : x.Stats.Losses)).ToList();
                            cap1 = await Context.Guild.GetUserAsync(CSorted[0].UserID);
                            cap2 = await Context.Guild.GetUserAsync(CSorted[1].UserID);
                        }
                    }
                    else
                    {
                        var CSorted = userlist.OrderByDescending(x => rnd.Next()).ToList();
                        cap1 = await Context.Guild.GetUserAsync(CSorted[0].UserID);
                        cap2 = await Context.Guild.GetUserAsync(CSorted[1].UserID);
                    }

                    Context.Elo.Lobby.Game.Team1.Captain = cap1.Id;
                    Context.Elo.Lobby.Game.Team2.Captain = cap2.Id;
                    Context.Elo.Lobby.Game.Team1.Players.Add(cap1.Id);
                    Context.Elo.Lobby.Game.Team2.Players.Add(cap2.Id);
                    Context.Elo.Lobby.Game.QueuedPlayerIDs.Remove(cap1.Id);
                    Context.Elo.Lobby.Game.QueuedPlayerIDs.Remove(cap2.Id);
                    await Context.Channel.SendMessageAsync($"**Team1 Captain** {cap1.Mention}\n" +
                                                           $"**Team2 Captain** {cap2.Mention}\n" +
                                                           $"**Select Your Teams using `{Context.Prefix}pick <@user>`**\n" +
                                                           "**Captain 1 Always Picks First**\n" +
                                                           "**Player Pool**\n" +
                                                           $"{string.Join(" ", users.Select(x => x.Mention))}");
                    Context.Elo.Lobby.Game.IsPickingTeams = true;
                    Context.Elo.Lobby.Game.Team1.TurnToPick = true;
                    Context.Elo.Lobby.Game.Team2.TurnToPick = false;
                }

                if (!Context.Elo.Lobby.Game.IsPickingTeams)
                {
                    Context.Elo.Lobby.GamesPlayed++;
                    await Context.Channel.SendMessageAsync("**Game has Started**\n" +
                                                           $"Team1: {string.Join(", ", Context.Elo.Lobby.Game.Team1.Players.Select(x => Context.Socket.Guild.GetUser(x)?.Mention).ToList())}\n" +
                                                           $"Team2: {string.Join(", ", Context.Elo.Lobby.Game.Team2.Players.Select(x => Context.Socket.Guild.GetUser(x)?.Mention).ToList())}\n" +
                                                           $"**Game #{Context.Elo.Lobby.GamesPlayed}**");
                    Context.Server.Results.Add(new GuildModel.GameResult
                    {
                        Comments = new List<GuildModel.GameResult.Comment>(),
                        Gamenumber = Context.Elo.Lobby.GamesPlayed,
                        LobbyID = Context.Elo.Lobby.ChannelID,
                        Result = GuildModel.GameResult._Result.Undecided,
                        Team1 = Context.Elo.Lobby.Game.Team1.Players,
                        Team2 = Context.Elo.Lobby.Game.Team2.Players,
                        Time = DateTime.UtcNow
                    });
                    await AnnounceGame(Context);
                    Context.Elo.Lobby.Game = new GuildModel.Lobby.CurrentGame();
                }

                Context.Server.Save();
            }
        }

        public static async Task AnnounceGame(Context.Context Context)
        {
            if (Context.Socket.Guild.GetChannel(Context.Server.Settings.GameSettings.AnnouncementsChannel) is IMessageChannel AnnouncementsChannel)
            {
                try
                {
                    var T1Mentions = string.Join(" ", Context.Elo.Lobby.Game.Team1.Players.Select(x => Context.Socket.Guild.GetUser(x)?.Mention).ToList());
                    var T2Mentions = string.Join(" ", Context.Elo.Lobby.Game.Team2.Players.Select(x => Context.Socket.Guild.GetUser(x)?.Mention).ToList());
                    var mentions = $"{T1Mentions} {T2Mentions}";
                    var embed = new EmbedBuilder
                        {
                            Title = "Game has Started"
                        }.AddField("Game Info", $"Lobby: {(Context.Channel as ITextChannel).Mention}\n" +
                                                $"Game: {Context.Elo.Lobby.GamesPlayed}\n")
                        .AddField("Team 1", T1Mentions)
                        .AddField("Team 2", T2Mentions);
                    await AnnouncementsChannel.SendMessageAsync(mentions, false, embed.Build());
                }
                catch (Exception e)
                {
                    LogHandler.LogMessage(Context, e.ToString(), LogSeverity.Error);
                }
            }

            if (Context.Server.Settings.GameSettings.DMAnnouncements)
            {
                var DMEmbed = new EmbedBuilder
                    {
                        Title = "Game has Started"
                    }.AddField("Game Info", $"Lobby: {Context.Channel.Name}\n" +
                                            $"Game: {Context.Elo.Lobby.GamesPlayed}\n")
                    .AddField("Team 1", $"{string.Join(" ", Context.Elo.Lobby.Game.Team1.Players.Select(x => Context.Server.Users.FirstOrDefault(u => u.UserID == x)?.Username).Where(x => x != null))}")
                    .AddField("Team 2", $"{string.Join(" ", Context.Elo.Lobby.Game.Team2.Players.Select(x => Context.Server.Users.FirstOrDefault(u => u.UserID == x)?.Username).Where(x => x != null))}")
                    .Build();
                var AllPlayers = new List<ulong>();
                AllPlayers.AddRange(Context.Elo.Lobby.Game.Team1.Players);
                AllPlayers.AddRange(Context.Elo.Lobby.Game.Team2.Players);
                foreach (var user in AllPlayers)
                {
                    try
                    {
                        var u = Context.Socket.Client.GetUser(user);
                        if (u != null)
                        {
                            await u.SendMessageAsync("", false, DMEmbed);
                        }
                    }
                    catch (Exception e)
                    {
                        LogHandler.LogMessage(Context, e.ToString(), LogSeverity.Error);
                    }
                }
            }
        }
    }
}