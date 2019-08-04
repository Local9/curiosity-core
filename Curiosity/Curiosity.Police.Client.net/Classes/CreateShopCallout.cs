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
                    Ped p = await CreatePed.Create(m, Location, shopkeeperHeading - 45f, suspectGroup);
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
                bool pedFlee = false;
                while(Suspects.Count > 0)
                {
                    await Client.Delay(100);

                    List<Ped> SuspectCopy = new List<Ped>(Suspects);

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

                            if (ped.AttachedBlip.Exists())
                            {
                                ped.AttachedBlip.Delete();
                            }

                            Entity killer = ped.GetKiller();

                            if (killer.Handle == Game.PlayerPed.Handle)
                            {
                                Vector3 dmgPos = ped.Position;
                                int experience = 10;
                                if (ped.Bones.LastDamaged.Index == (int)Bone.SKEL_Head
                                    || ped.Bones.LastDamaged.Index == (int)Bone.IK_Head)
                                {
                                    experience = 20;
                                }
                                Experience(dmgPos, experience, 2500, true);
                            }
                            ped.MarkAsNoLongerNeeded();
                            Suspects.Remove(ped);
                        }
                    }
                }

                if (ShopKeeper.IsDead) // Player Check
                {
                    Entity killer = ShopKeeper.GetKiller();
                    if (killer.Handle == Game.PlayerPed.Handle)
                    {
                        await Client.Delay(10);
                        Experience(ShopKeeper.Position, 30, 2500, false);
                        await Client.Delay(10);
                        Client.TriggerServerEvent("curiosity:Server:Bank:DecreaseCash", Player.PlayerInformation.playerInfo.Wallet, random.Next(100, 250));
                        await Client.Delay(10);
                        Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "Civilian Killed", $"Paid medical fees for civilian", string.Empty, 2);
                    }
                }

                Client.TriggerServerEvent("curiosity:Server:Bank:IncreaseCash", Player.PlayerInformation.playerInfo.Wallet, random.Next(100, 200));
                Client.TriggerServerEvent("curiosity:Server:Skills:Increase", $"{Enums.Skills.policexp}", random.Next(10, 16));
                Game.PlayerPed.Weapons.Current.Ammo = Game.PlayerPed.Weapons.Current.Ammo + random.Next(12);

                ShopKeeper.MarkAsNoLongerNeeded();
                await Client.Delay(10);
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

                    if (ShopKeeper.IsAlive)
                    {
                        ShopKeeper.Task.FleeFrom(Game.PlayerPed);
                    }
                    ShopKeeper.MarkAsNoLongerNeeded();
                    ped.MarkAsNoLongerNeeded();
                    peds.Remove(ped);
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
            NativeWrappers.Draw3DTextTimeout(dmgPos.X, dmgPos.Y, dmgPos.Z, message, timeout, 40.0f);
        }
    }
}
