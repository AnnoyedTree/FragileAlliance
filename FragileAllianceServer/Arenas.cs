using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

namespace FragileAllianceServer
{
    internal class ArenaTeamData
    {
        public string ID;

        public List<float> playerSpawnArea;
    }
    class GameEvent
    {
        public static string ID, Type;

        public List<float> Location;
    }

    class SecurityGaurds
    {
        public List<float> SpawnLocation;
    }

    class ArenaData
    {
        public string ID, Title, Theme;

        public List<ArenaTeamData> Teams;

        [JsonProperty("GameEvents")]
        public List<GameEvent> Events;

        public List<SecurityGaurds> Gaurds;

        public List<GameEvent> OnStart;
    }

    class ArenasList
    {
        [JsonProperty("arenas")]
        public List<ArenaData> ArenaList { get; set; }

        public ArenaData GetRandomArena()
        {
            int c = ArenaList.Count;
            Random rand = new Random();
            int i = rand.Next(c);

            return ArenaList[i];
        }
    }

    class Arenas : BaseScript
    {
        public static ArenasList ArenaList;

        public Arenas()
        {
            string file = LoadResourceFile(GetCurrentResourceName(), "arenas.json");
            ArenaList = JsonConvert.DeserializeObject<ArenasList>(file);

            // Set random arena
            ArenaData data = ArenaList.GetRandomArena();
            GameRules.arenaID = data.ID;

            Debug.WriteLine($"[FA] Arena selected: {data.ID}");
        }

        public static ArenaData GetRandomArena()
        {
            return ArenaList.GetRandomArena();
        }

    }
}
