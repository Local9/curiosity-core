using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.CalloutAdvanced.Client
{
    public class Client : BaseScript
    {
        public Client()
        {
            API.RegisterCommand("fuck", new Action<int, List<object>, string>(OnTest), false);
        }

        private void OnTest(int arg1, List<object> arg2, string arg3)
        {
            CalloutMessage calloutMessage = new CalloutMessage();
            calloutMessage.Success = true;

            string json = JsonConvert.SerializeObject(calloutMessage);

            Debug.WriteLine(json);
        }
    }
    public class CalloutMessage
    {
        public string Name;
        public bool Success = false;
        public int NumberKilled;
        public int NumberRescued;
        public List<CalloutPed> calloutPed = new List<CalloutPed>();
        public List<CalloutVehicle> calloutVehicle = new List<CalloutVehicle>();
    }

    public class CalloutVehicle
    {
        public bool IsDestroyed;
        public bool IsImpounded;
        public bool IsStolen;

        public List<CalloutItem> Items = new List<CalloutItem>();
    }

    public class CalloutPed
    {
        public string Name;

        public bool IsAlive;
        public bool IsDrunk;
        public bool IsUnderInfluence;

        public List<CalloutItem> Items = new List<CalloutItem>();
    }

    public class CalloutItem
    {
        public bool IsIllegal;
        public string Label;
    }
}
