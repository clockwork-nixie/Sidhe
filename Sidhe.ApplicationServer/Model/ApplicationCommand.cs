using JetBrains.Annotations;

namespace Sidhe.ApplicationServer.Model
{
    public enum ApplicationCommand
    {
        [UsedImplicitly] Unknown = 0,
        BeginSession,
        EndSession,
        Execute,
        GetSession,
        ResetWorld
    }
}