using CitizenFX.Core.UI;
using Curiosity.Framework.Client.Extensions;
using Curiosity.Framework.Client.Managers.Events;
using Curiosity.Framework.Client.Utils;
using Curiosity.Framework.Shared.SerializedModels;
using FxEvents;
using FxEvents.Shared.TypeExtensions;
using ScaleformUI;
using ScaleformUI.Scaleforms.WebBrowser;
using ScaleformUI.Scaleforms.WebBrowser.Data;
using ScaleformUI.Scaleforms.WebBrowser.Model;
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

        RelationshipGroup RelationshipGang1;
        RelationshipGroup RelationshipGang2;
        RelationshipGroup RelationshipPlayer;

        int group = 0;

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

            Game.PlayerPed.Weapons.Give(WeaponHash.Firework, 999, false, true);
            Game.PlayerPed.Weapons.Give(WeaponHash.AdvancedRifle, 999, true, true);

            Game.Player.State.Set("player:spawned", true, true);

            await _webBrowser.Load();

            _webBrowser.SetSkin(ScaleformUI.Scaleforms.WebBrowser.WebBrowserHandler.Skin.IFRUIT2);
            _webBrowser.SetMultiplayer(true);
            _webBrowser.SetWidescreen(GetIsWidescreen());

            int hangarCount = 1;
            while (hangarCount <= 5)
            {
                await BaseScript.Delay(0);
                _webBrowser.AddHangar(CreateHangar(hangarCount));
                hangarCount++;
            }

            int bunkerCount = 1;
            while (bunkerCount <= 11)
            {
                await BaseScript.Delay(0);
                _webBrowser.AddBunker(CreateBunker(bunkerCount));
                bunkerCount++;
            }

            Instance.AttachTickHandler(OnSomeOtherShit);
            Instance.AttachTickHandler(OnTestSomeShit);

            RelationshipGang1 = (uint)GetHashKey("GANG_1");
            RelationshipGang2 = (uint)GetHashKey("GANG_2");
            RelationshipPlayer = (uint)GetHashKey("PLAYER");

            RelationshipGang1.SetRelationshipBetweenGroups(RelationshipGang2, Relationship.Hate, true);
            
            RelationshipPlayer.SetRelationshipBetweenGroups(RelationshipGang1, Relationship.Hate, true);
            RelationshipPlayer.SetRelationshipBetweenGroups(RelationshipGang2, Relationship.Hate, true);

            Game.PlayerPed.RelationshipGroup.Remove();
            Game.PlayerPed.RelationshipGroup = RelationshipPlayer;

            NetworkSetFriendlyFireOption(true);
            SetCanAttackFriendly(Game.PlayerPed.Handle, true, false);

            SetPlayerVehicleDefenseModifier(Game.Player.Handle, 1f);
            DisablePlayerVehicleRewards(Game.Player.Handle);

            DisableIdleCamera(true);
        }

        public Hangar CreateHangar(int i)
        {
            Hangar hangar = new(47 + i, i + 200);

            hangar.Id = hangar.GetDefaultId(i);

            hangar.Position = hangar.GetDefaultPosition(hangar.Id);

            hangar.BaseCost = hangar.GetDefaultCost(i);

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

        public Bunker CreateBunker(int i)
        {
            Bunker bunker = new Bunker(69 + i, i + 300);
            bunker.Id = bunker.GetDefaultId(i);
            bunker.Position = bunker.GetDefaultPosition(bunker.Id);
            bunker.BaseCost = 1000000;
            bunker.StyleCost = new int[3] { 0, 1000, 2000 };
            bunker.QuartersCost = 3000;
            bunker.FiringRangeCost = new int[3] { 1000, 2000, 3000 };
            bunker.GunLockerCost = 4000;
            bunker.TransportationCost1 = 10000;
            bunker.TransportationCost2 = 20000;
            return bunker;
        }

        int lastGlobal = 0;

        private async Task OnSomeOtherShit()
        {
            foreach(Player player in PluginManager.PlayerList)
            {
                // if (player == Game.Player) continue;

                float x = 0;
                float y = 0;
                Vector3 pos = player.Character.Position;

                SetDrawOrigin(pos.X, pos.Y, pos.Z, 0);

                if (GetScreenCoordFromWorldCoord(pos.X, pos.Y, pos.Z, ref x, ref y))
                {
                    UIResText uIResText = new UIResText($"{player.Character.RelationshipGroup.NativeValue}", new PointF(x, y), .25f);
                    uIResText.Draw();
                }

                ClearDrawOrigin();
            }
        }

        bool isPurchasing = false;

        private async Task OnTestSomeShit()
        {
            if (Game.IsControlJustPressed(0, Control.Context))
            {
                N_0x061cb768363d6424(Game.PlayerPed.Handle, false); // SET_ALLOW_LOCKON_TO_PED_IF_FRIENDLY
                
                ++group;
                if (group > 2)
                    group = 0;

                Game.PlayerPed.RelationshipGroup.Remove();
                int pedHandle = Game.PlayerPed.Handle;

                if (group == 1)
                {
                    Game.PlayerPed.RelationshipGroup = RelationshipGang1;
                    // NetworkSetFriendlyFireOption(false);
                    SetCanAttackFriendly(pedHandle, false, false);
                    N_0x0f62619393661d6e(pedHandle, 1, 0); // SET_PED_TREATED_AS_FRIENDLY
                    Screen.ShowNotification($"Joined Group 1", true);
                }
                else if (group == 2)
                {
                    Game.PlayerPed.RelationshipGroup = RelationshipGang2;
                    // NetworkSetFriendlyFireOption(false);
                    SetCanAttackFriendly(pedHandle, false, false);
                    N_0x0f62619393661d6e(pedHandle, 1, 0); // SET_PED_TREATED_AS_FRIENDLY
                    Screen.ShowNotification($"Joined Group 2", true);
                }
                else if (group == 0)
                {
                    Game.PlayerPed.RelationshipGroup = RelationshipPlayer;
                    // NetworkSetFriendlyFireOption(true);
                    SetCanAttackFriendly(pedHandle, true, false);
                    N_0x0f62619393661d6e(pedHandle, 0, 0); // SET_PED_TREATED_AS_FRIENDLY
                    Screen.ShowNotification($"Left Group", true);
                }
            }
            
            if (Game.IsControlJustPressed(0, Control.MultiplayerInfo))
            {
                _toggle = !_toggle;

                _webBrowser.GotoWebPage("WWW_EYEFIND_INFO_S_PAGE1");

                _webBrowser.ShowBrowser = _toggle;

                _drawBrowser = !_drawBrowser;
            }

            if (_drawBrowser)
            {
                _webBrowser.Update();

                
                int bitwiseValue = GetGlobalActionscriptFlag(13);

                if (bitwiseValue > 0 && bitwiseValue != lastGlobal)
                {
                    Logger.Debug($"Global Actionscript Flag 13: {bitwiseValue}");

                    int siteId = await _webBrowser.GetSiteId();

                    if (siteId == (int)eWebsiteDynamic.FORECLOSURES_MAZE_D_BANK_COM && isPurchasing)
                    {
                        // Bitwise 0, 6 contains the index of the item

                        int pageId = await _webBrowser.GetPageId();
                        int itemIndex = Tools.GetValueFromBitwiseValue(bitwiseValue, 0, 6);
                        Logger.Debug($"Page ID: {pageId}, Item Index: {itemIndex}");

                        //Hangar hangar = _webBrowser.Hangars[hangarKey];

                        //int hangarIndex = 0;
                        //int styleIndex = 0;
                        //int lightingIndex = 0;
                        //int decalIndex = 0;
                        //int furnatureIndex = 0;
                        //int quartersIndex = 0;
                        //bool purchasedWorkshop = false;

                        //hangar.GetPurchasedData(bitwiseValue, ref hangarIndex, ref styleIndex, ref lightingIndex, ref decalIndex, ref furnatureIndex, ref quartersIndex, ref purchasedWorkshop);

                        //int totalValue = hangar.GetTotalValueOfPurchases(bitwiseValue);

                        //int workshopValue = purchasedWorkshop ? Tools.GetBestPrice(hangar.WorkshopCost, hangar.WorkshopSaleCost) : 0;

                        //Logger.Info($"HANGER PURCHASE");
                        //Logger.Debug($"BITWISE: {bitwiseValue}");
                        //Logger.Debug($"HANGAR INDEX: {hangarIndex}[{hangarKey}] => {Tools.GetBestPrice(hangar.BaseCost, hangar.BaseSaleCost)}");
                        //Logger.Debug($"STYLE: {styleIndex} => {Tools.GetBestPrice(hangar.StyleCost[styleIndex], hangar.StyleSaleCost[styleIndex])}");
                        //Logger.Debug($"LIGHTING: {lightingIndex} => {Tools.GetBestPrice(hangar.LightingCost[lightingIndex], hangar.LightingSaleCost[lightingIndex])}");
                        //Logger.Debug($"DECAL: {decalIndex} => {Tools.GetBestPrice(hangar.DecalCost[decalIndex], hangar.DecalSaleCost[decalIndex])}");
                        //Logger.Debug($"FURNITURE: {furnatureIndex} => {Tools.GetBestPrice(hangar.FurnatureCost[furnatureIndex], hangar.FurnatureSaleCost[furnatureIndex])}");
                        //Logger.Debug($"QUARTERS: {quartersIndex} => {Tools.GetBestPrice(hangar.QuartersCost[quartersIndex], hangar.QuartersSaleCost[quartersIndex])}");
                        //Logger.Debug($"WORKSHOP: {purchasedWorkshop} => {workshopValue}");
                        //Logger.Debug($"TOTAL: {totalValue}");

                        isPurchasing = false;
                    }

                    isPurchasing = (bitwiseValue & 2097152) != 0;

                    lastGlobal = bitwiseValue;
                    ResetGlobalActionscriptFlag(13);
                }
            }
        }
    }
}
