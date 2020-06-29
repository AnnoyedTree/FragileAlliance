using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace FragileAllianceServer
{
    class GameRules : BaseScript
    {
        public static int GameState;

        public static string arenaID { get; set; }

        private static int timeLeft { get; set; }
        private static int roundCount { get; set; }
        private static int maxRounds = 5;

        private static Player spawnHost;

        public GameRules()
        {
            SetGameState(Util.GameStates.STATE_WAITING);

            Tick += OnTick;
        }

        private async Task OnTick()
        {
            if (GameState == (int)Util.GameStates.STATE_GAMEOVER)
                return;
            else if (GameState == (int)Util.GameStates.STATE_WAITING)
            {
                if (Util.GetNumberPlayers() <= 0)
                    return;

                SetGameState(Util.GameStates.STATE_STARTING);
            }

            timeLeft--;
            if (timeLeft <= 0)
            {
                Util.GameStates state = (Util.GameStates)GameState;
                switch (state)
                {
                    case Util.GameStates.STATE_STARTING:
                        SetGameState(Util.GameStates.STATE_ACTIVE);
                        break;
                    case Util.GameStates.STATE_ACTIVE:
                        SetGameState(Util.GameStates.STATE_DONE);
                        break;
                    case Util.GameStates.STATE_DONE:
                        SetGameState(Util.GameStates.STATE_COOLDOWN);
                        break;
                    case Util.GameStates.STATE_COOLDOWN:
                        SetGameState(Util.GameStates.STATE_STARTING);
                        break;
                }

            }
            await Delay(1000);
        }

        public static async void SetGameState(Util.GameStates state)
        {
            GameState = (int)state;

            switch (state)
            {
                case Util.GameStates.STATE_WAITING:
                    break;
                case Util.GameStates.STATE_STARTING:
                    onStateStarting();
                    break;
                case Util.GameStates.STATE_ACTIVE:
                    await onStateActive();
                    break;
                case Util.GameStates.STATE_DONE:
                    onStateDone();
                    break;
                case Util.GameStates.STATE_COOLDOWN:
                    break;
                case Util.GameStates.STATE_GAMEOVER:
                    return;
            }
            TriggerClientEvent("fa:setGameState", GameState, timeLeft, roundCount, maxRounds, arenaID);
        }

        public static void SendGameState(Player player)
        {
            TriggerClientEvent("fa:setGameState", GameState, timeLeft, roundCount, maxRounds, arenaID);
        }

        public static void ResetGameState()
        {
            // Get new arena, maybe they dropped because the map sucks
            string arena = Arenas.GetRandomArena().ID;
            arenaID = arena;

            GameState = (int)Util.GameStates.STATE_WAITING;
            roundCount = 0;

            Debug.WriteLine($"[FA] All players dropped resetting GameState. New Arena -> {arena}");

        }

        // -----------------------------------
        // GAMERULES NETWORKED FUNCTIONS BEGIN

        // This recieves the message from client that 'getSpawnHost()' sends
        public static void OnPingHost([FromSource] Player player)
        {
            if (spawnHost == null)
                spawnHost = player;
        }

        public static void AddGameEntity([FromSource] Player player, int netID, string eventID, int amount)
        {
            Util.AddGameEntity(netID, eventID, amount);
        }

        public static void RequestEntityPickup([FromSource]Player player, int netID)
        {
            Util.RequestEntityPickup(player, netID);
        }

        // GAMERULES NETWORKED FUNCTIONS ENDS
        // ----------------------------------

        private static void onStateStarting()
        {
            Users.ResetTeams();
            timeLeft = 10;
            roundCount++;
        }

        private static async Task onStateActive()
        {
            timeLeft = (5 * 60); // Active match is for 5 minutes

            Player host = await getSpawnHost();
            Debug.WriteLine($"[FA] Response from '{host.Name}' -> Temporary SpawnHost");

            TriggerClientEvent("fa:spawnStartEvents");
        }

        private static void onStateDone()
        {
            timeLeft = 10; // 10 second cooldown before we show the end-of-stats screen

            if (roundCount >= maxRounds)
                SetGameState(Util.GameStates.STATE_GAMEOVER);

            TriggerClientEvent("fa:cleanupGameEntities");
            Util.CleanupGameEntities();
        }

        // Returns the enum instead of the int
        private static Util.GameStates getGameState(int state)
        {
            return (Util.GameStates)state;
        }

        // Awaits to ping the players for a spawn host
        private static async Task<Player> getSpawnHost()
        {
            spawnHost = null;
            TriggerClientEvent("fa:pingForHost");

            while (spawnHost == null)
                await Delay(1);

            return spawnHost;
        }
    }
}
