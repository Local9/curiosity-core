using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Menus.Client.net.Extensions;
using Curiosity.Menus.Client.net.Static;
using Curiosity.Shared.Client.net.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Menus.Client.net.Classes.Scripts
{
    class Companion
    {
        static Ped ped;
        static bool IsHuman;
        static Client client;
        static int failureCount = 0;

        static public void Init()
        {
            client = Client.GetInstance();
            Relationships.SetupRelationShips();
        }

        static public async void CreateCompanion(PedHash pedHash, bool isHuman)
        {
            if (ped != null)
            {
                if (ped.Exists())
                {
                    if (IsHuman)
                        ped.PlayAmbientSpeech("GENERIC_BYE", SpeechModifier.Standard);

                    await Client.Delay(500);
                    API.NetworkFadeOutEntity(ped.Handle, false, false);
                    await Client.Delay(500);
                    ped.Delete();
                }
            }

            Relationships.SetupRelationShips();

            Vector3 offset = new Vector3(1f, 0f, 0f);
            Vector3 spawn = Game.PlayerPed.GetOffsetPosition(offset);
            float groundZ = spawn.Z;
            Vector3 groundNormal = Vector3.Zero;

            if (API.GetGroundZAndNormalFor_3dCoord(spawn.X, spawn.Y, spawn.Z, ref groundZ, ref groundNormal))
            {
                spawn.Z = groundZ;
            }

            ped = await World.CreatePed(pedHash, spawn, Game.PlayerPed.Heading);

            if (Game.PlayerPed.IsInVehicle())
                ped.Task.WarpIntoVehicle(Game.PlayerPed.CurrentVehicle, VehicleSeat.Any);

            API.NetworkFadeInEntity(ped.Handle, false);

            if (isHuman)
            {
                IsHuman = true;
                Weapon weapon = Game.PlayerPed.Weapons.BestWeapon;

                ped.Weapons.Give(WeaponHash.CombatPistol, 99, false, true);
                ped.Weapons.Give(weapon.Hash, 1, false, true);

                ped.DropsWeaponsOnDeath = false;
            }

            ped.Health = 200;
            ped.Armor = 100;
            ped.CanSufferCriticalHits = false;

            ped.LeaveGroup();
            API.SetPedRagdollOnCollision(ped.Handle, false);
            ped.Task.ClearAll();

            PedGroup currentPedGroup = Game.PlayerPed.PedGroup;
            currentPedGroup.SeparationRange = 2.14748365E+09f;
            if (!currentPedGroup.Contains(Game.PlayerPed))
            {
                currentPedGroup.Add(Game.PlayerPed, true);
            }
            if (!currentPedGroup.Contains(ped))
            {
                currentPedGroup.Add(ped, false);
            }
            ped.CanBeTargetted = false;
            ped.Accuracy = 100;
            ped.IsInvincible = false;
            ped.IsPersistent = true;
            ped.RelationshipGroup = Game.PlayerPed.RelationshipGroup;
            ped.NeverLeavesGroup = true;
            ped.CanSwitchWeapons = true;

            Blip blip1 = ped.AttachBlip();
            blip1.Color = BlipColor.Blue;
            blip1.Scale = 0.7f;
            blip1.Name = "Friend";

            await Client.Delay(500);

            ped.PlayAmbientSpeech("GENERIC_HI", SpeechModifier.Standard);

            client.RegisterTickHandler(OnCompanionTick);
        }

        private static async Task OnCompanionTick()
        {
            if (ped == null)
            {
                failureCount++;

                if (failureCount >= 5)
                    client.DeregisterTickHandler(OnCompanionTick);

                return;
            }

            if (!ped.Exists())
            {
                failureCount++;

                if (failureCount >= 5)
                    client.DeregisterTickHandler(OnCompanionTick);

                return;
            }

            failureCount = 0;

            if (Game.PlayerPed.IsInVehicle() && !ped.IsInVehicle() && ped.Position.Distance(Game.PlayerPed.Position) > 50f)
            {
                ped.Task.WarpIntoVehicle(Game.PlayerPed.CurrentVehicle, VehicleSeat.Any);
            }

            if (ped.IsDead && Game.PlayerPed.Position.Distance(ped.Position) < 2.5f && !Game.PlayerPed.IsInVehicle())
            {
                Screen.DisplayHelpTextThisFrame($"Press ~INPUT_CONTEXT~ to revive companion.");

                if (Game.IsControlJustPressed(0, Control.Context))
                {
                    API.NetworkFadeOutEntity(ped.Handle, false, false);
                    await Client.Delay(500);
                    ped.Resurrect();
                    ped.Health = 200;
                    ped.Armor = 100;
                    API.NetworkFadeInEntity(ped.Handle, false);

                    PedGroup currentPedGroup = Game.PlayerPed.PedGroup;
                    currentPedGroup.SeparationRange = 2.14748365E+09f;
                    if (!currentPedGroup.Contains(Game.PlayerPed))
                    {
                        currentPedGroup.Add(Game.PlayerPed, true);
                    }
                    if (!currentPedGroup.Contains(ped))
                    {
                        currentPedGroup.Add(ped, false);
                    }

                    await Client.Delay(500);
                }
            }

            if (Game.PlayerPed.IsDead && ped.IsAlive && !Game.PlayerPed.IsInVehicle())
            {
                while (ped.Position.Distance(Game.PlayerPed.Position) > 2f)
                {
                    ped.Task.GoTo(Game.PlayerPed);
                    await Client.Delay(500);
                }

                Client.TriggerEvent("curiosity:Client:Player:Revive");
                await Client.Delay(3000);
            }

            if (!Game.PlayerPed.IsInVehicle() && ped.IsInVehicle() && !IsHuman)
            {
                API.NetworkFadeOutEntity(ped.Handle, false, false);
                ped.Position = Client.CurrentVehicle.GetOffsetPosition(new Vector3(2f, 0f, 0f));
                API.NetworkFadeInEntity(ped.Handle, false);
            }
        }

        public static async void RemoveCompanion()
        {
            if (ped != null)
            {
                if (ped.Exists())
                {
                    if (IsHuman)
                        ped.PlayAmbientSpeech("GENERIC_BYE", SpeechModifier.Standard);

                    await Client.Delay(500);
                    API.NetworkFadeOutEntity(ped.Handle, false, false);
                    await Client.Delay(500);
                    ped.Delete();
                    client.DeregisterTickHandler(OnCompanionTick);
                }
            }
        }
    }
}
