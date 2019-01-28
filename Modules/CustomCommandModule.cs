using System;
using System.Linq;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace BrackeysBot.Modules
{
    /// <summary>
    /// A module that handles custom commands.
    /// </summary>
    public class CustomCommandModule
    {
        /// <summary>
        /// The names of the registered commands.
        /// </summary>
        public string[] CommandNames => _table.CommandNames;

        private CustomCommandsTable _table;

        /// <summary>
        /// Creates a new custom command module, with data from a reference table.
        /// </summary>
        /// <param name="referenceTable"></param>
        public CustomCommandModule(CustomCommandsTable referenceTable)
        {
            _table = referenceTable;
        }

        /// <summary>
        /// Finds a command by its name and returns it, but returns null if no command was found.
        /// </summary>
        public CustomCommand FindCommand(string name)
        {
            string commandName = _table.CommandNames.FirstOrDefault(n => string.Equals(name, n, StringComparison.InvariantCultureIgnoreCase));

            if (commandName == null) return null;

            string json = _table.Get(commandName);
            CustomCommand command = JsonConvert.DeserializeObject<CustomCommand>(json);
            return command;
        }
        /// <summary>
        /// Parses and returns all commands in the module.
        /// </summary>
        public IEnumerable<CustomCommand> ParseCommandsFromTable()
        {
            foreach (string commandName in _table.CommandNames)
            {
                string json = _table.Get(commandName);
                CustomCommand command = JsonConvert.DeserializeObject<CustomCommand>(json);
                yield return command;
            }
        }
    }
}
