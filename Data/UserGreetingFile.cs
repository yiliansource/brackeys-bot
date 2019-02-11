namespace BrackeysBot.Data
{
    public class UserGreetingFile : DataFile
    {
        public string Message { get; set; }

        public override string FileName => "greeting";

        protected override string SaveToString()
            => Message;
        protected override void LoadFromString(string value)
            => Message = value;
    }
}
