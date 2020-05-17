using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Menus.Client.net.Classes.Data;
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
        static bool hasNotMoved;
        static CompanionData currentCompanion;

        static Vector3 lastLocation;
        static long LastMovement;

        const string ANIM_DICT_IDLE = "idle_a";
        const string ANIM_RETRIEVER_IDLE = "creatures@retriever@amb@world_dog_sitting@idle_a";
        const string ANIM_ROTTWEILER_IDLE = "creatures@rottweiler@amb@world_dog_sitting@idle_a";

        const string ANIM_DICT_TRICKS = "creatures@rottweiler@tricks@";
        const string ANIM_SIT_ENTER = "sit_enter";
        const string ANIM_SIT_LOOP = "sit_loop";
        const string ANIM_SIT_EXIT = "sit_exit";

        static public void Init()
        {
            client = Client.GetInstance();
            Relationships.SetupRelationShips();

            client.RegisterTickHandler(OnPlayerMovementTask);
        }

        private static async Task OnPlayerMovementTask()
        {
            await Client.Delay(5000);
            if (Game.PlayerPed.Position.Distance(lastLocation) > 3f)
            {
                LastMovement = API.GetGameTimer();
                lastLocation = Game.PlayerPed.Position;
                hasNotMoved = false;
            }
            else
            {
                hasNotMoved = true;
            }
        }

        static public async void CreateCompanion(CompanionData companionData)
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

            ped = await World.CreatePed(companionData.PedHash, spawn, Game.PlayerPed.Heading);

            currentCompanion = companionData;

            if (Game.PlayerPed.IsInVehicle())
                ped.Task.WarpIntoVehicle(Game.PlayerPed.CurrentVehicle, VehicleSeat.Any);

            API.NetworkFadeInEntity(ped.Handle, false);

            if (companionData.IsHuman)
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

            if (currentCompanion.CanInteract && hasNotMoved)
            {
                if ((API.GetGameTimer() - LastMovement) < 10000
                    && !API.IsEntityPlayingAnim(ped.Handle, ANIM_DICT_TRICKS, ANIM_SIT_LOOP, 2))
                {
                    ped.Task.TurnTo(Game.PlayerPed, 2000);
                    await BaseScript.Delay(1000);
                    ped.Task.PlayAnimation(ANIM_DICT_TRICKS, ANIM_SIT_ENTER, 1f, -1, AnimationFlags.StayInEndFrame);
                    await BaseScript.Delay(2000);
                    ped.Task.PlayAnimation(ANIM_DICT_TRICKS, ANIM_SIT_LOOP, 1f, -1, AnimationFlags.Loop);
                    await BaseScript.Delay(2000);
                }
            }
            else if (currentCompanion.CanInteract && !hasNotMoved && API.IsEntityPlayingAnim(ped.Handle, ANIM_DICT_TRICKS, ANIM_SIT_LOOP, 3))
            {
                ped.Task.PlayAnimation(ANIM_DICT_TRICKS, ANIM_SIT_EXIT, 1f, -1, AnimationFlags.StayInEndFrame);
                await BaseScript.Delay(1000);
                ped.Task.ClearAll();
            }
        }

        //private void Pat()
        //{
        //    int num = Function.Call<int>(4558360841545231921L, new InputArgument[] { Game.get_Player().get_Character(), 26610 });
        //    Function.Call<Vector3>(4947482061849130808L, new InputArgument[] { Game.get_Player().get_Character(), num });
        //    if ((!(this.CommonPet != null) || !this.CommonPet.get_IsAlive() || this.HuskyIsPetting ? false : !Function.Call<bool>(-2038431349897255266L, new InputArgument[] { Game.get_Player().get_Character() })))
        //    {
        //        this.HuskyIsPetting = true;
        //        this.inAction = true;
        //        this.CameraObject = this.CommonPet;
        //        this.followCamera = true;
        //        if (Game.get_Player().get_Character().get_Position().DistanceTo(this.CommonPet.get_Position()) <= 4f)
        //        {
        //            Game.DisableAllControlsThisFrame();
        //            Function.Call(6544360553900626860L, new InputArgument[] { Game.get_Player().get_Character(), this.CommonPet, 1000 });
        //            Function.Call(6544360553900626860L, new InputArgument[] { this.CommonPet, Game.get_Player().get_Character(), 1000 });
        //            Script.Wait(1000);
        //            float x = Game.get_Player().get_Character().get_Position().X;
        //            float y = Game.get_Player().get_Character().get_Position().Y;
        //            float z = Game.get_Player().get_Character().get_Position().Z;
        //            float single = Game.get_Player().get_Character().get_Rotation().X;
        //            float y1 = Game.get_Player().get_Character().get_Rotation().Y;
        //            float z1 = Game.get_Player().get_Character().get_Rotation().Z;
        //            int num1 = Function.Call<int>(-8351678148772176525L, new InputArgument[] { x, y, z - 1f, single, y1, z1, 0 });
        //            Function.Call(-1249422255215503276L, new InputArgument[] { this.CommonPet, num1, "creatures@rottweiler@tricks@", "petting_chop", 1000, -8, 4, 0, 1148846080, 0 });
        //            Function.Call(-1249422255215503276L, new InputArgument[] { Game.get_Player().get_Character(), num1, "creatures@rottweiler@tricks@", "petting_franklin", 1000, -8, 4, 0, 1148846080, 0 });
        //            if (this.allowSpeech)
        //            {
        //                this.BarkSpeech();
        //            }
        //            Script.Wait(4000);
        //            this.CommonPet.get_Task().ClearAll();
        //            Game.get_Player().get_Character().get_Task().ClearAll();
        //            this.HuskyIsPetting = false;
        //            this.inAction = false;
        //            this.followCamera = false;
        //        }
        //    }
        //}

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
