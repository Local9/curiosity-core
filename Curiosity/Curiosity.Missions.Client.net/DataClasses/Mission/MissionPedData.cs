using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;

namespace Curiosity.Missions.Client.net.DataClasses.Mission
{
    class MissionPedData
    {
        public PedHash Model;
        public Vector3 SpawnPoint;
        public float SpawnHeading;
        public WeaponHash Weapon;
        public Extensions.Alertness Alertness;
        public Extensions.Difficulty Difficulty;
        public float VisionDistance;
        public bool IsHostage;
    }
}
