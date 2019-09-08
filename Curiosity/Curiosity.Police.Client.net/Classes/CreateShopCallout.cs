using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Shared.Client.net.Helper;
using Curiosity.Shared.Client.net;
using System;
using System.Threading.Tasks;
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

                if (LocationBlip != null)
                {
                    if (LocationBlip.Exists())
                        LocationBlip.Delete();
                }
                //AcceptedCallout = false;
                //GameTime = API.GetGameTimer();

                Name = name;
                Location = location;
                SuspectModel = suspectModel;
                SuspectPosition = suspectLocation;
                ShopKeeperModel = shopkeeperModel;
                ShopKeeperPosition = shopkeeperLocation;

                SetupLocationBlip();

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

                while (API.GetDistanceBetweenCoords(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, Location.X, Location.Y, Location.Z, false) > 200.0f)
                {
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

                suspectGroup.SetRelationshipBetweenGroups(Client.PlayerRelationshipGroup, Relationship.Neutral, true);
                suspectGroup.SetRelationshipBetweenGroups(Environment.Job.DutyManager.PoliceRelationshipGroup, Relationship.Hate, true);
                suspectGroup.SetRelationshipBetweenGroups(suspectGroup, Relationship.Respect, true);

                await SuspectModel.Request(10000);
                Ped ped = await CreatePed.Create(suspectModel, SuspectPosition, suspectHeading, suspectGroup);
                SuspectModel.MarkAsNoLongerNeeded();
                await Client.Delay(5);

                API.GetPedAsGroupLeader(ped.Handle);

                ped.Task.TurnTo(location);
                Suspects.Add(ped);

                Model m = random.Next(1) == 1 ? PedHash.ArmGoon01GMM : PedHash.ArmGoon02GMY;
                await m.Request(10000);

                Ped p = await CreatePed.Create(m, Location, shopkeeperHeading - 45f, suspectGroup);
                m.MarkAsNoLongerNeeded();
                await Client.Delay(5);
                p.Task.WanderAround();
                Suspects.Add(p);

                if (random.Next(2) == 1)
                {
                    Model randomPedModel = random.Next(1) == 1 ? PedHash.Dealer01SMY : PedHash.KorLieut01GMY;
                    await randomPedModel.Request(10000);

                    Ped randomPed = await CreatePed.Create(randomPedModel, Location, shopkeeperHeading - 45f, suspectGroup);
                    randomPedModel.MarkAsNoLongerNeeded();
                    await Client.Delay(5);
                    randomPed.Task.WanderAround();
                    Suspects.Add(randomPed);
                }

                if (random.Next(5) == 1)
                {
                    Model randomPedModel = random.Next(1) == 1 ? PedHash.ExArmy01 : PedHash.Lost01GFY;
                    await randomPedModel.Request(10000);

                    Ped randomPed = await CreatePed.Create(randomPedModel, Location, shopkeeperHeading - 45f, suspectGroup);
                    randomPedModel.MarkAsNoLongerNeeded();
                    await Client.Delay(5);
                    randomPed.Task.WanderAround();
                    Suspects.Add(randomPed);
                }

                if (random.Next(10) == 1)
                {
                    Model randomPedModel = random.Next(1) == 1 ? PedHash.Lost01GMY : PedHash.Lost03GMY;
                    await randomPedModel.Request(10000);

                    Ped randomPed = await CreatePed.Create(randomPedModel, Location, shopkeeperHeading - 45f, suspectGroup);
                    randomPedModel.MarkAsNoLongerNeeded();
                    await Client.Delay(5);
                    randomPed.Task.WanderAround();
                    Suspects.Add(randomPed);
                }

                await Client.Delay(50);

                while (NativeWrappers.GetDistanceBetween(Game.PlayerPed.Position, Location) > 180.0f)
                {
                    List<Ped> SuspectCopy = new List<Ped>(Suspects);

                    int numberDead = 0;

                    foreach (Ped pedCopy in SuspectCopy)
                    {
                        if (pedCopy.Exists())
                        {
                            if (pedCopy.IsDead)
                            {
                                numberDead++;
                            }
                        }
                    }

                    if (numberDead == Suspects.Count)
                        return;

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
                bool pedFlee = false;

                while(Suspects.Count > 0)
                {
                    List<Ped> SuspectCopy = new List<Ped>(Suspects);

                    if (Game.PlayerPed.IsDead)
                    {
                        Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "Commissioner ", $"Better luck next time", $"Get back to the station and try again.", 2);
                        Client.TriggerEvent("curiosity:Client:Interface:Duty", true, false, "police");

                        foreach (Ped ped in SuspectCopy)
                        {
                            if (ped != null)
                            {
                                if (ped.Exists())
                                {
                                    API.SetEntityAsMissionEntity(ped.Handle, false, false);
                                    ped.MarkAsNoLongerNeeded();
                                }
                            }
                        }

                        if (ShopKeeper != null)
                        {
                            if (ShopKeeper.Exists())
                            {
                                ShopKeeper.MarkAsNoLongerNeeded();
                            }
                        }

                        Suspects.Clear();

                        break;
                    }
                    
                    if (NativeWrappers.GetDistanceBetween(Game.PlayerPed.Position, Location) > 250.0f)
                    {
                        Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "Commissioner ", $"well well well", $"I guess you're not cut out for police work.", 2);
                        Client.TriggerEvent("curiosity:Client:Interface:Duty", true, false, "police");

                        foreach (Ped ped in SuspectCopy)
                        {
                            if (ped != null)
                            {
                                if (ped.Exists())
                                {
                                    API.SetEntityAsMissionEntity(ped.Handle, false, false);
                                    ped.MarkAsNoLongerNeeded();
                                }
                            }
                        }

                        if (ShopKeeper != null)
                        {
                            if (ShopKeeper.Exists()) {
                                ShopKeeper.MarkAsNoLongerNeeded();
                            }
                        }

                        Suspects.Clear();

                        break;
                    }

                    await Client.Delay(100);

                    foreach(Ped ped in SuspectCopy)
                    {
                        await Client.Delay(100);
                        if (ped.IsDead)
                        {
                            if (!pedFlee && random.Next(2) == 1)
                            {
                                ShopKeeper.Task.FleeFrom(ped);
                                pedFlee = true;
                            }

                            Entity killer = ped.GetKiller();

                            if (killer != null)
                            {

                                if (killer.Handle == Game.PlayerPed.Handle)
                                {
                                    Vector3 dmgPos = ped.Position;
                                    int experience = random.Next(5, 11);
                                    if (ped.Bones.LastDamaged.Index == (int)Bone.SKEL_Head
                                        || ped.Bones.LastDamaged.Index == (int)Bone.IK_Head)
                                    {
                                        experience = 10;
                                    }
                                    Experience(dmgPos, experience, 2500, true);
                                }
                            }

                            API.SetEntityAsMissionEntity(ped.Handle, false, false);
                            ped.MarkAsNoLongerNeeded();
                            Suspects.Remove(ped);
                        }
                    }
                }

                if (ShopKeeper != null)
                {
                    if (ShopKeeper.Exists())
                    {
                        if (ShopKeeper.IsDead) // Player Check
                        {
                            Entity killer = ShopKeeper.GetKiller();

                            //foreach(CitizenFX.Core.Player p in Client.players)
                            //{
                            //    if () // NEED A SERVER EVENT
                            //}

                            if (killer.Handle == Game.PlayerPed.Handle)
                            {
                                await Client.Delay(0);
                                Experience(ShopKeeper.Position, random.Next(20, 30), 2500, false);
                                await Client.Delay(0);
                                Client.TriggerServerEvent("curiosity:Server:Bank:DecreaseCash", Player.PlayerInformation.playerInfo.Wallet, random.Next(100, 250));
                                await Client.Delay(0);
                                Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "Civilian Killed", $"Paid medical fees for civilian", string.Empty, 2);
                            }
                        }
                        ShopKeeper.MarkAsNoLongerNeeded();
                    }
                }
                
                Client.TriggerServerEvent("curiosity:Server:Bank:IncreaseCash", Player.PlayerInformation.playerInfo.Wallet, random.Next(80, 100));
                Client.TriggerServerEvent("curiosity:Server:Skills:Increase", $"{Enums.Skills.policexp}", random.Next(5, 10));
                Client.TriggerServerEvent("curiosity:Server:Skills:Increase", $"knowledge", random.Next(5, 8));

                await Client.Delay(10);
                Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "10-26", $"Location is clear", string.Empty, 2);
                API.ShowTickOnBlip(LocationBlip.Handle, true);
                Environment.Job.DutyManager.OnSetCallOutStatus(false);

                Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "Code 4", $"No further assistance needed", string.Empty, 2);
                await Tidy();
            }
            catch (Exception ex)
            {
                Log.Error($"CalloutCompleted -> {ex.ToString()}");
                EndCallout("There was an error found, callout ended. Sorry.");
            }
        }

        public static async void EndCallout(string message = "")
        {
            try
            {
                if (!string.IsNullOrEmpty(message))
                {
                    Client.TriggerEvent("curiosity:Client:Interface:Duty", true, false, "error");
                }

                List<Ped> peds = new List<Ped>(Suspects);

                foreach (Ped ped in peds)
                {
                    if (ped.Exists())
                    {
                        if (ped.AttachedBlip.Exists())
                        {
                            ped.AttachedBlip.Delete();
                        }
                    }

                    API.NetworkRequestControlOfNetworkId(ped.NetworkId);
                    ped.MarkAsNoLongerNeeded();
                    ped.Delete();
                }

                if (ShopKeeper != null)
                {
                    if (ShopKeeper.IsAlive)
                    {
                        ShopKeeper.Task.FleeFrom(Game.PlayerPed);
                    }
                    ShopKeeper.MarkAsNoLongerNeeded();
                }

                Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "10-7", $"Out of Service", message, 2);
                await Tidy();
            }
            catch (Exception ex)
            {
                Log.Error($"EndCallout -> {ex.ToString()}");
            }
        }

        static async Task Tidy()
        {
            try
            {
                if (LocationBlip != null)
                {
                    if (LocationBlip.Exists())
                    {
                        LocationBlip.ShowRoute = false;
                        API.SetBlipFade(LocationBlip.Handle, 0, 3000);
                        await Client.Delay(3000);
                        LocationBlip.Delete();
                        int handle = LocationBlip.Handle;
                        API.RemoveBlip(ref handle);
                    }
                }

                Suspects.Clear();
                Environment.Tasks.CalloutHandler.CalloutEnded();
            }
            catch (Exception ex)
            {
                Log.Error($"Tidy -> {ex.ToString()}");
            }
            await Task.FromResult(0);
        }

        static void Experience(Vector3 dmgPos, int xp, int timeout, bool increase)
        {
            string message = $"{xp}xp";
            if (increase)
            {
                Client.TriggerServerEvent("curiosity:Server:Skills:Increase", $"{Enums.Skills.policexp}", xp);
            }
            else
            {
                Client.TriggerServerEvent("curiosity:Server:Skills:Decrease", $"{Enums.Skills.policexp}", xp);
                message = $"-{xp}xp";
            }
            NativeWrappers.Draw3DTextTimeout(dmgPos.X, dmgPos.Y, dmgPos.Z, message, timeout, 40f, 60.0f);
        }
    }
}
