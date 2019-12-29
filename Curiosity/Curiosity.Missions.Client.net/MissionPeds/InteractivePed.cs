﻿using CitizenFX.Core;
using CitizenFX.Core.UI;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;
using Curiosity.Missions.Client.net.Wrappers;
using System;
using System.Threading.Tasks;
using Curiosity.Missions.Client.net.Extensions;
using Curiosity.Shared.Client.net.Extensions;
using Curiosity.Missions.Client.net.DataClasses;
// INTERACTIONS
using Curiosity.Missions.Client.net.Scripts.Interactions.PedInteractions;
using Curiosity.Shared.Client.net;
using System.Collections.Generic;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Global.Shared.net;
using Newtonsoft.Json;
using Curiosity.Missions.Client.net.Scripts;

namespace Curiosity.Missions.Client.net.MissionPeds
{
    abstract class InteractivePed : Entity, IEquatable<Ped>
    {
        private static Client client = Client.GetInstance();

        public const int MovementUpdateInterval = 10;
        private const string MOVEMENT_ANIMATION_SET_DRUNK = "MOVE_M@DRUNK@VERYDRUNK";
        public readonly Ped Ped;
        private Ped _target;
        private Vehicle Vehicle;
        // MENU STATES
        public bool IsMenuVisible = false;
        public bool IsTrafficStop;
        public bool IsPerformingCpr = false;
        public bool HasCprFailed = false;
        public bool HasIdentifcationBeenRan;
        // Safe
        public bool IsVictim;
        public bool IsHostage;
        // Evil
        public bool CanBeArrested;
        public bool IsHandcuffed;
        public bool IsArrested;
        // Arrested States
        public bool IsUnderTheInfluence;
        public bool IsCarryingIllegalItems;
        public bool IsCarryingStolenItems;
        public bool IsUsingCannabis;
        public bool IsUsingCocaine;
        public bool IsAllowedToBeSearched;
        public bool HasLostId;
        public bool HasProvidedId;
        public bool HasBeenSearched;
        public bool HasBeenGrabbed;
        // MENU STATES
        private bool IsCoronerCalled;
        // Settings
        public virtual string MovementStyle { get; set; }
        public virtual bool PlayAudio { get; set; }
        // Events
        private EntityEventWrapper _eventWrapper;
        private bool _IsAttackingTarget, _hasBeenReleased, _stolenCar;

        private static string helpText;

        private string _firstname, _surname, _dateOfBirth, _offence;
        private int _attitude, _bloodAlcaholLimit, _chanceOfFlee, _chanceOfShootAndFlee, _numberOfCitations;
        private DateTime _currentMovementUpdateTime;

        // PED INFORMATION
        public string Name
        {
            get
            {
                return $"{_firstname} {_surname}";
            }
        }
        public string DateOfBirth
        {
            get
            {
                return _dateOfBirth;
            }
        }
        public int Attitude
        {
            get
            {
                return _attitude;
            }
        }

        public int BloodAlcaholLimit
        {
            get
            {
                return _bloodAlcaholLimit;
            }
        }

        public int NumberOfCitations
        {
            get
            {
                return _numberOfCitations;
            }
        }

        public string Offence
        {
            get
            {
                return _offence;
            }
        }

        static InteractivePed()
        {
        }

