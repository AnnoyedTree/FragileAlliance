using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FragileAllianceServer
{
    class PlayersScoreboard
    {
        public int TeamID;
        public int Score;
        public Player Player;

        public PlayersScoreboard(int TeamID, int Score, Player Player)
        {
            this.Player = Player;
            this.TeamID = TeamID;
            this.Score = Score;
        }
    }

    class GameEntity
    {
        public string ID;
        public int NetID, Amount;

        public GameEntity(int NetID, string ID, int Amount)
        {
            this.ID = ID;
            this.NetID = NetID;
            this.Amount = Amount;
        }
    }


    class Util : BaseScript
    {
        public static Dictionary<int, PlayersScoreboard> Scoreboard;
        public static Dictionary<int, GameEntity> GameEntities;

        public enum Teams
        {
            TEAM_CRIMINAL = 0,
            TEAM_TRAITOR,
            TEAM_POLICE
        }

        public enum GameStates
        { 
            STATE_WAITING = 0,
            STATE_STARTING,
            STATE_ACTIVE,
            STATE_DONE,
            STATE_COOLDOWN,
            STATE_GAMEOVER
        }

        public Util()
        {
            Scoreboard = new Dictionary<int, PlayersScoreboard>();
            GameEntities = new Dictionary<int, GameEntity>();
        }

        public static void AddPlayerToScoreboard(Player player)
        {
            int serverID = GetServerId(player);
            if (!Scoreboard.ContainsKey(serverID))
            {
                PlayersScoreboard newScore = new PlayersScoreboard(0, 0, player);
                Scoreboard.Add(serverID, newScore);

                Debug.WriteLine($"[FA] Player {player.Name} added to scoreboard.");
            }
        }

        public static void RemovePlayerFromScoreboard(Player player)
        {
            int serverID = GetServerId(player);
            if (Scoreboard.ContainsKey(serverID))
            {
                Scoreboard.Remove(serverID);
                Debug.WriteLine($"[FA] Player {player.Name} removed from scoreboard.");
            }
        }

        public static void AddGameEntity(int netID, string eventID, int amount)
        {
            Debug.WriteLine($"[FA] Game Entity Added: {netID}, {eventID}");

            GameEntity gameEnt = new GameEntity(netID, eventID, amount);
            GameEntities.Add(netID, gameEnt);

            TriggerClientEvent("fa:addNewEventEntity", netID, eventID, amount);
        }

        public static void RemoveGameEntity(int netID)
        {
            Debug.WriteLine($"[FA] Game Entity Removed: {netID}");

            if (GameEntities.ContainsKey(netID))
                 GameEntities.Remove(netID);
        }

        public static void RequestEntityPickup(Player player, int netID)
        {
            if (!GameEntities.ContainsKey(netID))
                return;

            GameEntity gameEnt = GameEntities[netID];
            if (gameEnt == null)
                return;

            RemoveGameEntity(netID);

            TriggerClientEvent(player, "fa:pickupGameEntity", netID, gameEnt.Amount);
            TriggerClientEvent("fa:removeGameEntity", netID);
        }

        public static void OnPlayerKilled(Player victim, int killerID)
        {
            Player killer = GetPlayerByServerId(killerID);
            if (Scoreboard.ContainsKey(killerID))
            {
                Scoreboard[killerID].TeamID = (int)Teams.TEAM_TRAITOR;
                //TriggerClientEvent("fa:onPlayerKilled", serverID);
            }
            OnPlayerDied(victim);
        }

        public static void OnPlayerDied(Player victim)
        {
            int serverID = GetServerId(victim);
            if (Scoreboard.ContainsKey(serverID))
                Scoreboard[serverID].TeamID = (int)Teams.TEAM_POLICE;

            if (GetNumCriminals() <= 0)
            {
                GameRules.SetGameState(GameStates.STATE_DONE);
                return;
            }

            TriggerClientEvent(victim, "h:onPlayerDied");
        }

        public static void CleanupGameEntities()
        {
            foreach (KeyValuePair<int, GameEntity> entry in GameEntities.ToList())
            {
                if (GameEntities.ContainsKey(entry.Key))
                    GameEntities.Remove(entry.Key);
            }
        }

        public static void ClearScoreboard()
        {
            if (Scoreboard == null || Scoreboard.Count <= 0)
                return;

            foreach(KeyValuePair<int, PlayersScoreboard> entry in Scoreboard.ToList())
            {
                if (Scoreboard.ContainsKey(entry.Key))
                    Scoreboard[entry.Key].Score = 0;
            }
        }

        public static int GetNumberPlayers()
        {
            return Scoreboard.Count;
        }

        public static int GetNumCriminals()
        {
            int count = 0;
            foreach (KeyValuePair<int, PlayersScoreboard> entry in Scoreboard)
            {
                if (Scoreboard[entry.Key].TeamID == (int)Teams.TEAM_CRIMINAL)
                    count++;
            }
            return count;
        }

        public static int GetServerId(Player player)
        {
            return Convert.ToInt32(player.Handle) <= 65535 ? Convert.ToInt32(player.Handle) : Convert.ToInt32(player.Handle) - 65535;
        }

        public static Player GetPlayerByServerId(int serverID)
        {
            if (Scoreboard.ContainsKey(serverID))
                return Scoreboard[serverID].Player;
            return null;
        }
    }
}
