using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Systems.Client.Diagnostics;
using Curiosity.Systems.Client.Extensions;
using Curiosity.Systems.Library.Enums;
using Curiosity.Systems.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Systems.Client.Environment
{
    public class CuriosityPed
    {
        public CuriosityPlugin Curiosity { get; set; }
        public Position Position { get; set; }
        public float LoadInDistance { get; set; } = 100f;
        public float DespawnDistance { get; set; } = 110f;
        public PedHash PedHash { get; set; }
        public string Message { get; set; } = "Press ~INPUT_CONTEXT~ to interact...";
        public event Action Callback;
        public bool FreezeInPosition { get; set; } = false;
        public string AnimationDict { get; set; }
        public string AnimationBone { get; set; }

        public Vector3 ForwardOffset { get; set; }

        private Ped _ped;
        private Model _model;

        public int Handle { get { return _ped.Handle; } }

        public CuriosityPed(Position position, PedHash pedHash)
        {
            Position = position;
            PedHash = pedHash;

            Curiosity = CuriosityPlugin.Instance;
        }

        public void Init()
        {
            Curiosity.AttachTickHandler(OnTick);
        }

        public void Stop()
        {
            Curiosity.DetachTickHandler(OnTick);
        }

        private async Task OnTick()
        {
            if (Curiosity?.Local?.Entity != null)
            {
                var position = Cache.Entity.Position;
                var distance = position.Distance(Position, true);

                if (distance < LoadInDistance)
                {
                    if (_ped == null)
                    {
                        Load();
                    }
                }
                
                if (distance > DespawnDistance)
                {
                    Dispose();
                }
            }
            else
            {
                Curiosity = CuriosityPlugin.Instance;
                await BaseScript.Delay(1000);
            }
        }

        private async void Load()
        {
            if (Curiosity?.Local?.Entity != null)
            {
                _model = PedHash;
                await _model.Request(5000);

                Ped pedInArea = World.GetAllPeds().Select(p => p).Where(x => x.Model.Hash == (int)PedHash).FirstOrDefault();

                if (pedInArea == null)
                {
                    API.ClearAreaOfPeds(Position.X, Position.Y, Position.Z, 10f, 1);

                    Vector3 spawnPosition = Position.AsVector();

                    // Create ped but not over network, don't need duplications
                    int ped = API.CreatePed((int)PedTypes.PED_TYPE_MISSION, (uint)_model.Hash, spawnPosition.X, spawnPosition.Y, spawnPosition.Z, Position.Heading, false, true);
                    _ped = new Ped(ped);

                    API.PlaceObjectOnGroundProperly(_ped.Handle);

                    _ped.IsPositionFrozen = FreezeInPosition;

                    API.TaskSetBlockingOfNonTemporaryEvents(_ped.Handle, true);

                    API.DecorSetInt(_ped.Handle, CuriosityPlugin.DECOR_PED_OWNER, Game.PlayerPed.Handle);

                    Logger.Debug($"Spawned Ped with '{PedHash}'");
                }
                else
                {
                    _ped = pedInArea;

                    Logger.Debug($"Found existing Ped with '{PedHash}'");
                }

                ForwardOffset = API.GetOffsetFromEntityInWorldCoords(_ped.Handle, 0f, 2f, 0f);

                if (!string.IsNullOrEmpty(AnimationDict))
                {
                    await CommonFunctions.LoadAnimationDict(AnimationDict);
                    API.TaskPlayAnim(_ped.Handle, AnimationDict, AnimationBone, 1f, -1f, -1, 1, 1f, true, true, true);
                }

                _model.MarkAsNoLongerNeeded();
            }
        }

        private void Dispose()
        {
            if (Curiosity?.Local?.Entity != null)
            {
                if (_ped != null)
                {

                    if (_ped.Exists())
                    {
                        int owner = API.DecorGetInt(_ped.Handle, CuriosityPlugin.DECOR_PED_OWNER);

                        if (owner == Game.PlayerPed.Handle)
                        {
                            API.RemoveAnimDict(AnimationDict);

                            _ped.MarkAsNoLongerNeeded();

                            _ped.Delete();

                            _ped = null;

                            Logger.Debug($"Removed Ped");
                        }
                    }
                }
            }
        }
    }
}
