using System;
using System.Collections.Generic;
using ELOBOT.Handlers;

namespace ELOBOT.Models
{
    public class GuildModel
    {
        /// <summary>
        ///     The Server ID
        /// </summary>
        public ulong ID { get; set; }


        public List<Lobby> Lobbies { get; set; } = new List<Lobby>();

        public List<GameResult> Results { get; set; } = new List<GameResult>();

        public List<User> Users { get; set; } = new List<User>();

        public List<Rank> Ranks { get; set; } = new List<Rank>();

        public GuildSettings Settings { get; set; } = new GuildSettings();

        public void Save()
        {
            using (var Session = DatabaseHandler.Store.OpenSession(CommandHandler.Config.DBName))
            {
                Session.Store(this, ID.ToString());
                Session.SaveChanges();
            }
        }

        public class Lobby
        {
            public enum _PickMode
            {
                CompleteRandom,
                Captains,
                SortByScore
            }

            public enum CaptainSort
            {
                MostWins,
                MostPoints,
                HighestWinLoss,
                Random,
                RandomTop4MostPoints,
                RandomTop4MostWins,
                RandomTop4HighestWinLoss
            }

            public ulong ChannelID { get; set; }
            public int UserLimit { get; set; } = 10;
            public string Description { get; set; } = null;
            public int GamesPlayed { get; set; } = 0;

            public List<string> Maps { get; set; } = new List<string>();

            public _PickMode PickMode { get; set; } = _PickMode.CompleteRandom;
            public CaptainSort CaptainSortMode { get; set; } = CaptainSort.MostPoints;

            public CurrentGame Game { get; set; } = new CurrentGame();

            public class CurrentGame
            {
                public bool IsPickingTeams { get; set; } = false;
                public List<ulong> QueuedPlayerIDs { get; set; } = new List<ulong>();

                public Team Team1 { get; set; } = new Team();
                public Team Team2 { get; set; } = new Team();

                public class Team
                {
                    public List<ulong> Players { get; set; } = new List<ulong>();
                    public ulong Captain { get; set; }
                    public bool TurnToPick { get; set; } = false;
                }
            }
        }

        public class GameResult
        {
            public enum _Result
            {
                Team1,
                Team2,
                Undecided,
                Cancelled
            }

            public _Result Result = _Result.Undecided;

            public DateTime Time { get; set; } = DateTime.UtcNow;
            public ulong LobbyID { get; set; }
            public int Gamenumber { get; set; }
            public List<ulong> Team1 { get; set; } = new List<ulong>();
            public List<ulong> Team2 { get; set; } = new List<ulong>();

            public List<Comment> Comments { get; set; } = new List<Comment>();

            public class Comment
            {
                public int ID { get; set; } = 1;
                public ulong CommenterID { get; set; }
                public string Content { get; set; } = null;
            }
        }

        public class User
        {
            public string Username { get; set; }
            public ulong UserID { get; set; }
            public Score Stats { get; set; } = new Score();
            public Ban Banned { get; set; } = new Ban();

            public class Score
            {
                public int Points { get; set; } = 0;
                public int Wins { get; set; } = 0;
                public int Losses { get; set; } = 0;
                public int Draws { get; set; } = 0;
                public int Kills { get; set; } = 0;
                public int Deaths { get; set; } = 0;

                public int GamesPlayed { get; set; } = 0;
            }

            public class Ban
            {
                public bool Banned { get; set; } = false;
                public string Reason { get; set; } = null;
                public DateTime ExpiryTime { get; set; }
                public ulong Moderator { get; set; }
            }
        }

        public class Rank
        {
            public ulong RoleID { get; set; }
            public int Threshhold { get; set; }

            public int WinModifier { get; set; } = 0;
            public int LossModifier { get; set; } = 0;

            public bool IsDefault { get; set; } = false;
        }

        public class GuildSettings
        {
            public string CustomPrefix { get; set; } = null;
            public _Premium Premium { get; set; } = new _Premium();

            public _Moderation Moderation { get; set; } = new _Moderation();

            public _Registration Registration { get; set; } = new _Registration();
            public _GameSettings GameSettings { get; set; } = new _GameSettings();
            public _CommandAccess CustomPermissions { get; set; } = new _CommandAccess();

            public class _Moderation
            {
                public List<ulong> ModRoles { get; set; } = new List<ulong>();
                public List<ulong> AdminRoles { get; set; } = new List<ulong>();
            }

            public class _GameSettings
            {
                public ulong AnnouncementsChannel { get; set; }
                public bool DMAnnouncements { get; set; } = false;
                public bool RemoveOnAfk { get; set; } = true;
                public bool BlockMultiQueuing { get; set; } = true;
                public bool AllowNegativeScore { get; set; } = false;
                public TimeSpan ReQueueDelay { get; set; } = TimeSpan.Zero;
                public bool useKD { get; set; } = false;
            }

            public class _CommandAccess
            {
                public List<CustomPermission> CustomisedPermission { get; set; } = new List<CustomPermission>();

                public class CustomPermission
                {
                    public enum accesstype
                    {
                        ServerOwner,
                        Admin,
                        Moderator,
                        Registered
                    }

                    public bool IsCommand { get; set; } = true;
                    public string Name { get; set; }
                    public accesstype Setting { get; set; } = accesstype.Admin;
                }
            }


            public class _Registration
            {
                public string Message { get; set; } = "Thankyou for Registering!";
                public int DefaultWinModifier { get; set; } = 10;
                public int DefaultLossModifier { get; set; } = 5;
                public int RegistrationBonus { get; set; } = 0;
                public bool DeleteProfileOnLeave { get; set; } = false;
                public string NameFormat { get; set; } = "[{score}] - {username}";
            }

            public class _Premium
            {
                public bool IsPremium { get; set; } = false;
                public List<Key> PremiumKeys { get; set; } = new List<Key>();
                public DateTime Expiry { get; set; } = DateTime.MaxValue;

                public class Key
                {
                    public string Token { get; set; }
                    public TimeSpan ValidFor { get; set; } = TimeSpan.FromDays(28);
                }
            }
        }
    }
}