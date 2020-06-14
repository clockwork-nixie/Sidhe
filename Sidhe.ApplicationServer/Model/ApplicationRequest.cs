namespace Sidhe.ApplicationServer.Model
{
    public class ApplicationRequest
    {
        public ApplicationRequest(ApplicationCommand command)
        {
            Command = command;
        }


        public ApplicationCommand Command { get; }
        public string Data { get; set; }
        public string ErrorMessage { get; set; }
        public Session Session { get; set; }
        public int UserId { get; set; }
    }
}
