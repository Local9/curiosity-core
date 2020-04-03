using CitizenFX.Core;

namespace Curiosity.Missions.Client.net.DataClasses.Mission
{
    class MissionPedData
    {
        public PedHash Model;
        public Vector3 SpawnPoint;
        public float SpawnHeading;
        public WeaponHash Weapon = WeaponHash.Unarmed;
        public Extensions.Alertness Alertness = Extensions.Alertness.Nuetral;
        public Extensions.Difficulty Difficulty = Extensions.Difficulty.BringItOn;
        public float VisionDistance = 35f;
        public bool IsHostage = false;

        public RelationshipGroup RelationShipGroup { get; internal set; }
    }
}
