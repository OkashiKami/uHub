using System;
using System.Collections.Generic;
using System.Threading;

namespace uHub
{
    class Program
    {
        private static Thread threadConsole;
        private static  List<Action> actions = new List<Action>();

        public static void Log(object data) { actions.Add(() => { Console.WriteLine(data);  }); }
        public static void Log(string format, object data) { actions.Add(() => { Console.WriteLine(format, data); }); }


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
                if(actions.Count > 0)
                {
                    actions[0]?.Invoke();
                    actions.RemoveAt(0);
                }
            }
        }
    }
}
