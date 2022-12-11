namespace Curiosity.Framework.Client.Managers.GameWorld.Entity
{
    internal class WorldVehiclePartyBus : WorldVehicle
    {
        uint _propBattlePartyBusScreen;
        Prop _propBattlePartyBusScreenProp;
        int _namedRenderTargetId = -1;
        Scaleform _partyBusScaleform;

        int _currentAnimation;

        int _stateBagHandler;

        public WorldVehiclePartyBus(Vehicle vehicle) : base(vehicle)
        {
            Instance.AttachTickHandler(OnPartyBusAsync);
            _stateBagHandler = AddStateBagChangeHandler("VEHICLE_ANIMATION", $"entity:{Vehicle.NetworkId}", new Action<string, string, dynamic, int, bool>(OnVehicleAnimationStateBagChange));
        }

        private void OnVehicleAnimationStateBagChange(string bag, string key, dynamic animation, int reserved, bool replicated)
        {
            // Instance.Logger.Debug($"bag: {bag}, key: {key}, animation: {animation}, reserved: {reserved}, replicated: {replicated}");
            if (_partyBusScaleform is null) return;

            string netId = bag.Split(':').Last();
            if (Vehicle.NetworkId.ToString() != netId) return;

            _partyBusScaleform.CallFunction("SHOW_ANIMATION", (int)animation, false);
        }

        public async Task OnPartyBusAsync()
        {
            try
            {
                if (Vehicle == null || !Vehicle.Exists())
                {
                    if (_propBattlePartyBusScreenProp != null && _propBattlePartyBusScreenProp.Exists())
                        _propBattlePartyBusScreenProp.Delete();

                    Dispose();
                    
                    return;
                }

                _propBattlePartyBusScreen = (uint)GetHashKey("ba_prop_battle_pbus_screen");

                if (_partyBusScaleform is null)
                {
                    _partyBusScaleform = new Scaleform("PARTY_BUS");
                }

                SetupNamedRenderTarget();

                if ((await IsModelLoaded(_propBattlePartyBusScreen)) && _propBattlePartyBusScreenProp is null)
                {
                    if (!Vehicle.IsDriveable) return;
                    if (!Vehicle.Exists()) return;

                    Vector3 propPosition = Vehicle.Position + new Vector3(-50f, 0f, 0f);
                    _propBattlePartyBusScreenProp = new Prop(CreateObject((int)_propBattlePartyBusScreen, propPosition.X, propPosition.Y, propPosition.Z, false, false, false));
                    
                    if (_propBattlePartyBusScreenProp.IsAttached()) return;
                    
                    AttachEntityToEntity(_propBattlePartyBusScreenProp.Handle, Vehicle.Handle, 0, 0f, 0f, 0f, 0f, 0f, 0f, false, false, false, false, 2, true);
                }

                if (_partyBusScaleform.IsLoaded)
                {
                    N_0x32f34ff7f617643b(_partyBusScaleform.Handle, 1); // SET_SCALEFORM_MOVIE_TO_USE_LARGE_RT
                    SetTextRenderId(_namedRenderTargetId);
                    SetScriptGfxDrawOrder(4);
                    SetScriptGfxDrawBehindPausemenu(true);
                    DrawScaleformMovie(_partyBusScaleform.Handle, 0.4f, 0.045f, 0.8f, 0.09f, 255, 255, 255, 255, 0);
                    SetTextRenderId(GetDefaultScriptRendertargetRenderId());
                }

                if (Game.IsControlJustPressed(0, Control.CharacterWheel) && Game.PlayerPed == Vehicle.Driver)
                {
                    _currentAnimation++;
                    
                    if (_currentAnimation > 3)
                        _currentAnimation = 0;

                    Vehicle.State.Set("VEHICLE_ANIMATION", _currentAnimation, true);

                    //_partyBusScaleform.CallFunction("SHOW_ANIMATION", _currentAnimation, false);
                }
            }
            catch(Exception ex)
            {
                Instance.Logger.Error($"{ex}");
                Dispose();
            }
        }

        private void SetupNamedRenderTarget()
        {
            if (_namedRenderTargetId > 0) return;
            
            if (IsNamedRendertargetRegistered("PBus_Screen")) return;
            
            RegisterNamedRendertarget("PBus_Screen", false);
            
            if (IsNamedRendertargetLinked(_propBattlePartyBusScreen)) return;
            
            LinkNamedRendertarget(_propBattlePartyBusScreen);
            
            if (_namedRenderTargetId > -1) return;
            
            _namedRenderTargetId = GetNamedRendertargetRenderId("PBus_Screen");
        }

        private async Task<bool> IsModelLoaded(uint hash)
        {
            if (hash == 0) return true;
            Model model = (int)hash;
            await model.Request(1000);
            return model.IsLoaded;
        }

        public override void Dispose()
        {
            Instance.DetachTickHandler(OnPartyBusAsync);
            _partyBusScaleform.Dispose();
            _propBattlePartyBusScreenProp?.MarkAsNoLongerNeeded();
            _propBattlePartyBusScreenProp?.Delete();
            ReleaseNamedRendertarget("PBus_Screen");
            base.Dispose();
        }
    }
}
