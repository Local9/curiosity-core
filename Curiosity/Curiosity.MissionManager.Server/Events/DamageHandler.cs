using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.MissionManager.Server.Events
{
    public class DamageHandler : BaseScript
    {
        public DamageHandler()
        {
            EventHandlers["s:mm:damage"] += new Action<int, float, float, float, float, float, bool, int>(OnApplyDamage);
        }

        private void OnApplyDamage(int networkId, float x, float y, float z, float force, float radius, bool fromEntity, int numberOfHits)
        {
            BaseScript.TriggerClientEvent("c:mm:damage", networkId, x, y, z, force, radius, fromEntity, numberOfHits);
        }
    }
}
