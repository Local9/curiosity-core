namespace Curiosity.Core.Client.Managers.GameWorld.Properties.Models.Property
{
    internal class GarageSixCars
    {
        public Vector3 Interior = new Vector3(173.1176f, -1003.27887f, -99f);
        public Vector3 Elevator = new Vector3(179.1001f, -1005.655f, -99.9999f);
        public Quaternion ElevatorInside = new Quaternion(179.0661f, -1002.717f, -99.99992f, 179.181f);
        public Quaternion SpawnInPos = new Quaternion(176.6249f, -1007.737f, -99.9999f, 32.54933f);
        public Vector3 GarageDoor = new Vector3(172.9447f, -1008.339f, -99.9999f);
        public Vector3 MenuActivator = new Vector3(178.9034f, -1007.407f, -99.99998f);
        public Quaternion Veh0Pos = new Quaternion(175.2132f, -1004.104f, -99.99999f, -178.4487f);
        public Quaternion Veh1Pos = new Quaternion(171.7141f, -1004.023f, -99.99999f, -178.4487f);
        public BuildingCamera MenuCam = new BuildingCamera(new Vector3(179.6135f, -1008.107f, -97.29996f), new Vector3(-24.14237f, 0f, 57.91766f), 50.0f);

        public Vehicle Vehicle0;
        public Vehicle Vehicle1;
        public Vehicle Vehicle2;
        public Vehicle Vehicle3;
        public Vehicle Vehicle4;
        public Vehicle Vehicle5;
    }
}
