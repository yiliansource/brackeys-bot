using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Commands;

namespace BrackeysBot.Core.Models
{
    [Name("Roles")]
    [Summary("Adds or removes a role from a user.")]
    public class RolesFeature : CustomCommandFeature
    {
        public bool Reply { get; set; }
        public string[] Operations { get; set; }

        public override void FillArguments(string arguments)
        {
            Operations = arguments.Split(',')
                .Select(operation => operation.Trim())
                .ToArray();

            if (Operations.Length > 0 && bool.TryParse(Operations[0].ToLower(), out bool reply))
            {
                Reply = reply;
                Operations = Operations.Skip(1).ToArray();
            }

            if (Operations.Any(o => !o.StartsWith("+") && !o.StartsWith("-") && !o.StartsWith("~")))
                throw new ArgumentException("Invalid role operations.");
        }
        public override async Task Execute(ICommandContext context)
        {
            if (context.Guild == null)
                throw new Exception("Role commands can only be called in a guild!");

            IGuildUser user = context.User as IGuildUser;

            var addChanges = new List<IRole>();
            foreach (IRole role in ConvertToRoles(GetRolesToAdd(), context))
            {
                if (!user.RoleIds.Contains(role.Id))
                {
                    addChanges.Add(role);
                    await user.AddRoleAsync(role);
                }
            }

            var removeChanges = new List<IRole>();
            foreach (IRole role in ConvertToRoles(GetRolesToRemove(), context))
            {
                if (user.RoleIds.Contains(role.Id))
                {
                    removeChanges.Add(role);
                    await user.RemoveRoleAsync(role);
                }
            }

            foreach (IRole role in ConvertToRoles(GetRolesToToggle(), context))
            {
                if (user.RoleIds.Contains(role.Id))
                {
                    removeChanges.Add(role);
                    await user.RemoveRoleAsync(role);
                }
                else
                {
                    addChanges.Add(role);
                    await user.AddRoleAsync(role);
                }
            }

            if (Reply)
            {
                StringBuilder replyMessageContent = new StringBuilder();
                if (addChanges.Count > 0)
                {
                    replyMessageContent.Append("You now have the role(s): ")
                        .AppendJoin(", ", addChanges.Select(r => r.Mention));
                }
                if (removeChanges.Count > 0)
                {
                    replyMessageContent.Append("You no longer have the role(s): ")
                        .AppendJoin(", ", removeChanges.Select(r => r.Mention));
                }

                await new EmbedBuilder()
                    .WithColor(replyMessageContent.Length > 0 ? Color.Green : Color.Red)
                    .WithDescription(replyMessageContent.ToString().WithAlternative("No roles have been added or removed."))
                    .Build()
                    .SendToChannel(context.Channel);
            }
        }

        private IEnumerable<string> GetRolesToAdd()
            => Operations.Where(o => o.StartsWith("+")).Select(o => o[1..]);
        private IEnumerable<string> GetRolesToRemove()
            => Operations.Where(o => o.StartsWith("-")).Select(o => o[1..]);
        private IEnumerable<string> GetRolesToToggle()
            => Operations.Where(o => o.StartsWith("~")).Select(o => o[1..]);

        private IEnumerable<IRole> ConvertToRoles(IEnumerable<string> names, ICommandContext context)
            => names.Select(r => GetRole(r, context)).Where(r => r != null);

        private IRole GetRole(string name, ICommandContext context)
            => context.Guild.Roles.FirstOrDefault(r => r.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            var add = GetRolesToAdd();
            if (add.Count() > 0)
            {
                builder.Append("Adds roles: ")
                    .AppendJoin(", ", add)
                    .AppendLine();
            }

            var remove = GetRolesToRemove();
            if (remove.Count() > 0)
            {
                builder.Append("Removes roles: ")
                    .AppendJoin(", ", remove)
                    .AppendLine();
            }

            var toggle = GetRolesToToggle();
            if (toggle.Count() > 0)
            {
                builder.Append("Toggles roles: ")
                    .AppendJoin(", ", toggle)
                    .AppendLine();
            }

            if (Reply)
            {
                builder.AppendLine("Also replies with the changes applied.");
            }

            return builder.ToString();
        }
    }
}