        protected InteractivePed(int handle) : base(handle)
        {
            if (Classes.PlayerClient.ClientInformation.IsDeveloper() && Client.DeveloperNpcUiEnabled)
            {
                Screen.ShowNotification($"~r~[~g~D~b~E~y~V~o~]~w~ Creating Interactive Ped");
            }

            if (DecorGetBool(handle, Client.NPC_WAS_RELEASED))
            {
                Screen.ShowNotification("~r~This pedestrian was recently released.");
                return;
            }

            this.Ped = new Ped(handle);

            if (this.Ped.IsInVehicle())
                this.Vehicle = this.Ped.CurrentVehicle;

            NetworkRequestControlOfEntity(Ped.Handle);
            SetNetworkIdCanMigrate(Ped.NetworkId, true);
            NetworkRegisterEntityAsNetworked(Ped.NetworkId);
            SetNetworkIdExistsOnAllMachines(Ped.NetworkId, true);

            if (!IsEntityAMissionEntity(Ped.Handle))
                SetEntityAsMissionEntity(Ped.Handle, true, true);

            API.SetPedFleeAttributes(Ped.Handle, 0, false);
            API.SetBlockingOfNonTemporaryEvents(Ped.Handle, true);
            API.TaskSetBlockingOfNonTemporaryEvents(Ped.Handle, true);
            API.SetPedDiesInWater(Ped.Handle, false);
            API.SetPedDiesWhenInjured(Ped.Handle, false);
            API.SetPedDiesInstantlyInWater(Ped.Handle, true);
            Ped.SetCombatAttributes((CombatAttributes)17, false);

            IsMenuVisible = false;
            IsPerformingCpr = false;
            IsCoronerCalled = false;
            HasCprFailed = false;
            IsArrested = false;
            HasProvidedId = false;

            IsUsingCannabis = false;
            IsUsingCocaine = false;
            HasLostId = false;
            HasBeenSearched = false;
            HasIdentifcationBeenRan = false;

            _chanceOfFlee = 0;
            _chanceOfShootAndFlee = 0;

            this.Ped.Health = 200;
            this.Ped.IsPersistent = true;

            _firstname = string.Empty;
            _surname = string.Empty;

            if (this.Ped.Gender == Gender.Female)
            {
                _firstname = PedNames.FirstNameFemale[Client.Random.Next(PedNames.FirstNameFemale.Count)];
            }
            else
            {
                _firstname = PedNames.FirstNameMale[Client.Random.Next(PedNames.FirstNameMale.Count)];
            }
            _surname = PedNames.Surname[Client.Random.Next(PedNames.Surname.Count)];

            DateTime StartDateForDriverDoB = new DateTime(1949, 1, 1);
            double Range = (DateTime.Today - StartDateForDriverDoB).TotalDays;
            Range = Range - 6570; // MINUS 18 YEARS
            _dateOfBirth = StartDateForDriverDoB.AddDays(Client.Random.Next((int)Range)).ToString("yyyy-MM-dd");

            _attitude = Client.Random.Next(100);

            int breathlyzerLimit = Client.Random.Next(100);
            _bloodAlcaholLimit = 0;
            IsUnderTheInfluence = false;
            IsAllowedToBeSearched = false;

            if (breathlyzerLimit >= 60)
            {
                _bloodAlcaholLimit = Client.Random.Next(1, 7);
                if (breathlyzerLimit >= 88)
                {
                    IsUnderTheInfluence = true;
                    IsAllowedToBeSearched = true;
                    CanBeArrested = true;
                    _bloodAlcaholLimit = Client.Random.Next(8, 10);
                    _chanceOfFlee = Client.Random.Next(25, 30);

                    if (breathlyzerLimit >= 95)
                    {
                        _bloodAlcaholLimit = Client.Random.Next(10, 20);
                        _chanceOfShootAndFlee = Client.Random.Next(1, 5);
                    }
                }
            }

            if (Client.Random.Next(100) >= 85)
            {
                IsUsingCannabis = true; // Its weed ffs
                IsAllowedToBeSearched = true;
            }

            if (Client.Random.Next(100) >= 90)
            {
                IsUsingCocaine = true;
                CanBeArrested = true;
                IsAllowedToBeSearched = true;
            }

            if (Client.Random.Next(100) >= 95)
            {
                HasLostId = true;
                IsAllowedToBeSearched = true;
            }

            if (Client.Random.Next(100) >= 95)
            {
                IsAllowedToBeSearched = true;
            }

            _numberOfCitations = Client.Random.Next(8);

            _offence = "~g~NONE";
            if (Client.Random.Next(100) >= 75)
            {
                List<string> Offense = new List<string>() { "WANTED BY LSPD", "WANTED FOR ASSAULT", "WANTED FOR UNPAID FINES", "WANTED FOR RUNNING FROM THE POLICE", "WANTED FOR EVADING LAW", "WANTED FOR HIT AND RUN", "WANTED FOR DUI" };
                _offence = $"~r~{Offense[Client.Random.Next(Offense.Count)]}";
                CanBeArrested = true;
            }

            _hasBeenReleased = false;

            Create();
        }

