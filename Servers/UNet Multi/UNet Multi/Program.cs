using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UNetMulti
{
    class Program
    {
        
        static void Main(string[] args)
        {
            new Server();
            while (true) { }
        }
    }
}
