using CitizenFX.Core;
using Curiosity.Missions.Client.net.Wrappers;
using Curiosity.Missions.Client.net.DataClasses;
using System;
using System.Threading.Tasks;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
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

namespace Curiosity.Missions.Client.net.MissionVehicles
{
    abstract class InteractiveVehicle : Entity, IEquatable<Vehicle>
    {
        private static Client client = Client.GetInstance();

        public readonly Vehicle Vehicle;
        public MissionPeds.InteractivePed InteractivePed;
        private EntityEventWrapper _eventWrapper;

        private string helpText = string.Empty;

        private bool _vehicleStolen, _vehicleInsured, _vehicleRegistered, _canSearchVehicle, _vehicleStopped;
        public bool IsVehicleStolen
        {
            get
            {
                return _vehicleStolen;
            }
        }

        public bool IsVehicleRegistered
        {
            get
            {
                return _vehicleRegistered;
            }
        }

        public bool IsVehicleInsured
        {
            get
            {
                return _vehicleInsured;
            }
        }

        public bool CanSearchVehicle
        {
            get
            {
                return _canSearchVehicle;
            }
        }

        string _firstname, _surname, _dateOfBirth;
        public string Name
        {
            get
            {
                if (_vehicleStolen)
                    return $"{_firstname} {_surname}";
                
                return this.InteractivePed.Name;
            }
        }
        public string DateOfBirth
        {
            get
            {
                if (_vehicleStolen)
                    return _dateOfBirth;

                return this.InteractivePed.DateOfBirth;
            }
        }

        // Vehicle Flee States
        int _chanceOfFlee, _chanceOfShootAndFlee;

        public InteractiveVehicle(int handle) : base(handle)
        {
            this.Vehicle = new Vehicle(handle);

            this.InteractivePed = Scripts.PedCreators.InteractivePedCreator.Ped(this.Vehicle.Driver);

            _chanceOfFlee = 0;
            _chanceOfShootAndFlee = 0;

            Vector3 pos = this.Vehicle.Position;
            string zoneKey = API.GetNameOfZone(pos.X, pos.Y, pos.Z);
            string currentZoneName = Zones.Locations[zoneKey];
            if (currentZoneName == "Davis" || currentZoneName == "Rancho" || currentZoneName == "Strawberry")
            {
                _chanceOfFlee = Client.Random.Next(23, 30);
                _chanceOfShootAndFlee = Client.Random.Next(1, 5);
            }
            else if (this.Vehicle.Model.IsBike)
            {
                _chanceOfFlee = Client.Random.Next(23, 30);
                _chanceOfShootAndFlee = 0;
            }
            else
            {
                _chanceOfFlee = Client.Random.Next(30);
                _chanceOfShootAndFlee = Client.Random.Next(5);
            }

            int chanceOfStolen = Client.Random.Next(25);
            int chanceOfRegistered = Client.Random.Next(13);
            int chanceOfInsured = Client.Random.Next(9);

            _vehicleRegistered = true; // we presume all drivers are registered and insured
            _vehicleInsured = true;

            if (chanceOfStolen == 24)
            {
                _vehicleStolen = true;
                // generate new registration name
                _firstname = string.Empty;
                _surname = string.Empty;

                if (Client.Random.Next(2) == 1)
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

                _chanceOfFlee = Client.Random.Next(25, 30);
                _chanceOfShootAndFlee = Client.Random.Next(1, 5);
                // Add event to interactive ped to update LostID Flag
                Client.TriggerEvent("curiosity:interaction:hasLostId", this.InteractivePed.Ped.NetworkId);
            }
            else if (chanceOfRegistered == 12)
            {
                _vehicleRegistered = false;
            }
            else if (chanceOfInsured == 8)
            {
                _vehicleInsured = false;
            }

            _canSearchVehicle = false;
            int searchChance = Client.Random.Next(100);
            if (searchChance >= 90)
            {
                _canSearchVehicle = true;
                Client.TriggerEvent("curiosity:interaction:hasLostId", this.InteractivePed.Ped.NetworkId);
            }

            Create();
        }