        private async void Create()
        {

            this._eventWrapper = new EntityEventWrapper(this.Ped);
            this._eventWrapper.Died += new EntityEventWrapper.OnDeathEvent(this.OnDied);
            this._eventWrapper.Updated += new EntityEventWrapper.OnWrapperUpdateEvent(this.Update);
            this._eventWrapper.Aborted += new EntityEventWrapper.OnWrapperAbortedEvent(this.Abort);

            client.RegisterEventHandler("curiosity:interaction:idRequesed", new Action<int>(OnIdRequested));
            client.RegisterEventHandler("curiosity:interaction:arrest", new Action<int>(OnArrest));
            client.RegisterEventHandler("curiosity:interaction:idRan", new Action<int>(OnIdRan));
            client.RegisterEventHandler("curiosity:interaction:handcuffs", new Action<int, bool>(OnHandcuffs));
            client.RegisterEventHandler("curiosity:interaction:cpr", new Action<int, bool>(OnCpr));
            client.RegisterEventHandler("curiosity:interaction:cpr:failed", new Action<int>(OnCprFailed));
            client.RegisterEventHandler("curiosity:interaction:coroner", new Action<int>(OnCoronerCalled));
            client.RegisterEventHandler("curiosity:interaction:searched", new Action<int, bool>(OnPedHasBeenSearched));
            client.RegisterEventHandler("curiosity:interaction:leaveAllGroups", new Action<int>(OnPedLeaveGroups));
            client.RegisterEventHandler("curiosity:interaction:released", new Action<int>(OnPedHasBeenReleased));
            client.RegisterEventHandler("curiosity:interaction:stolencar", new Action<int>(OnStolenCar));
            client.RegisterEventHandler("curiosity:interaction:flagRelease", new Action<int>(OnFlagRelease));
            client.RegisterEventHandler("curiosity:interaction:grab", new Action<int>(OnGrabPed));

            client.RegisterEventHandler("curiosity:setting:group:join", new Action<int>(OnGroupJoin));
            client.RegisterEventHandler("curiosity:setting:group:leave", new Action<int>(OnGroupLeave));
            // car stolen
            client.RegisterEventHandler("curiosity:interaction:hasLostId", new Action<int>(OnHasLostId));

            client.RegisterTickHandler(OnShowHelpTextTask);
            client.RegisterTickHandler(OnMenuTask);

            if (Classes.PlayerClient.ClientInformation.IsDeveloper())
                client.RegisterTickHandler(OnShowDeveloperOverlayTask);

            if (Ped.AttachedBlip == null)
            {
                Ped.AttachBlip();
                Ped.AttachedBlip.Color = BlipColor.TrevorOrange;
                Ped.AttachedBlip.Sprite = BlipSprite.Standard;
                Ped.AttachedBlip.Scale = 0.5f;
            }

            if (IsUnderTheInfluence)
            {
                if (!HasAnimSetLoaded(MOVEMENT_ANIMATION_SET_DRUNK))
                {
                    RequestAnimSet(MOVEMENT_ANIMATION_SET_DRUNK);
                    while (!HasAnimSetLoaded(MOVEMENT_ANIMATION_SET_DRUNK))
                    {
                        await Client.Delay(0);
                    }
                }
                this.Ped.MovementAnimationSet = MOVEMENT_ANIMATION_SET_DRUNK;
            }

            if (!this.Ped.IsInVehicle()) // Ignore this if they are in a vehicle
            {
                if (_chanceOfShootAndFlee == 4)
                {
                    WeaponHash weaponHash = WeaponHash.Pistol;
                    if (Client.Random.Next(5) == 4)
                    {
                        weaponHash = WeaponHash.SawnOffShotgun;
                    }
                    this.Ped.Weapons.Give(weaponHash, 1, true, true);
                    this.Ped.Task.ShootAt(Game.PlayerPed);
                    await Client.Delay(3000);
                    this.Ped.Task.FleeFrom(Game.PlayerPed);

                    if (Ped.AttachedBlip != null)
                        Ped.AttachedBlip.Color = BlipColor.Red;
                }
                else if (_chanceOfFlee >= 28)
                {
                    this.Ped.Task.FleeFrom(Game.PlayerPed);

                    if (Ped.AttachedBlip != null)
                        Ped.AttachedBlip.Color = BlipColor.Red;
                }
            }
        }

