namespace ELO.Modules.Admin
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using ELO.Discord.Context;
    using ELO.Discord.Preconditions;
    using ELO.Models;

    using global::Discord;
    using global::Discord.Commands;

    [GuildOwner]
    public class Owner : Base
    {
        private readonly CommandService _service;

        public Owner(CommandService service)
        {
            _service = service;
        }

        [Command("Premium")]
        public Task DoPremiumAsync([Remainder]string code = null)
        {
            var tokenModel = TokenModel.Load();
            var tokens = tokenModel.TokenList;
            var match = tokens.FirstOrDefault(x => x.Token == code);
            if (match == null)
            {
                throw new Exception($"Invalid Token\nYou may purchase tokens at:\n{ConfigModel.Load().PurchaseLink}");
            }

            Context.Server.Settings.Premium.IsPremium = true;
            Context.Server.Settings.Premium.Expiry = Context.Server.Settings.Premium.Expiry < DateTime.UtcNow ? DateTime.UtcNow + TimeSpan.FromDays(match.Days) : Context.Server.Settings.Premium.Expiry + TimeSpan.FromDays(match.Days);
            Context.Server.Settings.Premium.PremiumKeys.Add(new GuildModel.GuildSettings._Premium.Key
                                                                {
                                                                    Token = code,
                                                                    ValidFor = TimeSpan.FromDays(match.Days)
                                                                });
            Context.Server.Save();
            tokens.Remove(match);
            tokenModel.TokenList = tokens;
            tokenModel.Save();
            return SimpleEmbedAsync(
                $"Success, Token Redeemed ({match.Days} days)\n"
                + $"Server expires on {Context.Server.Settings.Premium.Expiry.ToLongDateString()} {Context.Server.Settings.Premium.Expiry.ToLongTimeString()}");
        }

        [Command("AddPermissionOverride")]
        public async Task AddOverrideAsync(string commandName, GuildModel.GuildSettings._CommandAccess.CustomPermission.AccessType type)
        {
            var matched = _service.Commands.FirstOrDefault(x => x.Aliases.Any(a => string.Equals(a, commandName, StringComparison.CurrentCultureIgnoreCase)));
            if (matched == null)
            {
                throw new Exception("Unknown Command Name");
            }

            var modified = false;
            var toEdit = Context.Server.Settings.CustomPermissions.CustomizedPermission.FirstOrDefault(x => string.Equals(x.Name, matched.Name, StringComparison.CurrentCultureIgnoreCase));
            if (toEdit != null)
            {
                toEdit.Setting = type;
                modified = true;
            }
            else
            {
                Context.Server.Settings.CustomPermissions.CustomizedPermission.Add(new GuildModel.GuildSettings._CommandAccess.CustomPermission
                {
                    Name = matched.Name,
                    Setting = type
                });
            }

            await SimpleEmbedAsync($"Custom Permission override {(modified ? "Modified" : "Added")}, users with {type.ToString()} and above permissions, will be able to access it");
            Context.Server.Save();
        }

        [Command("RemovePermissionOverride")]
        public async Task RemoveOverrideAsync(string commandName)
        {
            var matched = Context.Server.Settings.CustomPermissions.CustomizedPermission.FirstOrDefault(x => string.Equals(x.Name, commandName, StringComparison.CurrentCultureIgnoreCase));
            if (matched == null)
            {
                throw new Exception("Unknown override name");
            }

            Context.Server.Settings.CustomPermissions.CustomizedPermission.Remove(matched);
            await SimpleEmbedAsync("Custom Permission override removed.");
            Context.Server.Save();
        }

        [Command("OverrideList")]
        public Task ListAsync()
        {
            var list = Context.Server.Settings.CustomPermissions.CustomizedPermission.Select(x => $"Name: {x.Name} Accessibility: {x.Setting.ToString()}");
            return SimpleEmbedAsync(string.Join("\n", list));
        }

        [Command("AddMod")]
        public Task ModAddAsync(IRole modRole)
        {
            if (Context.Server.Settings.Moderation.ModRoles.Contains(modRole.Id))
            {
                throw new Exception("Role is already a mod role");
            }

            Context.Server.Settings.Moderation.ModRoles.Add(modRole.Id);
            Context.Server.Save();
            return SimpleEmbedAsync("Mod Role Added.");
        }

        [Command("AddAdmin")]
        public Task AdminAddAsync(IRole adminRole)
        {
            if (Context.Server.Settings.Moderation.AdminRoles.Contains(adminRole.Id))
            {
                throw new Exception("Role is already a Admin role");
            }

            Context.Server.Settings.Moderation.AdminRoles.Add(adminRole.Id);
            Context.Server.Save();
            return SimpleEmbedAsync("Admin Role Added.");
        }

        [Command("ModeratorList")]
        public Task ModeratorListAsync()
        {
            var role = Context.Server.Settings.Moderation.ModRoles.Select(x => Context.Guild.GetRole(x)?.Mention).Where(x => x != null);
            return SimpleEmbedAsync("Moderator Roles\n" +
                                    $"{string.Join("\n", role)}");
        }

        [Command("AdminList")]
        public Task AdminListAsync()
        {
            var role = Context.Server.Settings.Moderation.AdminRoles.Select(x => Context.Guild.GetRole(x)?.Mention).Where(x => x != null);
            return SimpleEmbedAsync("Admin Roles\n" +
                                    $"{string.Join("\n", role)}");
        }

        [Command("DelMod")]
        public Task ModDelAsync(IRole modRole)
        {
            if (!Context.Server.Settings.Moderation.ModRoles.Contains(modRole.Id))
            {
                throw new Exception("Role is not a Moderator role");
            }

            Context.Server.Settings.Moderation.ModRoles.Remove(modRole.Id);
            Context.Server.Save();
            return SimpleEmbedAsync("Moderator Role Removed.");
        }

        [Command("DelAdmin")]
        public Task AdminDelAsync(IRole adminRole)
        {
            if (!Context.Server.Settings.Moderation.AdminRoles.Contains(adminRole.Id))
            {
                throw new Exception("Role is not an Admin role");
            }

            Context.Server.Settings.Moderation.AdminRoles.Remove(adminRole.Id);
            Context.Server.Save();
            return SimpleEmbedAsync("Admin Role Added.");
        }
    }
}