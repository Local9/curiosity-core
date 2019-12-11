//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Curiosity.Missions.Client.net.Scripts.PedCreators;
//using CitizenFX.Core.Native;
//using CitizenFX.Core.UI;
//using CitizenFX.Core;
//using Curiosity.Shared.Client.net;
//using Curiosity.Shared.Client.net.Extensions;
//using Curiosity.Missions.Client.net.MissionPeds;
//using Curiosity.Missions.Client.net.MissionPedTypes;
//using Curiosity.Missions.Client.net.Extensions;
//using Curiosity.Missions.Client.net.Wrappers;
//using Curiosity.Global.Shared.net.Data;
//using Curiosity.Shared.Client.net.Models;
//using Curiosity.Global.Shared.net.Enums;
//using Curiosity.Global.Shared.net.Entity;
//using Curiosity.Global.Shared.net;
//using Newtonsoft.Json;

//namespace Curiosity.Missions.Client.net.Scripts.Police
//{
//    class ArrestPed
//    {
//        static Client client = Client.GetInstance();

//        static public Ped ArrestedPed = null;
//        static public Ped PedInHandcuffs = null;
//        static public bool IsPedBeingArrested = false;
//        static public bool IsPedCuffed = false;
//        static public bool IsPedGrabbed = false;

//        static List<BlipData> jailHouseBlips = new List<BlipData>();
//        static List<Marker> jailHouseMarkers = new List<Marker>();

//        public static async void Setup()
//        {
//            MarkerHandler.Init();
//            MarkerHandler.HideAllMarkers = false;

//            await Client.Delay(100);
//            Static.Relationships.SetupRelationShips();

//            await Client.Delay(100);
//            // kill it incase it doubles
//            client.DeregisterTickHandler(OnTaskArrestPed);
//            client.RegisterTickHandler(OnTaskArrestPed);

//            client.DeregisterTickHandler(OnTaskJailPed);
//            client.RegisterTickHandler(OnTaskJailPed);

//            await Client.Delay(100);
//            SetupJailHouses();

//            Screen.ShowNotification($"~b~Arrests~s~: ~g~Enabled");
//        }

//        public static void Dispose()
//        {
//            MarkerHandler.HideAllMarkers = true;
//            MarkerHandler.Dispose();

//            jailHouseBlips.ForEach(m => BlipHandler.RemoveBlip(m.BlipId));

//            client.DeregisterTickHandler(OnTaskArrestPed);
//            client.DeregisterTickHandler(OnTaskJailPed);
//            // Screen.ShowNotification("~b~Arrests~s~: ~r~Disabled");
//        }

//        static async void SetupJailHouses()
//        {
//            if (jailHouseMarkers.Count > 0)
//                jailHouseMarkers.Clear();

//            System.Drawing.Color c = System.Drawing.Color.FromArgb(255, 65, 105, 225);

//            jailHouseMarkers.Add(new Marker("~b~Bolingbroke Penitentiary\n~w~Jail Arrested Ped", new Vector3(1690.975f, 2592.581f, 44.71336f), c));
//            jailHouseMarkers.Add(new Marker("~b~Paleto Bay PD\n~w~Jail Arrested Ped", new Vector3(-449.3008f, 6012.623f, 30.71638f), c));
//            jailHouseMarkers.Add(new Marker("~b~Vespucci PD\n~w~Jail Arrested Ped", new Vector3(-1113.08f, -848.6609f, 12.4414f), c));
//            jailHouseMarkers.Add(new Marker("~b~Eastbourn PD\n~w~Jail Arrested Ped", new Vector3(-583.1123f, -146.7518f, 38.23016f), c));
//            jailHouseMarkers.Add(new Marker("~b~La Mesa\n~w~Jail Arrested Ped", new Vector3(830.4728f, -1310.793f, 27.13673f), c));
//            jailHouseMarkers.Add(new Marker("~b~Rancho\n~w~Jail Arrested Ped", new Vector3(370.3031f, -1608.2098f, 28.2919f), c));
//            jailHouseMarkers.Add(new Marker("~b~LSPD\n~w~Jail Arrested Ped", new Vector3(458.9393f, -1001.6194f, 23.9148f), c));
//            jailHouseMarkers.Add(new Marker("~b~LSPD\n~w~Jail Arrested Ped", new Vector3(458.9213f, -997.9607f, 23.9148f), c));
//            jailHouseMarkers.Add(new Marker("~b~LSPD\n~w~Jail Arrested Ped", new Vector3(460.7617f, -994.2283f, 23.9148f), c));

