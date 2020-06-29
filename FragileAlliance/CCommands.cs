using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace FragileAlliance
{
    class CCommands : BaseScript
    {
        public CCommands()
        {
            RegisterCommand("pos", new Action<int, List<object>, string>((src, args, raw) =>
            {
                Vector3 id = Game.PlayerPed.Position;
                Debug.WriteLine($"{id.X}, {id.Y}, {id.Z}, Heading={Game.PlayerPed.Heading}");
            }), false);

            RegisterCommand("kill", new Action<int, List<object>, string>((src, args, raw) =>
            {
                SetEntityHealth(GetPlayerPed(-1), 0);
            }), false);
        }
    }
}
