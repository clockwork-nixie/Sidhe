using System.Threading;

namespace Sidhe.ApplicationServer.Model
{
    public class Session
    {
        private static volatile int _sessionSequence;


        public Session(int userId)
        {
            IsValid = true;
            SessionId = Interlocked.Increment(ref _sessionSequence);
            UserId = userId;
        }

        public void Invalidate() { IsValid = false; }

        
        public bool IsValid { get; set; }
        public int SessionId { get; set; }
        public int UserId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
