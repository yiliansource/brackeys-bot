using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using BrackeysBot.Commands;

using Newtonsoft.Json;

using Discord;

namespace BrackeysBot.Modules
{
    /// <summary>
    /// Represents a custom command, that can have various, customizeable functionalities!
    /// </summary>
    public class CustomCommand
    {
        /// <summary>
        /// A result than can be returned from a role operation.
        /// </summary>
        protected class RoleOperationResult
        {
            /// <summary>
            /// Represents a successful result.
            /// </summary>
            public static RoleOperationResult Success => new RoleOperationResult { IsError = false };

            /// <summary>
            /// The names of the conflicting roles that the user has.
            /// </summary>
            public string[] ConflictingOwnedRoles { get; set; }
            /// <summary>
            /// The names of the conflicting roles that the user doesn't have.
            /// </summary>
            public string[] ConflictingUnownedRoles { get; set; }
            /// <summary>
            /// Is the result an error?
            /// </summary>
            public bool IsError { get; set; }
        }

        /// <summary>
        /// The message that will be printed by the command.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;
        /// <summary>
        /// Should the sent message be sent as an embed?
        /// </summary>
        [JsonProperty("embed")]
        public bool Embed { get; set; } = false;
        /// <summary>
        /// The role operations that will be perfomed, seperated by ','.
        /// </summary>
        [JsonProperty("roleOperations")]
        public string RoleOperations { get; set; } = null;

        /// <summary>
        /// Executes the command with the specified user in the specified channel.
        /// </summary>
        public async Task Execute(IGuildUser user, IMessageChannel channel)
        {
            RoleOperationResult result = await PerformRoleOperations(user);
            if (result.IsError)
            {
                // If there are any error results, print the errors.
                EmbedBuilder eb = new EmbedBuilder()
                    .WithTitle("Info")
                    .WithDescription(string.Join('\n',
                        result.ConflictingOwnedRoles.Select(r => $"You already have the role {r}!")
                            .Concat(result.ConflictingUnownedRoles.Select(r => $"You don't have the role {r}!"))));

                await channel.SendMessageAsync(string.Empty, false, eb);
            }
            else
            {
                // If not, print the regular command message.
                await PerformMessageOperation(channel);
            }
        }

        /// <summary>
        /// Prints the message for this command to the specified channel.
        /// </summary>
        private async Task PerformMessageOperation(IMessageChannel channel)
        {
            if (string.IsNullOrEmpty(Message)) return;

            // Conditionally encapsulate the message in an embed
            if (Embed) await channel.SendMessageAsync(string.Empty, false, new EmbedBuilder().WithDescription(Message));
            else await channel.SendMessageAsync(Message);
        }
        /// <summary>
        /// Adds roles prefixed with '+' to the user and removes roles prefixed with '-' from the user.
        /// </summary>
        private async Task<RoleOperationResult> PerformRoleOperations(IGuildUser user)
        {
            if (string.IsNullOrEmpty(RoleOperations)) return RoleOperationResult.Success;

            List<string> conflictingOwnedRoles = new List<string>();
            List<string> conflictingUnownedRoles = new List<string>();

            foreach (string roleOperation in RoleOperations.Split(','))
            {
                string trimmed = roleOperation.Trim();

                char operation = trimmed[0];
                string roleName = trimmed.Substring(1);

                // For example: The RoleOperation "+MyRole" will have '+' as the operation and "MyRole" as the roleName.

                IRole role = RoleCommand.GetRoleByName(user.Guild, roleName);

                if (role == null) throw new InvalidOperationException($"The role {roleName} does not exist.");

                switch (operation)
                {
                    case '+':
                        if (!user.HasRole(roleName)) await user.AddRoleAsync(role);
                        else conflictingOwnedRoles.Add(roleName);
                        break;
                    case '-':
                        if (user.HasRole(roleName)) await user.RemoveRoleAsync(role);
                        else conflictingUnownedRoles.Add(roleName);
                        break;

                    default:
                        throw new InvalidOperationException("A role must be prefixed by either '+' or '-' in order to be processed.");
                }
            }

            return new RoleOperationResult
            {
                ConflictingOwnedRoles = conflictingOwnedRoles.ToArray(),
                ConflictingUnownedRoles = conflictingUnownedRoles.ToArray(),
                IsError = (conflictingOwnedRoles.Count + conflictingUnownedRoles.Count) > 0
            };
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
