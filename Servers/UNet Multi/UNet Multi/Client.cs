using System;
using System.Net.Sockets;

namespace UNetMulti
{
    internal class Client
    {
        private TcpClient client;
        private NetworkStream stream;
        private byte[] asyncbuff;

        public Client(TcpClient tcpClient)
        {
            client = tcpClient;
            client.ReceiveBufferSize = 4096;
            client.SendBufferSize = 4096;
            client.NoDelay = false;
            asyncbuff = new byte[4096];
            stream = client.GetStream();

            while(true)
            {
                stream.BeginRead(asyncbuff, 0, 8192, new AsyncCallback(OnRecieve), null);
            }
        }

        private void OnRecieve(IAsyncResult ar)
        {
            try
            {
                int byteAmt = stream.EndRead(ar);
                byte[] mybytes = new byte[byteAmt];
                Buffer.BlockCopy(asyncbuff, 0, mybytes, 0, byteAmt);
                if (byteAmt == 0) return;

                UThread.executeInUpdate(() =>
                {
                    ClientDataHandler.HandelData(mybytes);
                });
                mystream.BeginRead(asyncbuff, 0, 8192, new AsyncCallback(OnRecieve), null);
            }
            catch (Exception)
            {

            }
        }
    }
}