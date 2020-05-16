using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Shared.Client.net.Helper;
using System;
using System.Threading.Tasks;

namespace Curiosity.Client.net.Classes.Player
{
    static class MugshotCreator
    {
        static bool inAnimation = false;
        static int scaleformMovie;
        static int scaleformHandle;
        static int signObject1;
        static int signObject2;
        static int signProp1;
        static int signProp2;
        static string maleAnimation = "mp_character_creation@customise@male_a";

        public static void Init()
        {
            // https://forum.fivem.net/t/c-render-target-example-using-mugshot-sign/180682

            Client.GetInstance().RegisterTickHandler(OnMugshotBoardTick);
        }

        static async Task OnMugshotBoardTick()
        {
            if (ControlHelper.IsControlJustPressed(Control.InteractionMenu, true, ControlModifier.Shift))
            {
                if (!inAnimation)
                {
                    RunAnimation();
                    inAnimation = true;
                }
                else
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
            }

            await Task.FromResult(0);
        }

        static async void RunAnimation()
        {
            int player = Game.PlayerPed.Handle;

            Game.PlayerPed.Position = new Vector3(405.81110f, -995.05540f, -99.49297f);

            API.RequestAnimDict(maleAnimation);
            while (!API.HasAnimDictLoaded(maleAnimation))
            {
                await BaseScript.Delay(0);
            };

            API.TaskPlayAnim(player, maleAnimation, "loop", 8, -8, -1, 49, 0, false, false, false);
            signObject1 = API.GetHashKey("prop_police_id_board");
            signObject2 = API.GetHashKey("prop_police_id_text");

            signProp1 = API.CreateObject(signObject1, 1, 1, 1, true, true, false);
            signProp2 = API.CreateObject(signObject2, 1, 1, 1, true, true, false);

            API.AttachEntityToEntity(signProp1, player, API.GetPedBoneIndex(API.GetPlayerPed(-1), 58868), 0.12f, 0.24f, 0.0f, 5.0f, 0.0f, 70.0f, true, true, false, false, 2, true);
            API.AttachEntityToEntity(signProp2, player, API.GetPedBoneIndex(API.GetPlayerPed(-1), 58868), 0.12f, 0.24f, 0.0f, 5.0f, 0.0f, 70.0f, true, true, false, false, 2, true);

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

        // Credits to throwarray converted function in his post from lua to C#
        static int CreateScaleformHandle(string name, int model)
        {
            int handle = 0;
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
            return handle;
        }
    }
}