        protected bool Equals(InteractivePed other)
        {
            return (!base.Equals(other) ? false : object.Equals(this.Ped, other.Ped));
        }

        public override bool Equals(object obj)
        {
            bool flag;
            if (obj == null)
            {
                flag = false;
            }
            else
            {
                flag = (this) != obj ? obj.GetType() != GetType() ? false : Equals((InteractivePed)obj) : true;
            }
            return flag;
        }

        public bool Equals(Ped other)
        {
            return object.Equals(this.Ped, other);
        }

        public static implicit operator Ped(InteractivePed v)
        {
            return v.Ped;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() * 397 ^ (this.Ped != null ? this.Ped.GetHashCode() : 0);
        }

        private async Task OnShowHelpTextTask()
        {
            await Task.FromResult(0);
            if (!string.IsNullOrEmpty(helpText))
                Screen.DisplayHelpTextThisFrame(helpText);
        }

        private void OnDied(EntityEventWrapper sender, Entity entity)
        {
            Blip currentBlip = base.AttachedBlip;
            if (currentBlip != null)
            {
                currentBlip.Delete();
            }
        }

        public async void Update(EntityEventWrapper entityEventWrapper, Entity entity)
        {
            if (!NetworkHasControlOfEntity(Handle))
            {
                while (!API.NetworkRequestControlOfEntity(Handle))
                {
                    await BaseScript.Delay(0);
                }
            }

            if (_hasBeenReleased)
            {
                OnPedLeaveGroups(Ped.Handle);
                OnPedHasBeenReleased(Ped.Handle);

                if (Ped.IsInGroup)
                    Ped.PedGroup.Delete();

                Ped.LeaveGroup();

                await Client.Delay(100);

                Ped.Task.WanderAround();

                if (Ped.IsOccluded)
                    base.Delete();

                return;
            }

            if (this.Ped == null)
            {
                base.Delete();

                client.DeregisterTickHandler(OnMenuTask);
                client.DeregisterTickHandler(OnShowHelpTextTask);

                if (Classes.PlayerClient.ClientInformation.IsDeveloper())
                    client.DeregisterTickHandler(OnShowDeveloperOverlayTask);

                return;
            }

            if (this.Ped.IsBeingStunned)
            {
                this.Ped.Health = 200;

                if (Client.Random.Next(5) >= 3 && !IsArrested)
                {
                    IsArrested = true;
                    ArrestInteractions.InteractionArrestInit(this);
                }
            }

            if (this.Ped.IsInjured)
            {
                if (Client.Random.Next(5) >= 3 && !IsArrested)
                {
                    if (Client.Random.Next(30) == 28)
                    {
                        this.Ped.SetConfigFlag(187, true);
                    }
                    else
                    {
                        this.Ped.SetConfigFlag(166, true);
                        IsArrested = true;
                        ArrestInteractions.InteractionArrestInit(this);
                    }
                }
            }

            if (this.Ped.IsOccluded)
            {
                if (Ped.AttachedBlip.Alpha == 255)
                    Ped.AttachedBlip.Alpha = 0;
            }
            else
            {
                if (Ped.AttachedBlip.Alpha == 0)
                    Ped.AttachedBlip.Alpha = 255;
            }

            if (this.Ped.Position.Distance(Game.PlayerPed.Position) >= 20f && !this.Ped.IsInVehicle())
            {
                IsArrested = false;
                this.Ped.LeaveGroup();
                
                if (Ped.IsInGroup)
                    Ped.PedGroup.Delete();

                this.Ped.Task.FleeFrom(Game.PlayerPed);

                if (Ped.AttachedBlip != null)
                    Ped.AttachedBlip.Color = BlipColor.Red;
            }
            else if (Ped.Position.Distance(Game.PlayerPed.Position) >= 200f)
            {
                OnPedLeaveGroups(Ped.Handle);
                OnPedHasBeenReleased(Ped.Handle);
            }
        }

