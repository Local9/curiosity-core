using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Environment.Entities.Models;
using Curiosity.Core.Client.Environment.Entities.Modules;
using Curiosity.Core.Client.Managers;
using Curiosity.Systems.Library.Models;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Environment.Entities
{
    public class CuriosityEntity
    {
        private Ped CitizenPed => Cache.PlayerPed;
        public int Id { get { return Cache.PlayerPed.Handle; } }
        public EntityModuleRegistry Modules { get; set; } = new EntityModuleRegistry();
        public int Model => API.GetEntityModel(Id);
        public AnimationQueue AnimationQueue { get; set; }

        public bool Important
        {
            get => API.IsEntityAMissionEntity(Id);
            set => API.SetEntityAsMissionEntity(Id, value, value);
        }

        public bool Movable
        {
            get => !Entity.FromHandle(Id).IsPositionFrozen;
            set => API.FreezeEntityPosition(Id, !value);
        }

        public Position Position
        {
            get
            {
                var position = API.GetEntityCoords(Id, false);
                var heading = API.GetEntityHeading(Id);

                return new Position(position.X, position.Y, position.Z, heading);
            }
            set
            {
                API.SetEntityCoords(Id, value.X, value.Y, value.Z, false, false, false, false);
                API.SetEntityHeading(Id, value.Heading);
            }
        }

        public bool Collision
        {
            set => API.SetEntityCollision(Id, value, true);
        }

        public bool Physics
        {
            set
            {
                if (value) API.SetEntityCollision(Id, true, false);
                else API.ActivatePhysics(Id);
            }
            get => API.DoesEntityHavePhysics(Id);
        }

        public bool Gravity
        {
            set => API.SetPedGravity(Id, value);
        }

        public bool Dynamic
        {
            set => API.SetEntityDynamic(Id, value);
        }

        public int Health
        {
            get => API.GetEntityHealth(Id);
            set => API.SetEntityHealth(Id, value);
        }

        public bool IsDead => API.IsEntityDead(Id);
        public Vehicle Vehicle => CitizenPed?.CurrentVehicle;
        public Tasks Task => CitizenPed?.Task;

        public void SetDefaultStyle()
        {
            API.SetPedDefaultComponentVariation(Id);
        }

        public void ToggleGodMode()
        {
            Cache.PlayerPed.IsInvincible = !Cache.PlayerPed.IsInvincible;
        }

        public WeaponCollection Weapons => CitizenPed.Weapons;

        public async Task Teleport(Position position)
        {
            await SafeTeleport.Teleport(Id, position);
        }

        public void InstallModule(string key, EntityModule module)
        {
            Modules.Add(key, module);

            module.CallBeginOperation(this, Id);
        }

        public CuriosityEntity(int id)
        {
            AnimationQueue = new AnimationQueue(Cache.PlayerPed.Handle);

            GameEventManager.OnPlayerKillPed += GameEventManager_OnPlayerKillPed;
            GameEventManager.OnPedKillPlayer += GameEventManager_OnPedKillPlayer;
            GameEventManager.OnPlayerKillPlayer += GameEventManager_OnPlayerKillPlayer;

            GameEventManager.OnDeath += GameEventManager_OnDeath;
        }

        private void GameEventManager_OnPlayerKillPlayer(Player attacker, Player victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag)
        {
            // message server
            // keep a server side count
            // nuke player if the count is too high
        }

        private void GameEventManager_OnPedKillPlayer(Ped attacker, Player victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag)
        {
            // ha ha
            // can laugh at these
        }

        private void GameEventManager_OnPlayerKillPed(Player attacker, Ped victim, bool isMeleeDamage, uint weaponInfoHash, int damageTypeFlag)
        {
            // monitor and police
            // Just beat up a player if required, ped justice
        }

        private void GameEventManager_OnDeath(Entity attacker, bool isMeleeDamage, uint weaponHashInfo, int damageTypeFlag)
        {
            // ha ha
            // suicide? or something else?
            Logger.Debug($"GameEventManager_OnDeath -> CuriosityEntity");
        }
    }
}