using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace FragileAlliance
{
    class NetMessenger : BaseScript
    {
        public NetMessenger()
        {
            // Natives
            EventHandlers["onClientGameTypeStart"] += new Action(User.OnGameTypeStart);

            // User Net Messages
            EventHandlers["fa:onPlayerConnected"] += new Action(User.OnPlayerConnected);
            EventHandlers["fa:onPlayerDied"] += new Action(User.OnPlayerDied);

            // GameRules Net Messages
            EventHandlers["fa:setGameState"] += new Action<int, int, int, int, string>(GameRules.SetGameState);

            // ArenaData Net Messages
            EventHandlers["fa:spawnStartEvents"] += new Action(Arenas.SpawnOnStartEvents);
            EventHandlers["fa:addNewEventEntity"] += new Action<int,  string, int>(Arenas.AddGameEntity);
            EventHandlers["fa:pickupGameEntity"] += new Action<int, int>(Arenas.PickupGameEntity);
            EventHandlers["fa:removeGameEntity"] += new Action<int>(Arenas.RemoveGameEntity);

            // NetMessenger Net Messages
            EventHandlers["fa:pingForHost"] += new Action(PingForHost);
        }

        public static void PingForHost()
        {
            TriggerServerEvent("fa:srv_pingForHost");
        }
    }
}
