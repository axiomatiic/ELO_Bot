using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ELO_Bot.Commands
{
    public class Globals
    {
        public static string GetNamePrefix(Servers.Server Server, ulong userID, bool serverdefaultscore = false)
        {
            var usernameSelection = Server.UsernameSelection;
            var user = Server.UserList.FirstOrDefault(x => x.UserId == userID);
            var ispatreon = false;
            if (CommandHandler.VerifiedUsers != null)
                if (CommandHandler.VerifiedUsers.Contains(userID))
                    ispatreon = true;

            if (serverdefaultscore)
            {
                user.Points = Server.registerpoints;
            }

            if (ispatreon)
            {
                switch (usernameSelection)
                {
                    case 1:
                        return $"👑{user.Points} ~";
                    case 2:
                        return $"👑[{user.Points}]";
                    case 3:
                        return $"👑";
                }
            }
            else
            {
                switch (usernameSelection)
                {
                    case 1:
                        return $"{user.Points}";
                    case 2:
                        return $"[{user.Points}]";
                    case 3:
                        return $"";
                }
            }

            return $"{user.Points} ~";
        }
    }
}
