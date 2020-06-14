using Sidhe.ApplicationServer.Model;

namespace Sidhe.ApplicationServer.Network
{
    public static class Helpers
    {
        public static bool HasContents(this ClientResponse response)
            => (response?.Changes?.Length ?? -1) > 0;
    }
}
