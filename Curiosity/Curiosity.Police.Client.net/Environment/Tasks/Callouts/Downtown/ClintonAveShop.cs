using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net.Enums;
using System;

namespace Curiosity.Police.Client.net.Environment.Tasks.Callouts.Downtown
{
    class ClintonAveShop
    {
        static Random random = new Random();
        static Client client = Client.GetInstance();

        static string Name = "24/7 on Clinton Ave";
        static Vector3 Location = new Vector3(377.0479f, 324.0891f, 103.5665f);
        static Blip LocationBlip;

        static Ped Suspect;
        static Model SuspectModel = PedHash.ChiCold01GMM;
        static Vector3 SuspectPosition = new Vector3(361.1253f, 358.807f, 103.8156f);

        static Ped ShopKeeper;
        static Model ShopKeeperModel = PedHash.ShopKeep01;
        static Vector3 ShopKeeperPosition = new Vector3(372.8627f, 328.1952f, 103.5664f);

        static Vector3 StartHoldUpCoords = new Vector3(375.6602f, 325.6703f, 103.5664f);

        public static async void Init()
        {
            await Client.Delay(0);

            if (LocationBlip != null)
            {
                if (LocationBlip.Exists())
                    LocationBlip.Delete();
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
            ShopKeeper = await World.CreatePed(ShopKeeperModel, ShopKeeperPosition, 255.3216f);
            ShopKeeperModel.MarkAsNoLongerNeeded();
            // BLIP
            Blip shopKeeperBlip = ShopKeeper.AttachBlip();
            shopKeeperBlip.Alpha = 0;
            // TASK
            ShopKeeper.Task.Wait(-1);

            await SuspectModel.Request(10000);
            Suspect = await World.CreatePed(SuspectModel, SuspectPosition, 255.8121f);
            SuspectModel.MarkAsNoLongerNeeded();

            // Suspect.Task.FightAgainstHatedTargets(30.0f);

            Blip suspectBlip = Suspect.AttachBlip();
            suspectBlip.Sprite = BlipSprite.Enemy;
            suspectBlip.Color = BlipColor.Red;
            suspectBlip.Priority = 10;
            suspectBlip.IsShortRange = true;
            suspectBlip.Alpha = 0;

            CalloutCompleted();

            API.TaskFollowNavMeshToCoord(Suspect.Handle, StartHoldUpCoords.X, StartHoldUpCoords.Y, StartHoldUpCoords.Z, 2.0f, -1, 0.0f, true, 0);

            while (API.GetDistanceBetweenCoords(Suspect.Position.X, Suspect.Position.Y, Suspect.Position.Z, StartHoldUpCoords.X, StartHoldUpCoords.Y, StartHoldUpCoords.Z, false) > 1.0f)
            {
                await Client.Delay(50);
            }

            Suspect.Task.AimAt(ShopKeeper, -1);
            Suspect.Weapons.Give(WeaponHash.Pistol, 30, true, true);
            ShopKeeper.Task.Cower(-1);

            SetupLocationBlip(); // ALERT
            Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "Code: 459S", $"{Name}", string.Empty, 2);

            while (API.GetDistanceBetweenCoords(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, StartHoldUpCoords.X, StartHoldUpCoords.Y, StartHoldUpCoords.Z, false) > 250.0f)
            {
                await Client.Delay(50);
            }
            suspectBlip.Alpha = 255;
            LocationBlip.ShowRoute = false;

            Suspect.Accuracy = random.Next(30, 100);
            Suspect.AlwaysDiesOnLowHealth = true;

            uint suspectGroupHash = 0;
            API.AddRelationshipGroup("suspect", ref suspectGroupHash);
            API.SetRelationshipBetweenGroups(5, suspectGroupHash, Client.PlayerGroupHash);
            API.SetRelationshipBetweenGroups(5, Client.PlayerGroupHash, suspectGroupHash);

            while (API.GetDistanceBetweenCoords(Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z, StartHoldUpCoords.X, StartHoldUpCoords.Y, StartHoldUpCoords.Z, false) > 30.0f)
            {
                await Client.Delay(50);
            }

            Player player = new Player(API.GetNearestPlayerToEntity(Suspect.Handle));

            Suspect.Task.ShootAt(player.Character, -1, FiringPattern.BurstFirePistol);
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
            API.SetBlipAsMissionCreatorBlip(LocationBlip.Handle, true);
            API.SetBlipCategory(LocationBlip.Handle, 1);
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

                Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 1, "Code: 10-26", $"Location is clear", string.Empty, 2);
                API.ShowTickOnBlip(LocationBlip.Handle, true);
                API.SetBlipFade(LocationBlip.Handle, 0, 3000);
                await Client.Delay(3000);
                LocationBlip.Delete();
            }
        }

        public static void EndCallOut()
        {
            LocationBlip.Delete();
        }
    }
}
