using System.Net;

namespace FootballReport.Net
{
    internal static class ConnectivityCheck
    {
        internal static bool HasInternetByDns()
        {
            try
            {
                _ = Dns.GetHostEntry("example.com");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
