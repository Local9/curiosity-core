using CitizenFX.Core;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Environment.Entities.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;

namespace Curiosity.Core.Client.Managers
{
    public class SpeedCameraManager : Manager<SpeedCameraManager>
    {
        Map _speedCameraMap;

        List<MapObject> sets = new List<MapObject>();
        List<MapObject> spawns = new List<MapObject>();

        public async override void Begin()
        {
            //try
            //{
            //    await Session.Loading();

            //    foreach (MapObject mapObj in Get().Objects.MapObject)
            //    {
            //        if (mapObj.Type == "Prop")
            //        {
            //            int hash = GetHashKey(mapObj.Hash);
            //            mapObj.PropHash = hash;
            //            sets.Add(mapObj);
            //        }
            //    }

            //    Logger.Info($"-> {sets.Count} Speed Cameras Loaded");

            //    Instance.AttachTickHandler(OnSpeedCameraPropLoader);
            //    Instance.AttachTickHandler(OnSpeedCameraProps);
            //}
            //catch (Exception ex)
            //{
            //    Logger.Error($"{ex}");
            //}
        }

        private Map Get()
        {
            Map config = new();

            try
            {
                if (_speedCameraMap is not null)
                    return _speedCameraMap;

                _speedCameraMap = JsonConvert.DeserializeObject<Map>(Properties.Resources.speedCameras);
                return _speedCameraMap;
            }
            catch (Exception ex)
            {
                Logger.Error($"Speed Camera JSON File Exception\nDetails: {ex.Message}\nStackTrace:\n{ex.StackTrace}");
            }

            return config;
        }

        // https://github.com/blattersturm/cfx-object-loader/blob/master/object-loader/object_loader.lua

        public async Task OnSpeedCameraPropLoader()
        {
            if (sets.Count == 0)
            {
                // Instance.DetachTickHandler(OnSpeedCameraPropLoader);
            }

            foreach (MapObject mapObject in sets)
            {
                int hash = GetHashKey(mapObject.Hash);
                mapObject.PropHash = hash;
                RequestModel((uint)mapObject.PropHash);
            }

            while (true)
            {
                bool loaded = true;
                await BaseScript.Delay(0);
                foreach (MapObject mapObject in sets)
                {
                    if (!HasModelLoaded((uint)mapObject.PropHash))
                    {
                        loaded = false;
                        break;
                    }
                    if (loaded) break;
                }
            }
        }

        public async Task OnSpeedCameraProps()
        {
            try
            {
                if (sets.Count == 0)
                {
                    // Instance.DetachTickHandler(OnSpeedCameraProps);
                }

                await BaseScript.Delay(100);
                Vector3 currentPosition = GetEntityCoords(PlayerPedId(), false);

                int count = 0;

                foreach (MapObject obj in sets)
                {
                    bool isNear = IsNearObject(obj, currentPosition);
                    if (isNear && obj.PropHandle == 0)
                    {
                        Vector3 propPos = obj.Position.Vector3;
                        int propHandle = CreateObjectNoOffset((uint)obj.PropHash, propPos.X, propPos.Y, propPos.Z, false, false, false);

                        if (propHandle != 0)
                        {
                            Vector3 propRot = obj.Rotation.Vector3;
                            SetEntityRotation(propHandle, propRot.X, propRot.Y, propRot.Z, 2, false);
                            SetEntityQuaternion(propHandle, obj.Quaternion.X, obj.Quaternion.Y, obj.Quaternion.Z, obj.Quaternion.W);
                            SetEntityDynamic(propHandle, obj.Dynamic);
                            FreezeEntityPosition(propHandle, !obj.Dynamic);
                            obj.PropHandle = propHandle;
                        }
                    }
                    else if (!isNear && obj.PropHandle != 0)
                    {
                        DeleteObject(ref obj.PropHandle);
                        obj.PropHandle = 0;
                    }

                    if (count % 75 == 0)
                    {
                        await BaseScript.Delay(15);
                    }

                    count++;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{ex}");
            }
        }

        bool IsNearObject(MapObject mapObject, Vector3 position)
        {
            Vector3 diff = mapObject.Position.Vector3 - position;
            float dist = (diff.X * diff.X) + (diff.Y * diff.Y);
            return (dist < (200 * 200));
        }
    }
}
