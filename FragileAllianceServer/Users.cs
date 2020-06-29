using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace FragileAllianceServer
{
    class Users : BaseScript
    {
        public static void OnPlayerDropped([FromSource]Player player)
        {
            Util.RemovePlayerFromScoreboard(player);

            // All players dropped
            //if (Util.GetNumberPlayers() <= 0)
                //GameRules.ResetGameState();
        }

        public static void OnPlayerConnected([FromSource] Player player)
        {
            Util.AddPlayerToScoreboard(player);
            TriggerClientEvent(player, "fa:onPlayerConnected");

            GameRules.SendGameState(player);
        }

        public static void OnPlayerKilled([FromSource]Player player, int killerID)
        {
            Util.OnPlayerKilled(player, killerID);
        }

        public static void OnPlayerDied([FromSource] Player player, int killerID)
        {
            Util.OnPlayerDied(player);
        }

        public static void ResetTeams()
        {
            foreach (KeyValuePair<int, PlayersScoreboard> entry in Util.Scoreboard)
                Util.Scoreboard[entry.Key].TeamID = (int)Util.Teams.TEAM_CRIMINAL;
        }
    }
}
