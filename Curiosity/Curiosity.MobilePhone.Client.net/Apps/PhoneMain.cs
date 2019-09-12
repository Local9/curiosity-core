using CitizenFX.Core;
using Curiosity.MobilePhone.Client.net.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.MobilePhone.Client.net.Apps
{
    public class PhoneMain : App
    {
        public int SelectedItem { get; set; }
        public List<App> AllApps { get; set; }

        public PhoneMain(ClientData.Phone phone, List<App> allApps) : base("Main", 0, phone, false)
        {
            AllApps = allApps;

            Debug.WriteLine($"Apps totali {AllApps.Count}");
        }

        public override async Task Tick()
        {
            try
            {
                if (Phone.Scaleform == null || AllApps == null)
                {
                    return;
                }

                for (int i = 0; i < 9; i++)
                {
                    var thirdParam = 3;

                    if (i < AllApps.Count)
                    {
                        if (AllApps[i].Icon != 0)
                        {
                            thirdParam = AllApps[i].Icon;
                        }
                    }

                    Phone.Scaleform.CallFunction("SET_DATA_SLOT", 1, i, thirdParam);
                    
                }

                Phone.Scaleform.CallFunction("DISPLAY_VIEW", 1, SelectedItem);
                Phone.Scaleform.CallFunction("CELLPHONE_APP", SelectedItem, "Hi", true);

                var appName = "";
                if (SelectedItem < AllApps.Count)
                {
                    if(!String.IsNullOrEmpty(AllApps[SelectedItem].Name)) { 
                        appName = AllApps[SelectedItem].Name;
                    }
                }

                Phone.Scaleform.CallFunction("SET_HEADER", appName);

                var navigated = true;

                if (Game.IsControlJustPressed(1, Control.PhoneUp))
                {
					SelectedItem -= 3;
					MoveFinger(1);
					if (SelectedItem < 0)
                    {
                        SelectedItem = 8 + SelectedItem;
                    }
                }
                else if (Game.IsControlJustPressed(1, Control.PhoneDown))
                {
					SelectedItem += 3;
					MoveFinger(2);
					if (SelectedItem > 8)
                    {
                        SelectedItem = SelectedItem - 8;
                    }
                }
                else if (Game.IsControlJustPressed(1, Control.PhoneRight))
                {
                    SelectedItem += 1;
					MoveFinger(3);
					if (SelectedItem > 8)
                    {
                        SelectedItem = 0;
                    }
                }
                else if (Game.IsControlJustPressed(1, Control.PhoneLeft))
                {
                    SelectedItem -= 1;
					MoveFinger(3);
					if (SelectedItem < 0)
                    {
                        SelectedItem = 8;
                    }
                }
                else if (Game.IsControlJustPressed(1, Control.PhoneSelect))
                {
					MoveFinger(5);
					BaseScript.TriggerEvent("lprp:phone_start", AllApps[SelectedItem].Name);
                }
                else
                {
                    navigated = false;
                }

                if (navigated)
                {
                    Game.PlaySound("Menu_Navigate", "Phone_SoundSet_Default");
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{e.Message} : Exception thrown on Apps.Main.Tick()");
            }

            await Task.FromResult(0);
        }

        public override void Initialize(ClientData.Phone phone)
        {
            Phone = phone;

            SelectedItem = 0;
        }

        public override void Kill()
        {
        }
    }
}
