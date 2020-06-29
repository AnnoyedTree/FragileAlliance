using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;

namespace FragileAlliance
{
    class Util
    {
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

    }

    public class GameEntity
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

    public static class EntityCreate
    {
        public static bool bChangingSkins;

        public static async Task<Ped> CreatePed(Model model, int pedtype, Vector3 pos, float heading)
        {
            uint hash = (uint)model.Hash;

            if (!IsModelAPed(hash))
                return null;

            RequestModel(hash);

            while (!HasModelLoaded(hash))
                await BaseScript.Delay(1);

            Ped ped = new Ped(API.CreatePed((int)pedtype, hash, pos.X, pos.Y, pos.Z, heading, true, false));
            return ped;
        }

        public static async Task<Prop> CreateProp(string model, Vector3 pos, float rotation = 0, bool dynamic = true, bool OnGround = true)
        {
            uint hash = (uint)GetHashKey(model);

            if (!IsModelValid(hash))
                return null;

            RequestModel(hash);
            while (!HasModelLoaded(hash))
                await BaseScript.Delay(1);

            if (OnGround)
                pos.Z = World.GetGroundHeight(pos);

            //Prop prop = new Prop( CreateObjectNoOffset(hash, pos.X, pos.Y, pos.X, networked, true, dynamic) );
            Prop prop = new Prop(CreateObject((int)hash, pos.X, pos.Y, pos.Z, true, true, dynamic));
            prop.PositionNoOffset = pos;
            prop.Rotation = new Vector3(0, 0, rotation);

            NetworkRegisterEntityAsNetworked(prop.Handle);

            return prop;
        }

        public static async Task<Vehicle> CreateVehicle(uint hash, Vector3 pos, float heading, bool networked = true)
        {
            if (!IsModelInCdimage(hash))
                return null;

            RequestModel(hash);

            while (!HasModelLoaded(hash))
                await BaseScript.Delay(1);

            Vehicle veh = new Vehicle(API.CreateVehicle(hash, pos.X, pos.Y, pos.Z, heading, true, true));
            NetworkRegisterEntityAsNetworked(veh.Handle);

            return veh;
        }

        public static async Task<bool> SetSkin(string mdlName, int playerID)
        {
            bChangingSkins = true;

            uint mdlHash = (uint)GetHashKey(mdlName);

            if (!IsModelInCdimage(mdlHash))
            {
                bChangingSkins = false;
                return false;
            }

            RequestModel(mdlHash);
            while (!HasModelLoaded(mdlHash))
                await BaseScript.Delay(1);

            SetPlayerModel(playerID, mdlHash);

            int ped = GetPlayerPed(playerID);
            if (GetEntityModel(ped) != mdlHash)
            {
                bChangingSkins = false;
                return false;
            }

            bChangingSkins = false;
            return true;
        }
    }

    public class EntityUtil
    {
        public static float GetDistance(int Ent1, int Ent2, bool bUseZ = true)
        {
            Vector3 pos1 = GetEntityCoords(Ent1, false);
            Vector3 pos2 = GetEntityCoords(Ent2, false);
            return GetDistanceBetweenCoords(pos1.X, pos1.Y, pos1.Z, pos2.X, pos2.Y, pos2.Z, bUseZ);
        }
    }
}
