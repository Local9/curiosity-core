using CitizenFX.Core;
using System.Collections.Concurrent;

namespace Curiosity.Global.Shared.Data
{
    class MissionFile
    {
        public string Name { get; set; }
        public Vector3 Location { get; set; }
        public string TriggerHandle { get; set; }
        public ConcurrentDictionary<string, Vector3> Triggers = new ConcurrentDictionary<string, Vector3>();

        public void AddTrigger(string triggerName, Vector3 vector)
        {
            this.Triggers.GetOrAdd(triggerName, vector);
        }

        //public List<(Vector3, float)> Enemies = new List<(Vector3, float)>();

        //public void AddEnemy(Vector3 vector, float heading)
        //{
        //    this.Enemies.Add((vector, heading));
        //}

        //public List<(Vector3, float)> Bosses = new List<(Vector3, float)>();

        //public void AddBoss(Vector3 vector, float heading)
        //{
        //    this.Bosses.Add((vector, heading));
        //}
    }
}
