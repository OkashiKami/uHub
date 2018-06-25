using System;
using System.Collections.Generic;
using System.Threading;

namespace uHub
{
    using uHub.Entities;
    using uHub.Utils;

    class Program
    {
        private static Thread threadConsole;
        private static  List<Action> actions = new List<Action>();

        public static void Log(object value)
        {
            actions.Add(() => 
            {
                Console.WriteLine("[" + DateTime.Now.ToShortDateString() + "|" +  DateTime.Now.ToShortTimeString() + "] " + value);
            });
        }
        public static void Log(string format, params object[] args)
        {
            actions.Add(() => 
            {
                Console.WriteLine("[" + DateTime.Now.ToShortDateString() + "|"  + DateTime.Now.ToShortTimeString() + "] " + string.Format(format, args));
            });
        }

        static void Main(string[] args)
        {
            threadConsole = new Thread(new ThreadStart(ConsoleThread));
            SetupServer();
            threadConsole.Start();
                        

        }

        static void SetupServer()
        {
            ServerTCP.clients = new List<Client>(Constants.MAX_PLAYERS);
            Types.tmpPlayers = new List<Types.TempPlayer>(Constants.MAX_PLAYERS);

            ServerDataHandler.InitMessages();
            ServerTCP.InitializeNetwork();
        }

        private static void ConsoleThread()
        {
            while (true)
            {
                for (int i = 0; i < ServerTCP.clients.Count; i++)
                {
                    if (ServerTCP.clients[i].isClosed)
                    {
                        ServerTCP.clients.RemoveAt(i);
                        Log("Client Removed");
                    }

                }
                if(actions.Count > 0)
                {
                    actions[0]?.Invoke();
                    actions.RemoveAt(0);
                }
            }
        }
    }
}
