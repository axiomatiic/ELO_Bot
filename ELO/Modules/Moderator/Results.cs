namespace ELO.Modules.Moderator
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using ELO.Discord.Context;
    using ELO.Discord.Extensions;
    using ELO.Discord.Preconditions;
    using ELO.Models;

    using global::Discord;
    using global::Discord.Addons.Interactive;
    using global::Discord.Commands;

    [CustomPermissions(true, true)]
    public class Results : Base
    {
        [Command("Game")]
        [Summary("Submit a game result")]
        public async Task GameAsync(IMessageChannel lobby, int gameNumber, GuildModel.GameResult._Result result)
        {
            if (Context.Server.Lobbies.All(x => x.ChannelID != lobby.Id))
            {
                throw new Exception("Channel is not a lobby");
            }

            var game = Context.Server.Results.FirstOrDefault(x => x.LobbyID == lobby.Id && x.GameNumber == gameNumber);
            if (game.Result != GuildModel.GameResult._Result.Undecided)
            {
                await InlineReactionReplyAsync(
                    new ReactionCallbackData(
                        "",
                        new EmbedBuilder
                            {
                                Description =
                                    "This game's Result has already been set to:\n"
                                    + $"{game.Result.ToString()}\n"
                                    + "Please reply with `Continue` To Still modify the result and update scores\n"
                                    + "Or Reply with `Cancel` to cancel this command"
                            }.Build()).WithCallback(new Emoji("☑"),
                        (c, r) => GameManagement.GameResultAsync(Context, game, result))
                        .WithCallback(new Emoji("🇽"), (c,r) => SimpleEmbedAsync("Canceled Game Result")));
            }
            else
            {
                await GameManagement.GameResultAsync(Context, game, result);
            }
        }
    }
}