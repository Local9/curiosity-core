using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System;
using System.Collections.Generic;
using MenuAPI;
using System.Threading.Tasks;

namespace Curiosity.Client.net.Classes.Player
{
    class Creation
    {
        static public bool hasCreatedCharacter = false;

        static Client client = Client.GetInstance();

        static Vector3 creatorCamera = new Vector3(403.0164f, -1002.847f, -99.2587f);
        static Vector3 creatorCameraRotation = new Vector3(-5.0f, 0.0f, 0.0f);
        static float moveTick = 0.0f;

        static int playerPedHandle = Game.PlayerPed.Handle;

        static bool inAnimation = false;
        static int scaleformMovie;
        static int scaleformHandle;
        static int signObject1;
        static int signObject2;
        static int signProp1;
        static int signProp2;
        static string maleAnimation = "mp_character_creation@customise@male_a";
        public static bool IsPedHoldingSign = false;
        static bool IsCameraMoving = true;

        // MENU
        //static Menu menu = Menus.MenuBase.Menu;

        static public void Init()
        {
            client.RegisterEventHandler("playerSpawned", new Action(PlayerMugshot));

            client.RegisterTickHandler(DisableMovement);

            client.RegisterTickHandler(HoldSign);

            IsCameraMoving = true;
        }

        static async void SetupMenu()
        {
            try
            {
                while (IsCameraMoving)
                {
                    await BaseScript.Delay(0);
                }

                await BaseScript.Delay(0);
                //Menus.MenuBase.Menu.IgnoreDontOpenMenus = true;
                //Menus.PlayerCreator.PlayerCreatorMenu.ManualOpenMenu();
                Debug.WriteLine($"SetupMenu()");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"SetupMenu() -> {ex.Message}");
            }
            await BaseScript.Delay(0);
        }

