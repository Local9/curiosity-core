﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Shared.Client.net.Helper;
using System;

namespace Curiosity.Police.Client.net.Classes
{
    class CreateShopCallout
    {
        static Random random = new Random();
        static Client client = Client.GetInstance();

        static string Name;
        static Vector3 Location;
        static Blip LocationBlip;

        static Ped Suspect;
        static Model SuspectModel = PedHash.ChiCold01GMM;
        static Vector3 SuspectPosition;

        static Ped ShopKeeper;
        static Model ShopKeeperModel = PedHash.ShopKeep01;
        static Vector3 ShopKeeperPosition;

        //static bool AcceptedCallout = false;
        //static long GameTime = API.GetGameTimer();

        public static async void StartCallout(string name, Vector3 location, Model suspectModel, Vector3 suspectLocation, float suspectHeading, Model shopkeeperModel, Vector3 shopkeeperLocation, float shopkeeperHeading)
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

            Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 2, "459S", $"{Name}", string.Empty, 2);
            API.PlaySoundFrontend(-1, "Menu_Accept", "Phone_SoundSet_Default", true);

            await Client.Delay(0);

            if (LocationBlip != null)
            {
                if (LocationBlip.Exists())
                    LocationBlip.Delete();
            }

            SetupLocationBlip();

            while (API.GetDistanceBetweenCoords(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, Location.X, Location.Y, Location.Z, false) > 500.0f)
            {
                await Client.Delay(50);
            }

            if (ShopKeeper != null)
            {
                if (ShopKeeper.Exists())
                    ShopKeeper.Delete();
            }

            if (Suspect != null)
            {
                if (Suspect.Exists())
                    Suspect.Delete();
            }

            // PED
            await ShopKeeperModel.Request(10000);
            ShopKeeper = await World.CreatePed(ShopKeeperModel, ShopKeeperPosition, shopkeeperHeading);
            ShopKeeperModel.MarkAsNoLongerNeeded();
            // BLIP
            Blip shopKeeperBlip = ShopKeeper.AttachBlip();
            shopKeeperBlip.Alpha = 0;
            // TASK
            ShopKeeper.Task.Wait(-1);

            await SuspectModel.Request(10000);
            Suspect = await World.CreatePed(SuspectModel, SuspectPosition, suspectHeading);
            SuspectModel.MarkAsNoLongerNeeded();

            // Suspect.Task.FightAgainstHatedTargets(30.0f);

            Blip suspectBlip = Suspect.AttachBlip();
            suspectBlip.Sprite = BlipSprite.Enemy;
            suspectBlip.Color = BlipColor.Red;
            suspectBlip.Priority = 10;
            suspectBlip.IsShortRange = true;
            suspectBlip.Alpha = 0;

            Suspect.Weapons.Give(WeaponHash.Pistol, 30, true, true);
            Suspect.Task.AimAt(ShopKeeper, -1);
            ShopKeeper.Task.Cower(-1);
            Suspect.Accuracy = random.Next(30, 100);
            Suspect.AlwaysDiesOnLowHealth = true;

            uint suspectGroupHash = 0;
            API.AddRelationshipGroup("suspect", ref suspectGroupHash);
            API.SetRelationshipBetweenGroups(5, suspectGroupHash, Client.PlayerGroupHash);
            API.SetRelationshipBetweenGroups(5, Client.PlayerGroupHash, suspectGroupHash);
            
            while (NativeWrappers.GetDistanceBetween(Game.PlayerPed.Position, Location) > 40.0f)
            {
                await Client.Delay(50);
            }

            CitizenFX.Core.Player player = new CitizenFX.Core.Player(API.GetNearestPlayerToEntity(Suspect.Handle));

            Suspect.Task.ShootAt(player.Character, -1, FiringPattern.BurstFirePistol);
            suspectBlip.Alpha = 255;
            LocationBlip.ShowRoute = false;

            CalloutCompleted();
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
            while (Suspect.IsAlive)
            {
                await Client.Delay(100);
            }

            if (Suspect.IsDead)
            {
                Suspect.AttachedBlip.Delete();
                ShopKeeper.Task.ReactAndFlee(Suspect);
                ShopKeeper.MarkAsNoLongerNeeded();
                Suspect.MarkAsNoLongerNeeded();

                Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "10-26", $"Location is clear", string.Empty, 2);
                API.ShowTickOnBlip(LocationBlip.Handle, true);
                API.SetBlipFade(LocationBlip.Handle, 0, 3000);
                await Client.Delay(3000);
                LocationBlip.Delete();

                // API.PlaySoundFrontend(-1, "FLIGHT_SCHOOL_LESSON_PASSED", "HUD_AWARDS", true);

                Environment.Job.DutyManager.OnSetCallOutStatus(false);

                Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "Code 4", $"No further assistance needed", string.Empty, 2);
            }
        }

        public static void EndCallout()
        {
            if (Suspect != null)
            {
                Suspect.AttachedBlip.Delete();
                ShopKeeper.Task.ReactAndFlee(Suspect);
                ShopKeeper.MarkAsNoLongerNeeded();
                Suspect.MarkAsNoLongerNeeded();
            }
            LocationBlip.Delete();
            Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "10-7", $"Out of Service", string.Empty, 2);
        }
    }
}
