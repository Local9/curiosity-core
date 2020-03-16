using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Systems.Client.Extensions;
using Curiosity.Systems.Library.Models;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace Curiosity.Systems.Client.Environment
{
    public class Marker
    {
        public CuriosityPlugin Curiosity { get; set; }
        public Position Position { get; set; }
        public float RenderDistance { get; set; } = 15f;
        public float Scale { get; set; } = 1.5f;
        public MarkerType MarkerType { get; set; } = MarkerType.HorizontalSplitArrowCircle;
        public Color Color { get; set; } = Color.FromArgb(235, 0, 150, 235);
        public Predicate<Marker> Condition { get; set; }
        public Control Control { get; set; } = Control.Context;
        public string Message { get; set; } = "Press ~INPUT_CONTEXT~ to...";
        public event Action Callback;

        public Marker(Position position)
        {
            Debug.WriteLine($"Creating marker with position: {position}");
            Position = position;
            Curiosity = CuriosityPlugin.Instance;
        }

        public void Show()
        {
            Curiosity.AttachTickHandler(OnTick);
        }

        public void Hide()
        {
            Curiosity.DetachTickHandler(OnTick);
        }

        private async Task OnTick()
        {
            if (Curiosity?.Local?.Entity != null)
            {
                var position = Cache.Entity.Position;
                var distance = position.Distance(Position, true);

                if (distance < RenderDistance)
                {
                    World.DrawMarker(MarkerType, Position.AsVector(), Vector3.Zero, Vector3.Zero,
                        new Vector3(Scale, Scale, Scale),
                        Color.FromArgb(Color.A, Color.R, Color.G, Color.B));

                    if (distance < Scale && (Condition?.Invoke(this) ?? true))
                    {
                        API.BeginTextCommandDisplayHelp("STRING");
                        API.AddTextComponentSubstringPlayerName(Message);
                        API.EndTextCommandDisplayHelp(0, false, true, -1);

                        if (Game.IsControlJustPressed(0, Control) && Callback?.GetInvocationList().Length > 0)
                        {
                            foreach (var invocation in Callback.GetInvocationList())
                            {
                                ((Action) invocation).Invoke();
                            }
                        }
                    }
                }
                else
                {
                    await BaseScript.Delay(Convert.ToInt32(distance * 2));
                }
            }
            else
            {
                Curiosity = CuriosityPlugin.Instance;

                await BaseScript.Delay(1000);
            }
        }
    }
}