        public async void Create()
        {
            this._eventWrapper = new EntityEventWrapper(this.Vehicle);
            this._eventWrapper.Died += new EntityEventWrapper.OnDeathEvent(this.OnDied);
            this._eventWrapper.Updated += new EntityEventWrapper.OnWrapperUpdateEvent(this.Update);
            this._eventWrapper.Aborted += new EntityEventWrapper.OnWrapperAbortedEvent(this.Abort);

            client.RegisterEventHandler("curiosity:interaction:released", new Action<int>(OnPedHasBeenReleased));

            client.RegisterTickHandler(OnShowHelpTextTask);
            if (Classes.PlayerClient.ClientInformation.IsDeveloper())
                client.RegisterTickHandler(OnShowDeveloperOverlayTask);

            // Should we do anything?
            if (_chanceOfFlee == 29)
            {
                TaskFlee();
            }
            else if (_chanceOfFlee == 28)
            {
                // SHOOT?
                if (_chanceOfShootAndFlee == 4)
                {
                    int timeAfterStop = Client.Random.Next(5, 30) * 1000;
                    int timeAfterStoot = Client.Random.Next(5, 30) * 1000;
                    TaskStopVehicle();
                    await Client.Delay(timeAfterStop);
                    TaskShootAtPlayer();
                    await Client.Delay(timeAfterStoot);
                    TaskFlee();
                }
                else
                {
                    bool isDriverGoingToFlee = true;
                    TaskStopVehicle();
                    while (isDriverGoingToFlee)
                    {
                        await Client.Delay(100);
                        if (!Game.PlayerPed.IsInVehicle() && Game.PlayerPed.Position.Distance(this.InteractivePed.Position) <= 3)
                        {
                            TaskFlee();
                            isDriverGoingToFlee = false;
                        }
                    }
                }
            }
            else
            {
                TaskStopVehicle();
            }
        }

        protected bool Equals(InteractiveVehicle other)
        {
            return (!base.Equals(other) ? false : object.Equals(this.Vehicle, other.Vehicle));
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
                flag = (this) != obj ? obj.GetType() != GetType() ? false : Equals((InteractiveVehicle)obj) : true;
            }
            return flag;
        }

        public bool Equals(Vehicle other)
        {
            return object.Equals(this.Vehicle, other);
        }

        public static implicit operator Vehicle(InteractiveVehicle v)
        {
            return v.Vehicle;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() * 397 ^ (this.Vehicle != null ? this.Vehicle.GetHashCode() : 0);
        }

        private async Task OnShowHelpTextTask()
        {
            await Task.FromResult(0);
            if (!string.IsNullOrEmpty(helpText))
                Screen.DisplayHelpTextThisFrame(helpText);
        }

        public void Update(EntityEventWrapper entityEventWrapper, Entity entity)
        {
            if (_vehicleStopped && this.Vehicle.IsEngineRunning)
            {
                this.Vehicle.IsEngineRunning = false;

                if (this.Vehicle.Speed <= 1f)
                    this.Vehicle.Windows.RollDownAllWindows();
            }

            if (Game.PlayerPed.Position.Distance(this.Vehicle.Position) >= 300f)
            {
                this.Delete();

                client.DeregisterTickHandler(OnShowHelpTextTask);

                if (Classes.PlayerClient.ClientInformation.IsDeveloper())
                    client.DeregisterTickHandler(OnShowDeveloperOverlayTask);
            }
        }

        private void OnDied(EntityEventWrapper sender, Entity entity)
        {
            Blip currentBlip = base.AttachedBlip;
            if (currentBlip != null)
            {
                currentBlip.Delete();
            }
        }

        public void Abort(EntityEventWrapper sender, Entity entity)
        {
            base.Delete();
            
            client.DeregisterTickHandler(OnShowHelpTextTask);

            if (Classes.PlayerClient.ClientInformation.IsDeveloper())
                client.DeregisterTickHandler(OnShowDeveloperOverlayTask);
        }