//            await Client.Delay(100);

//            foreach (Marker m in jailHouseMarkers)
//            {
//                int id = jailHouseBlips.Count + 1;
//                jailHouseBlips.Add(new BlipData(id, "Jail", m.Position, BlipSprite.Handcuffs, BlipCategory.Unknown, BlipColor.Blue, true));
//                MarkerHandler.MarkersAll.Add(m);
//            }

//            foreach (BlipData b in jailHouseBlips)
//            {
//                BlipHandler.AddBlip(b);
//            }
//        }

//        static async Task OnTaskJailPed()
//        {
//            await Task.FromResult(0);
//            Marker marker = MarkerHandler.GetActiveMarker();
//            if (marker == null)
//            {
//                await Client.Delay(500);
//            }
//            else
//            {
//                Screen.DisplayHelpTextThisFrame($"Press ~INPUT_PICKUP~ to book the ped.");

//                if (Game.IsControlJustPressed(0, Control.Pickup))
//                {
//                    /// MOVE INTO INTERACTIE PED


//                    if (ArrestedPed == null)
//                    {
//                        Screen.ShowNotification($"~o~You have no one to arrest, don't waste my time.");
//                        return;
//                    }

//                    API.NetworkRequestControlOfEntity(ArrestedPed.Handle);

//                    if (!IsPedCuffed)
//                    {
//                        List<string> vs = new List<string> { $"~o~WHY AREN'T THEY CUFFED!", "~o~Handcuff them you idoit!", "~r~WHAT IS YOUR MAJOR MALFUNCTION! PUT ON THE CUFFS!!!", "~r~Cuff them, fecking muppet!" };
//                        Screen.ShowNotification(vs[Client.Random.Next(vs.Count)]);
//                        return;
//                    }

//                    ArrestedPedData arrestedPedData = new ArrestedPedData();
//                    arrestedPedData.IsAllowedToBeArrested = TrafficStop.CanDriverBeArrested;

//                    arrestedPedData.IsDrunk = TrafficStop.IsDriverUnderTheInfluence;
//                    arrestedPedData.IsDrugged = TrafficStop.IsDriverUnderTheInfluenceOfDrugs;
//                    arrestedPedData.IsDrivingStolenCar = TrafficStop.HasVehicleBeenStolen;
//                    arrestedPedData.IsCarryingIllegalItems = TrafficStop.IsCarryingIllegalItems;

//                    arrestedPedData.IsAllowedToBeArrested = (arrestedPedData.IsDrunk || arrestedPedData.IsDrugged || arrestedPedData.IsDrivingStolenCar || arrestedPedData.IsCarryingIllegalItems);

//                    string encoded = Encode.StringToBase64(JsonConvert.SerializeObject(arrestedPedData));

//                    Client.TriggerServerEvent("curiosity:Server:Missions:ArrestedPed", encoded);

//                    if (arrestedPedData.IsAllowedToBeArrested)
//                    {
//                        Screen.ShowNotification($"~g~They've been booked.");
//                    }
//                    else
//                    {
//                        Screen.ShowNotification($"~r~Why have you arrested this person?! We had to let them go free!");
//                    }

//                    if (ArrestedPed != null)
//                    {
//                        ArrestedPed.IsPositionFrozen = false;
//                        if (ArrestedPed.IsInVehicle())
//                        {
//                            ArrestedPed.SetConfigFlag(292, false);
//                            ArrestedPed.Task.LeaveVehicle();
//                        }
//                        API.NetworkFadeOutEntity(ArrestedPed.Handle, true, false);
//                        await Client.Delay(5000);
//                        ArrestedPed.Delete();
//                        ArrestedPed = null;
//                    }

//                    if (TrafficStop.StoppedDriver != null)
//                    {
//                        TrafficStop.StoppedDriver.IsPositionFrozen = false;
//                        if (TrafficStop.StoppedDriver.IsInVehicle())
//                        {
//                            TrafficStop.StoppedDriver.SetConfigFlag(292, false);
//                            TrafficStop.StoppedDriver.Task.LeaveVehicle();
//                        }
//                        API.NetworkFadeOutEntity(TrafficStop.StoppedDriver.Handle, true, false);
//                        await Client.Delay(5000);
//                        TrafficStop.StoppedDriver.Delete();
//                    }

//                    await Client.Delay(5000);
//                }
//            }
//        }

//    }
//}
