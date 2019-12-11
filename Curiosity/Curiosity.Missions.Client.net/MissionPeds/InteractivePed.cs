using CitizenFX.Core;
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

namespace Curiosity.Missions.Client.net.MissionPeds
{
    abstract class InteractivePed : Entity, IEquatable<Ped>
    {
        private static Client client = Client.GetInstance();

        public const int MovementUpdateInterval = 5;
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
        // MENU STATES
        private bool IsCoronerCalled;
        // Settings
        public virtual string MovementStyle { get; set; }
        public virtual bool PlayAudio { get; set; }
        // Events
        private EntityEventWrapper _eventWrapper;
        private bool _IsAttackingTarget;

        private static string helpText;

        private string _firstname, _surname, _dateOfBirth, _offence;
        private int _attitude, _bloodAlcaholLimit, _chanceOfFlee, _chanceOfShootAndFlee, _numberOfCitations;

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

            Create();
        }

        private async void Create()
        {
            this._eventWrapper = new EntityEventWrapper(this.Ped);
            this._eventWrapper.Died += new EntityEventWrapper.OnDeathEvent(this.OnDied);
            this._eventWrapper.Updated += new EntityEventWrapper.OnWrapperUpdateEvent(this.Update);
            this._eventWrapper.Aborted += new EntityEventWrapper.OnWrapperAbortedEvent(this.Abort);

            client.RegisterEventHandler("curiosity:interaction:idRequesed", new Action<int>(OnIdRequested));
            
            client.RegisterEventHandler("curiosity:interaction:idRan", new Action<int>(OnIdRan));
            client.RegisterEventHandler("curiosity:interaction:handcuffs", new Action<int, bool>(OnHandcuffs));
            client.RegisterEventHandler("curiosity:interaction:cpr", new Action<int, bool>(OnCpr));
            client.RegisterEventHandler("curiosity:interaction:cpr:failed", new Action<int>(OnCprFailed));
            client.RegisterEventHandler("curiosity:interaction:coroner", new Action<int>(OnCoronerCalled));
            client.RegisterEventHandler("curiosity:interaction:searched", new Action<int, bool>(OnPedHasBeenSearched));

            client.RegisterEventHandler("curiosity:interaction:released", new Action<int>(OnPedHasBeenReleased));
            // car stolen
            client.RegisterEventHandler("curiosity:interaction:hasLostId", new Action<int>(OnHasLostId));

            client.RegisterTickHandler(OnShowHelpTextTask);
            client.RegisterTickHandler(OnMenuTask);

            if (Classes.PlayerClient.ClientInformation.IsDeveloper())
                client.RegisterTickHandler(OnShowDeveloperOverlayTask);

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
                }
                else if (_chanceOfFlee >= 28)
                {
                    this.Ped.Task.FleeFrom(Game.PlayerPed);
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

        public void Update(EntityEventWrapper entityEventWrapper, Entity entity)
        {
            if (this.Ped.IsBeingStunned)
            {
                this.Ped.Health = 200;

                if (Client.Random.Next(5) >= 3 && !IsArrested)
                {
                    IsArrested = true;
                    ArrestInteractions.InteractionArrestInit(this);
                }
            }

            if (this.Ped.Health < 100)
            {
                this.Ped.Health = 100; // stop them from dying

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

            if (this.Ped.Position.Distance(Game.PlayerPed.Position) >= 20f && IsArrested && !this.Ped.IsInVehicle())
            {
                IsArrested = false;
                this.Ped.LeaveGroup();
                this.Ped.Task.FleeFrom(Game.PlayerPed);
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
                if (this.Position.Distance(Game.PlayerPed.Position) <= 2f && CanDisplayMenu())
                {
                    helpText = "Press ~INPUT_CONTEXT~ to open the ~b~Interaction Menu";

                    if (Game.IsControlPressed(0, Control.Context))
                    {
                        helpText = string.Empty;
                        Scripts.Menus.PedInteractionMenu.MenuBase.Open(this);
                    }
                }

                if (Game.PlayerPed.IsAiming && Ped.IsAlive && !IsArrested) {
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

                return !IsMenuVisible && !IsPerformingCpr && !IsCoronerCalled && !Game.PlayerPed.IsInVehicle();
            }
            catch (Exception ex)
            {
                Log.Error("InteractivePed -> CanDisplayMenu");
                return false;
            }
        }

        private void OnCpr(int networkId, bool state)
        {
            if (Ped.NetworkId == networkId)
                IsPerformingCpr = state;
        }

        private void OnCprFailed(int networkId)
        {
            if (Ped.NetworkId == networkId)
            {
                IsPerformingCpr = false;
                HasCprFailed = true;
            }
        }

        private void OnCoronerCalled(int networkId)
        {
            if (Ped.NetworkId == networkId)
                IsCoronerCalled = true;
        }

        public void OnIdRequested(int networkId)
        {
            if (Ped.NetworkId == networkId)
                HasProvidedId = true;
        }

        public void OnPedHasBeenSearched(int networkId, bool illegalItems)
        {
            if (Ped.NetworkId == networkId)
            {
                HasBeenSearched = true;
                IsCarryingIllegalItems = illegalItems;
                CanBeArrested = illegalItems;
            }
        }

        public void OnHandcuffs(int networkId, bool state)
        {
            if (Ped.NetworkId == networkId)
            {
                IsHandcuffed = state;
                IsArrested = state;
            }
        }

        public void OnHasLostId(int networkId)
        {
            if (HasProvidedId) return;

            if (Ped.NetworkId == networkId)
            {
                if (Client.Random.Next(80, 100) >= 95)
                {
                    HasLostId = true;
                    IsAllowedToBeSearched = true;
                }
            }
        }

        public void OnIdRan(int networkId)
        {
            if (Ped.NetworkId == networkId)
            {
                HasIdentifcationBeenRan = true;
            }
        }

        private void OnPedHasBeenReleased(int networkId)
        {
            if (this.NetworkId == networkId)
            {
                this.Ped.SetConfigFlag(292, false);
                this.Ped.SetConfigFlag(301, false);

                if (this.Ped.IsInVehicle() && this.Ped.CurrentVehicle == Client.CurrentVehicle)
                {
                    
                    this.Ped.Task.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                }

                this.Ped.Task.ClearAll();
                this.Ped.MarkAsNoLongerNeeded();
                this.Ped.IsPersistent = false;
                this.Ped.LeaveGroup();

                API.TaskSetBlockingOfNonTemporaryEvents(this.Ped.Handle, false);

                DecorSetBool(this.Ped.Handle, Client.NPC_WAS_RELEASED, true);

                client.DeregisterTickHandler(OnMenuTask);
                client.DeregisterTickHandler(OnShowHelpTextTask);

                if (Classes.PlayerClient.ClientInformation.IsDeveloper())
                    client.DeregisterTickHandler(OnShowDeveloperOverlayTask);
            }
        }

        public event InteractivePed.OnAttackingTargetEvent AttackTarget;
        public delegate void OnAttackingTargetEvent(Ped target);

        private async Task OnShowDeveloperOverlayTask()
        {
            await Task.FromResult(0);

            if (!Client.DeveloperNpcUiEnabled)
            {
                await BaseScript.Delay(1000);
                return;
            }

            if (this.Position.Distance(Game.PlayerPed.Position) >= 6) return;

            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

            keyValuePairs.Add("Name", this.Name);
            keyValuePairs.Add("DoB", this.DateOfBirth);
            keyValuePairs.Add("Health", $"{this.Health} / {this.MaxHealth}");
            keyValuePairs.Add("-", "");
            keyValuePairs.Add("_ChanceOfFlee", $"{this._chanceOfFlee}");
            keyValuePairs.Add("_ChanceOfShootAndFlee", $"{this._chanceOfShootAndFlee}");
            keyValuePairs.Add("--", "");
            keyValuePairs.Add("IsArrested", $"{this.IsArrested}");
            keyValuePairs.Add("IsHandcuffed", $"{this.IsHandcuffed}");
            keyValuePairs.Add("HasProvidedId", $"{this.HasProvidedId}");
            keyValuePairs.Add("HasLostId", $"{this.HasLostId}");
            keyValuePairs.Add("HasBeenSearched", $"{this.HasBeenSearched}");
            keyValuePairs.Add("---", "");
            keyValuePairs.Add("CanBeArrested", $"{this.CanBeArrested}");
            keyValuePairs.Add("IsAllowedToBeSearched", $"{this.IsAllowedToBeSearched}");
            keyValuePairs.Add("IsUsingCannabis", $"{this.IsUsingCannabis}");
            keyValuePairs.Add("IsUsingCocaine", $"{this.IsUsingCocaine}");
            keyValuePairs.Add("IsCarryingIllegalItems", $"{this.IsCarryingIllegalItems}");
            keyValuePairs.Add("IsUnderTheInfluence", $"{this.IsUnderTheInfluence}");
            keyValuePairs.Add("IsInGroup", $"{this.Ped.IsInGroup}");
            keyValuePairs.Add("----", "");
            keyValuePairs.Add("BloodAlcaholLimit", $"{this.BloodAlcaholLimit}");
            keyValuePairs.Add("Attitude", $"{this.Attitude}");
            keyValuePairs.Add("NumberOfCitations", $"{this.NumberOfCitations}");
            keyValuePairs.Add("Offence", $"{this.Offence}");
            keyValuePairs.Add("-----", "");
            keyValuePairs.Add("NPC_CURRENT_VEHICLE", $"{DecorGetInt(this.Ped.Handle, Client.NPC_CURRENT_VEHICLE)}");
            keyValuePairs.Add("Local Vehicle", $"{this.Vehicle.Handle}");

            Wrappers.Helpers.DrawData(this, keyValuePairs);
        }
    }
}
