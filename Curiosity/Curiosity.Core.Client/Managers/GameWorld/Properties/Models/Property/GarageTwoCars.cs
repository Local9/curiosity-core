namespace Curiosity.Core.Client.Managers.GameWorld.Properties.Models.Property
{
    internal class GarageTwoCars
    {
        public Vector3 Interior = new Vector3(193.9493f, -1004.425f, -99.99999f);
        public Vector3 Elevator = new Vector3(207.1506f, -998.9948f, -99.9999f);
        public Quaternion ElevatorInside = new Quaternion(209.4215f, -999.0895f, -100.0f, 90.90394f);
        public Quaternion SpawnInPos = new Quaternion(198.3316f, -1007.227f, -99.99992f, 21.38214f);
        public Vector3 GarageDoorL = new Vector3(202.2906f, -1007.7249f, -99.9999f);
        public Vector3 GarageDoorR = new Vector3(194.4465f, -1007.7326f, -99.9999f);
        public Vector3 MenuActivator = new Vector3(204.1768f, -995.3179f, -99.9999f);
        public Quaternion Veh0Pos = new Quaternion(197.5f, -1004.425f, -99.99999f, -4.035995f);
        public Quaternion Veh1Pos = new Quaternion(201.06f, -1004.425f, -99.99999f, -4.035995f);
        public Quaternion Veh2Pos = new Quaternion(204.62f, -1004.425f, -99.99999f, -4.035995f);
        public Quaternion Veh3Pos = new Quaternion(192.9262f, -996.3292f, -99.99999f, 146.2832f);
        public Quaternion Veh4Pos = new Quaternion(197.5f, -996.3292f, -99.99999f, 146.2832f);
        public Quaternion Veh5Pos = new Quaternion(203.9257f, -999.1467f, -99.99999f, 146.2832f);
        public BuildingCamera MenuCam = new BuildingCamera(new Vector3(205.5775f, -1006.326f, -97.99998f), new Vector3(-13.71649f, 0f, 48.66722f), 50.0f);

        public Vehicle Vehicle0;
        public Vehicle Vehicle1;
    }
}
