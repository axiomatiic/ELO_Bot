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

    public class GameManagement
    {
        public static async Task GameResultAsync(Context context, GuildModel.GameResult game, GuildModel.GameResult._Result result)
        {
            try
            {
                var gameObject = context.Server.Results.FirstOrDefault(x => x.LobbyID == game.LobbyID && x.GameNumber == game.GameNumber);
                if (result == GuildModel.GameResult._Result.Canceled)
                {
                    gameObject.Result = result;
                    context.Server.Save();
                    await context.Channel.SendMessageAsync("", false, new EmbedBuilder
                    {
                        Color = Color.DarkOrange,
                        Description = "Success, game has been canceled."
                    }.Build());

                    return;
                }

                var userList = new List<ulong>();
                userList.AddRange(game.Team1);
                userList.AddRange(game.Team2);
                var winEmbed = new EmbedBuilder
                {
                    Color = Color.Green
                };
                var loseEmbed = new EmbedBuilder
                {
                    Color = Color.Red
                };
                foreach (var userID in userList)
                {
                    var user = context.Server.Users.FirstOrDefault(x => x.UserID == userID);
                    if (user == null)
                    {
                        continue;
                    }

                    var maxRank = UserManagement.MaxRole(context, user);
                    if ((result == GuildModel.GameResult._Result.Team1 && game.Team1.Contains(user.UserID)) || (result == GuildModel.GameResult._Result.Team2 && game.Team2.Contains(user.UserID)))
                    {
                        user.Stats.Points += maxRank.WinModifier;
                        user.Stats.Wins++;
                        winEmbed.AddField($"{user.Username} (+{maxRank.WinModifier})", $"Points: {user.Stats.Points}\n" +
                                                                                       $"Wins: {user.Stats.Wins}");
                    }
                    else
                    {
                        user.Stats.Points -= maxRank.LossModifier;
                        if (user.Stats.Points < 0)
                        {
                            if (!context.Server.Settings.GameSettings.AllowNegativeScore)
                            {
                                user.Stats.Points = 0;
                            }
                        }

                        user.Stats.Losses++;

                        loseEmbed.AddField($"{user.Username} (-{maxRank.LossModifier})", $"Points: {user.Stats.Points}\n" +
                                                                                         $"Losses: {user.Stats.Losses}");
                    }

                    user.Stats.GamesPlayed++;
                }

                gameObject.Result = result;
                context.Server.Save();
                await context.Channel.SendMessageAsync("", false, winEmbed.Build());
                await context.Channel.SendMessageAsync("", false, loseEmbed.Build());
            }
            catch (Exception e)
            {
                LogHandler.LogMessage(context, e.ToString(), LogSeverity.Error);
            }
        }
    }
}