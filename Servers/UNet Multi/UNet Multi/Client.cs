using System.Net.Sockets;

namespace UNetMulti
{
    internal class Client
    {
        private TcpClient client;
        private NetworkStream stream;
        private byte[] buffer;

        public Client(TcpClient tcpClient)
        {
            client = tcpClient;
            stream = client.GetStream();
            buffer = new byte[4096];
            while(true)
            {
                stream.ReadAsync(buffer, 0, buffer.Length);
                ByteBuffer b = new ByteBuffer();
                b.WriteBytes(buffer);
            }
        }
    }
}