        static async Task DisableMovement()
        {
            try
            {
                while (!hasCreatedCharacter)
                {
                    API.DisableAllControlActions(0);
                    API.DisableAllControlActions(1);
                    API.DisableAllControlActions(2);
                    await BaseScript.Delay(0);
                    API.HideHudAndRadarThisFrame();
                }

                if (hasCreatedCharacter)
                {
                    API.EnableAllControlActions(0);
                    API.EnableAllControlActions(1);
                    API.EnableAllControlActions(2);

                    client.DeregisterTickHandler(DisableMovement);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DisableMovement() -> {ex.Message}");
            }
            await Task.FromResult(0);
        }

        public static async void PlayerMugshot()
        {
            try
            {
                Environment.UI.Location.HideLocation = true;
                Environment.UI.WorldTime.HideClock = true;

                playerPedHandle = Game.PlayerPed.Handle;

                IsCameraMoving = true;

                Camera cam = World.CreateCamera(creatorCamera, creatorCameraRotation, 70);
                World.RenderingCamera = cam;

                Game.PlayerPed.Task.AchieveHeading(180.0f);
                Game.PlayerPed.Heading = 180.0f;
                Game.PlayerPed.Position = new Vector3(406.0043f, -997.2242f, -98.72668f);

                await BaseScript.Delay(500);

                Screen.Fading.FadeIn(2000);

                Game.PlayerPed.Task.GoTo(new Vector3(402.9426f, -997.2242f, -98.72668f), true, -1);

                while (moveTick < 1.0f)
                {
                    moveTick = moveTick + 0.005f;
                    cam.Position = new Vector3(403.0164f, -1002.847f + moveTick * 4.0f, -99.2587f + moveTick);
                    await BaseScript.Delay(10);
                }

                await BaseScript.Delay(500);

                Game.PlayerPed.Task.AchieveHeading(180.0f);

                await BaseScript.Delay(500);

                IsCameraMoving = false;
                IsPedHoldingSign = false;
                

                Debug.WriteLine($"DisableMovement()");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DisableMovement() -> {ex.Message}");
            }
}

        public static async Task HoldSign()
        {
            try
            {
                while (IsCameraMoving)
                {
                    await BaseScript.Delay(0);
                }

                if (IsPedHoldingSign)
                {
                    await BaseScript.Delay(0);
                    return;
                }
                IsPedHoldingSign = true;

                API.RequestAnimDict(maleAnimation);
                while (!API.HasAnimDictLoaded(maleAnimation))
                {
                    await BaseScript.Delay(0);
                };

                API.TaskPlayAnim(playerPedHandle, maleAnimation, "loop", 8, -8, -1, 49, 0, false, false, false);
                signObject1 = API.GetHashKey("prop_police_id_board");
                signObject2 = API.GetHashKey("prop_police_id_text");

                signProp1 = API.CreateObject(signObject1, 1, 1, 1, true, true, false);
                signProp2 = API.CreateObject(signObject2, 1, 1, 1, true, true, false);

                API.AttachEntityToEntity(signProp1, playerPedHandle, API.GetPedBoneIndex(API.GetPlayerPed(-1), 58868), 0.12f, 0.24f, 0.0f, 5.0f, 0.0f, 70.0f, true, true, false, false, 2, true);
                API.AttachEntityToEntity(signProp2, playerPedHandle, API.GetPedBoneIndex(API.GetPlayerPed(-1), 58868), 0.12f, 0.24f, 0.0f, 5.0f, 0.0f, 70.0f, true, true, false, false, 2, true);

                API.SetEntityAsMissionEntity(signProp1, true, true);
                API.SetEntityAsMissionEntity(signProp2, true, true);

                scaleformHandle = CreateScaleformHandle("ID_Text", signObject2);

                scaleformMovie = API.RequestScaleformMovie("MUGSHOT_BOARD_01");
                while (!API.HasScaleformMovieLoaded(scaleformMovie))
                {
                    await BaseScript.Delay(0);
                }

                while (API.HasScaleformMovieLoaded(scaleformMovie))
                {
                    API.PushScaleformMovieFunction(scaleformMovie, "SET_BOARD");
                    API.PushScaleformMovieFunctionParameterString("Line 1");
                    API.PushScaleformMovieFunctionParameterString(Game.Player.Name);
                    API.PushScaleformMovieFunctionParameterString(DateTime.Now.ToString("yyyy-MM-dd"));
                    API.PushScaleformMovieFunctionParameterString("Line 2");
                    API.PushScaleformMovieFunctionParameterInt(0); // No visible effect 
                    API.PushScaleformMovieFunctionParameterInt(0); // GTA Online Character level
                    API.PushScaleformMovieFunctionParameterInt(0); // No visible effect
                    API.PopScaleformMovieFunctionVoid();

                    API.SetTextRenderId(scaleformHandle);
                    Function.Call((Hash)0x40332D115A898AF5, scaleformMovie, true);
                    API.SetUiLayer(4);
                    Function.Call((Hash)0xc6372ecd45d73bcd, scaleformMovie, true);
                    API.DrawScaleformMovie(scaleformMovie, 0.4f, 0.35f, 0.8f, 0.75f, 255, 255, 255, 255, 255);
                    API.SetTextRenderId(API.GetDefaultScriptRendertargetRenderId());
                    Function.Call((Hash)0x40332D115A898AF5, scaleformMovie, false);
                    await BaseScript.Delay(0);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DisableMovement() -> {ex.Message}");
            }
            await Task.FromResult(0);
        }

        public static void ClearTasks()
        {
            API.ClearPedTasksImmediately(Game.PlayerPed.Handle);
            API.StopAnimTask(Game.PlayerPed.Handle, maleAnimation, "loop", 8.0f);

            inAnimation = false;

            API.DeleteObject(ref signProp1);
            API.DeleteObject(ref signProp2);
            API.SetObjectAsNoLongerNeeded(ref signProp1);
            API.SetObjectAsNoLongerNeeded(ref signProp2);

            API.SetScaleformMovieAsNoLongerNeeded(ref scaleformMovie);
            API.SetScaleformMovieAsNoLongerNeeded(ref scaleformHandle);
        }

        // Credits to throwarray converted function in his post from lua to C#
        static int CreateScaleformHandle(string name, int model)
        {
            int handle = 0;
            try
            {
                if (!API.IsNamedRendertargetRegistered(name))
                {
                    API.RegisterNamedRendertarget(name, false);
                }

                if (!API.IsNamedRendertargetLinked((uint)model))
                {
                    API.LinkNamedRendertarget((uint)model);
                }

                if (API.IsNamedRendertargetRegistered(name))
                {
                    handle = API.GetNamedRendertargetRenderId(name);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DisableMovement() -> {ex.Message}");
            }
            return handle;
        }
    }
}
