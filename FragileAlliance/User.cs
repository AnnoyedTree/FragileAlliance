using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;


namespace FragileAlliance
{
    class User : BaseScript
    {
        public static Util.Teams Team;
        public static int Cash;
        public static int RespawnTimer;

        private static bool bisDead = false;
        private static int deathCount = 0;

        public User()
        {
            Tick += OnTick;
        }

        // If successful, this will call the function 'OnPlayerConnected'
        public static void OnGameTypeStart()
        {
            TriggerServerEvent("fa:srv_onClientConnected");
        }

        public static void OnPlayerConnected()
        {
            ShutdownLoadingScreen();
            DoScreenFadeIn(0);
            SetMaxWantedLevel(0);

            Debug.WriteLine($"[FA] Do we have a gamestate?? -> {GameRules.GameState}");
        }

        public static void OnPlayerDropped()
        {
            TriggerServerEvent("fa:srv_onClientDropped");
        }

        public static void OnPlayerDied()
        {
            Team = Util.Teams.TEAM_POLICE;
        }

        public static void AddCash(int amount)
        {
            Cash += amount;
            if (Cash < 0)
                Cash = 0;
            else if (Cash > 5000)
                Cash = 5000;
        }

        public static bool IsDead()
        {
            int ped = GetPlayerPed(PlayerId());
            if (IsEntityDead(ped) && !EntityCreate.bChangingSkins)
                return true;

            return false;
        }

        public static void HandleGameState(Util.GameStates gameState)
        {
            int playerID = PlayerId();
            int ped = GetPlayerPed(playerID);

            switch (gameState)
            {
                case Util.GameStates.STATE_WAITING:
                    onMatchWaiting(playerID, ped);
                    break;
                case Util.GameStates.STATE_STARTING:
                    onMatchStarting(playerID, ped);
                    break;
                case Util.GameStates.STATE_ACTIVE:
                    onMatchActive(playerID, ped);
                    break;
                case Util.GameStates.STATE_DONE:
                    onMatchDone(playerID, ped);
                    break;
                case Util.GameStates.STATE_COOLDOWN:
                    onMatchCooldown(playerID, ped);
                    break;
            }

        }

        // Private methods
        private async Task OnTick()
        {
            if (GameRules.GameState == Util.GameStates.STATE_DONE || GameRules.GameState == Util.GameStates.STATE_COOLDOWN)
                return;

            if (GameRules.GameState == Util.GameStates.STATE_STARTING)
                disableControlsOnCooldown();

            int playerID = PlayerId();
            int ped = GetPlayerPed(playerID);
            if (IsDead())
            {
                int time = GetGameTimer();
                if (!bisDead)
                {
                    bisDead = true;
                    onDeath(playerID, ped);
                    Debug.WriteLine($"Died -> {RespawnTimer}, {deathCount}");
                }
                else
                {
                    if (time > RespawnTimer)
                    {
                        bisDead = false;
                        await respawnPlayer(Util.Teams.TEAM_POLICE, playerID, ped);
                    }
                }
            }
            await Delay(1);
        }

        private static async void onDeath(int playerID, int ped)
        {
            if (GameRules.GameState == Util.GameStates.STATE_STARTING)
            {
                onMatchStarting(playerID, ped);
                return;
            }

            deathCount++;
            RespawnTimer = GetGameTimer() + ((5 * deathCount) * 1000);

            int killer = GetPedSourceOfDeath(ped);
            int killerID = NetworkGetPlayerIndexFromPed(killer);
            killerID = GetPlayerServerId(killerID);

            Debug.WriteLine($"Killer: {killer}, serverID: {killerID}");
            if (killerID > 0 && killerID != GetPlayerServerId(playerID))
            {
                Debug.WriteLine("Murdered");
                TriggerServerEvent("fa:srv_onPlayerKilled", killerID);
            }
            else
            {
                Debug.WriteLine("Death");
                TriggerServerEvent("fa:srv_onPlayerDied", killer);
            }

            if (Team == Util.Teams.TEAM_CRIMINAL)
            {
                int bag = GameEntities.GetCarryBag();
                if (DoesEntityExist(bag))
                {
                    float rot = GetEntityHeading(bag);
                    DeleteEntity(ref bag);

                    Vector3 pos = GetEntityCoords(ped, false);
                    Prop newbag = await EntityCreate.CreateProp("p_ld_heist_bag_s_1", pos, rot);
                    SetEntityCoords(newbag.Handle, pos.X, pos.Y, pos.Z, false, false, false, false);

                    int netID = NetworkGetNetworkIdFromEntity(newbag.Handle);
                    TriggerServerEvent("fa:srv_addGameEntity", netID, "money_bag_drop", Cash);
                }
            }
        }

