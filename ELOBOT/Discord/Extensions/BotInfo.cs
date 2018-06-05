using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.WebSocket;

namespace ELOBOT.Discord.Extensions
{
    public class BotInfo
    {
        public static string GetInvite(Context.Context Context)
        {
            return GetInvite(Context.Client);
        }
        public static string GetInvite(DiscordSocketClient Client)
        {
            return $"https://discordapp.com/oauth2/authorize?client_id={Client.CurrentUser.Id}&scope=bot&permissions=2146958591";
        }
        public static string GetInvite(IDiscordClient Client)
        {
            return $"https://discordapp.com/oauth2/authorize?client_id={Client.CurrentUser.Id}&scope=bot&permissions=2146958591";
        }
    }
}
