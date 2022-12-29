using CitizenFX.Core.UI;
using Curiosity.Framework.Client.Extensions;
using Curiosity.Framework.Client.Managers.Events;
using Curiosity.Framework.Client.Utils;
using Curiosity.Framework.Shared.SerializedModels;
using FxEvents;
using ScaleformUI.Scaleforms.WebBrowser.Data;
using System.Drawing;

namespace Curiosity.Framework.Client.Managers
{
    public class UserManager : Manager<UserManager>
    {
        public User _user => Session.User; // refactor
        bool _playerSpawned = false;
        Vehicle _vehicle;
        Vector3 _position = new Vector3(0, 0, 0);
        float _heading = 0f;

        bool _toggle;
        bool _drawBrowser;

        ScaleformUI.Scaleforms.WebBrowser.WebBrowserHandler _webBrowser = new();

        public async override void Begin()
        {
            Event("onResourceStop", new Action<string>(OnResourceStop));

            InternalGameEvents.PlayerJoined += OnPlayerJoined;

            _playerSpawned = Game.Player.State.Get("player:spawned") ?? false;
            if (!_playerSpawned)
            {
                Logger.Debug($"Player not spawned, waiting for spawn");
                GameInterface.Hud.FadeOut(0);
                await LoadTransition.OnUpAsync();
                ScreenInterface.DisableHud();
                ScreenInterface.CloseLoadingScreen();
                ScreenInterface.StartLoadingMessage("PM_WAIT");
            }


            // Get last position
            float x = GetResourceKvpFloat("pos:x");
            float y = GetResourceKvpFloat("pos:y");
            float z = GetResourceKvpFloat("pos:z");
            _position = new Vector3(x, y, z);
            _heading = GetResourceKvpFloat("pos:h");

            OnRequestCharactersAsync();
        }

        private async void OnPlayerJoined()
        {

        }

        private void OnResourceStop(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;

            Logger.Debug($"Resource '{GetCurrentResourceName()}' Stopping.");

            Vector3 pos = Game.PlayerPed.Position;

            SetResourceKvpFloat("pos:x", pos.X);
            SetResourceKvpFloat("pos:y", pos.Y);
            SetResourceKvpFloat("pos:z", pos.Z - 1f);
            SetResourceKvpFloat("pos:h", Game.PlayerPed.Heading);

            if (Game.PlayerPed.IsInVehicle())
            {
                Vehicle vehicle = Game.PlayerPed.CurrentVehicle;

                Entity entity = vehicle.GetEntityAttachedTo();
                entity?.Delete();

                SetResourceKvpInt("vehicle:last:model", vehicle.Model.Hash);
            }

            if (_vehicle != null)
            {
                if (_vehicle.Exists())
                {
                    _vehicle.Delete();
                    Logger.Debug($"Vehicle Deleted");
                }
            }
        }

