using System;
using System.Linq;
using BrackeysBot.Models.Database;
using Discord;

namespace BrackeysBot.Services
{
    public class DatabaseService : BrackeysBotService, IInitializeableService
    {
        private readonly DataService _dataService;

        private BotConfiguration _config;
        private DatabaseContext _context;
        
        public DatabaseService(DataService dataService)
        {
            _dataService = dataService;
        }
        public void Initialize()
        {
            _config = _dataService.Configuration;
            _context = new DatabaseContext(_config);
        }

        public DatabaseContext GetContext()
            => _context; 

        public Infraction GetInfraction(int infractionId)
            => _context.Infractions
                .AsQueryable()
                .Where(x => x.Id == infractionId)
                .FirstOrDefault();

        public Infraction[] GetAllInfractionsOfUser(ulong target)
            => _context.Infractions
                .AsQueryable()
                .Where(x => x.TargetUserId == target)
                .ToArray();

        public TemporaryInfraction GetActiveTemporaryInfractionForInfraction(Infraction infraction)
            => _context.TemporaryInfractions
                .AsQueryable()
                .Where(x => x.InfractionId == infraction.Id)
                .Where(x => x.EndDate > DateTime.UtcNow)
                .FirstOrDefault();
        
        public TemporaryInfraction GetActiveTemporaryInfractionForInfraction(int infractionId)
            => _context.TemporaryInfractions
                .AsQueryable()
                .Where(x => x.InfractionId == infractionId)
                .Where(x => x.EndDate > DateTime.UtcNow)
                .Join(_context.Infractions,
                    Temp => Temp.InfractionId,
                    Infr => Infr.Id,
                    (Temp, Infr) => Temp)
                .FirstOrDefault();

        public TemporaryInfraction[] GetActiveTemporaryInfractionsOfUser(ulong target, InfractionType type)
            => _context.Infractions
                .AsQueryable()
                .Where(x => x.TargetUserId == target)
                .Where(x => x.ModerationTypeId == (int) type)
                .Join(_context.TemporaryInfractions,
                    Infr => Infr.Id,
                    Temp => Temp.InfractionId,
                    (Infr, Temp) => Temp)
                .Where(x => x.EndDate > DateTime.UtcNow)
                .ToArray();

        public void RemoveInfraction(Infraction infr)
        {
            _context.Infractions.Remove(infr);

            _context.SaveChanges();
        }

        public void RemoveTemporaryInfraction(TemporaryInfraction tempInfraction)
        {
            _context.TemporaryInfractions.Remove(tempInfraction);

            _context.SaveChanges();
        }

        public void AddInfraction(Infraction infraction)
        {
            _context.Infractions.Add(infraction);

            _context.SaveChanges();
        }

        public void AddTempInfraction(Infraction infraction, TimeSpan duration)
        {
            _context.TemporaryInfractions.Add(new TemporaryInfraction {
                InfractionId = infraction.Id,
                EndDate = DateTime.UtcNow.Add(duration)
            });

            _context.SaveChanges();
        }

        public void StoreMessage(LoggedMessage message, Infraction boundTo = null)
        {
            _context.Messages.Add(message);

            if (boundTo != null) 
                _context.InfrMsgs.Add(new InfractionsMessages {
                    InfractionId = boundTo.Id,
                    MessageId = message.MsgId
                });

            _context.SaveChanges();
        }
    }
}