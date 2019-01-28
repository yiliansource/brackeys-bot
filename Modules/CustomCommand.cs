using System;
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
        /// The message that will be printed by the command.
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; } = string.Empty;
        /// <summary>
        /// The role operations that will be perfomed, seperated by ','.
        /// </summary>
        [JsonProperty("roleOperations")]
        public string RoleOperations { get; set; } = string.Empty;

        /// <summary>
        /// Executes the command with the specified user in the specified channel.
        /// </summary>
        public async Task Execute(IGuildUser user, IMessageChannel channel)
        {
            await PerformRoleOperations(user);
            await PerformMessageOperation(channel);
        }

        /// <summary>
        /// Prints the message for this command to the specified channel.
        /// </summary>
        private async Task PerformMessageOperation(IMessageChannel channel)
        {
            if (string.IsNullOrEmpty(Message)) return;

            await channel.SendMessageAsync(Message);
        }
        /// <summary>
        /// Adds roles prefixed with '+' to the user and removes roles prefixed with '-' from the user.
        /// </summary>
        private async Task PerformRoleOperations(IGuildUser user)
        {
            if (string.IsNullOrEmpty(RoleOperations)) return;

            string[] roleOperations = RoleOperations.Split(',');
            foreach (string roleOperation in roleOperations)
            {
                char operation = roleOperation[0];
                string roleName = roleOperation.Substring(1);

                IRole role = RoleCommand.GetRoleByName(user.Guild, roleName);

                if (role == null) throw new InvalidOperationException($"The role {roleName} does not exist.");

                switch (operation)
                {
                    case '+':
                        if (!user.HasRole(roleName)) await user.AddRoleAsync(role);
                        else throw new InvalidOperationException($"You already have the role {roleName}.");
                        break;
                    case '-':
                        if (user.HasRole(roleName)) await user.RemoveRoleAsync(role);
                        else throw new InvalidOperationException($"You don't have the role {roleName}.");
                        break;

                    default:
                        throw new InvalidOperationException("A role must be prefixed by either '+' or '-' in order to be processed.");
                }
            }
        }

        public override string ToString()
        {
            return $"CustomCommand (\"{Message}\")";
        }
    }
}