        private void TaskStopVehicle()
        {
            TaskSetBlockingOfNonTemporaryEvents(this.InteractivePed.Handle, true);
            _vehicleStolen = true;
        }

        private async void TaskShootAtPlayer()
        {
            _vehicleStopped = false;
            this.InteractivePed.Ped.SetConfigFlag(292, false);

            WeaponHash weaponHash = WeaponHash.Pistol;
            int chanceOfScale = Client.Random.Next(30);

            if (chanceOfScale >= 25)
            {
                weaponHash = WeaponHash.SawnOffShotgun;
            }

            if (chanceOfScale >= 28)
            {
                weaponHash = WeaponHash.AssaultRifle;
            }

            this.InteractivePed.Ped.Weapons.Give(weaponHash, 30, false, true);
            this.InteractivePed.Ped.Task.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
            await Client.Delay(1000);
            this.InteractivePed.Ped.Task.ShootAt(Game.PlayerPed, 10000000, FiringPattern.FullAuto);
        }

        private async void TaskFlee()
        {
            try
            {
                TaskSetBlockingOfNonTemporaryEvents(this.InteractivePed.Ped.Handle, false);
                this.InteractivePed.Ped.SetConfigFlag(292, false);

                _vehicleStopped = false;
                this.Vehicle.IsEngineRunning = true;
                SetVehicleCanBeUsedByFleeingPeds(this.Vehicle.Handle, true);

                if (this.Vehicle.Driver == null)
                    this.InteractivePed.Ped.Task.EnterVehicle(this.Vehicle, VehicleSeat.Driver, 20000, 5f);
                
                await Client.Delay(1000);

                int willRam = Client.Random.Next(5);

                if (willRam == 4)
                {
                    TaskVehicleTempAction(this.Vehicle.Driver.Handle, this.Vehicle.Handle, 28, 3000);
                }

                await Client.Delay(2000);

                TaskVehicleTempAction(this.Vehicle.Driver.Handle, this.Vehicle.Handle, 32, 30000);
                this.Vehicle.Driver.Task.FleeFrom(Game.PlayerPed);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TrafficStopVehicleFlee -> {ex}");
            }
        }

        private void OnPedHasBeenReleased(int networkId)
        {
            if (this.InteractivePed.NetworkId == networkId)
            {
                client.DeregisterTickHandler(OnShowHelpTextTask);

                if (Classes.PlayerClient.ClientInformation.IsDeveloper() && Client.DeveloperNpcUiEnabled)
                    client.DeregisterTickHandler(OnShowDeveloperOverlayTask);
            }
        }

        private async Task OnShowDeveloperOverlayTask()
        {
            await Task.FromResult(0);

            if (Client.DeveloperVehUiEnabled)
            {
                if (this.Position.Distance(Game.PlayerPed.Position) >= 6) return;

                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

                keyValuePairs.Add("Name", this.Name);
                keyValuePairs.Add("DoB", this.DateOfBirth);
                keyValuePairs.Add("Health", $"{this.Health} / {this.MaxHealth}");
                keyValuePairs.Add("-", "");
                keyValuePairs.Add("_ChanceOfFlee", $"{this._chanceOfFlee}");
                keyValuePairs.Add("_ChanceOfShootAndFlee", $"{this._chanceOfShootAndFlee}");
                keyValuePairs.Add("--", $"");
                keyValuePairs.Add("_vehicleStopped", $"{this._vehicleStopped}");
                keyValuePairs.Add("_vehicleStolen", $"{this._vehicleStolen}");
                keyValuePairs.Add("_vehicleInsured", $"{this._vehicleInsured}");
                keyValuePairs.Add("_vehicleRegistered", $"{this._vehicleRegistered}");
                keyValuePairs.Add("_canSearchVehicle", $"{this._canSearchVehicle}");

                Wrappers.Helpers.DrawData(this, keyValuePairs);
            }
            else
            {
                await Client.Delay(1000);
            }
        }
    }
}
