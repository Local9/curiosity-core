using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Models;

namespace Curiosity.Core.Server.Environment.Entities
{
    public class CuriosityEntity
    {
        private EventPed CitizenPed => API.DoesEntityExist(NetId) ? (EventPed)Entity.FromHandle(NetId) : null;
        public int NetId { get; set; }
        public int Model => API.GetEntityModel(NetId);

        public EventPosition Position
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
                API.SetEntityHeading(NetId, value.H);
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