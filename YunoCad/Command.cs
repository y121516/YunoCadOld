namespace YunoCad
{
    public class Command
    {
        public string MenuName { get; } = "";
        public string CommandLine { get; } = "";

        public Command(string menuName, string commandLine)
        {
            MenuName = menuName;
            CommandLine = commandLine;
        }
    }
}
