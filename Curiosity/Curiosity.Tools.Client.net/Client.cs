using CitizenFX.Core;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Global.Shared.net.Enums;
using Curiosity.Tools.Client.net.Controllers;
using Curiosity.Tools.Client.net.Menus;
using System;
using System.Threading.Tasks;

// ReSharper disable ClassNeverInstantiated.Global

namespace Curiosity.Tools.Client.net
{
    public class Client : BaseScript
    {
        public DevTools Tools { get; }
        public MenuController Menu { get; }
        public NoclipController NoClip { get; }
        public EntityDebugger Debugger { get; }

        public PlayerList players;

        private bool _isInstantiated;

        public bool IsDeveloper = false;

        public Client()
        {
            players = Players;

            Menu = new MenuController(this);
            Tools = new DevTools(this);
            NoClip = new NoclipController(this);
            Debugger = new EntityDebugger(this);

            RegisterEventHandler("playerSpawned", new Action(OnSpawn));
            RegisterEventHandler("curiosity:Client:Player:GetInformation", new Action<string>(SetPriviledge));

            if (Game.PlayerPed.Position != default(Vector3))
                OnSpawn();
        }

        private void SetPriviledge(string json)
        {
            PlayerInformationModel playerInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<PlayerInformationModel>(json);
            Privilege privilege = (Privilege)playerInfo.RoleId;
            bool serverIsDeveloper = (privilege == Privilege.DEVELOPER || privilege == Privilege.PROJECTMANAGER);

            if (serverIsDeveloper && !IsDeveloper)
            {
                CitizenFX.Core.UI.Screen.ShowNotification("~r~DEV TOOLS: ~g~Activated");
            }

            IsDeveloper = serverIsDeveloper;
        }

        private void OnSpawn()
        {
            if (!_isInstantiated)
                TriggerServerEvent("curiosity:Tools:Player:Ready");
            _isInstantiated = true;
        }

        public void RegisterEventHandler(string eventName, Delegate action)
        {
            EventHandlers[eventName] += action;
        }

        public void RegisterTickHandler(Func<Task> tick)
        {
            Tick += tick;
        }

        public void DeregisterTickHandler(Func<Task> tick)
        {
            Tick -= tick;
        }
    }
}
