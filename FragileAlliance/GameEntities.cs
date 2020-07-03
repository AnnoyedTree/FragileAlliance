using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace FragileAlliance
{
    class GameEntities : BaseScript
    {
        private static int cashDelay;
        private static int cashBagID = 0;

        public static void HandleEntities(string eventID, int entID)
        {
            switch (eventID)
            {
                case "cash_crate":
                    cashCrate(entID);
                    break;
                case "money_bag_drop":
                    moneyBag(entID);
                    break;
            }
        }

        private static void cashCrate(int entID)
        {
            int ped = GetPlayerPed(PlayerId());

            float dist = EntityUtil.GetDistance(ped, entID, true);
            if (dist <= 2)
            {
                int time = GetGameTimer();
                if (time > cashDelay)
                {
                    User.AddCash(50);
                    cashDelay = time + 500;
                }
            }
        }

        private static void moneyBag(int entID)
        {
            int ped = GetPlayerPed(PlayerId());

            float dist = EntityUtil.GetDistance(ped, entID, true);
            if (dist <= 2)
            {
                int netID = NetworkGetNetworkIdFromEntity(entID);
                TriggerServerEvent("fa:srv_reqEntityPickup", netID);
            }
        }

        public static async void PickupBag(int ped)
        {
            cashBagID = -1;

            Vector3 pos = GetEntityCoords(ped, true);
            Prop prop = await EntityCreate.CreateProp("hei_p_f_bag_var6_bus_s", pos);

            int index = GetPedBoneIndex(ped, 0x60F0);
            AttachEntityToEntity(prop.Handle, ped, index, -0.09f, -0.05f, 0, 0, -90, 180, false, false, false, false, 0, true);

            cashBagID = prop.NetworkId;

            SetPedRelationshipGroupHash(ped, (uint)GetHashKey("traitor"));
            SetCanAttackFriendly(ped, true, true);
        }

        public static int GetCarryBag()
        {
            if (cashBagID <= 0)
                return 0;

            return NetworkGetEntityFromNetworkId(cashBagID);
        }

        public static void ResetCarryBag()
        {
            cashBagID = 0;
        }
    }
}
