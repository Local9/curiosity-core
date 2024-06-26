﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Vehicles.Client.net.Classes.CurPlayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Vehicles.Client.net.Classes.CuriosityVehicle
{
    static class VehicleDamage
    {
        static Plugin client = Plugin.GetInstance();

        // TODO: Move to config
        static bool preventVehicleFlip = true;
        static float sundayDriverAcceleratorCurve = 7.5f;
        static float sundayDriverBrakeCurve = 5.0f;
        static float engineSafeGuard = 100.0f;
        static float limpModeMultiplier = 0.15f;
        static float damageFactorEngine = 5.1f;
        static float damageFactorBody = 5.1f;
        static float damageFactorPetrolTank = 61.0f;
        static float cascadingFailureThreshold = 310.0f;
        static float degradingFailureThreshold = 677.0f;
        static float cascadingFailureSpeedFactor = 1.5f;
        static float degradingHealthSpeedFactor = 7.4f;
        static float engineDamageExponent = 1.0f;
        static float weaponsDamageMultiplier = 0.124f;
        static float deformationExponent = 1.0f;
        static float deformationMultiplier = 1.0f;
        static float collisionDamageExponent = 1.0f;

        // TIRES
        static int tireBurstLuckyNumber;
        static int randomTireBurstInterval = 300;
        static int tireBurstMaxNumber = randomTireBurstInterval * 1200;

        // DAMAGE AND FUCKERY
        static float fCollisionDamageMult = 0.0f;
        static float fDeformationDamageMult = 0.0f;
        static float fEngineDamageMult = 0.0f;
        static float fBrakeForce = 1.0f;
        static bool isBrakingForward = false;
        static bool isBrakingReverse = false;

        static float healthEngineLast = 1000.0f;
        static float healthEngineCurrent = 1000.0f;
        static float healthEngineNew = 1000.0f;
        static float healthEngineDelta = 0.0f;
        static float healthEngineDeltaScaled = 0.0f;

        static float healthBodyLast = 1000.0f;
        static float healthBodyCurrent = 1000.0f;
        static float healthBodyNew = 1000.0f;
        static float healthBodyDelta = 0.0f;
        static float healthBodyDeltaScaled = 0.0f;

        static float healthPetrolTankLast = 1000.0f;
        static float healthPetrolTankCurrent = 1000.0f;
        static float healthPetrolTankNew = 1000.0f;
        static float healthPetrolTankDelta = 0.0f;
        static float healthPetrolTankDeltaScaled = 0.0f;

        static Dictionary<VehicleClass, float> classDamageMultiplier = new Dictionary<VehicleClass, float>();

        // local checks
        static bool isPedInSameVehicle = false;
        static Random random = new Random(API.GetGameTimer());

        static CitizenFX.Core.Vehicle currentVehicle;
        static CitizenFX.Core.Vehicle lastVehicle;

        //static MenuItemStandard menuItemFix = new MenuItemStandard { Title = "Fix Vehicle", OnActivate = (item) => Fix() };

        static public void Init()
        {
            //InteractionListMenu.RegisterInteractionMenuItem(menuItemFix, () => Game.PlayerPed.IsInVehicle() && Player.PlayerInformation.IsDeveloper(), 999);
            client.RegisterTickHandler(VehicleFuckery);
            // client.RegisterTickHandler(AdditionalVehicleFuckery);

            client.RegisterEventHandler("curiosity:Client:Vehicle:Detonate", new Action<int>(OnVehicleDetonate));

            client.RegisterEventHandler("curiosity:Client:Vehicle:DevRepair", new Action(DevRepair));

            if (randomTireBurstInterval != 0)
            {
                tireBurstLuckyNumber = random.Next(tireBurstMaxNumber);
            }

            classDamageMultiplier.Add(VehicleClass.Compacts, 1.0f);
            classDamageMultiplier.Add(VehicleClass.Sedans, 1.0f);
            classDamageMultiplier.Add(VehicleClass.SUVs, 1.0f);
            classDamageMultiplier.Add(VehicleClass.Coupes, 0.95f);
            classDamageMultiplier.Add(VehicleClass.Muscle, 1.0f);
            classDamageMultiplier.Add(VehicleClass.SportsClassics, 0.95f);
            classDamageMultiplier.Add(VehicleClass.Sports, 0.95f);
            classDamageMultiplier.Add(VehicleClass.Super, 0.95f);
            classDamageMultiplier.Add(VehicleClass.Motorcycles, 0.27f);
            classDamageMultiplier.Add(VehicleClass.OffRoad, 0.7f);
            classDamageMultiplier.Add(VehicleClass.Industrial, 0.25f);
            classDamageMultiplier.Add(VehicleClass.Utility, 0.35f);
            classDamageMultiplier.Add(VehicleClass.Vans, 0.85f);
            classDamageMultiplier.Add(VehicleClass.Cycles, 1.0f);
            classDamageMultiplier.Add(VehicleClass.Boats, 0.4f);
            classDamageMultiplier.Add(VehicleClass.Helicopters, 0.7f);
            classDamageMultiplier.Add(VehicleClass.Planes, 0.7f);
            classDamageMultiplier.Add(VehicleClass.Service, 0.75f);
            classDamageMultiplier.Add(VehicleClass.Emergency, 0.85f);
            classDamageMultiplier.Add(VehicleClass.Military, 0.67f);
            classDamageMultiplier.Add(VehicleClass.Commercial, 0.43f);
            classDamageMultiplier.Add(VehicleClass.Trains, 1.0f);
        }

        static void OnVehicleDetonate(int vehicleId)
        {
            int id = API.NetworkGetEntityFromNetworkId(vehicleId);
            CitizenFX.Core.Vehicle vehicle = new CitizenFX.Core.Vehicle(id);
            if (vehicle != null)
            {
                Vector3 pos = vehicle.Position;
                API.AddExplosion(pos.X, pos.Y, pos.Z, (int)ExplosionType.Car, 1000.0f, true, false, 3.0f);
                vehicle.Explode();
                vehicle.ExplodeNetworked();
            }
        }

        static float rangedScale(float inputValue, float originalMin, float originalMax, float newBegin, float newEnd, float curve)
        {
            float originalRange = 0.0f;
            float newRange = 0.0f;
            float zeroRefCurVal = 0.0f;
            float normalizedCurVal = 0.0f;
            float rangedValue = 0.0f;
            bool invFlag = false;

            if (curve > 10.0f) { curve = 10.0f; }
            if (curve < -10.0f) { curve = -10.0f; }

            curve = (curve * -.1f);
            curve = (float)Math.Pow(10.0, curve);

            if (inputValue < originalMin) inputValue = originalMin;
            if (inputValue > originalMax) inputValue = originalMax;

            originalRange = originalMax - originalMin;

            if (newEnd > newBegin)
            {
                newRange = newEnd - newBegin;
            }
            else
            {
                newRange = newBegin - newEnd;
                invFlag = true;
            }

            zeroRefCurVal = inputValue - originalMin;
            normalizedCurVal = zeroRefCurVal / originalRange;

            if (originalMin > originalMax) return 0;

            if (!invFlag)
            {
                rangedValue = ((float)(Math.Pow(normalizedCurVal, curve) * newRange) + newBegin);
            }
            else
            {
                rangedValue = newBegin - (float)(Math.Pow(normalizedCurVal, curve) * newRange);
            }

            return rangedValue;
        }

        static bool IsPlayerDrivingAVehicle()
        {
            if (API.IsPedInAnyVehicle(Game.PlayerPed.Handle, false))
            {
                if (API.GetPedInVehicleSeat(Game.PlayerPed.CurrentVehicle.Handle, -1) == Game.PlayerPed.Handle)
                {
                    VehicleClass vehicleClass = (VehicleClass)API.GetVehicleClass(Game.PlayerPed.CurrentVehicle.Handle);
                    if (!vehicleClass.HasFlag(VehicleClass.Helicopters) && !vehicleClass.HasFlag(VehicleClass.Planes) && !vehicleClass.HasFlag(VehicleClass.Cycles) && !vehicleClass.HasFlag(VehicleClass.Trains))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        static void TireBurstLottery(CitizenFX.Core.Vehicle vehicle)
        {
            int tireBurstNumber = random.Next(tireBurstMaxNumber);
            if (tireBurstNumber == tireBurstLuckyNumber)
            {
                // WE WIN!
                if (!vehicle.CanTiresBurst) return; // DAM WE LOST!

                int numWheels = API.GetVehicleNumberOfWheels(vehicle.Handle);
                int affectedTyre;
                if (numWheels == 2)
                {
                    affectedTyre = (random.Next(2) - 1) * 4;
                }
                else if (numWheels == 4)
                {
                    affectedTyre = (random.Next(4) - 1);
                    if (affectedTyre > 1) affectedTyre = affectedTyre + 2; // 0, 1, 4, 5
                }
                else if (numWheels == 6)
                {
                    affectedTyre = (random.Next(6) - 1);
                }
                else
                {
                    affectedTyre = 0;
                }
                API.SetVehicleTyreBurst(vehicle.Handle, affectedTyre, false, 1000.0f);
                tireBurstLuckyNumber = random.Next(tireBurstMaxNumber);
            }
        }

        async static Task VehicleFuckery()
        {
            while (true)
            {
                await Plugin.Delay(50);

                if (IsPlayerDrivingAVehicle())
                {
                    currentVehicle = Game.PlayerPed.CurrentVehicle;
                    // ENGINE
                    healthEngineCurrent = currentVehicle.EngineHealth;
                    if (healthEngineCurrent == 1000.0f) healthEngineLast = 1000.0f;
                    healthEngineNew = healthEngineCurrent;
                    healthEngineDelta = healthEngineLast - healthEngineCurrent;
                    healthEngineDeltaScaled = healthEngineDelta * damageFactorEngine * classDamageMultiplier[currentVehicle.ClassType];

                    healthBodyCurrent = currentVehicle.BodyHealth;
                    if (healthBodyCurrent == 1000.0f) healthBodyLast = 1000.0f;
                    healthBodyNew = healthBodyCurrent;
                    healthBodyDelta = healthBodyLast - healthBodyCurrent;
                    healthBodyDeltaScaled = healthBodyDelta * damageFactorBody * classDamageMultiplier[currentVehicle.ClassType];

                    healthPetrolTankCurrent = currentVehicle.PetrolTankHealth;
                    if (healthPetrolTankCurrent == 1000.0f) healthPetrolTankLast = 1000.0f;
                    healthPetrolTankNew = healthPetrolTankCurrent;
                    healthPetrolTankDelta = healthPetrolTankLast - healthPetrolTankCurrent;
                    healthPetrolTankDeltaScaled = healthPetrolTankDelta * damageFactorPetrolTank * classDamageMultiplier[currentVehicle.ClassType];

                    if (healthEngineCurrent < engineSafeGuard + 1) API.SetVehicleUndriveable(currentVehicle.Handle, true);
                    if (lastVehicle != currentVehicle) isPedInSameVehicle = false;

                    if (isPedInSameVehicle)
                    {
                        if (healthEngineCurrent != 1000.0f || healthBodyCurrent != 1000.0f || healthPetrolTankCurrent != 1000.0f)
                        {
                            float healthEngineCombinedDelta = new float[] { healthEngineDeltaScaled, healthBodyDeltaScaled, healthPetrolTankDeltaScaled }.Max();

                            if (healthEngineCombinedDelta > (healthEngineCurrent - engineSafeGuard)) healthEngineCombinedDelta = (float)(healthEngineCombinedDelta * 0.7);
                            if (healthEngineCombinedDelta > healthEngineCurrent) healthEngineCombinedDelta = healthEngineCurrent - (cascadingFailureThreshold / 5);

                            healthEngineNew = healthEngineLast - healthEngineCombinedDelta;

                            if (healthEngineNew > (cascadingFailureThreshold + 5) && healthEngineNew < degradingFailureThreshold) healthEngineNew = healthEngineNew - (0.038f * cascadingFailureSpeedFactor);
                            if (healthEngineNew < engineSafeGuard) healthEngineNew = engineSafeGuard;
                            if (healthPetrolTankCurrent < 750f) healthPetrolTankNew = 750.0f;
                            if (healthBodyNew < 0) healthBodyNew = 0.0f;
                        }
                    }
                    else
                    {
                        fDeformationDamageMult = API.GetVehicleHandlingFloat(currentVehicle.Handle, "CHandlingData", "fDeformationDamageMult");
                        fBrakeForce = API.GetVehicleHandlingFloat(currentVehicle.Handle, "CHandlingData", "fBrakeForce");
                        float newFDeformationDamageMult = (float)Math.Pow(fDeformationDamageMult, deformationExponent);
                        if (deformationMultiplier != -1) API.SetVehicleHandlingFloat(currentVehicle.Handle, "CHandlingData", "fDeformationDamageMult", newFDeformationDamageMult * deformationMultiplier);
                        if (weaponsDamageMultiplier != -1) API.SetVehicleHandlingFloat(currentVehicle.Handle, "CHandlingData", "fWeaponDamageMult", weaponsDamageMultiplier / damageFactorBody);

                        fCollisionDamageMult = API.GetVehicleHandlingFloat(currentVehicle.Handle, "CHandlingData", "fCollisionDamageMult");
                        float newFCollisionDamageMultiplier = (float)Math.Pow(fCollisionDamageMult, collisionDamageExponent);
                        API.SetVehicleHandlingFloat(currentVehicle.Handle, "CHandlingData", "fCollisionDamageMult", newFCollisionDamageMultiplier);

                        fEngineDamageMult = API.GetVehicleHandlingFloat(currentVehicle.Handle, "CHandlingData", "fEngineDamageMult");
                        float newFEngineDamageMult = (float)Math.Pow(fEngineDamageMult, engineDamageExponent);
                        API.SetVehicleHandlingFloat(currentVehicle.Handle, "CHandlingData", "fEngineDamageMult", newFEngineDamageMult);

                        if (healthBodyCurrent < cascadingFailureThreshold) healthBodyNew = cascadingFailureThreshold;
                        isPedInSameVehicle = true;
                    }

                    if (healthEngineNew != healthEngineCurrent) currentVehicle.EngineHealth = healthEngineNew;
                    if (healthBodyNew != healthBodyCurrent) currentVehicle.BodyHealth = healthBodyNew;
                    if (healthPetrolTankNew != healthPetrolTankCurrent) currentVehicle.PetrolTankHealth = healthPetrolTankCurrent;

                    healthEngineLast = healthEngineNew;
                    healthBodyLast = healthBodyNew;
                    healthPetrolTankLast = healthPetrolTankNew;

                    if (healthEngineLast < 100.0f)
                    {
                        currentVehicle.EngineHealth = 100.0f;
                        currentVehicle.IsEngineRunning = false;
                    }

                    if (randomTireBurstInterval != 0 && currentVehicle.Speed > 10f) TireBurstLottery(currentVehicle);
                }
                else
                {
                    if (IsPlayerDrivingAVehicle())
                    {
                        if (isPedInSameVehicle)
                        {
                            lastVehicle = Game.PlayerPed.LastVehicle;
                            if (deformationMultiplier != -1) API.SetVehicleHandlingFloat(lastVehicle.Handle, "CHandlingData", "fDeformationDamageMult", 0.0f);
                            API.SetVehicleHandlingFloat(lastVehicle.Handle, "CHandlingData", "fBrakeForce", 1.0f);
                            if (weaponsDamageMultiplier != -1) API.SetVehicleHandlingFloat(lastVehicle.Handle, "CHandlingData", "fWeaponDamageMult", 0.124f);
                            API.SetVehicleHandlingFloat(lastVehicle.Handle, "CHandlingData", "fCollisionDamageMult", 0.0f);
                            API.SetVehicleHandlingFloat(lastVehicle.Handle, "CHandlingData", "fEngineDamageMult", 0.0f);
                        }
                    }
                    isPedInSameVehicle = false;
                }

                await Task.FromResult(0);
            }
        }

        async static Task AdditionalVehicleFuckery()
        {
            if (!IsPlayerDrivingAVehicle()) return;

            //if (isPedInSameVehicle)
            //{
            //    float factor = 1.0f;
            //    if (healthEngineNew < 900)
            //    {
            //        factor = (float)(healthEngineNew + 200.0) / 1100;
            //    }

            //    if (Game.PlayerPed.CurrentVehicle.ClassType != VehicleClass.Boats)
            //    {
            //        int accelerator = API.GetControlValue(2, (int)Control.VehicleAccelerate);
            //        int brake = API.GetControlValue(2, (int)Control.VehicleBrake);
            //        float speed = API.GetEntitySpeedVector(Game.PlayerPed.CurrentVehicle.Handle, false).Y;
            //        float brk = fBrakeForce;
            //        if (speed >= 1.0f)
            //        {
            //            if (accelerator > 127)
            //            {
            //                float acc = rangedScale(accelerator, 127.0f, 254.0f, 0.1f, 0.1f, 10.0f - (sundayDriverAcceleratorCurve * 2.0f));
            //                factor = factor * acc;
            //            }
            //            if (brake > 127)
            //            {
            //                isBrakingForward = true;
            //                brk = rangedScale(brake, 127.0f, 254.0f, 0.1f, fBrakeForce, 10.0f - (sundayDriverBrakeCurve * 2.0f));
            //            }
            //        }
            //        else if (speed <= -1.0f)
            //        {
            //            if (brake > 127)
            //            {
            //                float rev = rangedScale(brake, 127.0f, 254.0f, 0.1f, 0.1f, 10.0f - (sundayDriverAcceleratorCurve * 2.0f));
            //                factor = factor * rev;
            //            }
            //            if (accelerator > 127)
            //            {
            //                isBrakingReverse = true;
            //                brk = rangedScale(accelerator, 127.0f, 254.0f, 0.1f, fBrakeForce, 10.0f - (sundayDriverBrakeCurve * 2.0f));
            //            }
            //        }
            //        else
            //        {
            //            float entitySpeed = Game.PlayerPed.CurrentVehicle.Speed;
            //            if (entitySpeed < 1.0f)
            //            {
            //                if (isBrakingForward)
            //                {
            //                    API.DisableControlAction(2, (int)Control.VehicleBrake, true);
            //                    Game.PlayerPed.CurrentVehicle.Speed = speed * 0.98f;
            //                    Game.PlayerPed.CurrentVehicle.AreBrakeLightsOn = true;
            //                }
            //                if (isBrakingReverse)
            //                {
            //                    API.DisableControlAction(2, (int)Control.VehicleAccelerate, true);
            //                    Game.PlayerPed.CurrentVehicle.Speed = speed * 0.98f;
            //                    Game.PlayerPed.CurrentVehicle.AreBrakeLightsOn = true;
            //                }
            //                if (isBrakingForward && API.GetDisabledControlNormal(2, (int)Control.VehicleBrake) == 0) isBrakingForward = false;
            //                if (isBrakingReverse && API.GetDisabledControlNormal(2, (int)Control.VehicleAccelerate) == 0) isBrakingReverse = false;
            //            }
            //        }
            //        if (brk > fBrakeForce - 0.02f) brk = fBrakeForce;
            //        API.SetVehicleHandlingFloat(Game.PlayerPed.CurrentVehicle.Handle, "CHandlingData", "fBrakeForce", brk);
            //    }
            //    if (healthEngineNew < engineSafeGuard) factor = limpModeMultiplier;
            //    Game.PlayerPed.CurrentVehicle.EngineTorqueMultiplier = factor;
            //}

            if (preventVehicleFlip && IsPlayerDrivingAVehicle())
            {
                float roll = API.GetEntityRoll(Game.PlayerPed.CurrentVehicle.Handle);
                if ((roll > 75.0f || roll < -75.0f) && Game.PlayerPed.CurrentVehicle.Speed < 2f)
                {
                    API.DisableControlAction(2, 59, true);
                    API.DisableControlAction(2, 60, true);
                    Game.PlayerPed.CurrentVehicle.IsEngineRunning = false;
                    await Plugin.Delay(2000);
                    Game.PlayerPed.Task.LeaveVehicle((LeaveVehicleFlags)4160);
                }
            }
            await Task.FromResult(0);
        }

        public static void DevRepair()
        {
            if (Game.PlayerPed.IsInVehicle() && PlayerInformation.IsDeveloper())
            {
                Game.PlayerPed.CurrentVehicle.Repair();
                Game.PlayerPed.CurrentVehicle.EngineHealth = 1000f;
                Game.PlayerPed.CurrentVehicle.BodyHealth = 1000f;
                Game.PlayerPed.CurrentVehicle.PetrolTankHealth = 1000f;
                Game.PlayerPed.CurrentVehicle.DirtLevel = 0.0f;
                // Leave engine alone if it's already running to prevent the off/on animation
                if (!Game.PlayerPed.CurrentVehicle.IsEngineRunning)
                {
                    Game.PlayerPed.CurrentVehicle.IsEngineRunning = true;
                }
            }
        }
    }
}
