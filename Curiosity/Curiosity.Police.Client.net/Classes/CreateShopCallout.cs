using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Shared.Client.net.Helper;
using Curiosity.Shared.Client.net;
using System;
using System.Collections.Generic;

namespace Curiosity.Police.Client.net.Classes
{
    class CreateShopCallout
    {
        static Random random = new Random();
        static Client client = Client.GetInstance();

        static List<Ped> Suspects = new List<Ped>();

        static string Name;
        static Vector3 Location;
        static Blip LocationBlip;

        static Model SuspectModel = PedHash.ChiCold01GMM;
        static Vector3 SuspectPosition;

        static Ped ShopKeeper;
        static Model ShopKeeperModel = PedHash.ShopKeep01;
        static Vector3 ShopKeeperPosition;

        //static bool AcceptedCallout = false;
        //static long GameTime = API.GetGameTimer();

        public static async void StartCallout(string name, Vector3 location, Model suspectModel, Vector3 suspectLocation, float suspectHeading, Model shopkeeperModel, Vector3 shopkeeperLocation, float shopkeeperHeading)
        {
            try
            {
                Environment.Job.DutyManager.OnSetCallOutStatus(true);
                //AcceptedCallout = false;
                //GameTime = API.GetGameTimer();

                Name = name;
                Location = location;
                SuspectModel = suspectModel;
                SuspectPosition = suspectLocation;
                ShopKeeperModel = shopkeeperModel;
                ShopKeeperPosition = shopkeeperLocation;

                // AN IDEA TO WORK ON
                //while (!AcceptedCallout)
                //{
                //    await Client.Delay(0);

                //    if ((API.GetGameTimer() - GameTime) > (1000 * 30))
                //    {
                //        Environment.Job.DutyManager.OnSetCallOutStatus(true);
                //        Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "Code 4", $"No further assistance needed", "", 2);
                //        return;
                //    }

                //    Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "459S", $"{Name}", "~y~Accept: ~g~E~n~~y~Decline: ~g~BACKSPACE", 2);
                //    if (Game.IsControlPressed(0, Control.CreatorAccept))
                //    {
                //        AcceptedCallout = true;
                //    }

                //    if (Game.IsControlPressed(0, Control.FrontendCancel))
                //    {
                //        Environment.Job.DutyManager.OnSetCallOutStatus(true);
                //        Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "10-4", $"Message received, understood", "Callout Declined", 2);
                //        return;
                //    }
                //}

                Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 2, "459S Burglar alarm, silent", $"{Name}", string.Empty, 2);
                API.PlaySoundFrontend(-1, "Menu_Accept", "Phone_SoundSet_Default", true);

                await Client.Delay(0);

                if (LocationBlip != null)
                {
                    if (LocationBlip.Exists())
                        LocationBlip.Delete();
                }

                SetupLocationBlip();

                while (API.GetDistanceBetweenCoords(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, Location.X, Location.Y, Location.Z, false) > 200.0f)
                {
                    LocationBlip.Name = string.Empty;
                    await Client.Delay(50);
                }

                if (ShopKeeper != null)
                {
                    if (ShopKeeper.Exists())
                        ShopKeeper.Delete();
                }

                if (Suspects.Count > 0)
                {
                    List<Ped> peds = Suspects;
                    foreach (Ped pedSus in peds)
                    {
                        pedSus.Delete();
                    }
                    Suspects.Clear();
                }

                // PED
                await ShopKeeperModel.Request(10000);
                ShopKeeper = await World.CreatePed(ShopKeeperModel, ShopKeeperPosition, shopkeeperHeading);
                ShopKeeperModel.MarkAsNoLongerNeeded();
                API.SetNetworkIdCanMigrate(ShopKeeper.NetworkId, true);
                // BLIP
                Blip shopKeeperBlip = ShopKeeper.AttachBlip();
                shopKeeperBlip.Alpha = 0;
                ShopKeeper.Task.Cower(-1);
                // TASK
                await Client.Delay(0);

                string group = "SUSPECT";
                RelationshipGroup suspectGroup = World.AddRelationshipGroup(group);
                suspectGroup.SetRelationshipBetweenGroups(Client.PlayerRelationshipGroup, Relationship.Hate, true);

                await SuspectModel.Request(10000);
                Ped ped = await CreatePed.Create(suspectModel, SuspectPosition, suspectHeading, suspectGroup);
                SuspectModel.MarkAsNoLongerNeeded();

                Suspects.Add(ped);

                if (random.Next(2) == 1)
                {
                    Model m = random.Next(1) == 1 ? PedHash.ArmGoon01GMM : PedHash.ArmGoon02GMY;
                    await m.Request(10000);
                    Ped p = await CreatePed.Create(m, Location, suspectHeading - 180f, suspectGroup);
                    m.MarkAsNoLongerNeeded();
                    Suspects.Add(p);
                }

                await Client.Delay(50);

                while (NativeWrappers.GetDistanceBetween(Game.PlayerPed.Position, Location) > 40.0f)
                {
                    await Client.Delay(50);
                }

                LocationBlip.ShowRoute = false;

                CalloutCompleted();
            }
            catch (Exception ex)
            {
                Log.Error($"StartCallout -> {ex.ToString()}");
            }
}

