using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Managers.GameWorld.Properties.Enums;

namespace Curiosity.Core.Client.Managers.GameWorld.Properties.Models
{
    internal class Building
    {
        private const BlipColor BLIP_COLOR_BLACK = (BlipColor)40;
        private const BlipColor BLIP_COLOR_WHITE = (BlipColor)4;
        private bool _hideHud;

        public string Name { get; set; }
        public Quaternion Enterance { get; set; }
        public Quaternion Exit { get; set; }
        public Quaternion Lobby { get; set; }
        public BuildingCamera Camera { get; set; }
        public BuildingCamera EnteranceCamera1 { get; set; }
        public BuildingCamera EnteranceCamera2 { get; set; }
        public BuildingCamera GarageCamera1 { get; set; }
        public BuildingCamera GarageCamera2 { get; set; }
        public eBuildingType BuildingType { get; set; }
        public List<Apartment> Apartments { get; set; } = new();
        public int ExteriorIndex { get; set; }
        public SaleSign SaleSign { get; set; }
        public eFrontDoor FrontDoor { get; set; }
        public Door Door1 { get; set; }
        public Door Door2 { get; set; }
        public Door Door3 { get; set; }
        public Blip BuildingBlip { get; set; }
        public Blip SaleSignBlip { get; set; }
        public Garage Garage { get; set; }

        public bool BuildingSetup { get; set; }

        public void CreateBuilding()
        {
            SetupBlip();
            SaleSign.CreateForSaleSign();
        }

        void SetupBlip()
        {
            BuildingBlip = World.CreateBlip(Enterance.AsVector());
            BuildingBlip.IsShortRange = true;
            SetBlipCategory(BuildingBlip.Handle, 10); // 10 - Property / 11 = Owned Property

            // Need to know what ones the player owns?
            // Local KVP Store?

            switch(BuildingType)
            {
                case eBuildingType.Apartment:
                    BuildingBlip.Sprite = BlipSprite.SafehouseForSale;
                    BuildingBlip.Name = Game.GetGXTEntry("MP_PROP_SALE1");
                    break;
            }

            // BuildingBlip.Color = BLIP_COLOR_BLACK;
        }

        public void ToggleDoors(bool unlock = false)
        {
            if (unlock)
            {
                switch (FrontDoor)
                {
                    case eFrontDoor.DoubleDoors:
                        Door1.Unlock();
                        Door2.Unlock();
                        break;
                    case eFrontDoor.StandardDoor:
                        Door1.Unlock();
                        break;
                }
                return;
            }
            switch (FrontDoor)
            {
                case eFrontDoor.DoubleDoors:
                    Door1.Lock();
                    Door2.Lock();
                    break;
                case eFrontDoor.StandardDoor:
                    Door1.Lock();
                    break;
            }
        }

        public async Task PlayEnterApartmentCamera(int duration, bool easePosition, bool easeRotation, CameraShake cameraShake, float cameraShakeAmplitude)
        {
            _hideHud = true;
            ToggleDoors(true);
            Game.PlayerPed.Task.GoTo(Lobby.AsVector(), true, 7000);
            Camera scriptCamera = World.CreateCamera(EnteranceCamera1.Position, EnteranceCamera1.Rotation, EnteranceCamera1.FieldOfView);
            Camera interpCamera = World.CreateCamera(EnteranceCamera2.Position, EnteranceCamera2.Rotation, EnteranceCamera2.FieldOfView);
            World.RenderingCamera = scriptCamera;
            scriptCamera.InterpTo(interpCamera, duration, easePosition, easeRotation);
            World.RenderingCamera = interpCamera;
            interpCamera.Shake(cameraShake, cameraShakeAmplitude);
            await BaseScript.Delay(duration);
            ToggleDoors(false);
            _hideHud = false;
        }

        public async Task PlayExitApartmentCamera(int duration, bool easePosition, bool easeRotation, CameraShake cameraShake, float cameraShakeAmplitude)
        {
            _hideHud = true;
            Game.PlayerPed.Position = Lobby.AsVector();
            Game.PlayerPed.Heading = Exit.W;
            ToggleDoors(true);
            Game.PlayerPed.Task.GoTo(Exit.AsVector(), true, 7000);
            Camera scriptCamera = World.CreateCamera(EnteranceCamera2.Position, EnteranceCamera2.Rotation, EnteranceCamera2.FieldOfView);
            Camera interpCamera = World.CreateCamera(EnteranceCamera1.Position, EnteranceCamera1.Rotation, EnteranceCamera1.FieldOfView);
            World.RenderingCamera = scriptCamera;
            scriptCamera.InterpTo(interpCamera, duration, easePosition, easeRotation);
            World.RenderingCamera = interpCamera;
            interpCamera.Shake(cameraShake, cameraShakeAmplitude);
            await BaseScript.Delay(duration);
            World.DestroyAllCameras();
            World.RenderingCamera = null;
            ToggleDoors(false);
            _hideHud = false;
        }
    }
}
