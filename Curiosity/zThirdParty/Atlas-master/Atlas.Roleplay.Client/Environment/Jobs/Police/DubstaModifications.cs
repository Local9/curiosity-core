using Atlas.Roleplay.Client.Vehicles;
using CitizenFX.Core;

namespace Atlas.Roleplay.Client.Environment.Jobs.Police
{
    public class DubstaModifications : VehicleModifications
    {
        public DubstaModifications()
        {
            PrimaryColor = (int)VehicleColor.MatteBlack;
            SecondaryColor = (int)VehicleColor.MatteBlack;
            PearlescentColor = (int)VehicleColor.MatteBlack;
            WheelType = (int)VehicleWheelType.Offroad;
            WheelColor = (int)VehicleColor.MatteBlack;
            FrontWheels = 1;
            WindowTint = (int)VehicleWindowTint.PureBlack;
            Xenon = true;
            Roof = 3;
            Fender = 1;
            Spoiler = 1;
            FrontBumper = 6;
            RearBumper = 2;
            Grille = 2;
            SideSkirt = 2;
        }
    }
}