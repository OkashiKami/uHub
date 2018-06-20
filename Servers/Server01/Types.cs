using System.Collections.Generic;

namespace uHub
{
    class Types
    {
        public static List<TempPlayer> tmpPlayers = new List<TempPlayer>(Constants.MAX_PLAYERS);
        public struct TempPlayer
        {
            public ByteBuffer buffer;
            public long databytes;
            public long datapackets;
            public string id;
        }
    }
}
