﻿using CitizenFX.Core.Native;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Models;
using System;

namespace Curiosity.Core.Client.Managers
{
    public class MarqueeManager : Manager<MarqueeManager>
    {
        public override void Begin()
        {
            Instance.ExportDictionary.Add("Marquee", new Func<string, bool>(
                (message) =>
                {
                    SendNui(message);
                    return true;
                }));

            EventSystem.Attach("ui:marquee", new EventCallback(metadata =>
            {
                string message = metadata.Find<string>(0);
                SendNui(message);
                return true;
            }));
        }

        private void SendNui(string message)
        {
            string json = new JsonBuilder()
                .Add("operation", "MARQUEE")
                .Add("message", message)
                .Build();

            API.SendNuiMessage(json);
        }
    }
}