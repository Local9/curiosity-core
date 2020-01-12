using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.System.Client.Environment.Entities.Models;
using Curiosity.System.Client.Environment.Entities.Modules;
using Curiosity.System.Library.Models;
using System.Threading.Tasks;

namespace Curiosity.System.Client.Environment.Entities
{
    public class CuriosityEntity
    {
        private Ped CitizenPed => API.DoesEntityExist(Id) && API.IsEntityAPed(Id) ? (Ped) Entity.FromHandle(Id) : null;
        public int Id { get; set; }
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

        public WeaponCollection Weapons => CitizenPed.Weapons;

        public async Task Teleport(Position position)
        {
            await SafeTeleport.Teleport(Id, position);
        }

        public CuriosityEntity(int id)
        {
            Id = id;
            AnimationQueue = new AnimationQueue(id);
        }

        public void InstallModule(string key, EntityModule module)
        {
            Modules.Add(key, module);

            module.CallBeginOperation(this, Id);
        }
    }
}