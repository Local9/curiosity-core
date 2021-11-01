using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Environment.Entities.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Managers
{
    public class SpeedCameraManager : Manager<SpeedCameraManager>
    {
        Map _speedCameraMap;

        List<MapObject> sets = new List<MapObject>();
        List<MapObject> spawns = new List<MapObject>();

        public async override void Begin()
        {
            try
            {
                await Session.Loading();
                foreach (MapObject mapObj in Get().Objects.MapObject)
                {
                    if (mapObj.Type == "Prop")
                    {
                        sets.Add(mapObj);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private Map Get()
        {
            Map config = new();

            try
            {
                if (_speedCameraMap is not null)
                    return _speedCameraMap;

                _speedCameraMap = JsonConvert.DeserializeObject<Map>(Properties.Resources.config);
                return _speedCameraMap;
            }
            catch (Exception ex)
            {
                Logger.Error($"Speed Camera JSON File Exception\nDetails: {ex.Message}\nStackTrace:\n{ex.StackTrace}");
            }

            return config;
        }

        // https://github.com/blattersturm/cfx-object-loader/blob/master/object-loader/object_loader.lua

        [TickHandler(SessionWait = true)]
        private async Task OnSpeedCameraPropLoader()
        {
            foreach (MapObject mapObject in sets)
            {
                int hash = GetHashKey(mapObject.Hash);
                mapObject.PropHash = hash;
                RequestModel((uint)hash);
            }

            while (true)
            {
                bool loaded = true;
                await BaseScript.Delay(0);
                foreach (MapObject mapObject in sets)
                {
                    int hash = GetHashKey(mapObject.Hash);
                    if (!HasModelLoaded((uint)hash))
                    {
                        loaded = false;
                        break;
                    }
                    if (loaded) break;
                }
            }
        }

        [TickHandler(SessionWait = true)]
        private async Task OnSpeedCameraProps()
        {
            await BaseScript.Delay(100);
            Vector3 currentPosition = GetEntityCoords(PlayerPedId(), false);

            int count = 0;

            foreach (MapObject obj in sets)
            {
                bool isNear = IsNearObject(obj, currentPosition);
                if (isNear && obj.PropHandle == -1)
                {
                    Vector3 propPos = obj.Position.Vector3;
                    int propHandle = CreateObjectNoOffset((uint)obj.PropHash, propPos.X, propPos.Y, propPos.Z, false, false, false);
                    if (DoesEntityExist(propHandle))
                    {
                        Vector3 propRot = obj.Rotation.Vector3;
                        SetEntityRotation(propHandle, propRot.X, propRot.Y, propRot.Z, 2, false);
                        FreezeEntityPosition(propHandle, true);
                        obj.PropHandle = propHandle;
                    }
                }
                else if (!isNear && obj.PropHandle != -1)
                {
                    DeleteObject(ref obj.PropHandle);
                    obj.PropHandle = -1;
                }

                if (count % 75 == 0)
                {
                    await BaseScript.Delay(15);
                }

                count++;
            }

        }

        bool IsNearObject(MapObject mapObject, Vector3 position)
        {
            Vector3 diff = mapObject.Position.Vector3 - position;
            float dist = (diff.X * diff.X) + (diff.Y * diff.Y);
            return (dist < (400 * 400));
        }
    }
}
