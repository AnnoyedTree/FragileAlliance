using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace FragileAlliance
{
    class GameRules : BaseScript
    {
        public static string ArenaID;

        public static Util.GameStates GameState;
        public static int Time, Rounds, MaxRounds;

        public GameRules()
        {
            Tick += OnTick;
        }

        public static void SetGameState(int state, int time, int rounds, int maxRounds, string arenaID)
        {
            Debug.WriteLine($"[FA] GAME-STATE -> {(Util.GameStates)state}, {time}, {rounds}, {maxRounds}, {arenaID}");

            GameState = (Util.GameStates)state;
            Time = time;
            Rounds = rounds;
            MaxRounds = maxRounds;
            ArenaID = arenaID;

            User.HandleGameState(GameState);
        }

        public async Task OnTick()
        {
            Time--;

            switch (GameState)
            {
                case Util.GameStates.STATE_ACTIVE:
                    tickGameActive();
                    break;
            }
            await Delay(1);
        }

        public static ArenaData GetArenaInfo()
        {
            return Arenas.getArenaData(ArenaID);
        }

        // Privates
        private static void tickGameActive()
        {
            if (User.IsDead() || User.Team == Util.Teams.TEAM_POLICE)
                return;

            Dictionary<int, GameEntity> entities = Arenas.GetGameEntities();
            foreach (KeyValuePair<int, GameEntity> entry in entities)
            {
                if (NetworkDoesNetworkIdExist(entry.Key))
                {
                    int entID = NetworkGetEntityFromNetworkId(entry.Key);
                    bool exists = (entID > 0 && DoesEntityExist(entID));
                    if (exists)
                    {
                        string id = entities[entry.Key].ID;
                        GameEntities.HandleEntities(id, entID);
                    }
                }
            }
        }
    }
}
