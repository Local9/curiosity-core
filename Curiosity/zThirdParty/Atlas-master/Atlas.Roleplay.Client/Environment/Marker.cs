using System;
using System.Drawing;
using System.Threading.Tasks;
using Atlas.Roleplay.Client.Extensions;
using Atlas.Roleplay.Library.Models;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Atlas.Roleplay.Client.Environment
{
    public class Marker
    {
        public AtlasPlugin Atlas { get; set; }
        public Position Position { get; set; }
        public float RenderDistance { get; set; } = 15f;
        public float Scale { get; set; } = 1.5f;
        public MarkerType MarkerType { get; set; } = MarkerType.HorizontalSplitArrowCircle;
        public Color Color { get; set; } = Color.FromArgb(235, 0, 150, 235);
        public Predicate<Marker> Condition { get; set; }
        public Control Control { get; set; } = Control.Context;
        public string Message { get; set; } = "Tryck ~INPUT_CONTEXT~ för att ...";
        public event Action Callback;
        public int Ticks { get; set; } = -1;

        public Marker(Position position)
        {
            Position = position;
            Atlas = AtlasPlugin.Instance;
        }

        public void Show()
        {
            Atlas.AttachTickHandler(OnTick);
        }

        public void Hide()
        {
            Atlas.DetachTickHandler(OnTick);
        }

        private async Task OnTick()
        {
            if (Atlas?.Local?.Entity != null)
            {
                var position = Cache.Entity.Position;
                var distance = position.Distance(Position, true);

                if (distance < RenderDistance && (Ticks == -1 || Ticks > 0))
                {
                    if (Ticks > 0) Ticks--;

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
                Atlas = AtlasPlugin.Instance;

                await BaseScript.Delay(1000);
            }
        }
    }
}