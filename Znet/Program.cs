using Znet.Client;
using Znet.Server;

namespace Znet
{
    class Program
    {
        static void Main(string[] args)
        {
            ZServer server = new ZServer();
            server.Start(12345);

            ZClient client = new ZClient();
            client.Start(12345, 12300);
            client.Connect();
        }
    }
}