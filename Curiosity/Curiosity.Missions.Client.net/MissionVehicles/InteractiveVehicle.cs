using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Global.Shared.net.Data;
using Curiosity.Missions.Client.net.DataClasses;
using Curiosity.Missions.Client.net.Extensions;
using Curiosity.Missions.Client.net.Wrappers;
using Curiosity.Shared.Client.net;
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

        public bool CaughtSpeeding
        {
            get
            {
                return GetBoolean(Client.DECOR_VEHICLE_SPEEDING);
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

            NetworkRequestControlOfEntity(Vehicle.Handle);
            SetNetworkIdCanMigrate(Vehicle.NetworkId, true);
            NetworkRegisterEntityAsNetworked(Vehicle.NetworkId);
            SetNetworkIdExistsOnAllMachines(Vehicle.NetworkId, true);

            if (!IsEntityAMissionEntity(Vehicle.Handle))
                SetEntityAsMissionEntity(Vehicle.Handle, true, true);

            this.InteractivePed = Scripts.PedCreators.InteractivePedCreator.Ped(this.Vehicle.Driver);
            this.InteractivePed.Set(Client.DECOR_NPC_VEHICLE_HANDLE, this.Vehicle.Handle);

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

            if (chanceOfStolen == 20)
            {
                _vehicleStolen = true;
                InteractivePed.Set(Client.DECOR_VEHICLE_STOLEN, true);
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
                InteractivePed.Set(Client.DECOR_INTERACTION_LOST_ID, true);
            }
            else if (chanceOfRegistered == 10)
            {
                _vehicleRegistered = false;
            }
            else if (chanceOfInsured == 6)
            {
                _vehicleInsured = false;
            }

            _canSearchVehicle = false;
            int searchChance = Client.Random.Next(100);
            if (searchChance >= 90)
            {
                _canSearchVehicle = true;
                InteractivePed.Set(Client.DECOR_INTERACTION_LOST_ID, true);
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

            Set(Client.DECOR_TRAFFIC_STOP_VEHICLE_HANDLE, Handle);
            InteractivePed.Set(Client.DECOR_VEHICLE_SPEEDING, this.CaughtSpeeding);

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

            Decorators.Set(this.Vehicle.Handle, Client.DECOR_VEHICLE_MISSION, true);
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
            if (!NetworkHasControlOfEntity(Handle))
            {
                while (!API.NetworkRequestControlOfEntity(Handle))
                {
                    await BaseScript.Delay(0);
                }
            }

            if (Game.IsControlPressed(0, Control.Duck) && Game.PlayerPed.IsInVehicle() && _vehicleStopped && !_vehicleReleased)
            {
                if (!_vehicleStopped) return;

                _vehicleStopped = false;

                Vector3 outPos = new Vector3();
                Screen.ShowNotification("The NPC will find a suitable place to park and then stop, please wait.");
                if (GetNthClosestVehicleNode(InteractivePed.Position.X, InteractivePed.Position.Y, InteractivePed.Position.Z, 3, ref outPos, 0, 0, 0))
                {
                    ClearPedTasks(InteractivePed.Handle);
                    TaskVehiclePark(InteractivePed.Handle, Handle, outPos.X, outPos.Y, outPos.Z, InteractivePed.Heading, 3, 60f, true);
                    while (Vehicle.Position.DistanceToSquared2D(outPos) > 3f)
                    {
                        await BaseScript.Delay(0);
                    }
                    SetVehicleHalt(Handle, 3f, 0, false);
                    ClearPedTasks(InteractivePed.Handle);
                    Screen.ShowNotification("The NPC has stopped moving.");
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
                BaseScript.TriggerEvent("curiosity:interaction:vehicle:released", this.Vehicle.NetworkId);

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

        private async void TaskStopVehicle()
        {
            Vector3 outPos = new Vector3();
            if (GetNthClosestVehicleNode(InteractivePed.Position.X, InteractivePed.Position.Y, InteractivePed.Position.Z, 3, ref outPos, 0, 0, 0))
            {
                Vector3 roadside = Vector3.Zero;
                API.GetRoadSidePointWithHeading(outPos.X, outPos.Y, outPos.Z, Game.PlayerPed.Heading, ref roadside);

                if (!roadside.IsZero)
                    outPos = roadside;

                ClearPedTasks(InteractivePed.Handle);
                TaskVehiclePark(InteractivePed.Handle, Handle, outPos.X, outPos.Y, outPos.Z, InteractivePed.Heading, 3, 60f, true);
                long gameTimer = API.GetGameTimer();
                while (Vehicle.Position.DistanceToSquared2D(outPos) > 3f)
                {
                    if ((API.GetGameTimer() - gameTimer) > 30000)
                        break;

                    await BaseScript.Delay(0);
                }
                SetVehicleHalt(Handle, 3f, 0, false);
                ClearPedTasks(InteractivePed.Handle);
            }

            TaskSetBlockingOfNonTemporaryEvents(this.InteractivePed.Handle, true);
            _vehicleStopped = true;
            Set(Client.DECOR_VEHICLE_HAS_BEEN_TRAFFIC_STOPPED, true);
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

            InteractivePed.Set(Client.DECOR_INTERACTION_WANTED, true);
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

                InteractivePed.Set(Client.DECOR_NPC_RAN_FROM_POLICE, true);
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

                this.Vehicle.Driver.Task.WanderAround(this.Vehicle.Position, 1000f);

                if (Vehicle.AttachedBlip != null)
                {
                    if (Vehicle.AttachedBlip.Exists())
                        Vehicle.AttachedBlip.Delete();
                }

                _vehicleStopped = false;
                _vehicleReleased = true;

                client.DeregisterTickHandler(OnShowHelpTextTask);
                client.DeregisterTickHandler(OnShowDeveloperOverlayTask);

                BaseScript.TriggerEvent("curiosity:interaction:vehicle:released", Vehicle.NetworkId);
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

            BaseScript.TriggerEvent("curiosity:interaction:leaveAllGroups", handle);
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
                keyValuePairs.Add("Vehicle traffic stop", $"{GetBoolean(Client.DECOR_VEHICLE_HAS_BEEN_TRAFFIC_STOPPED)}");
                keyValuePairs.Add("Vehicle Ignored", $"{GetBoolean(Client.DECOR_VEHICLE_IGNORE)}");
                keyValuePairs.Add("Vehicle caught speeding", $"{GetBoolean(Client.DECOR_VEHICLE_SPEEDING)}");

                Wrappers.Helpers.DrawData(this, keyValuePairs);
            }
            else
            {
                await Client.Delay(1000);
            }
        }

        public void Set(string property, object value)
        {
            if (value is int i)
            {
                if (!API.DecorExistOn(Handle, property))
                {
                    API.DecorRegister(property, 3);
                }

                API.DecorSetInt(Handle, property, i);
            }
            else if (value is float f)
            {
                if (!API.DecorExistOn(Handle, property))
                {
                    API.DecorRegister(property, 1);
                }

                API.DecorSetFloat(Handle, property, f);
            }
            else if (value is bool b)
            {
                if (!API.DecorExistOn(Handle, property))
                {
                    API.DecorRegister(property, 2);
                }

                API.DecorSetBool(Handle, property, b);
            }
            else
            {
                Log.Info("[Decor] Could not set decor object due to it not being a supported type.");
            }
        }

        public int GetInteger(string property)
        {
            return API.DecorExistOn(Handle, property) ? API.DecorGetInt(Handle, property) : 0;
        }

        public float GetFloat(string property)
        {
            return API.DecorExistOn(Handle, property) ? API.DecorGetFloat(Handle, property) : 0f;
        }

        public bool GetBoolean(string property)
        {
            return API.DecorExistOn(Handle, property) && API.DecorGetBool(Handle, property);
        }
    }
}