        static void SetupLocationBlip()
        {
            LocationBlip = new Blip(API.AddBlipForCoord(Location.X, Location.Y, Location.Z));
            LocationBlip.Sprite = BlipSprite.BigCircle;
            LocationBlip.Scale = 0.5f;
            LocationBlip.Color = (BlipColor)5;
            LocationBlip.Alpha = 126;
            LocationBlip.ShowRoute = true;
            LocationBlip.Priority = 9;
            LocationBlip.IsShortRange = true;

            API.SetBlipDisplay(LocationBlip.Handle, 5);
        }

        static async void CalloutCompleted()
        {
            try
            {
                int PedsAlive = Suspects.Count;

                while(PedsAlive > 0)
                {
                    await Client.Delay(100);
                    foreach(Ped ped in Suspects)
                    {
                        CitizenFX.Core.UI.Screen.ShowNotification($"PED: {Suspects.Count} | {ped.Handle} State: {ped.IsDead}");
                        Ped pedToCheck = new Ped(ped.Handle);
                        await Client.Delay(100);
                        if (pedToCheck.IsDead) // TODO : Why was this null?
                        {
                            pedToCheck.MarkAsNoLongerNeeded();
                            PedsAlive--;
                        }
                    }
                }

                ShopKeeper.Task.FleeFrom(Game.PlayerPed);
                ShopKeeper.MarkAsNoLongerNeeded();

                Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "10-26", $"Location is clear", string.Empty, 2);
                API.ShowTickOnBlip(LocationBlip.Handle, true);
                API.SetBlipFade(LocationBlip.Handle, 0, 3000);
                await Client.Delay(3000);
                LocationBlip.Delete();

                Environment.Job.DutyManager.OnSetCallOutStatus(false);

                Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "Code 4", $"No further assistance needed", string.Empty, 2);

                Tidy();
            }
            catch (Exception ex)
            {
                Log.Error($"CalloutCompleted -> {ex.ToString()}");
            }
        }

        public static void EndCallout()
        {
            try
            {
                List<Ped> peds = Suspects;
                foreach (Ped ped in peds)
                {
                    ped.AttachedBlip.Delete();
                    ShopKeeper.Task.ReactAndFlee(ped);
                    ShopKeeper.MarkAsNoLongerNeeded();
                    ped.MarkAsNoLongerNeeded();
                }

                if (LocationBlip != null)
                {
                    if (LocationBlip.Exists())
                        LocationBlip.Delete();
                }
                Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "10-7", $"Out of Service", string.Empty, 2);
                Tidy();
            }
            catch (Exception ex)
            {
                // 
            }
        }

        static void Tidy()
        {
            Suspects.Clear();
            Environment.Tasks.CalloutHandler.CalloutEnded();
        }
    }
}
