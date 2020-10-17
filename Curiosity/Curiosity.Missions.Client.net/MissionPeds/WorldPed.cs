using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Global.Shared.NPC;
using Curiosity.Missions.Client.Classes.PlayerClient;
using Curiosity.Missions.Client.Extensions;
using Curiosity.Missions.Client.Utils;
using Curiosity.Missions.Client.Wrappers;
using Curiosity.Shared.Client.net.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Missions.Client.MissionPeds
{
    abstract class WorldPed : Entity, IEquatable<Ped>
    {
        // RAGE ENGINE
        private EntityEventWrapper _eventWrapper;

        public readonly Ped Ped;
        public readonly Vehicle Vehicle;
        private PluginManager PluginInstance => PluginManager.Instance;

        public NpcProfile Profile;

        static WorldPed()
        {
        }

        protected WorldPed(int handle) : base(handle)
        {
            if (ClientInformation.IsDeveloper && PluginManager.DeveloperNpcUiEnabled)
            {
                Screen.ShowNotification($"~r~[~g~D~b~E~y~V~o~]~w~ Creating World Ped");
            }

            Ped = new Ped(handle);

            Wrappers.Helpers.RequestControlOfEnt(Ped);

            this._eventWrapper = new EntityEventWrapper(Ped);
            this._eventWrapper.Died += new EntityEventWrapper.OnDeathEvent(OnDied);
            this._eventWrapper.Updated += new EntityEventWrapper.OnWrapperUpdateEvent(Update);
            this._eventWrapper.Aborted += new EntityEventWrapper.OnWrapperAbortedEvent(Abort);

            if (Ped.IsInVehicle())
            {
                Vehicle = this.Ped.CurrentVehicle;
            }

            int netId = API.NetworkGetNetworkIdFromEntity(Ped.Handle);
            Decorators.Set(Ped.Handle, Decorators.PED_NETWORKID, netId);
            API.SetNetworkIdCanMigrate(netId, true);

            Decorators.Set(Ped.Handle, Decorators.PED_INTERACTIVE, true);
            Decorators.Set(Ped.Handle, Decorators.PED_MISSION, true);

            API.SetPedFleeAttributes(Ped.Handle, 0, false);
            API.SetBlockingOfNonTemporaryEvents(Ped.Handle, true);
            API.TaskSetBlockingOfNonTemporaryEvents(Ped.Handle, true);
            API.SetPedDiesInWater(Ped.Handle, false);
            API.SetPedDiesWhenInjured(Ped.Handle, false);
            API.SetPedDiesInstantlyInWater(Ped.Handle, true);

            Ped.SetCombatAttributes((CombatAttributes)17, false);
            Ped.SetCombatAttributes((CombatAttributes)46, false);
            Ped.SetCombatAttributes((CombatAttributes)5, false);

            Ped.Health = 200;
            Ped.IsPersistent = true;

            PluginInstance.RegisterTickHandler(OnArrestablePedTick);
        }

        private void Abort(EntityEventWrapper sender, Entity entity)
        {
            PluginInstance.DeregisterTickHandler(OnArrestablePedTick);

            base.Delete();
        }

        private void Update(EntityEventWrapper sender, Entity entity)
        {
            bool flag;
            if (this.Position.VDist(Game.PlayerPed.Position) <= 120f)
            {
                flag = false;
            }
            else
            {
                flag = (!base.IsOnScreen ? true : base.IsDead);
            }
            if (flag)
            {
                base.Delete();
            }
        }

        private async void OnDied(EntityEventWrapper sender, Entity entity)
        {
            PluginInstance.DeregisterTickHandler(OnArrestablePedTick);

            Blip currentBlip = base.AttachedBlip;
            if (currentBlip != null)
            {
                currentBlip.Delete();
            }

            await PluginManager.Delay(3000);
            API.NetworkFadeOutEntity(this.Handle, false, false);
            await PluginManager.Delay(1000);
            sender.Dispose();
        }

        public bool Equals(WorldPed other)
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
                flag = (this) != obj ? obj.GetType() != GetType() ? false : Equals((WorldPed)obj) : true;
            }
            return flag;
        }

        public bool Equals(Ped other)
        {
            return object.Equals(this.Ped, other);
        }

        public static implicit operator Ped(WorldPed v)
        {
            return v.Ped;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() * 397 ^ (this.Ped != null ? this.Ped.GetHashCode() : 0);
        }

        public event WorldPed.OnAttackingTargetEvent AttackTarget;
        public event WorldPed.OnGoingToTargetEvent GoToTarget;

        public delegate void OnAttackingTargetEvent(Ped target);
        public delegate void OnGoingToTargetEvent(Ped target);

        async Task OnArrestablePedTick()
        {
            if (Decorators.GetBoolean(Game.PlayerPed.Handle, "player::npc::debug"))
            {
                if (Position.Distance(Game.PlayerPed.Position) >= 6) return;

                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

                keyValuePairs.Add("Name", $"{Profile.FirstName} {Profile.LastName}");
                keyValuePairs.Add("DoB", Profile.DOB);
                keyValuePairs.Add("Health", $"{Ped.Health} / {Ped.MaxHealth}");
                keyValuePairs.Add("-", "");
                keyValuePairs.Add("_ChanceOfFlee", $"{Profile.RiskOfFlee}");
                keyValuePairs.Add("_ChanceOfShootAndFlee", $"{Profile.RiskOfShootAndFlee}");
                keyValuePairs.Add("--", "");
                keyValuePairs.Add("IsArrestable", $"{Profile.IsArrestable}");

                Wrappers.Helpers.DrawData(this, keyValuePairs);
            }
        }
    }
}