        public async Task OnRequestCharactersAsync()
        {
            User user = await EventDispatcher.Get<User>("user:active", Game.Player.ServerId);

            if (user is null)
            {
                Logger.Error($"No user was returned from the server");
                return;
            }

            Session.User = user;

            //CharacterCreatorManager characterCreatorManager = CharacterCreatorManager.GetModule();
            //characterCreatorManager.OnCreateNewCharacter(new CharacterSkin());

            RegisterCommand("paint", new Action<int, List<object>, string>((source, args, raw) =>
            {
                if (!Game.PlayerPed.IsInVehicle())
                {
                    Logger.Debug($"Player is not in a vehicle");
                    return;
                }

                if (args.Count == 0)
                {
                    Logger.Debug($"No color specified");
                    return;
                }

                string paint = args[0].ToString();

                if ("prim" != paint && "sec" != paint)
                {
                    Logger.Debug($"Invalid paint type specified: {paint}");
                    return;
                }

                Vehicle vehicle = Game.PlayerPed.CurrentVehicle;
                vehicle.Mods.InstallModKit();

                int red = 0;
                int green = 0;
                int blue = 0;

                if (int.TryParse(args[1].ToString(), out int _red))
                    red = _red;

                if (int.TryParse(args[2].ToString(), out int _green))
                    green = _green;

                if (int.TryParse(args[3].ToString(), out int _blue))
                    blue = _blue;

                Color color = Color.FromArgb(255, red, green, blue);

                if ("prim" == paint)
                {
                    Logger.Debug($"Painting vehicle primary color to {color}");
                    vehicle.Mods.CustomPrimaryColor = color;
                    return;
                }
                if ("sec" == paint)
                {
                    Logger.Debug($"Setting secondary color to {color}");
                    vehicle.Mods.CustomSecondaryColor = color;
                    return;
                }

                Logger.Debug($"Invalid paint type specified");

            }), false);


            await Game.Player.ChangeModel(PedHash.FreemodeMale01);
            Game.PlayerPed.SetDefaultVariation();

            Game.PlayerPed.Position = _position;
            Game.PlayerPed.Heading = _heading;

            // Test Code
            int vehicleHash = GetResourceKvpInt("vehicle:last:model");

            if (vehicleHash == 0)
                vehicleHash = GetHashKey("pbus2");

            _vehicle = await World.CreateVehicle(vehicleHash, Game.PlayerPed.Position, Game.PlayerPed.Heading);

            if (_vehicle != null)
            {
                if (_vehicle.Exists())
                {
                    DecorSetInt(_vehicle.Handle, "Player_Vehicle", -1);
                    Game.PlayerPed.SetIntoVehicle(_vehicle, VehicleSeat.Driver);
                    Logger.Debug($"Vehicle Created");
                }
            }

            Screen.Fading.FadeIn(0);

            if (!_playerSpawned)
                await LoadTransition.OnDownAsync();

            Game.PlayerPed.Weapons.Give(WeaponHash.AdvancedRifle, 999, true, true);
            Game.PlayerPed.Weapons.Give(WeaponHash.Firework, 999, true, true);

            Game.Player.State.Set("player:spawned", true, true);

            await _webBrowser.Load();
            
            _webBrowser.SetSkin(ScaleformUI.Scaleforms.WebBrowser.WebBrowserHandler.Skin.IFRUIT3);
            _webBrowser.SetMultiplayer(true);
            _webBrowser.SetWidescreen(GetIsWidescreen());

            int hangarCount = 1;
            while (hangarCount <= 5)
            {
                await BaseScript.Delay(0);
                _webBrowser.AddHangar(CreateHangar(hangarCount));
                hangarCount++;
            }

            Instance.AttachTickHandler(OnTestSomeShit);
        }

        public Hangar CreateHangar(int i)
        {
            Hangar hangar = new(47 + i, i);
            hangar.Position = hangar.GetDefaultHangarPosition(i);

            hangar.Cost = hangar.GetDefaultHangarCost(i);

            hangar.StyleCost = new int[9] { 0, 1000, 2000, 3000, 4000, 5000, 6000, 7000, 8000 };
            // LIGHTING_INDEXES_BY_STYLE = [0,2,4,6,8,10,12,16,20]
            hangar.LightingCost = new int[24] {
                0, 1000,
                0, 2000,
                0, 3000,
                0, 4000,
                0, 5000,
                0, 10000,
                0, 12000,
                0, 15000,
                0, 17000, 18000, 19000,
                0, 21000, 22000, 23000 };
            hangar.DecalCost = new int[9] { 0, 1000, 2000, 3000, 4000, 5000, 6000, 7000, 8000 };
            hangar.FurnatureCost = new int[3] { 0, 1000, 2000 };
            hangar.QuartersCost = new int[3] { 0, 1000, 2000 };
            hangar.WorkshopCost = 250000;

            return hangar;
        }

        int lastGlobal = 0;

