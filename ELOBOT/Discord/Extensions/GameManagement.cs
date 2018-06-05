using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using ELOBOT.Models;

namespace ELOBOT.Discord.Extensions
{
    public class GameManagement
    {
        public static async Task GameResult(Context.Context Context, GuildModel.GameResult Game, GuildModel.GameResult._Result Result)
        {
            try
            {
                var sgame = Context.Server.Results.FirstOrDefault(x => x.LobbyID == Game.LobbyID && x.Gamenumber == Game.Gamenumber);
                if (Result == GuildModel.GameResult._Result.Cancelled)
                {
                    sgame.Result = Result;
                    Context.Server.Save();
                    await Context.Channel.SendMessageAsync("", false, new EmbedBuilder
                    {
                        Color = Color.DarkOrange,
                        Description = "Success, game has been cancelled."
                    }.Build());

                    return;
                }

                var UserList = new List<ulong>();
                UserList.AddRange(Game.Team1);
                UserList.AddRange(Game.Team2);
                var WinEmbed = new EmbedBuilder
                {
                    Color = Color.Green
                };
                var LoseEmbed = new EmbedBuilder
                {
                    Color = Color.Red
                };
                foreach (var userID in UserList)
                {
                    var user = Context.Server.Users.FirstOrDefault(x => x.UserID == userID);
                    if (user == null) continue;
                    var MaxRank = UserManagement.MaxRole(Context, user);
                    if (Result == GuildModel.GameResult._Result.Team1 && Game.Team1.Contains(user.UserID) || Result == GuildModel.GameResult._Result.Team2 && Game.Team2.Contains(user.UserID))
                    {
                        user.Stats.Points += MaxRank.WinModifier;
                        user.Stats.Wins++;
                        WinEmbed.AddField($"{user.Username} (+{MaxRank.WinModifier})", $"Points: {user.Stats.Points}\n" +
                                                                                       $"Wins: {user.Stats.Wins}");
                    }
                    else
                    {
                        user.Stats.Points -= MaxRank.LossModifier;
                        if (user.Stats.Points < 0)
                        {
                            if (!Context.Server.Settings.GameSettings.AllowNegativeScore)
                            {
                                user.Stats.Points = 0;
                            }
                        }

                        user.Stats.Losses++;

                        LoseEmbed.AddField($"{user.Username} (-{MaxRank.LossModifier})", $"Points: {user.Stats.Points}\n" +
                                                                                         $"Losses: {user.Stats.Losses}");
                    }

                    user.Stats.GamesPlayed++;
                }

                sgame.Result = Result;
                Context.Server.Save();
                await Context.Channel.SendMessageAsync("", false, WinEmbed.Build());
                await Context.Channel.SendMessageAsync("", false, LoseEmbed.Build());
            }
            catch (Exception e)
            {
                await LogError.Error(Context, e.ToString());
            }
        }
    }
}