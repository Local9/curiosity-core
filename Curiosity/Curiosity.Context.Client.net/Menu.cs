using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;
using Curiosity.Shared.Client.net;

namespace Curiosity.Context.Client.net
{
    public class Menu : BaseScript
    {
        private static Menu _instance;

        bool showMenu = false;

        public Menu()
        {
            _instance = this;
            RegisterNuiEventHandler("disablenuifocus", new Action<dynamic>(DisableNuiFocus));
            RegisterNuiEventHandler("togglelock", new Action<dynamic>(OnToggleLockStatus));
        }

        void OnToggleLockStatus(dynamic nui)
        {
            TriggerEvent("curiosity:Client:Menu:CarLock", nui.id);
        }

        void DisableNuiFocus(dynamic nui)
        {
            showMenu = nui.nuifocus;
            SetNuiFocus(showMenu, showMenu);
        }

        /// <summary>
        /// Registers a NUI/CEF event
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void RegisterNuiEventHandler(string name, Delegate action)
        {
            try
            {
                Function.Call(Hash.REGISTER_NUI_CALLBACK_TYPE, name);
                RegisterEventHandler(string.Concat("__cfx_nui:", name), action);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        /// <summary>
        /// Registers a network event
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        public void RegisterEventHandler(string name, Delegate action)
        {
            try
            {
                EventHandlers[name] += action;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }
    }
}
