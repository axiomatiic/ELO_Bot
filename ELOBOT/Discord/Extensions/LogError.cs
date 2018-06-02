using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using ELOBOT.Handlers;

namespace ELOBOT.Discord.Extensions
{
    public class LogError
    {
        public static async Task Error(Context.Context Context, string Error)
        {
            try
            {
                var owner = Context.Client.GetApplicationInfoAsync().Result.Owner;
                var errormsg = new EmbedBuilder
                {
                    Color = Color.Red,
                    Title = "ERROR",
                    Description = "MSG: \n" +
                                  $"{Error}"
                }.AddField("Context",
                    $"Guild: {Context.Guild?.Name} [{Context.Guild?.Id}]\n" +
                    $"Channel: {Context.Channel?.Name} [{Context.Channel?.Id}]\n" +
                    $"User: {Context.User.Username}\n" +
                    $"Message: {Context.Message.Content}").Build();
                await Context.Channel.SendMessageAsync("", false, errormsg);
                await owner.SendMessageAsync("", false, errormsg);
                LogHandler.LogMessage(Context, Error, LogSeverity.Error);
            }
            catch (Exception e)
            {
                LogHandler.LogMessage($"Fatal Error Logging Issue:\n" +
                                      $"{e}", LogSeverity.Error);
            }

        }
    }
}
