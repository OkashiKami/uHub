using System.Collections.Generic;

namespace UNetMulti
{
    public class Types
    {
        public static List<TempPlayer> tmpPlayers = new List<TempPlayer>();
        public struct TempPlayer
        {
            public ByteBuffer buffer;
            public long databytes;
            public long datapackets;
            public string id;
        }
    }
}
