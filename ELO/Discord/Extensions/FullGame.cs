namespace ELO.Discord.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using ELO.Discord.Context;
    using ELO.Handlers;
    using ELO.Models;

    using global::Discord;

    public class FullGame
    {
        public static async Task FullQueueAsync(Context context)
        {
            if (context.Elo.Lobby.Game.QueuedPlayerIDs.Count >= context.Elo.Lobby.UserLimit)
            {
                var users = context.Elo.Lobby.Game.QueuedPlayerIDs.Select(qu => context.Guild.GetUser(qu)).ToList();
                if (users.Any(x => x == null))
                {
                    context.Elo.Lobby.Game = new GuildModel.Lobby.CurrentGame();
                    context.Server.Save();
                    await context.Channel.SendMessageAsync("Game Aborted, Missing Player in queue");
                    return;
                }

                if (context.Elo.Lobby.PickMode == GuildModel.Lobby._PickMode.CompleteRandom)
                {
                    var shuffled = context.Elo.Lobby.Game.QueuedPlayerIDs.OrderBy(x => new Random().Next()).ToList();
                    context.Elo.Lobby.Game.Team1.Players = shuffled.Take(context.Elo.Lobby.UserLimit / 2).ToList();
                    context.Elo.Lobby.Game.Team2.Players = shuffled.Skip(context.Elo.Lobby.UserLimit / 2).Take(context.Elo.Lobby.UserLimit / 2).ToList();
                }
                else if (context.Elo.Lobby.PickMode == GuildModel.Lobby._PickMode.SortByScore)
                {
                    var ordered = context.Elo.Lobby.Game.QueuedPlayerIDs.Select(x => context.Server.Users.First(u => x == u.UserID)).OrderByDescending(x => x.Stats.Points).ToList();
                    foreach (var user in ordered)
                    {
                        if (context.Elo.Lobby.Game.Team1.Players.Count > context.Elo.Lobby.Game.Team2.Players.Count)
                        {
                            context.Elo.Lobby.Game.Team2.Players.Add(user.UserID);
                        }
                        else
                        {
                            context.Elo.Lobby.Game.Team1.Players.Add(user.UserID);
                        }
                    }
                }
                else if (context.Elo.Lobby.PickMode == GuildModel.Lobby._PickMode.Captains)
                {
                    IUser cap1;
                    IUser cap2;
                    var list = context.Elo.Lobby.Game.QueuedPlayerIDs.Select(x => context.Server.Users.First(u => u.UserID == x)).ToList();
                    var rnd = new Random();
                    if (context.Elo.Lobby.CaptainSortMode == GuildModel.Lobby.CaptainSort.MostPoints)
                    {
                        var cSorted = list.OrderByDescending(x => x.Stats.Points).ToList();
                        cap1 = context.Guild.GetUser(cSorted[0].UserID);
                        cap2 = context.Guild.GetUser(cSorted[1].UserID);
                    }
                    else if (context.Elo.Lobby.CaptainSortMode == GuildModel.Lobby.CaptainSort.MostWins)
                    {
                        var cSorted = list.OrderByDescending(x => x.Stats.Wins).ToList();
                        cap1 = context.Guild.GetUser(cSorted[0].UserID);
                        cap2 = context.Guild.GetUser(cSorted[1].UserID);
                    }
                    else if (context.Elo.Lobby.CaptainSortMode == GuildModel.Lobby.CaptainSort.HighestWinLoss)
                    {
                        var cSorted = list.OrderByDescending(x => (double)x.Stats.Wins / (x.Stats.Losses == 0 ? 1 : x.Stats.Losses)).ToList();
                        cap1 = context.Guild.GetUser(cSorted[0].UserID);
                        cap2 = context.Guild.GetUser(cSorted[1].UserID);
                    }
                    else if (context.Elo.Lobby.CaptainSortMode == GuildModel.Lobby.CaptainSort.Random)
                    {
                        var cSorted = list.OrderByDescending(x => rnd.Next()).ToList();
                        cap1 = context.Guild.GetUser(cSorted[0].UserID);
                        cap2 = context.Guild.GetUser(cSorted[1].UserID);
                    }
                    else if (context.Elo.Lobby.CaptainSortMode == GuildModel.Lobby.CaptainSort.RandomTop4MostPoints)
                    {
                        if (list.Count >= 4)
                        {
                            var cSorted = list.OrderByDescending(x => x.Stats.Points).Take(4).OrderByDescending(x => rnd.Next()).ToList();
                            cap1 = context.Guild.GetUser(cSorted[0].UserID);
                            cap2 = context.Guild.GetUser(cSorted[1].UserID);
                        }
                        else
                        {
                            var cSorted = list.OrderByDescending(x => x.Stats.Points).ToList();
                            cap1 = context.Guild.GetUser(cSorted[0].UserID);
                            cap2 = context.Guild.GetUser(cSorted[1].UserID);
                        }
                    }
                    else if (context.Elo.Lobby.CaptainSortMode == GuildModel.Lobby.CaptainSort.RandomTop4MostWins)
                    {
                        if (list.Count >= 4)
                        {
                            var cSorted = list.OrderByDescending(x => x.Stats.Wins).Take(4).OrderByDescending(x => rnd.Next()).ToList();
                            cap1 = context.Guild.GetUser(cSorted[0].UserID);
                            cap2 = context.Guild.GetUser(cSorted[1].UserID);
                        }
                        else
                        {
                            var cSorted = list.OrderByDescending(x => x.Stats.Wins).ToList();
                            cap1 = context.Guild.GetUser(cSorted[0].UserID);
                            cap2 = context.Guild.GetUser(cSorted[1].UserID);
                        }
                    }
                    else if (context.Elo.Lobby.CaptainSortMode == GuildModel.Lobby.CaptainSort.RandomTop4HighestWinLoss)
                    {
                        if (list.Count >= 4)
                        {
                            var cSorted = list.OrderByDescending(x => (double)x.Stats.Wins / (x.Stats.Losses == 0 ? 1 : x.Stats.Losses)).Take(4).OrderByDescending(x => rnd.Next()).ToList();
                            cap1 = context.Guild.GetUser(cSorted[0].UserID);
                            cap2 = context.Guild.GetUser(cSorted[1].UserID);
                        }
                        else
                        {
                            var cSorted = list.OrderByDescending(x => (double)x.Stats.Wins / (x.Stats.Losses == 0 ? 1 : x.Stats.Losses)).ToList();
                            cap1 = context.Guild.GetUser(cSorted[0].UserID);
                            cap2 = context.Guild.GetUser(cSorted[1].UserID);
                        }
                    }
                    else
                    {
                        var cSorted = list.OrderByDescending(x => rnd.Next()).ToList();
                        cap1 = context.Guild.GetUser(cSorted[0].UserID);
                        cap2 = context.Guild.GetUser(cSorted[1].UserID);
                    }

                    context.Elo.Lobby.Game.Team1.Captain = cap1.Id;
                    context.Elo.Lobby.Game.Team2.Captain = cap2.Id;
                    context.Elo.Lobby.Game.Team1.Players.Add(cap1.Id);
                    context.Elo.Lobby.Game.Team2.Players.Add(cap2.Id);
                    context.Elo.Lobby.Game.QueuedPlayerIDs.Remove(cap1.Id);
                    context.Elo.Lobby.Game.QueuedPlayerIDs.Remove(cap2.Id);
                    await context.Channel.SendMessageAsync($"**Team1 Captain** {cap1.Mention}\n" +
                                                           $"**Team2 Captain** {cap2.Mention}\n" +
                                                           $"**Select Your Teams using `{context.Prefix}pick <@user>`**\n" +
                                                           "**Captain 1 Always Picks First**\n" +
                                                           "**Player Pool**\n" +
                                                           $"{string.Join(" ", users.Select(x => x.Mention))}");
                    context.Elo.Lobby.Game.IsPickingTeams = true;
                    context.Elo.Lobby.Game.Team1.TurnToPick = true;
                    context.Elo.Lobby.Game.Team2.TurnToPick = false;
                }

                if (!context.Elo.Lobby.Game.IsPickingTeams)
                {
                    context.Elo.Lobby.GamesPlayed++;
                    await context.Channel.SendMessageAsync("**Game has Started**\n" +
                                                           $"Team1: {string.Join(", ", context.Elo.Lobby.Game.Team1.Players.Select(x => context.Guild.GetUser(x)?.Mention).ToList())}\n" +
                                                           $"Team2: {string.Join(", ", context.Elo.Lobby.Game.Team2.Players.Select(x => context.Guild.GetUser(x)?.Mention).ToList())}\n" +
                                                           $"**Game #{context.Elo.Lobby.GamesPlayed}**");
                    context.Server.Results.Add(new GuildModel.GameResult
                    {
                        Comments = new List<GuildModel.GameResult.Comment>(),
                        GameNumber = context.Elo.Lobby.GamesPlayed,
                        LobbyID = context.Elo.Lobby.ChannelID,
                        Result = GuildModel.GameResult._Result.Undecided,
                        Team1 = context.Elo.Lobby.Game.Team1.Players,
                        Team2 = context.Elo.Lobby.Game.Team2.Players,
                        Time = DateTime.UtcNow
                    });
                    await AnnounceGameAsync(context);
                    context.Elo.Lobby.Game = new GuildModel.Lobby.CurrentGame();
                }

                context.Server.Save();
            }
        }

        public static async Task AnnounceGameAsync(Context context)
        {
            var embed = new EmbedBuilder { Title = "Game has Started", Color = Color.Blue }.AddField("Game Info", $"Lobby: {(context.Channel as ITextChannel).Mention}\n" + $"Game: {context.Elo.Lobby.GamesPlayed}\n");
            try
            {
                var team1Mentions = string.Join(" ", context.Elo.Lobby.Game.Team1.Players.Select(x => context.Guild.GetUser(x)?.Mention).ToList());
                var team2Mentions = string.Join(" ", context.Elo.Lobby.Game.Team2.Players.Select(x => context.Guild.GetUser(x)?.Mention).ToList());
                var mentions = $"{team1Mentions} {team2Mentions}";
                embed.AddField("Team 1", team1Mentions)
                    .AddField("Team 2", team2Mentions);
                if (context.Elo.Lobby.RandomMapAnnounce)
                {
                    var field = AnnouncementManager.MapField(context.Elo.Lobby);
                    if (field != null)
                    {
                        embed.AddField(field);
                    }
                }

                if (context.Elo.Lobby.HostSelectionMode != GuildModel.Lobby.HostSelector.None)
                {
                    var field = AnnouncementManager.HostField(context, context.Elo.Lobby);
                    if (field != null)
                    {
                        embed.AddField(field);
                    }
                }
                
                if (context.Guild.GetChannel(context.Server.Settings.GameSettings.AnnouncementsChannel) is IMessageChannel AnnouncementsChannel)
                {
                    await AnnouncementsChannel.SendMessageAsync(mentions, false, embed.Build());
                }

                await context.Channel.SendMessageAsync(mentions, false, embed.Build());
            }
            catch (Exception e)
            {
                LogHandler.LogMessage(context, e.ToString(), LogSeverity.Error);
            }


            if (context.Server.Settings.GameSettings.DMAnnouncements)
            {
                var dmEmbed = new EmbedBuilder
                {
                    Title = "Game has Started"
                }.AddField("Game Info", $"Lobby: {context.Channel.Name}\n" +
                                            $"Game: {context.Elo.Lobby.GamesPlayed}\n")
                    .AddField("Team 1", $"{string.Join(" ", context.Elo.Lobby.Game.Team1.Players.Select(x => context.Server.Users.FirstOrDefault(u => u.UserID == x)?.Username).Where(x => x != null))}")
                    .AddField("Team 2", $"{string.Join(" ", context.Elo.Lobby.Game.Team2.Players.Select(x => context.Server.Users.FirstOrDefault(u => u.UserID == x)?.Username).Where(x => x != null))}");
                if (context.Elo.Lobby.RandomMapAnnounce)
                {
                    var field = AnnouncementManager.MapField(context.Elo.Lobby);
                    if (field != null)
                    {
                        dmEmbed.AddField(field);
                    }
                }

                if (context.Elo.Lobby.HostSelectionMode != GuildModel.Lobby.HostSelector.None)
                {
                    var field = AnnouncementManager.HostField(context, context.Elo.Lobby);
                    if (field != null)
                    {
                        dmEmbed.AddField(field);
                    }
                }

                var allPlayers = new List<ulong>();
                allPlayers.AddRange(context.Elo.Lobby.Game.Team1.Players);
                allPlayers.AddRange(context.Elo.Lobby.Game.Team2.Players);
                foreach (var user in allPlayers)
                {
                    try
                    {
                        var u = context.Client.GetUser(user);
                        if (u != null)
                        {
                            await u.SendMessageAsync("", false, dmEmbed.Build());
                        }
                    }
                    catch (Exception e)
                    {
                        LogHandler.LogMessage(context, e.ToString(), LogSeverity.Error);
                    }
                }
            }
        }
    }
}