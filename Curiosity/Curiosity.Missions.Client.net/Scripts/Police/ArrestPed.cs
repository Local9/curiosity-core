using CitizenFX.Core;
using CitizenFX.Core.UI;
using Curiosity.Global.Shared.Data;
using Curiosity.Global.Shared.Enums;
using Curiosity.Shared.Client.net.Extensions;
using Curiosity.Shared.Client.net.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Missions.Client.Scripts.Police
{
    class ArrestPed
    {
        static Client client = Client.GetInstance();

        static public Ped ArrestedPed = null;
        static public Ped PedInHandcuffs = null;
        static public bool IsPedBeingArrested = false;
        static public bool IsPedCuffed = false;
        static public bool IsPedGrabbed = false;

        static List<BlipData> jailHouseBlips = new List<BlipData>();
        static List<Marker> jailHouseMarkers = new List<Marker>();

        public static async void Setup()
        {
            MarkerHandler.HideAllMarkers = false;
            await Client.Delay(100);
            SetupJailHouses();
            await Client.Delay(100);
            Screen.ShowNotification($"~b~Arrests~s~: ~g~Enabled");
            client.RegisterTickHandler(OnTaskJailPed);
        }

        public static void Dispose()
        {
            MarkerHandler.HideAllMarkers = true;
            MarkerHandler.Dispose();

            jailHouseBlips.ForEach(m => BlipHandler.RemoveBlip(m.BlipId));

            client.DeregisterTickHandler(OnTaskJailPed);
        }

        static async void SetupJailHouses()
        {
            if (jailHouseMarkers.Count > 0)
                jailHouseMarkers.Clear();

            System.Drawing.Color c = System.Drawing.Color.FromArgb(255, 65, 105, 225);

            jailHouseMarkers.Add(new Marker("~b~Bolingbroke Penitentiary\n~w~Jail Arrested Ped", new Vector3(1690.975f, 2592.581f, 44.71336f - 0.5f), c));
            jailHouseMarkers.Add(new Marker("~b~Paleto Bay PD\n~w~Jail Arrested Ped", new Vector3(-449.3008f, 6012.623f, 30.71638f - 0.5f), c));
            jailHouseMarkers.Add(new Marker("~b~Vespucci PD\n~w~Jail Arrested Ped", new Vector3(-1113.08f, -848.6609f, 12.4414f - 0.5f), c));
            jailHouseMarkers.Add(new Marker("~b~Eastbourn PD\n~w~Jail Arrested Ped", new Vector3(-583.1123f, -146.7518f, 38.23016f - 0.5f), c));
            jailHouseMarkers.Add(new Marker("~b~La Mesa\n~w~Jail Arrested Ped", new Vector3(830.4728f, -1310.793f, 27.13673f - 0.5f), c));
            jailHouseMarkers.Add(new Marker("~b~Rancho\n~w~Jail Arrested Ped", new Vector3(370.3031f, -1608.2098f, 28.2919f - 0.5f), c));
            jailHouseMarkers.Add(new Marker("~b~LSPD\n~w~Jail Arrested Ped", new Vector3(458.9393f, -1001.6194f, 23.9148f - 0.5f), c));
            jailHouseMarkers.Add(new Marker("~b~LSPD\n~w~Jail Arrested Ped", new Vector3(458.9213f, -997.9607f, 23.9148f - 0.5f), c));
            jailHouseMarkers.Add(new Marker("~b~LSPD\n~w~Jail Arrested Ped", new Vector3(460.7617f, -994.2283f, 23.9148f - 0.5f), c));

            await Client.Delay(100);

            foreach (Marker m in jailHouseMarkers)
            {
                int id = jailHouseBlips.Count + 1;
                jailHouseBlips.Add(new BlipData(id, "Jail", m.Position, BlipSprite.Handcuffs, BlipCategory.Unknown, BlipColor.Blue, true));
                MarkerHandler.MarkersAll.Add(m);
            }

            foreach (BlipData b in jailHouseBlips)
            {
                BlipHandler.AddBlip(b);
            }

            MarkerHandler.Init();
        }

        static async Task OnTaskJailPed()
        {
            Marker marker = MarkerHandler.GetActiveMarker();

            if (marker == null)
            {
                await Client.Delay(1000);
            }
            else
            {
                Screen.DisplayHelpTextThisFrame($"Press ~INPUT_PICKUP~ to book the ped.");

                if (Game.IsControlJustPressed(0, Control.Pickup))
                {
                    List<Ped> peds = World.GetAllPeds().Where(x => x.Position.Distance(marker.Position) < 20f).Select(p => p).ToList();

                    if (peds.Count == 0)
                    {
                        Screen.ShowNotification("~b~Arrests: ~w~No peds to arrest found near");
                    }

                    peds.ForEach(p =>
                    {
                        if (DecorExistOn(p.Handle, Client.DECOR_NPC_ARRESTED))
                        {
                            if (DecorGetBool(p.Handle, Client.DECOR_NPC_ARRESTED))
                                Client.TriggerEvent("curiosity:interaction:arrest", p.Handle, false);
                        }
                    });

                    await Client.Delay(5000);
                }
            }
        }

    }
}
