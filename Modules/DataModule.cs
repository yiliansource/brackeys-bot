namespace BrackeysBot.Modules
{
    /// <summary>
    /// Provides a module to store the data files.
    /// </summary>
    public class DataModule
    {
        public EventPointTable EventPoints { get; private set; }
        public SettingsTable Settings { get; private set; }
        public StatisticsTable Statistics { get; private set; }
        public RuleTable Rules { get; private set; }
        public CustomCommandsTable CustomCommands { get; private set; }
        public UnityDocs UnityDocs { get; private set; }
        public CooldownData Cooldowns { get; private set; }

        public MuteTable Mutes { get; private set; }

        public BanTable Bans { get; private set; }

        private static readonly string[] templateFiles = { "template-appsettings.json", "template-cooldowns.json" };

        public DataModule()
        {
        }

        /// <summary>
        /// Initializes the data files from the disk
        /// </summary>
        public void InitializeDataFiles ()
        {
            // Load the lookup files
            EventPoints = new EventPointTable();
            Settings = new SettingsTable();
            Statistics = new StatisticsTable();
            CustomCommands = new CustomCommandsTable();
            Rules = new RuleTable();
            Mutes = new MuteTable();
            Bans = new BanTable();

            UnityDocs = new UnityDocs("manualReference.json", "scriptReference.json");
            Cooldowns = CooldownData.FromPath("cooldowns.json");
        }
    }
}
