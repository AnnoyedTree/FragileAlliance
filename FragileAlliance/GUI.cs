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
            int x = (int)(ScreenX * 0.01);
            int y = (int)(ScreenY * 0.8);

            Text entsHeader = new Text("Active Game-Related Entities", new PointF(x, y), 0.3f);
            entsHeader.Color = Color.FromArgb(255, 255, 255, 255);
            entsHeader.Draw();

            Dictionary<int, GameEntity> gameEnts = Arenas.GetGameEntities();
            foreach (KeyValuePair<int, GameEntity> entry in gameEnts)
            {
                y += (int)(ScreenY * 0.03);
                textWithShadow($"[{entry.Value.ID}] = {entry.Key}, {entry.Value.Amount}", new PointF(x, y), Color.FromArgb(255, 120, 120, 120), 0.2f);
            }

            // Cash
            x = (int)(ScreenX * 0.25);
            y = (int)(ScreenY * 1.35);

            textWithShadow($"$ {User.Cash}", new PointF(x, y), Color.FromArgb(255, 120, 255, 120), 0.2f);

            //Timeleft
            x = (int)(ScreenX * 0.8);
            y = (int)(ScreenY * 1.35);

            TimeSpan time = TimeSpan.FromSeconds(GameRules.Time);
            textWithShadow($"Timeleft: {time.ToString(@"mm\:ss")}", new PointF(x,y), Color.FromArgb(255, 255, 255, 255), 0.3f);
        }

        private static void textWithShadow(string text, PointF pos, Color color, float scale)
        {
            Text textShadow = new Text(text, new PointF(pos.X + 0.25f, pos.Y + 0.25f), scale);
            textShadow.Color = Color.FromArgb(255, 0, 0, 0);
            textShadow.Draw();

            Text textDraw = new Text(text, pos, scale);
            textDraw.Color = color;
            textDraw.Draw();
        }
    }
}
