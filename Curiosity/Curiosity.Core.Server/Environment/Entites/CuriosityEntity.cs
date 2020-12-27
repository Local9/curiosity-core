using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Models;

namespace Curiosity.Systems.Server.Environment.Entities
{
    public class CuriosityEntity
    {
        private Ped CitizenPed => API.DoesEntityExist(NetId) ? (Ped)Entity.FromHandle(NetId) : null;
        public int NetId { get; set; }
        public int Model => API.GetEntityModel(NetId);

        public Position Position
        {
            get
            {
                var position = API.GetEntityCoords(NetId);
                var heading = API.GetEntityHeading(NetId);

                return new Position(position.X, position.Y, position.Z, heading);
            }
            set
            {
                API.SetEntityCoords(NetId, value.X, value.Y, value.Z, false, false, false, false);
                API.SetEntityHeading(NetId, value.Heading);
            }
        }

        public int Health
        {
            get => API.GetEntityHealth(NetId);
        }


        public void SetDefaultStyle()
        {
            API.SetPedDefaultComponentVariation(NetId);
        }
    }
}