using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Shared.Client.net.Helper;

namespace Curiosity.Client.net.Classes.Environment
{
    class SpawnManagement
    {
        static List<Vector3> hospitals = new List<Vector3>()
        {
            new Vector3(297.8683f, -584.3318f, 43.25863f),
            new Vector3(356.434f, -598.5284f, 28.78098f),
            new Vector3(307.5486f, -1434.502f, 29.86082f),
            new Vector3(342.1533f, -1397.199f, 32.50924f),
            new Vector3(-496.291f, -336.9345f, 34.50164f),
            new Vector3(-449.0542f, -339.1804f, 34.50176f),
            new Vector3(1827.909f, 3691.912f, 34.22427f),
            new Vector3(-243.5568f, 6326.441f, 32.42619f)
        };

        static Random rnd = new Random();
        static Client client = Client.GetInstance();
        static bool hasPlayerSpawned = false;

        public static void Init()
        {
            client.RegisterEventHandler("playerSpawned", new Action(OnPlayerSpawned));
            client.RegisterTickHandler(OnWastedCheck);

            foreach (Vector3 pos in hospitals)
            {
                API.AddHospitalRestart(pos.X, pos.Y, pos.Z - 1.0f, 0.0f, 0);
            }
        }

        static void OnPlayerSpawned()
        {
            hasPlayerSpawned = true;
        }

        static async Task OnWastedCheck()
        {
            try
            {
                await Client.Delay(10);
                if (Game.PlayerPed.IsDead && hasPlayerSpawned)
                {
                    // Entity entity = Game.PlayerPed.GetKiller();

                    await Client.Delay(0);
                    UI.Scaleforms.Wasted();

                    await Client.Delay(2000);

                    Screen.Fading.FadeOut(1000);

                    while (Screen.Fading.IsFadingOut)
                    {
                        await Client.Delay(0);
                    }

                    Game.Player.WantedLevel = 0;
                    Game.PlayerPed.ClearBloodDamage();
                    Game.PlayerPed.ClearLastWeaponDamage();

                    int r = rnd.Next(hospitals.Count);

                    Vector3 playerPos = Game.PlayerPed.Position;

                    Vector3 pos = new Vector3();

                    foreach (Vector3 hosPos in hospitals)
                    {
                        float distance = API.GetDistanceBetweenCoords(playerPos.X, playerPos.Y, playerPos.Z, hosPos.X, hosPos.Y, hosPos.Z, false);

                        if (distance < 3000f)
                        {
                            pos = hosPos;
                            break;
                        }
                    }

                    if (pos.IsZero)
                    {
                        pos = hospitals[r];
                        UI.Notifications.LifeV(1, "EMS", "", "Looks like you had a bad coma...", 132);
                    }

                    float groundZ = pos.Z;
                    API.GetGroundZFor_3dCoord(pos.X, pos.Y, pos.Z, ref groundZ, false);

                    Game.PlayerPed.Position = new Vector3(pos.X, pos.Y, pos.Z - 1.0f);

                    Game.PlayerPed.Resurrect();
                    await Client.Delay(1000);

                    Screen.Fading.FadeIn(1000);

                    while (Screen.Fading.IsFadingIn)
                    {
                        await Client.Delay(0);
                    }

                    Client.TriggerServerEvent("curiosity:Server:Bank:MedicalFees");
                    UI.Notifications.LifeV(1, "EMS", "Medical Fees", "You have been charged for your stay, please try to stay alive.", 132);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnWastedCheck -> {ex.Message}");
            }
        }
    }

}