        private static async Task respawnPlayer(Util.Teams team, int playerID, int ped)
        {
            string rulesID = "criminal";
            if (team == Util.Teams.TEAM_POLICE)
                rulesID = "police";

            ArenaTeamData rules = GameRules.GetArenaInfo().getTeamData(rulesID);
            if (rules == null)
                return;

            if (team == Util.Teams.TEAM_CRIMINAL)
                await EntityCreate.SetSkin("mp_m_freemode_01", playerID);
            else
                await EntityCreate.SetSkin("s_m_y_cop_01", playerID);

            ped = GetPlayerPed(playerID);
            SetPlayerInvincible(playerID, false);
            SetPlayerInvisibleLocally(playerID, false);
            SetEntityVisible(ped, true, true);
            SetEntityCollision(ped, true, true);
            SetCanAttackFriendly(ped, false, true);
            SetEntityHealth(ped, 200);
            SetPlayerMaxArmour(playerID, 100);
            SetPedArmour(ped, 100);

            Vector3 pos = rules.playerSpawnLocation();
            Debug.WriteLine(pos.ToString());
            pos.Z = World.GetGroundHeight(pos);

            SetEntityCoords(ped, pos.X, pos.Y, pos.Z, false, false, false, false);
            SetEntityRotation(ped, 0, 0, rules.getSpawnHeading(), 0, true);
            NetworkResurrectLocalPlayer(pos.X, pos.Y, pos.Z, rules.getSpawnHeading(), true, true);
            FreezeEntityPosition(ped, false);

            if (team == Util.Teams.TEAM_CRIMINAL)
            {
                SetPedComponentVariation(ped, 0, 0, 0, 2); // Face
                SetPedComponentVariation(ped, 2, 11, 4, 2); // Hair
                SetPedComponentVariation(ped, 4, 1, 5, 2); // Pantalon
                SetPedComponentVariation(ped, 6, 1, 0, 2); // Shoees
                SetPedComponentVariation(ped, 11, 7, 2, 2); // Jacket

                GiveWeaponToPed(ped, (uint)GetHashKey("WEAPON_CROWBAR"), 0, false, false);
                GiveWeaponToPed(ped, (uint)GetHashKey("WEAPON_DBSHOTGUN"), 10, false, false);
                GiveWeaponToPed(ped, (uint)GetHashKey("WEAPON_COMBATPDW"), 260, false, true);
                GiveWeaponToPed(ped, (uint)GetHashKey("WEAPON_MOLOTOV"), 1, false, false);

                Team = Util.Teams.TEAM_CRIMINAL;
            }
            else
            {
                GiveWeaponToPed(ped, (uint)GetHashKey("WEAPON_PISTOL"), 120, false, false);
                GiveWeaponToPed(ped, (uint)GetHashKey("WEAPON_STUNGUN"), 2, false, false);
                GiveWeaponToPed(ped, (uint)GetHashKey("WEAPON_NIGHTSTICK"), 0, false, false);
                GiveWeaponToPed(ped, (uint)GetHashKey("WEAPON_PUMPSHOTGUN"), 120, false, true);

                Team = Util.Teams.TEAM_POLICE;
            }

            SetupTeamRelationships();
            SetPedRelationshipGroupHash(ped, (uint)GetHashKey(rulesID));

            bisDead = false;
        }

        private static void onMatchWaiting(int playerID, int ped)
        {
            SetEntityVisible(ped, false, false);
            SetPlayerInvisibleLocally(playerID, true);
        }

        private static async void onMatchStarting(int playerID, int ped)
        {
            await respawnPlayer(Util.Teams.TEAM_CRIMINAL, playerID, ped);

            ped = GetPlayerPed(playerID);
            SetPlayerInvincible(playerID, true);
            SetEntityCollision(ped, false, false);
            FreezeEntityPosition(ped, true);
        }

        private static void onMatchActive(int playerID, int ped)
        {
            FreezeEntityPosition(ped, false);
            SetEntityCollision(ped, true, true);
            SetPlayerInvincible(playerID, false);
        }

        private static void onMatchDone(int playerID, int ped)
        {
            SetPlayerInvincible(playerID, true);
            FreezeEntityPosition(ped, true);
        }

        private static void onMatchCooldown(int playerID, int ped)
        {
            SetEntityCollision(ped, false, false);
            SetPlayerInvisibleLocally(playerID, true);
            SetEntityVisible(ped, false, false);
        }

        private static void SetupTeamRelationships()
        {
            SetRelationshipBetweenGroups((int)Relationship.Hate, (uint)GetHashKey("criminal"), (uint)GetHashKey("police"));
            SetRelationshipBetweenGroups((int)Relationship.Hate, (uint)GetHashKey("criminal"), (uint)GetHashKey("traitor"));
            SetRelationshipBetweenGroups((int)Relationship.Hate, (uint)GetHashKey("police"), (uint)GetHashKey("criminal"));
            SetRelationshipBetweenGroups((int)Relationship.Hate, (uint)GetHashKey("police"), (uint)GetHashKey("traitor"));
            SetRelationshipBetweenGroups((int)Relationship.Hate, (uint)GetHashKey("traitor"), (uint)GetHashKey("police"));
            SetRelationshipBetweenGroups((int)Relationship.Hate, (uint)GetHashKey("traitor"), (uint)GetHashKey("criminal"));
            SetRelationshipBetweenGroups((int)Relationship.Hate, (uint)GetHashKey("traitor"), (uint)GetHashKey("traitor"));
        }

        private static void disableControlsOnCooldown()
        {
            Control[] controls = { Control.Aim, Control.Attack, Control.Attack2, Control.MeleeAttack1, Control.MeleeAttack2, Control.MoveUpDown, Control.MoveLeftRight };
            foreach (Control i in controls)
                Game.DisableControlThisFrame(0, i);
        }
    }
}
