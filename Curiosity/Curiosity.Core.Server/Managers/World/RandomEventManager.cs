using Curiosity.Core.Server.Environment;

namespace Curiosity.Core.Server.Managers.World
{
    public class RandomEventManager : Manager<RandomEventManager>
    {
        Dictionary<string, RandomEvent> _activeEvents = new();
        Dictionary<string, RandomEvent> _randomEvents = new();
        long _gameTimer;

        public override void Begin()
        {
            _gameTimer = GetGameTimer();
        }

        // [TickHandler]
        private async Task OnWorldRandomEvent()
        {
            foreach(KeyValuePair<string, RandomEvent> kvp in _randomEvents)
            {
                RandomEvent randomEvent = kvp.Value;
                if (_activeEvents.ContainsKey(randomEvent.Name)) continue; // its active, don't try to re-make it
                if ((GetGameTimer() - randomEvent.GameTimeActivated) < randomEvent.Cooldown) continue; // still to early
            }
        }
    }
}
