using System;
using System.Collections.Generic;
using Discord;

namespace BrackeysBot.Services
{
    public class MathService : BrackeysBotService
    {
        private readonly DataService _data;
        private readonly Dictionary<ulong, int> _lastLatexUsages = new Dictionary<ulong, int>();

        /// <inheritdoc />
        public MathService(DataService data)
        {
            _data = data;
        }

        public void UpdateLatexTimeout(IUser user)
        {
            _lastLatexUsages.Remove(user.Id);
            _lastLatexUsages.Add(user.Id, Environment.TickCount);
        }
        
        public int LatexTimeoutRemaining(IUser user) 
        {
            int now = Environment.TickCount;
            int result = 0;

            if (_lastLatexUsages.TryGetValue(user.Id, out int last))
            {
                int passed = now - last;
                result = _data.Configuration.LatexTimeoutMillis - passed;

                if (result < 0) 
                    result = 0;
            }

            return result;
        }
    }
}
