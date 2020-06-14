using System;
using System.Threading;

namespace Sidhe.ApplicationServer.Model
{
    public class WorldWorkRequest
    {
        private static int _sequence;


        public WorldWorkRequest()
        {
            RequestId = Math.Abs(Interlocked.Add(ref _sequence, 1));
        }


        public ManualResetEventSlim Complete { get; set; }
        public int RequestId { get; }
    }
}
