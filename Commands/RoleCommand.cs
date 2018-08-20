using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace BrackeysBot.Commands
{
    public class RoleCommand : ModuleBase
    {
        private readonly string[] _uniqueRoleNames;
        private readonly string[] _sharedUniqueRoleNames;

        public RoleCommand(SettingsTable settingsTable)
        {
            var settings = settingsTable;
            _uniqueRoleNames = settings.Get("uniques").Split(',');
            _sharedUniqueRoleNames = settings.Get("sharedUniques").Split(',');
        }

        [Command("giverole"), Alias("addrole")]
        public async Task GiveRoleCommand()
        {
            var embedBuilder = new EmbedBuilder()
                .WithColor(new Color(0, 255, 255))
                .WithTitle("How to give yourself a role:")
                .WithDescription("Example: []giverole Team Blue")
                .AddInlineField("Available Teams:", string.Join("\n", _uniqueRoleNames))
                .AddInlineField("Available Roles:", string.Join("\n", _sharedUniqueRoleNames));

            await ReplyAsync("", embed: embedBuilder);
        }

        [Command("giverole"), Alias("addrole")]
        [HelpData("giverole <role name>", "Give yourself a role.")]
        public async Task GiveRoleCommand([Remainder] string roleName)
        {
            var iGuildUser = (IGuildUser) Context.User;

            if (iGuildUser.HasRole(roleName))
            {
                throw new Exception("You already have that role.");
            }

            // If the role name is a unique role, assign a new unique role and return.
            if (_uniqueRoleNames.Any(x => string.Equals(x, roleName, StringComparison.CurrentCultureIgnoreCase)))
            {
                await SetNewUniqueRoleAsync((IGuildUser)Context.User, roleName);
                return;
            }

            var role = GetRoleByName(Context.Guild, roleName);

            if (role == null)
            {
                throw new NullReferenceException($"The role \"{roleName}\" could not be found. Please double-check your spelling and try again.");
            }

            // If the role is not a shared unique role
            if (!_sharedUniqueRoleNames.Any(x => string.Equals(x, roleName, StringComparison.CurrentCultureIgnoreCase)))
            {
                throw new Exception("I cannot give you that role.");
            }

            await ((SocketGuildUser) Context.User).AddRoleAsync(role);

            // Success
            var embedBuilder = new EmbedBuilder()
                .WithColor(Color.Green)
                .WithTitle("Success!")
                .WithDescription($"Added role \"{role.Name}\" to your roles.");

            await ReplyAsync("", embed: embedBuilder);
        }

        [Command("removerole")]
        public async Task RemoveRoleCommand()
        {
            var iGuildUser = (IGuildUser) Context.User;

            List<string> removeableRoles = _sharedUniqueRoleNames.Where(role => iGuildUser.HasRole(role)).ToList();

            var uniqueRole = GetUserUniqueRole((SocketGuildUser) iGuildUser);

            if (uniqueRole != null)
            {
                removeableRoles.Add(uniqueRole.Name);
            }

            var embedBuilder = new EmbedBuilder()
                .WithColor(new Color(0, 255, 255))
                .WithTitle("How to remove a role:")
                .WithDescription("Example: []removerole Team Blue");

            embedBuilder.AddInlineField("Roles you can remove:", removeableRoles.Any() 
                    ? string.Join('\n', removeableRoles) 
                    : "None!");

            await ReplyAsync("", embed: embedBuilder);
        }

        [Command("removerole")]
        [HelpData("removerole <role name>", "Remove one of your roles.")]
        public async Task RemoveRoleCommand([Remainder] string roleName)
        {
            var roleToRemove = GetRoleByName(Context.Guild, roleName);

            if (roleToRemove == null)
            {
                throw new NullReferenceException($"The role \"{roleName}\" could not be found. Please double-check your spelling and try again.");
            }

            var guildUser = (IGuildUser) Context.User;

            if (!guildUser.HasRole(roleName))
            {
                throw new Exception($"You don't the \"{roleName}\" role.");
            }

            if (!_sharedUniqueRoleNames.Any(role => string.Equals(role, roleName, StringComparison.CurrentCultureIgnoreCase)) &&
                !_uniqueRoleNames.Any(role => string.Equals(role, roleName, StringComparison.CurrentCultureIgnoreCase)))
            {
                throw new Exception("I cannot remove that role. Contact a moderator if you believe this to be an error.");
            }

            await guildUser.RemoveRoleAsync(roleToRemove);

            // Success
            var embedBuilder = new EmbedBuilder()
                .WithColor(Color.Green)
                .WithTitle("Success!")
                .WithDescription($"Removed role \"{roleToRemove.Name}\"");

            await ReplyAsync("", embed: embedBuilder);
        }

        [Command("team"), Alias("jointeam")]
        public async Task NewUniqueRoleCommand()
        {
            var embedBuilder = new EmbedBuilder()
                .WithColor(new Color(0, 255, 255))
                .WithTitle("How to join a new team:")
                .WithDescription("Example: []team blue")
                .AddInlineField("Available Teams:", string.Join("\n", _uniqueRoleNames));

            await ReplyAsync("", embed: embedBuilder);
        }

        [Command("team"), Alias("jointeam")]
        [HelpData("team <color>", "Join a new team.")]
        public async Task NewUniqueRoleCommand([Remainder] string uniqueRole)
        {
            var newTeamRoleName = $"Team {uniqueRole}";

            // If the team name matches any existing ones in _uniques
            if (_uniqueRoleNames.Any(unique => string.Equals(unique, newTeamRoleName, StringComparison.CurrentCultureIgnoreCase)))
            {
                await SetNewUniqueRoleAsync((IGuildUser)Context.User, newTeamRoleName);
            }
            else
            {
                throw new NullReferenceException($"The role \"{newTeamRoleName}\" could not be found. Please double-check your spelling and try again.");
            }
        }

        /// <summary>Sets a new unique role for the user. If they already have one of the unique roles, that role is removed.</summary>
        public async Task SetNewUniqueRoleAsync(IGuildUser user, string newUniqueRoleName)
        {
            var socketUser = (SocketGuildUser) user;

            var oldUnique = GetUserUniqueRole(socketUser);
            var newUnique = GetRoleByName(Context.Guild, newUniqueRoleName);

            if (oldUnique == newUnique && oldUnique != null)
            {
                throw new InvalidOperationException($"You are already in {newUnique.Name}.");
            }

            if (oldUnique != null)
            {
                await socketUser.RemoveRoleAsync(oldUnique);
            }

            if (newUnique == null)
            {
                throw new NullReferenceException($"The role \"{newUniqueRoleName}\" could not be found. Please double-check your spelling and try again.");
            }

            await socketUser.AddRoleAsync(newUnique);

            // Success
            var embedBuilder = new EmbedBuilder()
                .WithColor(Color.Green)
                .WithTitle("Success!")
                .WithDescription($"You are now {newUnique.Name}!");

            await ReplyAsync("", embed: embedBuilder);
        }

        /// <summary>Returns the user's unique role. If the user does not have one, it returns null.</summary>
        private IRole GetUserUniqueRole(SocketGuildUser user)
        {
            SocketRole currentUniqueRole = null;

            foreach (var roleName in _uniqueRoleNames)
            {
                currentUniqueRole = user.Roles.FirstOrDefault(x => string.Equals(x.Name, roleName, StringComparison.CurrentCultureIgnoreCase));

                if (currentUniqueRole != null) break;
            }

            return currentUniqueRole;
        }

        /// <summary>Returns a role in the guild by name. Case insensitive.</summary>
        public static IRole GetRoleByName(IGuild guild, string roleName)
        {
            return guild.Roles.FirstOrDefault(x => string.Equals(x.Name, roleName, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}
