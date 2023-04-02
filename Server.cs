using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CS2_Server_Browser
{
    internal class Server
    {
        public string name { get; set; }
        public string ip { get; set; }
        public string port { get; set; }
        public Gamemode gamemode { get; set; }
        public string location { get; set; }
        public Status status { get; set; }
    }

    public enum Status
    {
        Online,
        Offline
    }

    public enum Gamemode
    {
        Competitive,
        Casual,
        Deathmatch,
        ArmsRace,
        Demolition,
        Wingman,
        DangerZone
    }
}
