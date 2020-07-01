using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.UI;
using static CitizenFX.Core.Native.API;

namespace FragileAlliance
{
    class GUI : BaseScript
    {
        private static int ScreenX, ScreenY;

        public GUI()
        {
            Tick += OnTick;
        }

        private static async Task OnTick()
        {
            GetScreenActiveResolution(ref ScreenX, ref ScreenY);

            ScreenX /= 2;
            ScreenY /= 2;

            DeveloperGUI();

            await Task.FromResult(0);
        }

        private static void DeveloperGUI()
        {
            //Header
            int x = (int)(ScreenX * 1.0);
            int y = (int)(ScreenY * 1.0);

            Text entsHeader = new Text("Active Game-Related Entities", new PointF(x,y), 0.3f);
            entsHeader.Color = Color.FromArgb(255, 255, 255, 255);
            entsHeader.Draw();

            Dictionary<int, GameEntity> gameEnts = Arenas.GetGameEntities();
            foreach (KeyValuePair<int, GameEntity> entry in gameEnts)
            {
                y += (int)(ScreenY * 0.03);

                Text gameEnt = new Text($"[{entry.Value.ID}] = {entry.Key}, {entry.Value.Amount}", new PointF(x,y), 0.2f);
                gameEnt.Color = Color.FromArgb(255, 120, 120, 120);
                gameEnt.Draw();
            }
        }
    }
}
