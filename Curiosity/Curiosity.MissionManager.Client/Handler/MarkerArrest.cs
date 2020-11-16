using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Global.Shared.Data;
using Curiosity.Global.Shared.Enums;
using Curiosity.MissionManager.Client.Utils;
using Curiosity.Shared.Client.net.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Client.Handler
{
    internal class MarkerArrest
    {
        static PluginManager PluginInstance => PluginManager.Instance;

        static List<Marker> markers = new List<Marker>();
        static List<BlipData> blips = new List<BlipData>();

        static System.Drawing.Color markerColor = System.Drawing.Color.FromArgb(255, 65, 105, 225);

        internal static void Init()
        {
            MarkerHandler.HideAllMarkers = false;

            markers.Add(new Marker("~b~Bolingbroke Penitentiary\n~w~Book Suspect(s)", new Vector3(1690.975f, 2592.581f, 44.71336f - 0.5f), markerColor));
            markers.Add(new Marker("~b~Paleto Bay PD\n~w~Book Suspect(s)", new Vector3(-449.3008f, 6012.623f, 30.71638f - 0.5f), markerColor));
            markers.Add(new Marker("~b~Vespucci PD\n~w~Book Suspect(s)", new Vector3(-1113.08f, -848.6609f, 12.4414f - 0.5f), markerColor));
            markers.Add(new Marker("~b~Eastbourn PD\n~w~Book Suspect(s)", new Vector3(-583.1123f, -146.7518f, 38.23016f - 0.5f), markerColor));
            markers.Add(new Marker("~b~La Mesa\n~w~Book Suspect(s)", new Vector3(830.4728f, -1310.793f, 27.13673f - 0.5f), markerColor));
            markers.Add(new Marker("~b~Rancho\n~w~Book Suspect(s)", new Vector3(370.3031f, -1608.2098f, 28.2919f - 0.5f), markerColor));
            markers.Add(new Marker("~b~LSPD\n~w~Book Suspect(s)", new Vector3(458.9393f, -1001.6194f, 23.9148f - 0.5f), markerColor));
            markers.Add(new Marker("~b~LSPD\n~w~Book Suspect(s)", new Vector3(458.9213f, -997.9607f, 23.9148f - 0.5f), markerColor));
            markers.Add(new Marker("~b~LSPD\n~w~Book Suspect(s)", new Vector3(460.7617f, -994.2283f, 23.9148f - 0.5f), markerColor));
            markers.Add(new Marker("~b~County Sheriff\n~w~Book Suspect(s)", new Vector3(1852.139f, 3691.1f, 33.26702f - 0.5f), markerColor));

            int blipId = 0;

            foreach (Marker mark in markers)
            {
                BlipData blipData = new BlipData($"arrestBlip{blipId}", "Arrest Ped", mark.Position, BlipSprite.Handcuffs, BlipCategory.Unknown, BlipColor.Blue, true);
                blips.Add(blipData);
                BlipHandler.AddBlip(blipData);

                MarkerHandler.MarkersAll.Add(mark);
            }

            PluginInstance.RegisterTickHandler(OnArrestPedTick);
        }

        public static void Dispose()
        {
            MarkerHandler.HideAllMarkers = true;
            MarkerHandler.Dispose();

            blips.ForEach(m => BlipHandler.RemoveBlip(m.BlipName));
            markers.Clear();

            PluginInstance.DeregisterTickHandler(OnArrestPedTick);
        }

        private async static Task OnArrestPedTick()
        {
            Marker activeMarker = MarkerHandler.GetActiveMarker();

            if (activeMarker == null)
            {
                await BaseScript.Delay(1000);
                return;
            }

            Screen.DisplayHelpTextThisFrame($"Press ~INPUT_PICKUP~ to book the ~b~suspect(s)~s~.");

            if (Game.IsControlJustPressed(0, Control.Pickup))
            {
                List<Ped> peds = World.GetAllPeds().Where(x => x.IsInRangeOf(activeMarker.Position, 20f)).Select(p => p).ToList();

                if (peds.Count == 0)
                {
                    Screen.ShowNotification("~b~Arrests: ~s~No suspect(s) to book found near by.");
                }

                peds.ForEach(p =>
                {
                    bool arrested = Decorators.GetBoolean(p.Handle, Decorators.PED_ARRESTED) && Decorators.GetBoolean(p.Handle, Decorators.PED_HANDCUFFED);

                    if (arrested)
                    {
                        Mission.RegisteredPeds.ForEach(ped =>
                        {
                            if (ped.Handle == p.Handle)
                            {
                                ped.Dismiss();
                                Mission.CountArrest();
                            };
                        });
                    }
                });

                await PluginManager.Delay(5000);
            }
        }
    }
}
