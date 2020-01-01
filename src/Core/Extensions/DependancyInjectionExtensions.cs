using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using BrackeysBot.Services;

namespace BrackeysBot
{
    public static class DependancyInjectionExtensions
    {
        public static IServiceCollection AddBrackeysBotServices(this IServiceCollection col)
        {
            foreach (var service in Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => !t.HasAttribute<ObsoleteAttribute>() && t.Inherits<BrackeysBotService>() && !t.IsAbstract))
            {
                col.TryAddSingleton(service);
            }
            return col;
        }
    }
}
