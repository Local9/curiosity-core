using CitizenFX.Core;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Shared.Client.net.Extensions;
using Curiosity.Shared.Client.net.Models;
using System;
using System.Collections.Generic;
using Entity = Curiosity.Global.Shared.net.Entity;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;

namespace Curiosity.Client.net.Classes.Player
{
    static class Inventory
    {
        static List<Entity.Inventory> inventories = new List<Entity.Inventory>();
        static Client client = Client.GetInstance();

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:Client:Inventory:Items", new Action<string>(StoreInventory));
        }

        static void StoreInventory(string inventoryJson)
        {
            inventories = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Entity.Inventory>>(inventoryJson);

            if (inventories.Count == 0)
            {
                Screen.ShowNotification("~r~You are currently carrying no items.");
                return;
            }

            Invent invent = new Invent();
            invent.panel = "solo-inventory";
            invent.data = inventories;

            API.SendNuiMessage(Newtonsoft.Json.JsonConvert.SerializeObject(invent));
            API.SetNuiFocus(true, true);
            API.SetTransitionTimecycleModifier($"BLACKOUT", 5.0f);
        }
    }

    public class Invent
    {
        public string panel;
        public List<Entity.Inventory> data;
    }
}
