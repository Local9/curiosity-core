using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System;
using System.Threading.Tasks;

namespace Curiosity.Client.net.Classes.Environment.UI
{
    static class ScaleformWrapper
    {
        public async static Task<Scaleform> Request(string name)
        {
            int startTime = API.GetGameTimer();
            Scaleform scaleform = new Scaleform(name);

            while (!scaleform.IsLoaded)
            {
                await Client.Delay(0);
                if  (API.GetGameTimer() - startTime >= 5000)
                {
                    Debug.WriteLine("SCALEFORM -> Request loading failed");
                    return null;
                }
            }
            Debug.WriteLine("SCALEFORM -> Loaded");
            return scaleform;
        }

        public async static void RequestHud(HudComponent hudComponent)
        {
            API.RequestHudScaleform((int)hudComponent);
            int startTime = API.GetGameTimer();
            while (!API.HasHudScaleformLoaded((int)hudComponent))
            {
                await Client.Delay(0);
                if (API.GetGameTimer() - startTime >= 5000)
                {
                    Debug.WriteLine("SCALEFORM -> Request loading failed");
                    return;
                }
            }
            Debug.WriteLine("SCALEFORM -> Loaded");
        }

        public static void CallScaleformFunction(this Scaleform scaleform, ScaleFormType scaleFormType, string function, params object[] args)
        {
            if (scaleFormType == ScaleFormType.hud)
            {
                API.BeginScaleformMovieMethodHudComponent(scaleform.Handle, function);
            }
            else if (scaleFormType == ScaleFormType.normal)
            {
                API.BeginScaleformMovieMethod(scaleform.Handle, function);
            }
            foreach(object arg in args)
            {
                Type objectType = arg.GetType();
                if (objectType == typeof(bool))
                {
                    API.PushScaleformMovieFunctionParameterBool((bool)arg);
                }
                else if (objectType == typeof(int))
                {
                    API.PushScaleformMovieFunctionParameterInt((int)arg);
                }
                else if (objectType == typeof(float))
                {
                    API.PushScaleformMovieFunctionParameterFloat((float)arg);
                }
                else if (objectType == typeof(string))
                {
                    API.PushScaleformMovieFunctionParameterString((string)arg);
                }
                else
                {
                    API.PushScaleformMovieFunctionParameterInt(0);
                }
                API.EndScaleformMovieMethod();
            }
        }

        public static void CallHudFunction(this Scaleform scaleform, string function, params object[] args)
        {
            CallScaleformFunction(scaleform, ScaleFormType.hud, function, args);
        }

        public static void CallFunction(this Scaleform scaleform, string function, params object[] args)
        {
            CallScaleformFunction(scaleform, ScaleFormType.normal, function, args);
        }

        public static void Draw2D(this Scaleform scaleform)
        {
            API.DrawScaleformMovieFullscreen(scaleform.Handle, 255, 255, 255, 255, 0);
        }

        public static void Draw2DNormal(this Scaleform scaleform, float x, float y, int width, int height)
        {
            API.DrawScaleformMovie(scaleform.Handle, x, y, width, height, 255, 255, 255, 255, 0);
        }

        public static void Draw2DScreenSpace(this Scaleform scaleform, float locX, float locY, int sizeX, int sizeY)
        {
            float x = locY / Screen.Resolution.Width;
            float y = locX / Screen.Resolution.Height;

            float width = sizeX / Screen.Resolution.Width;
            float height = sizeY / Screen.Resolution.Height;

            API.DrawScaleformMovie(scaleform.Handle, x + (width / 2.0f), y + (height / 2.0f), width, height, 255, 255, 255, 255, 0);
        }

        public static void Render3D(this Scaleform scaleform, float x, float y, float z, float rx, float ry, float rz, float scaleX, float scaleY, float scaleZ)
        {
            API.DrawScaleformMovie_3dNonAdditive(scaleform.Handle, x, y, z, rx, ry, rz, 2.0f, 2.0f, 1.0f, scaleX, scaleY, scaleZ, 2);
        }

        public static void Render3DAdditive(this Scaleform scaleform, float x, float y, float z, float rx, float ry, float rz, float scaleX, float scaleY, float scaleZ)
        {
            API.DrawScaleformMovie_3d(scaleform.Handle, x, y, z, rx, ry, rz, 2.0f, 2.0f, 1.0f, scaleX, scaleY, scaleZ, 2);
        }

        public static bool IsValid(this Scaleform scaleform)
        {
            return scaleform != null ? true : false;
        }
    }

    public enum ScaleFormType
    {
        normal,
        hud
    }
}
