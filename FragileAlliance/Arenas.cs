using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Newtonsoft.Json;

namespace FragileAlliance
{
    public class ArenasList
    {
        [JsonProperty("arenas")]
        public List<ArenaData> ArenaList { get; set; }
    }

    public class SecurityGaurds
    {
        public List<float> SpawnLocation;

        public async void SpawnGaurds()
        {
            Ped gaurd = await EntityCreate.CreatePed(PedHash.Cop01SMY, 6, getSpawnLocation(), getHeading());
            int handle = gaurd.Handle;

            //PlaceObjectOnGroundProperly(handle);
            GiveWeaponToPed(gaurd.GetHashCode(), (uint)WeaponHash.Pistol, 120, false, true);

            gaurd.RelationshipGroup = GameRules.relationPolice;
            gaurd.RelationshipGroup.SetRelationshipBetweenGroups("criminal", Relationship.Hate, true);
            gaurd.RelationshipGroup.SetRelationshipBetweenGroups("traitor", Relationship.Hate, true);
            gaurd.RelationshipGroup.SetRelationshipBetweenGroups("cop", Relationship.Like, true);

            gaurd.Task.FightAgainstHatedTargets(1000.0f);
            //gaurd.Task.WanderAround();

            //SetPedAsNoLongerNeeded(ref handle);
            BaseScript.TriggerServerEvent("fa:srv_addGameEntity", gaurd.NetworkId, "cop_ped", -1);
        }

        private Vector3 getSpawnLocation()
        {
            return new Vector3(SpawnLocation[0], SpawnLocation[1], SpawnLocation[2]);
        }

        private float getHeading()
        {
            return SpawnLocation[3];
        }
    }

    public class ArenaTeamData
    {
        public string ID;
        public List<float> playerSpawnArea;

        // <Player Spawn Location>
        public Vector3 playerSpawnLocation()
        {
            return new Vector3(playerSpawnArea[0], playerSpawnArea[1], playerSpawnArea[2]);
        }

        public float getSpawnHeading()
        {
            return playerSpawnArea[3];
        }
    }

    public class GameEvent
    {
        public string ID;
        public string Type;
        public string Model;

        public List<float> Location;

        public Vector3 eventLocation()
        {
            return new Vector3(Location[0], Location[1], Location[2]);
        }

        public float getHeading()
        {
            return Location[3];
        }

        public async void SpawnEvent()
        {
            Debug.WriteLine($"[FA] Spawning Event: {ID}");

            Entity item = null;
            switch (Type.ToLower())
            {
                case "vehicle":
                    item = await EntityCreate.CreateVehicle((uint)GetHashKey(Model), eventLocation(), getHeading());
                    break;
                case "prop":
                    item = await EntityCreate.CreateProp(Model, eventLocation(), getHeading());
                    break;
            }

            if (item == null)
                return;

            BaseScript.TriggerServerEvent("fa:srv_addGameEntity", item.NetworkId, ID, -1);
        }
    }

    public class ArenaData
    {
        public string ID, Title, Theme;
        public List<ArenaTeamData> Teams;
        public List<GameEvent> GameEvents;

        [JsonProperty("Gaurds")]
        public List<SecurityGaurds> SecurityGaurds;

        [JsonProperty("OnStart")]
        public List<GameEvent> StartEvents;

        public ArenaTeamData getTeamData(string id)
        {
            foreach (ArenaTeamData data in Teams)
            {
                if (data.ID == id)
                    return data;
            }

            return null;
        }
    }

    public class Arenas : BaseScript
    {
        public static ArenasList ArenaList;
        public static Dictionary<int, GameEntity> gameEntities;

        public Arenas()
        {
            gameEntities = new Dictionary<int, GameEntity>();

            string file = LoadResourceFile(GetCurrentResourceName(), "arenas.json");
            ArenaList = JsonConvert.DeserializeObject<ArenasList>(file);
        }

        public static ArenaData getArenaData(string id)
        {
            foreach (ArenaData data in ArenaList.ArenaList)
            {
                if (id == data.ID)
                    return data;
            }
            return null;
        }

        public static void SpawnOnStartEvents()
        {
            ArenaData data = GameRules.GetArenaInfo();
            foreach (GameEvent events in data.StartEvents)
                events.SpawnEvent();

            foreach (SecurityGaurds guards in data.SecurityGaurds)
                guards.SpawnGaurds();
        }

        public static void AddGameEntity(int netID, string eventID, int amount)
        {
            GameEntity gameEnt = new GameEntity(netID, eventID, amount);
            gameEntities.Add(netID, gameEnt);
        }

        public static void PickupGameEntity(int netID, int amount)
        {
            User.AddCash(amount);

            if (NetworkDoesEntityExistWithNetworkId(netID))
            {
                int entID = NetworkGetEntityFromNetworkId(netID);
                DeleteEntity(ref entID);
            }
        }

        public static void RemoveGameEntity(int netID)
        {
            if (NetworkDoesNetworkIdExist(netID))
            {
                int entID = NetworkGetEntityFromNetworkId(netID);
                DeleteEntity(ref entID);
            }

            if (gameEntities.ContainsKey(netID))
                gameEntities.Remove(netID);
        }

        public static void CleanupGameEntities()
        {
            foreach (KeyValuePair<int, GameEntity> entry in GetGameEntities().ToList())
            {
                if (NetworkDoesNetworkIdExist(entry.Key))
                {
                    int entID = NetworkGetEntityFromNetworkId(entry.Key);
                    DeleteEntity(ref entID);
                }

                if (gameEntities.ContainsKey(entry.Key))
                    gameEntities.Remove(entry.Key);
            }
        }

        public static Dictionary<int, GameEntity> GetGameEntities()
        {
            return gameEntities;
        }
    }

}
