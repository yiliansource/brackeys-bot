using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using BrackeysBot.Commands;

namespace BrackeysBot.Services
{
    public class ModuleService : BrackeysBotService
    {
        private readonly CommandService _commands;
        private readonly DataService _data;
        private readonly LoggingService _log;
        private readonly IServiceProvider _services;

        private Type[] _moduleTypes;

        public ModuleService(CommandService commands, DataService data, LoggingService log, IServiceProvider services)
        {
            _commands = commands;
            _data = data;
            _log = log;
            _services = services;

            _moduleTypes = GetModuleTypes();
        }

        public async Task Initialize()
        {
            int essentialCount = 0, otherCount = 0;

            foreach (Type essential in _moduleTypes.Where(t => t.HasAttribute<EssentialModuleAttribute>()))
            {
                await _commands.AddModuleAsync(essential, _services);
                essentialCount++;
            }
            foreach (var config in _data.Configuration.ModuleConfigurations)
            {
                if (config.Value)
                {
                    await _commands.AddModuleAsync(GetModuleTypeByName(config.Key), _services);
                    otherCount++;
                }
            }

            await _log.LogMessageAsync(new LogMessage(LogSeverity.Info, "Modules",
                $"Initialized the module service with {essentialCount} essential module(s) and {otherCount} other module(s)."));
        }

        public bool VerifyModuleName(ref string name)
        {
            Type match = GetModuleTypeByName(name);
            if (match != null)
            {
                name = SanitizeModuleName(match.Name);
                return true;
            }
            return false;
        }
        public bool CheckModuleChangeable(string name)
            => !GetModuleTypeByName(name).HasAttribute<EssentialModuleAttribute>();
        public bool GetModuleState(string name)
        {
            if (_data.Configuration.ModuleConfigurations.TryGetValue(name, out bool active))
            {
                return active;
            }
            return false;
        }
        public async Task SetModuleState(string name, bool active)
        {
            if (!_data.Configuration.ModuleConfigurations.TryAdd(name, active))
            {
                _data.Configuration.ModuleConfigurations[name] = active;
            }

            Type target = GetModuleTypeByName(name);
            if (active)
                await _commands.AddModuleAsync(target, _services);
            else
                await _commands.RemoveModuleAsync(target);

            _data.SaveConfiguration();
        }

        public string[] GetAllModuleNames()
            => _moduleTypes.Select(m => SanitizeModuleName(m.Name)).ToArray();
        public string[] GetEnabledModules()
            => _data.Configuration.ModuleConfigurations
                .Where(k => k.Value)
                .Select(k => k.Key)
                .ToArray();
        public string[] GetDisabledModules()
            => GetAllModuleNames().Except(GetEnabledModules().Concat(GetEssentialModules())).ToArray();
        public string[] GetEssentialModules()
            => _moduleTypes
                .Where(m => m.HasAttribute<EssentialModuleAttribute>())
                .Select(m => SanitizeModuleName(m.Name))
                .ToArray();

        private Type[] GetModuleTypes()
            => Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => !t.HasAttribute<ObsoleteAttribute>() && typeof(BrackeysBotModule).IsAssignableFrom(t) && !t.IsAbstract)
                .ToArray();
        private Type GetModuleTypeByName(string name)
            => _moduleTypes.FirstOrDefault(m => string.Equals(SanitizeModuleName(m.Name), name, StringComparison.InvariantCultureIgnoreCase));
        
        public static string SanitizeModuleName(string typeName)
            => typeName.Replace("Module", string.Empty);
    }
}