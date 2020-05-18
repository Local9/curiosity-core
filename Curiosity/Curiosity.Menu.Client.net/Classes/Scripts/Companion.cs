using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Menus.Client.net.Classes.Data;
using Curiosity.Menus.Client.net.Extensions;
using Curiosity.Menus.Client.net.Static;
using Curiosity.Shared.Client.net;
using Curiosity.Shared.Client.net.Extensions;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
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

        const string ANIM_DICT_MOVE = "creatures@rottweiler@move";

        const string ANIM_DICT_TRICKS = "creatures@rottweiler@tricks@";
        const string ANIM_PETTING_PLAYER = "petting_franklin";
        const string ANIM_PETTING_PET = "petting_chop";
        const string ANIM_SIT_ENTER = "sit_enter";
        const string ANIM_SIT_LOOP = "sit_loop";
        const string ANIM_SIT_EXIT = "sit_exit";

        static bool InteractingWithPet = false;

        static int LastTimeWhine;

        static int WhineTime;
        static int WhineTimeInterval;

        static int sequenceGameTimer;
        static int sequenceTaskId;
        static int internalSequenceGameTimer;
        static int projectileEntityId;

        static int countAttemptsBeforeReturningToPlayer = 0;
        static int countAttemptsBeforeReturningToPlayer2 = 0;
        static bool companionIsFetchingBall = false;
        static bool isProjectileThrown = false;

        static Vector3 projectilePosition;
        static WeaponHash thrownProjectile;
        static WeaponHash weaponBall = WeaponHash.Ball;
        static int companionInteraction = 0;
        static int companionInteractionSequence = 0;

        static int audioSoundId = API.GetSoundId();

        static public void Init()
        {
            client = Client.GetInstance();
            Relationships.SetupRelationShips();
        }

        private static async Task OnMonitorCompanionTask()
        {
            if (API.GetProjectileNearPed(Game.PlayerPed.Handle, (uint)weaponBall, 50f, ref projectilePosition, ref projectileEntityId, false))
            {
                thrownProjectile = WeaponHash.Ball;
                companionInteraction = 15;
            }
            else
            {
                companionInteraction = 0;
            }
        }

        private static async Task OnPlayerMovementTask()
        {
            await BaseScript.Delay(5000);

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

            companionInteraction = 0;

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

            int groupIndex = API.GetPedGroupIndex(Game.PlayerPed.Handle);

            if (companionData.IsHuman)
            {
                ped.PlayAmbientSpeech("GENERIC_HI", SpeechModifier.Standard);
            }
            else
            {
                API.SetGroupFormationSpacing(groupIndex, 1f, 0.9f, 3f);
            }

            API.SetPedCanTeleportToGroupLeader(ped.Handle, groupIndex, true);

            if (Game.PlayerPed.Weapons.HasWeapon(WeaponHash.Ball))
                Game.PlayerPed.Weapons.Remove(WeaponHash.Ball);

            if (companionData.CanInteract)
            {
                PlaySound("BARK");
                GivePlayerBall(true, true, false);
            }

            Client.TriggerEvent("curiosity:Client:Notification:Curiosity", 1, "Companion", "", "Your companion will attack anyone who attacks you.", 3);

            client.RegisterTickHandler(OnPlayerMovementTask);
            client.RegisterTickHandler(OnMonitorCompanionTask);
            client.RegisterTickHandler(OnCompanionTick);
            client.RegisterTickHandler(OnCompanionInteractionTask);
        }

        private static async Task OnCompanionTick()
        {
            if (ped == null)
            {
                failureCount++;

                if (failureCount >= 5)
                {
                    client.DeregisterTickHandler(OnCompanionTick);
                    client.DeregisterTickHandler(OnCompanionInteractionTask);
                    client.DeregisterTickHandler(OnPlayerMovementTask);
                    client.DeregisterTickHandler(OnMonitorCompanionTask);
                }

                return;
            }

            if (!ped.Exists())
            {
                failureCount++;

                if (failureCount >= 5)
                {
                    client.DeregisterTickHandler(OnCompanionTick);
                    client.DeregisterTickHandler(OnCompanionInteractionTask);
                    client.DeregisterTickHandler(OnPlayerMovementTask);
                    client.DeregisterTickHandler(OnMonitorCompanionTask);
                }

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

            if (CanInteract)
            {
                // interaction menu

                Screen.DisplayHelpTextThisFrame($"Press ~INPUT_CONTEXT~ to pat your pet.");
                // DisplayHelp("CHOP_H_ATTACK", -1);

                if (Game.IsControlPressed(0, Control.Context))
                {
                    Pat();
                }

            }

            if (!Game.PlayerPed.IsInVehicle() && ped.IsInVehicle() && !IsHuman)
            {
                API.NetworkFadeOutEntity(ped.Handle, false, false);
                ped.Position = Client.CurrentVehicle.GetOffsetPosition(new Vector3(2f, 0f, 0f));
                API.NetworkFadeInEntity(ped.Handle, false);
            }

            if (currentCompanion.CanInteract && hasNotMoved && companionInteraction == 0)
            {
                if ((API.GetGameTimer() - LastMovement) < 10000
                    && !API.IsEntityPlayingAnim(ped.Handle, ANIM_DICT_TRICKS, ANIM_SIT_LOOP, 2))
                {
                    ped.Task.TurnTo(Game.PlayerPed, 2000);
                    await BaseScript.Delay(1000);
                    ped.Task.PlayAnimation(ANIM_DICT_TRICKS, ANIM_SIT_ENTER, 1f, -1, AnimationFlags.StayInEndFrame);
                    await BaseScript.Delay(2000);
                    PlaySound("WHINE");
                    ped.Task.PlayAnimation(ANIM_DICT_TRICKS, ANIM_SIT_LOOP, 1f, -1, AnimationFlags.Loop);
                    await BaseScript.Delay(2000);
                    LastTimeWhine = API.GetGameTimer();
                }
            }
            else if (currentCompanion.CanInteract && !hasNotMoved && API.IsEntityPlayingAnim(ped.Handle, ANIM_DICT_TRICKS, ANIM_SIT_LOOP, 3))
            {
                ped.Task.PlayAnimation(ANIM_DICT_TRICKS, ANIM_SIT_EXIT, 1f, -1, AnimationFlags.StayInEndFrame);
                await BaseScript.Delay(1000);
                ped.Task.ClearAll();
                PlaySound("BARK");
            }
        }

        static void DisplayHelp(string text, int shape)
        {
            API.BeginTextCommandDisplayHelp(text);
            API.EndTextCommandDisplayHelp(0, false, true, shape);
        }

        static void PlaySound(string sound)
        {
            if (Player.PlayerInformation.IsDeveloper())
                Log.Verbose($"PlaySound : {sound}");

            API.PlayAnimalVocalization(ped.Handle, 3, sound);
        }

        static bool CanInteract
        {
            get
            {
                return !Game.PlayerPed.IsInVehicle() && Game.PlayerPed.IsAlive && !ped.IsInVehicle() && ped.IsAlive && Game.PlayerPed.Position.Distance(ped.Position) < 1.5f && !InteractingWithPet;
            }
        }

        private static async void Paw()
        {
            PedBone boneIndex = Game.PlayerPed.Bones[Bone.SKEL_L_Finger00];
            Vector3 fingerPosition = boneIndex.Position;

            if (!ped.IsRagdoll && !InteractingWithPet)
            {
                InteractingWithPet = true;

                Game.DisableAllControlsThisFrame(0);

                Game.PlayerPed.Task.TurnTo(ped, 1000);
                ped.Task.TurnTo(Game.PlayerPed, 1000);
                await BaseScript.Delay(1000);

                ped.Task.PlayAnimation(ANIM_DICT_TRICKS, "paw_right_enter", 1f, -1.5f, -1, AnimationFlags.StayInEndFrame, 0f);
                await BaseScript.Delay(1000);
                ped.Task.PlayAnimation(ANIM_DICT_TRICKS, "paw_right_loop", 1f, -1.5f, -1, AnimationFlags.Loop, 0f);
                await BaseScript.Delay(3000);
                ped.Task.PlayAnimation(ANIM_DICT_TRICKS, "paw_right_exit", 1f, -1.5f, -1, AnimationFlags.StayInEndFrame, 0f);
                await BaseScript.Delay(1000);

                ped.Task.ClearAll();
                Game.PlayerPed.Task.ClearAll();

                InteractingWithPet = false;
            }
        }

        private static async void Pat()
        {
            PedBone boneIndex = Game.PlayerPed.Bones[Bone.SKEL_L_Finger00];
            Vector3 fingerPosition = boneIndex.Position;

            if (!ped.IsRagdoll && !InteractingWithPet)
            {
                InteractingWithPet = true;

                Game.DisableAllControlsThisFrame(0);

                Game.PlayerPed.Task.TurnTo(ped, 1000);
                ped.Task.TurnTo(Game.PlayerPed, 1000);
                await BaseScript.Delay(1000);
                Vector3 playerPos = Game.PlayerPed.Position;
                Vector3 playerRot = Game.PlayerPed.Rotation;

                int scene = API.CreateSynchronizedScene(playerPos.X, playerPos.Y, playerPos.Z - 1f, playerRot.X, playerRot.Y, playerRot.Z, 0);
                API.TaskSynchronizedScene(ped.Handle, scene, ANIM_DICT_TRICKS, ANIM_PETTING_PET, 1f, -8f, 4, 0, 1148846080, 0);
                API.TaskSynchronizedScene(Game.PlayerPed.Handle, scene, ANIM_DICT_TRICKS, ANIM_PETTING_PLAYER, 1f, -8f, 4, 0, 1148846080, 0);

                await BaseScript.Delay(4000);

                ped.Task.ClearAll();
                Game.PlayerPed.Task.ClearAll();

                InteractingWithPet = false;
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

                    if (currentCompanion.CanInteract)
                        Reset(2, true);

                    await Client.Delay(500);
                    API.NetworkFadeOutEntity(ped.Handle, false, false);
                    await Client.Delay(500);
                    ped.Delete();

                    Game.PlayerPed.Weapons.Remove(WeaponHash.Ball);

                    client.DeregisterTickHandler(OnCompanionTick);
                    client.DeregisterTickHandler(OnCompanionInteractionTask);
                    client.DeregisterTickHandler(OnPlayerMovementTask);
                    client.DeregisterTickHandler(OnMonitorCompanionTask);
                }
            }
        }

        // Scripts below engineered from chop.c

        static void GivePlayerBall(bool isHidden, bool playSound, bool equipNow)
        {
            if (!API.HasPedGotWeapon(Game.PlayerPed.Handle, (uint)WeaponHash.Ball, false) || API.GetAmmoInPedWeapon(Game.PlayerPed.Handle, (uint)WeaponHash.Ball) == 0)
            {
                API.GiveWeaponToPed(Game.PlayerPed.Handle, (uint)WeaponHash.Ball, 1, isHidden, equipNow);
                API.HudWeaponWheelGetSlotHash((int)WeaponHash.Ball);
                if (playSound)
                {
                    API.PlaySoundFrontend(audioSoundId, "PICKUP_WEAPON_BALL", "HUD_FRONTEND_WEAPONS_PICKUPS_SOUNDSET", true);
                }
            }
        }

        static float GetDistanceBetweenEntities(int entityOne, int entityTwo, bool useZ)
        {
            Vector3 entity1Coords;
            Vector3 entity2Coords;

            if (!API.IsEntityDead(entityOne))
            {
                entity1Coords = API.GetEntityCoords(entityOne, true);
            }
            else
            {
                entity1Coords = API.GetEntityCoords(entityOne, false);
            }

            if (!API.IsEntityDead(entityTwo))
            {
                entity2Coords = API.GetEntityCoords(entityTwo, true);
            }
            else
            {
                entity2Coords = API.GetEntityCoords(entityTwo, false);
            }

            return API.GetDistanceBetweenCoords(entity1Coords.X, entity1Coords.Y, entity1Coords.Z, entity2Coords.X, entity2Coords.Y, entity2Coords.Z, useZ);
        }

        static bool IsProjectileReachable()
        {
            Vector3 entityCoord;
            float groundZ = 0;

            if (GetDistanceBetweenEntities(ped.Handle, projectileEntityId, true) < (0.5f + 0.25f))
            {
                entityCoord = API.GetEntityCoords(projectileEntityId, true);
                if (API.GetGroundZFor_3dCoord(entityCoord.X, entityCoord.Y, (entityCoord.Z + 1f), ref groundZ, false))
                {
                    if (API.Absf(entityCoord.Z - groundZ) < 0.1f)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        static void RemoveOrDropBall(bool removeBall)
        {
            Vector3 projectilePosition;

            if (API.DoesEntityExist(projectileEntityId))
            {
                projectilePosition = API.GetEntityCoords(projectileEntityId, true);
                if (API.IsEntityAttachedToAnyPed(projectileEntityId))
                {
                    API.DetachEntity(projectileEntityId, true, true);
                }
                
                API.SetEntityAsNoLongerNeeded(ref projectileEntityId);

                if (removeBall && thrownProjectile == WeaponHash.Ball)
                {
                    API.ClearAreaOfProjectiles(projectilePosition.X, projectilePosition.Y, projectilePosition.Z, 0.1f, false);
                }
            }
        }

        static bool PedIsNotDead(int entityId)
        {
            if (API.DoesEntityExist(entityId))
            {
                if (!API.IsEntityDead(entityId))
                {
                    return true;
                }
            }

            return false;
        }

        static bool PedIsNotInjured(int entityId)
        { 
            if (PedIsNotDead(entityId))
            {
                if (!API.IsPedInjured(entityId))
                {
                    return true;
                }
            }
            return false;
        }


        static bool TaskStatus(int entityId, uint scriptHash) // func_113
        {
            if (PedIsNotInjured(entityId))
            {
                if (API.GetScriptTaskStatus(entityId, scriptHash) == 1 || API.GetScriptTaskStatus(entityId, scriptHash) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        static async Task OnCompanionInteractionTask()
        {
            int distanceRemaining;
            bool boolVar1 = false;
            float floatVar1 = 0f;

            Vector3 vVar3;
            float fVar4;
            float fVar5;
            int iVar6;
            int iVar7;

            // Screen.ShowSubtitle($"companionInteraction : {companionInteraction} / companionInteractionSequence: {companionInteractionSequence}~n~isProjectileThrown {isProjectileThrown}");

            int playerPedHandle = Game.PlayerPed.Handle;
            switch (companionInteraction)
            {
                case 2:
                    CleanUp();
                    if (companionInteractionSequence == 0)
                    {
                        sequenceGameTimer = API.GetGameTimer();
                        companionInteractionSequence++;
                    }
                    else if (companionInteractionSequence == 2)
                    {
                        RandomWhine();
                    }

                    break;
                case 15:
                    if (Game.PlayerPed.IsInVehicle())
                    {
                        RemoveOrDropBall(false);
                        sequenceGameTimer = API.GetGameTimer();
                    }
                    else
                    {
                        if (companionInteractionSequence == 0)
                        {
                            RemoveOrDropBall(true);
                            if (API.GetProjectileNearPed(playerPedHandle, (uint)weaponBall, 50f, ref projectilePosition, ref projectileEntityId, false))
                            {
                                GivePlayerBall(true, true, false);
                                API.SetCurrentPedWeapon(playerPedHandle, (uint)weaponBall, true);
                                isProjectileThrown = false;
                                companionIsFetchingBall = false;
                                countAttemptsBeforeReturningToPlayer = 0;
                                // Group thing...
                                internalSequenceGameTimer = API.GetGameTimer();
                                companionInteractionSequence++;
                            }
                        }
                        else if (companionInteractionSequence == 1)
                        {
                            if (API.DoesEntityExist(projectileEntityId))
                            {
                                if (!API.IsEntityInWater(projectileEntityId))
                                {
                                    if ((API.GetGameTimer() - internalSequenceGameTimer) > 500)
                                    {
                                        if (IsProjectileReachable()) // func_5
                                        {
                                            API.RequestAnimDict(ANIM_DICT_MOVE);
                                            if (API.HasAnimDictLoaded(ANIM_DICT_MOVE))
                                            {
                                                API.OpenSequenceTask(ref sequenceTaskId);
                                                if (thrownProjectile == weaponBall) // GameHashBall
                                                {
                                                    API.TaskPlayAnim(0, ANIM_DICT_MOVE, "fetch_pickup", 8f, -8f, -1, 49152, 0f, false, false, false);
                                                }
                                                API.TaskGoToEntity(0, playerPedHandle, 20000, 4f, 3f, 1073741824, 0);
                                                API.CloseSequenceTask(sequenceTaskId);
                                                API.TaskPerformSequence(ped.Handle, sequenceTaskId);
                                                API.ClearSequenceTask(ref sequenceTaskId);
                                                if (thrownProjectile == WeaponHash.Ball)
                                                {
                                                    isProjectileThrown = true;
                                                }
                                                companionInteractionSequence++;
                                            }
                                        }
                                        else if (!TaskStatus(ped.Handle, 1227113341))
                                        {
                                            API.TaskGoToEntity(ped.Handle, projectileEntityId, 30000, 0.5f, 3f, 1073741824, 0);
                                            countAttemptsBeforeReturningToPlayer++;
                                            countAttemptsBeforeReturningToPlayer2 = 0; // iLocal_339 = 0;
                                            if (countAttemptsBeforeReturningToPlayer > 3)
                                            {
                                                API.TaskGoToEntity(ped.Handle, playerPedHandle, 20000, 5f, 3f, 1073741824, 0);
                                                RemoveOrDropBall(true);
                                                companionInteractionSequence++;
                                            }
                                        }
                                        else
                                        {
                                            distanceRemaining = API.GetNavmeshRouteDistanceRemaining(ped.Handle, ref floatVar1, ref boolVar1);
                                            if (distanceRemaining == 2)
                                            {
                                                float entityHeightAboveGround = API.GetEntityHeightAboveGround(projectileEntityId);
                                                if (entityHeightAboveGround < 1f)
                                                {
                                                    countAttemptsBeforeReturningToPlayer2++;
                                                }
                                            }
                                            else if (distanceRemaining == 3)
                                            {
                                                if (!companionIsFetchingBall)
                                                {
                                                    PlaySound("BARK");
                                                    if (thrownProjectile == weaponBall)
                                                    {
                                                        // func_108 // Conversation function between franklin and chop
                                                    }
                                                    companionIsFetchingBall = true;
                                                }
                                            }
                                            if (countAttemptsBeforeReturningToPlayer2 > 9)
                                            {
                                                API.TaskGoToEntity(ped.Handle, playerPedHandle, 20000, 5f, 3f, 1073741824, 0);
                                                RemoveOrDropBall(true);
                                                companionInteractionSequence++;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    API.TaskGoToEntity(ped.Handle, playerPedHandle, 20000, 5f, 3f, 1073741824, 0);
                                    companionInteractionSequence++;
                                }
                            }
                            else
                            {
                                API.TaskGoToEntity(ped.Handle, playerPedHandle, 20000, 5f, 3f, 1073741824, 0);
                                companionInteractionSequence++;
                            }
                        }
                        else if (companionInteractionSequence == 2)
                        {
                            if (isProjectileThrown)
                            {
                                if ((API.DoesEntityExist(projectileEntityId) && API.IsEntityPlayingAnim(ped.Handle, ANIM_DICT_MOVE, "fetch_pickup", 3)) && API.GetEntityAnimCurrentTime(ped.Handle, ANIM_DICT_MOVE, "fetch_pickup") > 0.25f)
                                {
                                    API.AttachEntityToEntity(projectileEntityId, ped.Handle, 28, 0.2042f, 0f, -0.0608f, 0f, 0f, 0f, false, false, false, false, 2, true);
                                    API.SetAudioFlag("DisableBarks", true);
                                    companionInteractionSequence++;
                                }
                            }
                            else
                            {
                                companionInteractionSequence++;
                            }
                        }
                        else if (companionInteractionSequence == 3)
                        {
                            if (GetDistanceBetweenEntities(playerPedHandle, ped.Handle, true) < 5f)
                            {
                                if (thrownProjectile == WeaponHash.Ball)
                                {
                                    if (isProjectileThrown)
                                    {
                                        API.RequestAnimDict(ANIM_DICT_MOVE);
                                        if (API.HasAnimDictLoaded(ANIM_DICT_MOVE))
                                        {
                                            API.OpenSequenceTask(ref sequenceTaskId);
                                            API.TaskTurnPedToFaceEntity(0, playerPedHandle, 0);
                                            API.TaskPlayAnim(0, ANIM_DICT_MOVE, "fetch_drop", 8f, 8f, -1, 16384, 0, false, false, false);
                                            API.CloseSequenceTask(sequenceTaskId);
                                            API.TaskPerformSequence(ped.Handle, sequenceTaskId);
                                            API.ClearSequenceTask(ref sequenceTaskId);
                                            // return dialog
                                            API.SetAudioFlag("DisableBarks", false);
                                            companionInteractionSequence++;
                                        }
                                    }
                                    else
                                    {
                                        PlaySound("BREATH_AGITATED");
                                        // function for autio conversation
                                        // reset function
                                        Reset(2, true);
                                    }
                                }
                                else
                                {
                                    PlaySound("BARK_WHINE");
                                    RemoveOrDropBall(true);
                                    Reset(2, true);
                                }
                            }
                        }
                        else if (companionInteractionSequence == 4)
                        {
                            if (!TaskStatus(ped.Handle, 242628503) && !API.DoesEntityExist(projectileEntityId))
                            {
                                GivePlayerBall(true, true, false);
                                sequenceGameTimer = API.GetGameTimer();
                                Reset(2, true);
                            }
                            else if (API.DoesEntityExist(projectileEntityId))
                            {
                                if (API.IsEntityAttached(projectileEntityId))
                                {
                                    if (API.IsEntityPlayingAnim(ped.Handle, ANIM_DICT_MOVE, "fetch_drop", 3) && API.GetEntityAnimCurrentTime(ped.Handle, ANIM_DICT_MOVE, "fetch_drop") > 0.4f)
                                    {
                                        API.DetachEntity(projectileEntityId, true, true);
                                        await BaseScript.Delay(500);
                                        GivePlayerBall(true, true, true);
                                        Reset(2, true);
                                    }
                                }
                            }
                            else
                            {
                                if (GetDistanceBetweenEntities(playerPedHandle, projectileEntityId, true) < 1.5f || GetDistanceBetweenEntities(playerPedHandle, projectileEntityId, true) > 20f || Game.PlayerPed.IsInVehicle())
                                {
                                    RemoveOrDropBall(true);
                                    Reset(2, true);
                                }
                                if (!TaskStatus(ped.Handle, 242628503))
                                {
                                    
                                }
                            }
                        }
                        CleanUp();
                    }

                    break;
            }
        }

        private static void RandomWhine()
        {
            if ((API.GetGameTimer() - WhineTime) > WhineTimeInterval)
            {
                WhineTime = API.GetGameTimer();
                WhineTimeInterval = Client.Random.Next(10000, 15000);
                PlaySound("WHINE");
            }
        }

        private static void CleanUp()
        {
            //if (!Game.PlayerPed.IsInVehicle() && !Game.PlayerPed.IsInWater)
            //{
            //    if (companionInteraction == 15 && companionInteractionSequence > 1)
            //    {

            //    }
            //    else
            //    {
            //        RemoveOrDropBall(false);
            //        if (companionInteraction == 2)
            //        {
            //            Reset(2, false);
            //        } else
            //        {
            //            Reset(2, true);
            //        }
            //    }
            //}
        }

        private static void Reset(int compInteraction, bool clearTasks)
        {
            if (PedIsNotDead(ped.Handle))
            {
                API.SetBlockingOfNonTemporaryEvents(ped.Handle, true);
                ped.ClearLastWeaponDamage();
                if (!ped.IsInVehicle())
                {
                    if (clearTasks)
                    {
                        ped.Task.ClearAll();
                    }
                    ped.IsInvincible = false;
                }
            }

            companionInteractionSequence = 0;
            companionInteraction = compInteraction;
            companionIsFetchingBall = false;
            isProjectileThrown = false;
            API.SetAudioFlag("DisableBarks", false);
        }
    }
}
