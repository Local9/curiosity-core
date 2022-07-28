using Curiosity.Core.Client.Extensions;
using Curiosity.Core.Client.Managers.GameWorld.Properties.Enums;

namespace Curiosity.Core.Client.Managers.GameWorld.Properties.Models
{
    internal class Building
    {
        private const BlipColor BLIP_COLOR_BLACK = (BlipColor)40;
        private const BlipColor BLIP_COLOR_WHITE = (BlipColor)4;

        public string Name { get; set; }
        public Quaternion Enterance { get; set; }
        public Quaternion Exit { get; set; }
        public Quaternion Lobby { get; set; }
        public BuildingCamera Camera { get; set; }
        public BuildingCamera EnteranceCamera1 { get; set; }
        public BuildingCamera EnteranceCamera2 { get; set; }
        public BuildingCamera EnteranceCamera3 { get; set; }
        public BuildingCamera EnteranceCamera4 { get; set; }
        public eBuildingType BuildingType { get; set; }
        public List<Apartment> Apartments { get; set; }
        public int ExteriorIndex { get; set; }
        public SaleSign SaleSign { get; set; }
        public eFrontDoor FrontDoor { get; set; }
        public Door Door1 { get; set; }
        public Door Door2 { get; set; }
        public Door Door3 { get; set; }
        public Blip BuildingBlip { get; set; }
        public Blip SaleSignBlip { get; set; }
        public Garage Garage { get;  set; }

        public void CreateBuilding()
        {
            SetupBlip();
            if (SaleSign is not null) SaleSign.CreateForSaleSign();
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
    }
}
