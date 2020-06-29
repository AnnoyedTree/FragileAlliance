using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace FragileAllianceServer
{
    class NetMessenger : BaseScript
    {
        public NetMessenger()
        {
            // Natives
            EventHandlers["playerDropped"] += new Action<Player>(Users.OnPlayerDropped);

            // GameRules Messages
            EventHandlers["fa:srv_pingForHost"] += new Action<Player>(GameRules.OnPingHost);
            EventHandlers["fa:srv_addGameEntity"] += new Action<Player, int, string, int>(GameRules.AddGameEntity);
            EventHandlers["fa:srv_reqEntityPickup"] += new Action<Player, int>(GameRules.RequestEntityPickup);

            // Users Messages
            EventHandlers["fa:srv_onClientConnected"] += new Action<Player>(Users.OnPlayerConnected);
            EventHandlers["fa:srv_onClientDropped"] += new Action<Player>(Users.OnPlayerDropped);
            EventHandlers["fa:srv_onPlayerKilled"] += new Action<Player, int>(Users.OnPlayerKilled);
            EventHandlers["fa:srv_onPlayerDied"] += new Action<Player, int>(Users.OnPlayerDied);
        }
    }
}
