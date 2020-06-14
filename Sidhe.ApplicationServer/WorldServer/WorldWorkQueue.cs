using Sidhe.ApplicationServer.Interfaces;
using Sidhe.ApplicationServer.Model;
using Sidhe.Utilities;

namespace Sidhe.ApplicationServer.WorldServer
{
    public class WorldWorkQueue : BlockingQueue<WorldWorkRequest>, IWorldWorkQueue { }
}
