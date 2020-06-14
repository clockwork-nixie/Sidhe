using JetBrains.Annotations;

namespace Sidhe.ApplicationServer.Model
{
    public class ConsoleRequest
    {
        public ConsoleRequest(ConsoleCommand command, [NotNull] string[] arguments)
        {
            Command = command;
            Arguments = arguments;
        }


        [NotNull] public string[] Arguments { get; set; }
        public ConsoleCommand Command { get; set; }
        public int UserId { get; set; }
    }
}