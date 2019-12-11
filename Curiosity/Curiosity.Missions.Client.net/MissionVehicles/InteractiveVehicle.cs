using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Missions.Client.net.DataClasses;
using Curiosity.Missions.Client.net.Wrappers;
using Curiosity.Shared.Client.net.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Missions.Client.net.MissionVehicles
{
    abstract class InteractiveVehicle : Entity, IEquatable<Vehicle>
    {
        private static Client client = Client.GetInstance();

        public readonly Vehicle Vehicle;
        public MissionPeds.InteractivePed InteractivePed;
        private EntityEventWrapper _eventWrapper;

        private string helpText = string.Empty;

        private bool _vehicleStolen, _vehicleInsured, _vehicleRegistered, _canSearchVehicle, _vehicleStopped, _vehicleReleased;
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
            if (Classes.PlayerClient.ClientInformation.IsDeveloper() && Client.DeveloperNpcUiEnabled)
            {
                Screen.ShowNotification($"~r~[~g~D~b~E~y~V~o~]~w~ Creating Interactive Vehicle");
            }

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

            _vehicleReleased = false;

            Create();
        }

        public async void Create()
        {
            this._eventWrapper = new EntityEventWrapper(this.Vehicle);
            this._eventWrapper.Died += new EntityEventWrapper.OnDeathEvent(this.OnDied);
            this._eventWrapper.Updated += new EntityEventWrapper.OnWrapperUpdateEvent(this.Update);
            this._eventWrapper.Aborted += new EntityEventWrapper.OnWrapperAbortedEvent(this.Abort);

            client.RegisterEventHandler("curiosity:interaction:released", new Action<int>(OnPedHasBeenReleased));
            client.RegisterEventHandler("curiosity:interaction:veh:flee", new Action<int>(OnFlee));

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

        public async void Update(EntityEventWrapper entityEventWrapper, Entity entity)
        {
            if (Game.IsControlPressed(0, Control.Duck) && Game.PlayerPed.IsInVehicle() && _vehicleStopped && !_vehicleReleased)
            {
                if (!_vehicleStopped) return;

                _vehicleStopped = false;

                Vector3 offset = new Vector3(2.5f, 40f, 0f);
                Vector3 sidewalk = World.GetSafeCoordForPed(this.Vehicle.GetOffsetPosition(offset));

                if (sidewalk == Vector3.Zero)
                {
                    sidewalk = this.Vehicle.GetOffsetPosition(offset);
                }

                this.InteractivePed.Ped.Task.DriveTo(this.Vehicle, sidewalk, 4f, 5f);

                Screen.ShowNotification("Vehicle moving to a new location");

                while (this.Vehicle.Position.Distance(sidewalk) >= 5f)
                {
                    await BaseScript.Delay(10);
                }

                _vehicleStopped = true;
            }


            if (_vehicleStopped && this.Vehicle.IsEngineRunning && !_vehicleReleased)
            {
                this.InteractivePed.Ped.SetConfigFlag(301, true);
                this.Vehicle.IsEngineRunning = false;

                if (this.Vehicle.Speed <= 1f)
                    this.Vehicle.Windows.RollDownAllWindows();
            }

            if (!_vehicleStopped && !_vehicleReleased)
            {
                this.InteractivePed.Ped.SetConfigFlag(301, false);
            }

            if (Game.PlayerPed.Position.Distance(this.Vehicle.Position) >= 300f)
            {
                Client.TriggerEvent("curiosity:interaction:released", this.Vehicle.Driver.Handle);

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
            _vehicleStopped = true;
            DecorSetBool(this.Vehicle.Handle, Client.VEHICLE_HAS_BEEN_TRAFFIC_STOPPED, true);
        }

        private async void TaskShootAtPlayer()
        {
            _vehicleStopped = false;
            this.InteractivePed.Ped.SetConfigFlag(301, false);
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
                this.InteractivePed.Ped.SetConfigFlag(301, false);

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

                if (this.Vehicle.AttachedBlip != null)
                    this.Vehicle.AttachedBlip.Color = BlipColor.Red;

                if (this.InteractivePed.AttachedBlip != null)
                    this.InteractivePed.AttachedBlip.Color = BlipColor.Red;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"TrafficStopVehicleFlee -> {ex}");
            }
        }

        private void OnPedHasBeenReleased(int handle)
        {
            if (this.Handle == handle)
            {
                if (this.Vehicle.AttachedBlip.Exists())
                    this.Vehicle.AttachedBlip.Delete();

                this.Vehicle.IsPositionFrozen = false;

                this.Vehicle.Driver.LeaveGroup();

                this.Vehicle.IsPersistent = false;
                this.Vehicle.Driver.Task.ClearAll();
                this.Vehicle.MarkAsNoLongerNeeded();

                this.Vehicle.Driver.SetConfigFlag(292, false);
                this.Vehicle.Driver.SetConfigFlag(301, false);

                if (Vehicle.AttachedBlip != null)
                {
                    if (Vehicle.AttachedBlip.Exists())
                        Vehicle.AttachedBlip.Delete();
                }

                _vehicleStopped = false;
                _vehicleReleased = true;

                client.DeregisterTickHandler(OnShowHelpTextTask);
                client.DeregisterTickHandler(OnShowDeveloperOverlayTask);
            }

            if (this.InteractivePed.Handle == handle)
            {
                client.DeregisterTickHandler(OnShowHelpTextTask);
                
                this.InteractivePed.Ped.LeaveGroup();

                this.InteractivePed.Ped.SetConfigFlag(292, false);
                this.InteractivePed.Ped.SetConfigFlag(301, false);
                this.InteractivePed.Ped.IsPersistent = false;
                this.InteractivePed.Ped.Task.ClearAll();
                this.InteractivePed.Ped.MarkAsNoLongerNeeded();
                this.InteractivePed.Ped.IsPositionFrozen = false;

                _vehicleStopped = false;
                _vehicleReleased = true;

                if (InteractivePed.AttachedBlip != null)
                {
                    if (InteractivePed.AttachedBlip.Exists())
                        InteractivePed.AttachedBlip.Delete();
                }

                client.DeregisterTickHandler(OnShowHelpTextTask);
                client.DeregisterTickHandler(OnShowDeveloperOverlayTask);
            }

            Client.TriggerEvent("curiosity:interaction:leaveAllGroups", handle);
        }

        void OnFlee(int handle)
        {
            if (InteractivePed.Handle != handle) return;
            _vehicleStopped = false;
        }

        private async Task OnShowDeveloperOverlayTask()
        {
            await Task.FromResult(0);

            if (Client.DeveloperVehUiEnabled)
            {
                if (this.Position.Distance(Game.PlayerPed.Position) >= 30) return;

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
                keyValuePairs.Add("---", $"");
                keyValuePairs.Add("VEHICLE_HAS_BEEN_STOPPED", $"{DecorGetBool(this.Vehicle.Handle, "curiosity::VehicleStopped")}");
                keyValuePairs.Add("IS_TRAFFIC_STOPPED_PED", $"{DecorGetBool(this.Vehicle.Handle, "curiosity::PedIsTrafficStopped")}");
                keyValuePairs.Add("NPC_VEHICLE_IGNORE", $"{DecorGetBool(this.Vehicle.Handle, "NPC_VEHICLE_IGNORE")}");

                Wrappers.Helpers.DrawData(this, keyValuePairs);
            }
            else
            {
                await Client.Delay(1000);
            }
        }
    }
}
