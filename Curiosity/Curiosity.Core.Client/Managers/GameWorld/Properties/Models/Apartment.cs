using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Managers.GameWorld.Properties.Enums;

namespace Curiosity.Core.Client.Managers.GameWorld.Properties.Models
{
    internal class Apartment
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public Quaternion Enterance { get; set; }
        public Quaternion Exit { get; set; }
        public Quaternion Bed { get; set; }
        public Quaternion Wardrobe { get; set; }
        public BuildingCamera EnteranceCamera { get; set; }
        public BuildingCamera ExitCamera { get; set; }
        public Door Door { get; set; }
        public Quaternion DoorPosition { get; set; }
        // To be decided
        public string GarageFilePath { get; set; }
        public string IPL { get; set; }
        public BuildingCamera AptStyleCam { get; set; }
        public Quaternion GarageElevatorPosition { get; set; }
        public Quaternion GarageMenuPosition { get; set; }
        public eApartmentType ApartmentType { get; set; }

        public bool IsOwnedByPlayer { get; set; }

        private Vector3 _interiorCoords => Wardrobe.AsVector();
        private int _interiorId;

        public void SetInteriorActive()
        {
            _interiorId = GetInteriorAtCoords(_interiorCoords.X, _interiorCoords.Y, _interiorCoords.Z);
            PinInteriorInMemory(_interiorId);
            // SetInteriorActive();
            DisableInterior(_interiorId, false);

            if (ApartmentType == eApartmentType.IPL || ApartmentType == eApartmentType.IPLStyle)
                RequestIpl(IPL);
        }

        public void SetInteriorUnactive()
        {
            _interiorId = GetInteriorAtCoords(_interiorCoords.X, _interiorCoords.Y, _interiorCoords.Z);
            DisableInterior(_interiorId, true);

            if (ApartmentType == eApartmentType.IPL || ApartmentType == eApartmentType.IPLStyle)
                RemoveIpl(IPL);
        }

        public async Task PlayEnteranceCutscene()
        {
            Door.Unlock();
            World.RenderingCamera = World.CreateCamera(EnteranceCamera.Position, EnteranceCamera.Rotation, EnteranceCamera.FieldOfView);
            Game.PlayerPed.Position = DoorPosition.AsVector();
            Game.PlayerPed.Heading = DoorPosition.W;
            Game.PlayerPed.Task.GoTo(Enterance.AsVector(), true, 6000);
            await BaseScript.Delay(6000);
            Door.Lock();
            World.DestroyAllCameras();
            World.RenderingCamera = null;
        }

        public async Task PlayExitCutscene()
        {
            Door.Unlock();
            World.RenderingCamera = World.CreateCamera(EnteranceCamera.Position, EnteranceCamera.Rotation, EnteranceCamera.FieldOfView);
            Game.PlayerPed.Position = Enterance.AsVector();
            Game.PlayerPed.Heading = DoorPosition.W - 180f;
            Game.PlayerPed.Task.GoTo(DoorPosition.AsVector(), true, 3500);
            await BaseScript.Delay(3500);
            Door.Lock();
        }

        internal void SetAsGarage()
        {
            Bed = Quaternion.Zero;
            DoorPosition = Quaternion.Zero;
            Enterance = Quaternion.Zero;
            Exit = Quaternion.Zero;
            Wardrobe = Quaternion.Zero;
            IPL = String.Empty;
            AptStyleCam = null;
            EnteranceCamera = null;
            ExitCamera = null;
            GarageElevatorPosition = Quaternion.Zero;
            GarageMenuPosition = Quaternion.Zero;
            ApartmentType = eApartmentType.Other;
            Door = null;
        }
    }
}
