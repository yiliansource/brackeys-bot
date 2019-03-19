using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord.Commands;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BrackeysBot.Commands
{
    public class CustomizedCommand : ModuleBase
    {
        private readonly CustomCommandsTable _customCommands;

        public CustomizedCommand(CustomCommandsTable customCommands)
        {
            _customCommands = customCommands;
        }

        [Command("ccadd")]
        [PermissionRestriction(UserType.Staff)]
        [HelpData("ccadd <name> <message>", "Adds a command that can be customized with various features.")]
        public async Task AddCustomCommand (string name, [Remainder]string message)
        {
            string parsedCommand = ParseCommandInputToJSONString(message);

            if (_customCommands.Has(name))
            {
                _customCommands.Set(name, parsedCommand);
                await ReplyAsync("Custom command updated!");
            }
            else
            {
                _customCommands.Add(name, parsedCommand);
                await ReplyAsync("Custom command added!");
            }
        }

        [Command("ccdelete")]
        [PermissionRestriction(UserType.Staff)]
        [HelpData("ccdelete <name>", "Deletes the specified custom command.")]
        public async Task DeleteCustomCommand (string name)
        {
            if (_customCommands.Has(name))
            {
                _customCommands.Remove(name);

                await ReplyAsync("Custom command removed!");
                return;
            }
            else
            {
                await ReplyAsync("Custom command does not exist.");
            }
        }

        [Command("ccclear")]
        [PermissionRestriction(UserType.Staff)]
        [HelpData("ccclear", "Deletes all custom commands.")]
        public async Task ClearCustomCommands()
        {
            _customCommands.Clear();

            await ReplyAsync("Custom commands cleared!");
        }

        private string ParseCommandInputToJSONString(string input)
        {
            if (input.StartsWith('"') && input.EndsWith('"'))
            {
                // Input starts and ends with a ", therefore treat it as a plain string
                return $"{{\"Message\":{input}}}";
            }
            else if (input.StartsWith('{'))
            {
                // Input starts with a {, therefore treat it as a JSON string
                return input;
            }
            else
            {
                // Input doesn't have a specific starting character, therefore treat it as a list of arguments
                JObject command = new JObject();

                string[] arguments = input.Split(' ');
                for(int i = 0; i < arguments.Length; i++)
                {
                    string argument = arguments[i];
                    if (argument.StartsWith('-'))
                    {
                        // Treat it as an argument identifier, and the next one as a argument value
                        string identifier = argument.Substring(1);

                        if (i + 1 > arguments.Length)
                        {
                            throw new ArgumentException("Identifier doesn't have a following value.");
                        }

                        string value = arguments[++i];
                        string propertyValue = value;

                        if (value.StartsWith('"'))
                        {
                            // Treat as string, read until there are no more arguments, or another " is found.

                            List<string> valueList = new List<string> { value };

                            string currentInspectedArgument = value;
                            while (i + 1 < arguments.Length && !currentInspectedArgument.EndsWith('"'))
                            {
                                currentInspectedArgument = arguments[++i];
                                valueList.Add(currentInspectedArgument);
                            }

                            propertyValue = string.Join(' ', valueList.ToArray()).Trim('"');
                        }

                        command.Add(identifier, JToken.FromObject(propertyValue));
                    }
                    else
                    {
                        throw new ArgumentException("Argument doesn't have a preceding identifier.");
                    }
                }

                return command.ToString(Formatting.None);
            }
        }
    }
}
