using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Systems.Client.Extensions;
using Curiosity.Systems.Client.Interface;
using Curiosity.Systems.Library.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Systems.Client.Environment.Stores.Impl
{
    public class Convenience : Store
    {
        public Position[] Stores { get; } =
        {
            new Position(-1221.687f, -908.5657f, 12.32636f, 35.45065f)
        };

        public override void Load()
        {
            foreach (var store in Stores)
            {
                new BlipInfo
                {
                    Name = "Convenience Store",
                    Sprite = (BlipSprite)52,
                    Color = (BlipColor)4,
                    Position = store
                }.Commit();

                store.Z -= 1f; // lower ped position by 1 unit

                var storePed = new CuriosityPed(store, PedHash.ShopKeep01);
                storePed.FreezeInPosition = true;
                storePed.AnimationDict = "amb@world_human_hang_out_street@female_arms_crossed@base";
                storePed.AnimationBone = "base";
                storePed.Init();

                storePed.Callback += OpenShop;
            }
        }

        public void OpenShop()
        {
            Screen.ShowSubtitle("Hello World");
        }
    }
}
