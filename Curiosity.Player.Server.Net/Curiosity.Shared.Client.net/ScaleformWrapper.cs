using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;
using CitizenFX.Core.UI;

namespace Curiosity.Shared.Client.net
{
    public class ScaleformWrapper : BaseScript
    {
        int scaleformHandle;

        public ScaleformWrapper(string scaleformName)
        {
            scaleformHandle = RequestScaleformMovie(scaleformName);
            while (!HasHudScaleformLoaded(scaleformHandle)) { Delay(0); }
        }

        public void CallFunction(string functionName, params object[] args)
        {
            BeginScaleformMovieMethod(scaleformHandle, functionName);
            foreach(object obj in args)
            {
                string type = obj.GetType().ToString();
                switch (type)
                {
                    case "string":
                        PushScaleformMovieMethodParameterString($"{obj}");
                        break;
                    case "boolean":
                        PushScaleformMovieMethodParameterBool(bool.Parse($"{obj}"));
                        break;
                    case "int":
                        PushScaleformMovieMethodParameterInt(int.Parse($"{obj}"));
                        break;
                    case "float":
                        PushScaleformMovieMethodParameterFloat(float.Parse($"{obj}"));
                        break;
                    default:
                        Screen.ShowNotification($"Invalid -> {type}");
                        break;
                }
            }
            EndScaleformMovieMethod();
        }

        public void DrawScaleFormMovie(float x, float y, int width, int height, int red, int green, int blue, int alpha, int unk)
        {
            DrawScaleformMovie(scaleformHandle, x, y, width, height, red, green, blue, alpha, unk);
        }

        public void RenderFullscreen()
        {
            DrawScaleformMovieFullscreen(scaleformHandle, 255, 255, 255, 255, 0);
        }

        public void Dispose()
        {
            SetScaleformMovieAsNoLongerNeeded(ref scaleformHandle);
        }
    }
}