        private async Task OnTestSomeShit()
        {
            if (Game.IsControlJustPressed(0, Control.MultiplayerInfo))
            {
                _toggle = !_toggle;

                _webBrowser.GotoWebPage("WWW_EYEFIND_INFO");

                await BaseScript.Delay(250);

                _webBrowser.ShowBrowser = _toggle;

                _drawBrowser = !_drawBrowser;
            }   

            if (_drawBrowser)
            {
                _webBrowser.Update();

                int asGlobal = GetGlobalActionscriptFlag(13);
                if (asGlobal > 0 && asGlobal != lastGlobal)
                {
                    Logger.Debug($"Global Actionscript Flag 13: {asGlobal}");

                    int siteId = await _webBrowser.GetSiteId();
                    if (siteId == (int)eWebsiteDynamic.FORECLOSURES_MAZE_D_BANK_COM)
                    {
                        int hangerIndex = GetValueFromBit(asGlobal, 0, 6);

                        if (hangerIndex > 0)
                        {
                            int styleIndex = GetValueFromBit(asGlobal, 6, 4);
                            int lightingIndex = GetValueFromBit(asGlobal, 10, 2);
                            // LIGHTING_INDEXES_BY_STYLE = [0,2,4,6,8,10,12,16,20]
                            // There is a better way, just lazy atm
                            if (styleIndex == 1)
                                lightingIndex += 2;
                            else if (styleIndex == 2)
                                lightingIndex += 4;
                            else if (styleIndex == 3)
                                lightingIndex += 6;
                            else if (styleIndex == 4)
                                lightingIndex += 8;
                            else if (styleIndex == 5)
                                lightingIndex += 10;
                            else if (styleIndex == 6)
                                lightingIndex += 12;
                            else if (styleIndex == 7)
                                lightingIndex += 16;
                            else if (styleIndex == 8)
                                lightingIndex += 20;

                            int decalIndex = GetValueFromBit(asGlobal, 12, 4);

                            int newDecalIndex = decalIndex;
                            // new decal index, because some R* employee wanted to ruin someones day
                            if (decalIndex == 0)
                                newDecalIndex = 6;
                            else if (decalIndex == 1)
                                newDecalIndex = 1;
                            else if (decalIndex == 2)
                                newDecalIndex = 7;
                            else if (decalIndex == 3)
                                newDecalIndex = 8;
                            else if (decalIndex == 4)
                                newDecalIndex = 0;
                            else if (decalIndex == 5)
                                newDecalIndex = 5;
                            else if (decalIndex == 6)
                                newDecalIndex = 3;
                            else if (decalIndex == 7)
                                newDecalIndex = 2;
                            else if (decalIndex == 8)
                                newDecalIndex = 4;

                            int furnatureIndex = GetValueFromBit(asGlobal, 16, 2);
                            int quartersIndex = GetValueFromBit(asGlobal, 18, 2);
                            int workshopIndex = GetValueFromBit(asGlobal, 20, 1);

                            Hangar hangar = _webBrowser.Hangers[hangerIndex + 1];

                            Logger.Info($"HANGER PURCHASE");
                            Logger.Debug($"HANGAR INDEX: {hangerIndex + 1} => {hangar.Cost}");
                            Logger.Debug($"STYLE: {styleIndex} => {hangar.StyleCost[styleIndex]}");
                            Logger.Debug($"LIGHTING: {lightingIndex} => {hangar.LightingCost[lightingIndex]}");
                            Logger.Debug($"DECAL: {newDecalIndex} => {hangar.DecalCost[newDecalIndex]}");
                            Logger.Debug($"FURNITURE: {furnatureIndex} => {hangar.FurnatureCost[furnatureIndex]}");
                            Logger.Debug($"QUARTERS: {quartersIndex} => {hangar.QuartersCost[quartersIndex]}");
                            Logger.Debug($"WORKSHOP: {workshopIndex} => {hangar.WorkshopCost}");
                        }
                    }

                    lastGlobal = asGlobal;
                    ResetGlobalActionscriptFlag(13);
                }
            }
        }

        int GetValueFromBit(int bit, int p1, int p2)
        {
            int i = ((1 << p2) - 1);
            i = (i << p1);
            return ((bit & i) >> p1);
        }
    }
}
