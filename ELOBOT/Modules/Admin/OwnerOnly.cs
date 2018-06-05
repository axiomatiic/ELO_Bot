using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ELOBOT.Discord.Context;
using ELOBOT.Discord.Preconditions;
using ELOBOT.Models;

namespace ELOBOT.Modules.Admin
{
    [GuildOwner]
    public class OwnerOnly : Base
    {
        private readonly CommandService _service;

        public OwnerOnly(CommandService service)
        {
            _service = service;
        }

        [Command("AddPermissionOverride")]
        public async Task AddoverRide(string commandname, GuildModel.GuildSettings._CommandAccess.CustomPermission.accesstype Type)
        {
            var matched = _service.Commands.FirstOrDefault(x => x.Aliases.Any(a => string.Equals(a, commandname, StringComparison.CurrentCultureIgnoreCase)));
            if (matched == null)
            {
                throw new Exception("Unknown Command Name");
            }

            var modified = false;
            var ToEdit = Context.Server.Settings.CustomPermissions.CustomisedPermission.FirstOrDefault(x => string.Equals(x.Name, matched.Name, StringComparison.CurrentCultureIgnoreCase));
            if (ToEdit != null)
            {
                ToEdit.Setting = Type;
                modified = true;
            }
            else
            {
                Context.Server.Settings.CustomPermissions.CustomisedPermission.Add(new GuildModel.GuildSettings._CommandAccess.CustomPermission
                {
                    Name = matched.Name,
                    Setting = Type
                });
            }

            await SimpleEmbedAsync($"Custom Permission override {(modified ? "Modfied" : "Added")}, users with {Type.ToString()} and above permissions, will be able to access it");
            Context.Server.Save();
        }

        [Command("RemovePermissionOverride")]
        public async Task RemoveOverrride(string commandname)
        {
            var matched = Context.Server.Settings.CustomPermissions.CustomisedPermission.FirstOrDefault(x => string.Equals(x.Name, commandname, StringComparison.CurrentCultureIgnoreCase));
            if (matched == null)
            {
                throw new Exception("Unknown override name");
            }

            Context.Server.Settings.CustomPermissions.CustomisedPermission.Remove(matched);
            await SimpleEmbedAsync("Custom Permission override removed.");
            Context.Server.Save();
        }

        [Command("OverrideList")]
        public async Task List()
        {
            var list = Context.Server.Settings.CustomPermissions.CustomisedPermission.Select(x => $"Name: {x.Name} Accessibility: {x.Setting.ToString()}");
            await SimpleEmbedAsync(string.Join("\n", list));
        }

        [Command("AddMod")]
        public async Task ModAdd(IRole ModRole)
        {
            if (Context.Server.Settings.Moderation.ModRoles.Contains(ModRole.Id))
            {
                throw new Exception("Role is already a mod role");
            }

            Context.Server.Settings.Moderation.ModRoles.Add(ModRole.Id);
            Context.Server.Save();
            await SimpleEmbedAsync("Mod Role Added.");
        }

        [Command("AddAdmin")]
        public async Task AdminAdd(IRole AdminRole)
        {
            if (Context.Server.Settings.Moderation.AdminRoles.Contains(AdminRole.Id))
            {
                throw new Exception("Role is already a Admin role");
            }

            Context.Server.Settings.Moderation.AdminRoles.Add(AdminRole.Id);
            Context.Server.Save();
            await SimpleEmbedAsync("Admin Role Added.");
        }

        [Command("ModeratorList")]
        public async Task ModeratorList()
        {
            var role = Context.Server.Settings.Moderation.ModRoles.Select(x => Context.Socket.Guild.GetRole(x)?.Mention).Where(x => x != null);
            await SimpleEmbedAsync("Moderator Roles\n" +
                                   $"{string.Join("\n", role)}");
        }

        [Command("AdminList")]
        public async Task AdminList()
        {
            var role = Context.Server.Settings.Moderation.AdminRoles.Select(x => Context.Socket.Guild.GetRole(x)?.Mention).Where(x => x != null);
            await SimpleEmbedAsync("Admin Roles\n" +
                                   $"{string.Join("\n", role)}");
        }

        [Command("DelMod")]
        public async Task ModDel(IRole ModRole)
        {
            if (Context.Server.Settings.Moderation.ModRoles.Contains(ModRole.Id))
            {
                Context.Server.Settings.Moderation.ModRoles.Remove(ModRole.Id);
                Context.Server.Save();
                await SimpleEmbedAsync("Moderator Role Removed.");
            }
            else
            {
                throw new Exception("Role is not a Moderator role");
            }
        }

        [Command("Deladmin")]
        public async Task AdminDel(IRole AdminRole)
        {
            if (Context.Server.Settings.Moderation.AdminRoles.Contains(AdminRole.Id))
            {
                Context.Server.Settings.Moderation.AdminRoles.Remove(AdminRole.Id);
                Context.Server.Save();
                await SimpleEmbedAsync("Admin Role Added.");
            }
            else
            {
                throw new Exception("Role is not an Admin role");
            }
        }
    }
}