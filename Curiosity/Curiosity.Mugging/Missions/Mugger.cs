﻿using CitizenFX.Core;
using Curiosity.MissionManager.Client;
using Curiosity.MissionManager.Client.Attributes;
using Curiosity.MissionManager.Client.Extensions;
using Curiosity.MissionManager.Client.Interface;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Utils;
using System.Threading.Tasks;
using Ped = Curiosity.MissionManager.Client.Classes.Ped;

namespace Curiosity.Mugging.Missions
{
    [MissionInfo("Mugging", "misMugger", 0f, 0f, 0f, MissionType.Mission, true, "None", PatrolZone.Anywhere)]
    public class Mugger : Mission
    {
        MissionState missionState;

        Vector3 spawnPoint;
        Ped criminal;
        Ped victim;
        Blip locationBlip;

        public override void Start()
        {
            spawnPoint = Game.PlayerPed.Position.AroundStreet(100f, 200f);

            locationBlip = Functions.SetupLocationBlip(spawnPoint);
            RegisterBlip(locationBlip);

            Notify.DispatchAI("CODE 3", "We reports of a mugging taking place, get to the ~o~scene~s~.");

            MissionManager.Instance.RegisterTickHandler(OnMissionTick);
        }

        public override void End()
        {
            MissionManager.Instance.DeregisterTickHandler(OnMissionTick);
        }

        async Task OnMissionTick()
        {
            switch(missionState)
            {
                case MissionState.Start:
                    if (Game.PlayerPed.Position.DistanceTo(spawnPoint) < 100f)
                        missionState = MissionState.SpawnPeds;
                    break;
                case MissionState.SpawnPeds:

                    criminal = await this.PedSpawnRandom(spawnPoint, pedType: PedType.PED_TYPE_CRIMINAL);
                    victim = await this.PedSpawnRandom(spawnPoint);
                    await BaseScript.Delay(1000);

                    if (criminal is null || victim is null)
                    {
                        Stop(EndState.Error);
                        return;
                    }

                    if (!criminal.Exists() || !victim.Exists())
                    {
                        Stop(EndState.Error);
                        return;
                    }

                    criminal.IsImportant = true;
                    criminal.IsMission = true;
                    criminal.IsSuspect = true;

                    victim.IsMission = true;

                    RegisterPed(criminal);
                    RegisterPed(victim);

                    criminal.Fx.Weapons.Give(WeaponHash.Pistol, 60, true, true);

                    criminal.AttachSuspectBlip();

                    criminal.Task.AimAt(victim, -1);
                    victim.Task.HandsUp(-1);

                    missionState = MissionState.SpawnedPeds;
                    break;
                case MissionState.SpawnedPeds:
                    if (Game.PlayerPed.Position.DistanceTo(spawnPoint) < 100f)
                        missionState = MissionState.StartScene;
                    break;
                case MissionState.StartScene:

                    if (locationBlip.Exists())
                        locationBlip.Delete();

                    int r = Utility.RANDOM.Next(1, 4);

                    missionState = MissionState.SceneStarted;

                    if (r == 1)
                    {
                        criminal.Task.ShootAt(victim.Fx, 5000, FiringPattern.Default);
                        victim.Task.ReactAndFlee(criminal.Fx);
                        await BaseScript.Delay(5000);
                        criminal.Task.ReactAndFlee(Game.PlayerPed);
                        victim.Fx.Kill();

                        if (Utility.RANDOM.Next(1, 3) == 2)
                        {
                            criminal.Task.FightAgainst(Game.PlayerPed);
                            criminal.IsFriendly = false;
                        }
                    }
                    else
                    {
                        criminal.Task.ReactAndFlee(Game.PlayerPed);
                        victim.Task.ReactAndFlee(criminal.Fx);
                    }

                    break;
            }

            if (NumberPedsArrested > 0)
                Pass();

            if (NumberPedsArrested == 0)
            {
                if (criminal != null && criminal.IsDead)
                    PassButFailedRequirements($"Suspect is dead...");
            }
        }
    }

    enum MissionState
    {
        Start,
        SpawnPeds,
        SpawnedPeds,
        End,
        StartScene,
        SceneStarted
    }
}