        public void Abort(EntityEventWrapper sender, Entity entity)
        {
            base.Delete();

            client.DeregisterTickHandler(OnMenuTask);
            client.DeregisterTickHandler(OnShowHelpTextTask);

            if (Classes.PlayerClient.ClientInformation.IsDeveloper())
                client.DeregisterTickHandler(OnShowDeveloperOverlayTask);
        }

        private async Task OnMenuTask()
        {
            await Task.FromResult(0);
            helpText = string.Empty;

            if (Scripts.MissionEvents.HasAcceptedCallout) return; // Should also check at the traffic stop

            if (this.Position.VDist(Game.PlayerPed.Position) <= 120f)
            {
                if (CanDisplayMenu())
                {
                    helpText = "Press ~INPUT_CONTEXT~ to open the ~b~Interaction Menu";

                    if (Game.IsControlPressed(0, Control.Context))
                    {
                        helpText = string.Empty;
                        Scripts.Menus.PedInteractionMenu.MenuBase.Open(this);
                    }
                }

                if (Game.PlayerPed.IsAiming && Ped.IsAlive && !IsArrested && !Game.PlayerPed.IsInVehicle()) {
                    if (this.Position.Distance(Game.PlayerPed.Position) > 2f && this.Position.Distance(Game.PlayerPed.Position) <= 20f)
                    {
                        int entityHandle = 0;
                        Ped pedBeingAimedAt = null;
                        if (API.GetEntityPlayerIsFreeAimingAt(Game.Player.Handle, ref entityHandle))
                        {
                            if (entityHandle == 0) return;

                            if (API.GetEntityType(entityHandle) == 1 && API.GetPedType(entityHandle) != 28)
                            {
                                pedBeingAimedAt = new Ped(entityHandle);
                            }
                        }

                        if (pedBeingAimedAt != null)
                        {
                            if (Ped == pedBeingAimedAt)
                            {
                                if (Ped.IsInVehicle())
                                {
                                    Screen.DisplayHelpTextThisFrame("Press ~INPUT_CONTEXT~ to demand the suspect to exit their vehicle.");
                                }
                                else
                                {
                                    Screen.DisplayHelpTextThisFrame("Press ~INPUT_CONTEXT~ to demand the suspect to get on their knees.");
                                }

                                if (Game.IsControlJustPressed(0, Control.Context))
                                {
                                    IsArrested = true;
                                    ArrestInteractions.InteractionArrestInit(this);
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool CanDisplayMenu()
        {
            try
            {
                if (Scripts.Menus.PedInteractionMenu.MenuBase.MainMenu != null)
                    IsMenuVisible = Scripts.Menus.PedInteractionMenu.MenuBase.AnyMenuVisible();

                //if (Classes.PlayerClient.ClientInformation.IsDeveloper())
                //    Screen.ShowSubtitle($"Menu: {IsMenuVisible}, CPR: {IsPerformingCpr}");

                if (this.Ped.IsInVehicle())
                {
                    return this.Position.Distance(Game.PlayerPed.Position) <= 4f && !IsMenuVisible && !IsPerformingCpr && !IsCoronerCalled && !Game.PlayerPed.IsInVehicle() && !_hasBeenReleased;
                }

                return this.Position.Distance(Game.PlayerPed.Position) <= 2f && !IsMenuVisible && !IsPerformingCpr && !IsCoronerCalled && !Game.PlayerPed.IsInVehicle() && !_hasBeenReleased;
            }
            catch (Exception ex)
            {
                Log.Error("InteractivePed -> CanDisplayMenu");
                return false;
            }
        }

        private void OnCpr(int handle, bool state)
        {
            if (Handle == handle)
                IsPerformingCpr = state;
        }

        private void OnCprFailed(int handle)
        {
            if (Handle == handle)
            {
                IsPerformingCpr = false;
                HasCprFailed = true;
            }
        }

        private void OnCoronerCalled(int handle)
        {
            if (Handle == handle)
                IsCoronerCalled = true;
        }

        public void OnIdRequested(int handle)
        {
            if (Handle == handle)
                HasProvidedId = true;
        }

        public void OnPedHasBeenSearched(int handle, bool illegalItems)
        {
            if (Handle == handle)
            {
                HasBeenSearched = true;
                IsCarryingIllegalItems = illegalItems;
                CanBeArrested = illegalItems;
            }
        }

        public void OnHandcuffs(int handle, bool state)
        {
            if (Handle == handle)
            {
                SoundManager.PlayAudio($"sfx/CUFFS_TIGHTEN_01");
                
                API.SetPedFleeAttributes(Handle, 0, false);
                API.SetBlockingOfNonTemporaryEvents(Handle, true);
                API.TaskSetBlockingOfNonTemporaryEvents(Handle, true);

                IsHandcuffed = state;
                IsArrested = state;
                DecorSetBool(Handle, Client.NPC_ARRESTED, state);

                if (!IsArrested)
                {
                    if (Ped.IsInGroup)
                        Ped.PedGroup.Delete();
                }
            }
        }

        public void OnHasLostId(int handle)
        {
            if (HasProvidedId) return;

            if (Handle == handle)
            {
                if (Client.Random.Next(80, 100) >= 95)
                {
                    HasLostId = true;
                    IsAllowedToBeSearched = true;
                }
            }
        }

        public void OnIdRan(int handle)
        {
            if (Handle == handle)
            {
                HasIdentifcationBeenRan = true;
            }
        }

        public void OnStolenCar(int handle)
        {
            if (Handle == handle)
            {
                _stolenCar = true;
            }
        }

        public void OnGroupJoin(int handle)
        {
            if (Handle == handle)
            {
                int playerGroupId = API.GetPedGroupIndex(Game.PlayerPed.Handle);

                if (playerGroupId < 0)
                {
                    playerGroupId = API.CreateGroup(0);
                    SetPedAsGroupMember(playerGroupId, Game.PlayerPed.Handle);
                    SetPedAsGroupLeader(Game.PlayerPed.Handle, playerGroupId);
                }

                SetPedAsGroupMember(handle, playerGroupId);
                SetPedCanTeleportToGroupLeader(handle, playerGroupId, true);
            }
        }

        public async void OnGroupLeave(int handle)
        {
            await BaseScript.Delay(0);
            if (Handle == handle)
            {
                Ped.Task.ClearAll();
                Ped.Task.ClearSecondary();
                Game.PlayerPed.Task.ClearSecondary();

                Ped.LeaveGroup();
            }
        }

        public void OnFlagRelease(int handle)
        {
            if (Handle == handle)
            {
                _hasBeenReleased = true;
            }
        }

        async void OnPedLeaveGroups(int handle)
        {
            await BaseScript.Delay(100);

            if (Client.DeveloperNpcUiEnabled)
            {
                Screen.ShowNotification("~b~NPC: ~w~OnPedLeaveGroups");
                Screen.ShowNotification($"~b~NPC: ~w~{Handle}\nEvent: {handle}");
            }

            if (Handle == handle)
            {
                Ped.Task.ClearAll();
                Ped.Task.ClearSecondary();
                Game.PlayerPed.Task.ClearSecondary();

                Ped.NeverLeavesGroup = false;
                Ped.LeaveGroup();
                API.RemovePedFromGroup(Handle);

                if (Ped.IsInGroup)
                    Ped.PedGroup.Delete();

                if (Client.DeveloperNpcUiEnabled)
                {
                    Screen.ShowNotification("~b~NPC: ~w~OnPedLeaveGroups: ~r~Success");
                }
            }
        }

        private async void OnPedHasBeenReleased(int handle)
        {
            if (Handle == handle)
            {
                _hasBeenReleased = true;

                DecorSetBool(handle, Client.NPC_ARRESTED, false);

                Ped.SetConfigFlag(292, false);
                Ped.SetConfigFlag(301, false);

                if (Ped.IsInVehicle() && Ped.CurrentVehicle == Client.CurrentVehicle)
                {
                    Ped.Task.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                }

                if (Ped.CurrentVehicle != null)
                {
                    Ped.CurrentVehicle.IsPositionFrozen = false;

                    if (Ped.CurrentVehicle.AttachedBlip != null)
                    {
                        if (Ped.CurrentVehicle.AttachedBlip.Exists())
                            Ped.CurrentVehicle.AttachedBlip.Delete();
                    }
                }

                if (Ped.AttachedBlip != null)
                {
                    if (Ped.AttachedBlip.Exists())
                        Ped.AttachedBlip.Delete();
                }

                Ped.Task.ClearSecondary();
                Game.PlayerPed.Task.ClearSecondary();

                Ped.Task.ClearAll();
                Ped.MarkAsNoLongerNeeded();
                Ped.IsPersistent = false;
                Ped.LeaveGroup();

                API.SetPedFleeAttributes(Handle, 0, true);
                API.SetBlockingOfNonTemporaryEvents(Handle, false);

                if (Ped.IsInGroup)
                    Ped.PedGroup.Delete();

                API.TaskSetBlockingOfNonTemporaryEvents(Ped.Handle, false);

                DecorSetBool(Handle, Client.NPC_WAS_RELEASED, _hasBeenReleased);

                client.DeregisterTickHandler(OnMenuTask);
                client.DeregisterTickHandler(OnShowHelpTextTask);

                if (Classes.PlayerClient.ClientInformation.IsDeveloper())
                    client.DeregisterTickHandler(OnShowDeveloperOverlayTask);

                int playerGroupId = API.GetPedGroupIndex(Game.PlayerPed.Handle);
                RemoveGroup(playerGroupId);

                if (DecorExistOn(Handle, Client.NPC_CURRENT_VEHICLE))
                {
                    int vehId = DecorGetInt(Handle, Client.NPC_CURRENT_VEHICLE);
                    if (DoesEntityExist(vehId))
                    {
                        Vehicle vehicle = new Vehicle(vehId);
                        Ped.Task.EnterVehicle(vehicle, VehicleSeat.Driver, 5000, 5f);
                        await Client.Delay(5000);
                        Ped.Task.WanderAround(Ped.Position, 1000f);

                        if (vehicle.AttachedBlip != null)
                        {
                            if (vehicle.AttachedBlip.Exists())
                                vehicle.AttachedBlip.Delete();
                        }
                    }
                }
                else
                {
                    Ped.Task.WanderAround(Ped.Position, 1000f);
                }
            }
        }

        public event InteractivePed.OnAttackingTargetEvent AttackTarget;
        public delegate void OnAttackingTargetEvent(Ped target);

        void OnGrabPed(int handle)
        {
            if (handle != Handle) return;

            if (HasBeenGrabbed)
            {
                Ped.Detach();
                HasBeenGrabbed = false;
            }
            else if (!HasBeenGrabbed)
            {
                Vector3 attachedPos = new Vector3(-0.3f, 0.4f, 0.0f);
                Ped.AttachTo(Game.PlayerPed, attachedPos);
                SetBlockingOfNonTemporaryEvents(handle, true);
                HasBeenGrabbed = true;
            }
        }

        private async void OnArrest(int handle)
        {
            if (Handle != handle)
            {
                base.Delete();

                client.DeregisterTickHandler(OnMenuTask);
                client.DeregisterTickHandler(OnShowHelpTextTask);

                if (Classes.PlayerClient.ClientInformation.IsDeveloper())
                    client.DeregisterTickHandler(OnShowDeveloperOverlayTask);

                return;
            }

            if (!IsHandcuffed)
            {
                List<string> vs = new List<string> { $"~o~WHY AREN'T THEY CUFFED!", "~o~Handcuff them you idoit!", "~r~WHAT IS YOUR MAJOR MALFUNCTION! PUT ON THE CUFFS!!!", "~r~Cuff them, fecking muppet!" };
                Screen.ShowNotification(vs[Client.Random.Next(vs.Count)]);
                return;
            }

            ArrestedPedData arrestedPedData = new ArrestedPedData();
            arrestedPedData.IsAllowedToBeArrested = CanBeArrested;

            arrestedPedData.IsDrunk = IsUnderTheInfluence;
            arrestedPedData.IsDrugged = IsUsingCocaine || IsUsingCannabis;
            arrestedPedData.IsDrivingStolenCar = _stolenCar;
            arrestedPedData.IsCarryingIllegalItems = IsCarryingIllegalItems;

            arrestedPedData.IsAllowedToBeArrested = (arrestedPedData.IsDrunk || arrestedPedData.IsDrugged || arrestedPedData.IsDrivingStolenCar || arrestedPedData.IsCarryingIllegalItems);

            string encoded = Encode.StringToBase64(JsonConvert.SerializeObject(arrestedPedData));

            Client.TriggerServerEvent("curiosity:Server:Missions:ArrestedPed", encoded);

            if (CanBeArrested)
            {
                Screen.ShowNotification($"~g~They've been booked.");
            }

            Ped.IsPositionFrozen = false;
            if (Ped.IsInVehicle())
            {
                Ped.SetConfigFlag(292, false);
                Ped.Task.LeaveVehicle();
            }
            API.NetworkFadeOutEntity(Handle, true, false);
            _hasBeenReleased = true;
            await Client.Delay(5000);
            Ped.Delete();
        }

        private async Task OnShowDeveloperOverlayTask()
        {
            await Task.FromResult(0);

            if (!Client.DeveloperNpcUiEnabled)
            {
                await BaseScript.Delay(1000);
                return;
            }

            if (Position.Distance(Game.PlayerPed.Position) >= 6) return;

            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

            keyValuePairs.Add("Name", Name);
            keyValuePairs.Add("DoB", DateOfBirth);
            keyValuePairs.Add("Health", $"{Health} / {MaxHealth}");
            keyValuePairs.Add("-", "");
            keyValuePairs.Add("_ChanceOfFlee", $"{_chanceOfFlee}");
            keyValuePairs.Add("_ChanceOfShootAndFlee", $"{_chanceOfShootAndFlee}");
            keyValuePairs.Add("--", "");
            keyValuePairs.Add("IsArrested", $"{IsArrested}");
            keyValuePairs.Add("IsHandcuffed", $"{IsHandcuffed}");
            keyValuePairs.Add("HasProvidedId", $"{HasProvidedId}");
            keyValuePairs.Add("HasLostId", $"{HasLostId}");
            keyValuePairs.Add("HasBeenSearched", $"{HasBeenSearched}");
            keyValuePairs.Add("---", "");
            keyValuePairs.Add("CanBeArrested", $"{CanBeArrested}");
            keyValuePairs.Add("IsAllowedToBeSearched", $"{IsAllowedToBeSearched}");
            keyValuePairs.Add("IsUsingCannabis", $"{IsUsingCannabis}");
            keyValuePairs.Add("IsUsingCocaine", $"{IsUsingCocaine}");
            keyValuePairs.Add("IsCarryingIllegalItems", $"{IsCarryingIllegalItems}");
            keyValuePairs.Add("IsUnderTheInfluence", $"{IsUnderTheInfluence}");
            keyValuePairs.Add("IsInGroup", $"{Ped.IsInGroup}");
            keyValuePairs.Add("----", "");
            keyValuePairs.Add("BloodAlcaholLimit", $"{BloodAlcaholLimit}");
            keyValuePairs.Add("Attitude", $"{Attitude}");
            keyValuePairs.Add("NumberOfCitations", $"{NumberOfCitations}");
            keyValuePairs.Add("Offence", $"{Offence}");
            keyValuePairs.Add("-----", "");
            keyValuePairs.Add("NPC_CURRENT_VEHICLE", $"{DecorGetInt(Ped.Handle, Client.NPC_CURRENT_VEHICLE)}");
            keyValuePairs.Add("Local Vehicle", $"{Vehicle.Handle}");

            Wrappers.Helpers.DrawData(this, keyValuePairs);
        }
    }
}
