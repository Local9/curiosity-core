namespace Curiosity.Shared.Client.net.Enums
{ 
    namespace Helicopter
    {
        public enum CameraMode
        {
            None = 0,
            FLIR = 1,
            Nightvision = 2
        }
    }

    // Hobble uses same method as cuff, just different parameters, so flags won't make any (technical) sense anyway
    public enum CuffState
    {
        None = 0,
        Cuffed = 1,
        Hobbled = 2
    }

    namespace Patrol
    {
        public enum PatrolZone
        {
            City = 1,
            Country,
            Ocean,
            Highway,
            Rural
        }
    }
}
