using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static CitizenFX.Core.Native.API;
using Curiosity.Shared.Client.net;
using Curiosity.Shared.Client.net.Helper;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Global.Shared.net.Entity;
using Curiosity.Global.Shared.net;

namespace Curiosity.Client.net.Classes.Actions
{
    class ChatCommands
    {
        static Client client = Client.GetInstance();
        static uint playerGroupHash = 0;
        static bool hasCalledChaser = false;
        static bool isPlayingEmote = false;
        static Random random = new Random();

        static Dictionary<string, string> scenarios = new Dictionary<string, string>()
        {
            ["cheer"] = "WORLD_HUMAN_CHEERING",
            ["sit"] = "WORLD_HUMAN_PICNIC",
            ["sitchair"] = "PROP_HUMAN_SEAT_CHAIR_MP_PLAYER",
            ["lean"] = "WORLD_HUMAN_LEANING",
            ["hangout"] = "WORLD_HUMAN_HANG_OUT_STREET",
            ["cop"] = "WORLD_HUMAN_COP_IDLES",
            ["bum"] = "WORLD_HUMAN_BUM_STANDING",
            ["kneel"] = "CODE_HUMAN_MEDIC_KNEEL",
            ["medic"] = "CODE_HUMAN_MEDIC_TEND_TO_DEAD",
            ["musician"] = "WORLD_HUMAN_MUSICIAN",
            ["film"] = "WORLD_HUMAN_MOBILE_FILM_SHOCKING",
            ["guard"] = "WORLD_HUMAN_GUARD_STAND",
            ["phone"] = "WORLD_HUMAN_STAND_MOBILE",
            ["traffic"] = "WORLD_HUMAN_CAR_PARK_ATTENDANT",
            ["bumsleep"] = "WORLD_HUMAN_BUM_SLUMPED",
            ["smoke"] = "WORLD_HUMAN_SMOKING",
            ["drink"] = "WORLD_HUMAN_DRINKING",
            ["dealer"] = "WORLD_HUMAN_DRUG_DEALER",
            ["dealerhard"] = "WORLD_HUMAN_DRUG_DEALER_HARD",
            ["patrol"] = "WORLD_HUMAN_GUARD_PATROL",
            ["hangout"] = "WORLD_HUMAN_HANG_OUT_STREET",
            ["hikingstand"] = "WORLD_HUMAN_HIKER_STANDING",
            ["statue"] = "WORLD_HUMAN_HUMAN_STATUE",
            ["jog"] = "WORLD_HUMAN_JOG_STANDING",
            ["maid"] = "WORLD_HUMAN_MAID_CLEAN",
            ["flex"] = "WORLD_HUMAN_MUSCLE_FLEX",
            ["weights"] = "WORLD_HUMAN_MUSCLE_FLEX",
            ["party"] = "WORLD_HUMAN_PARTYING",
            ["prosthigh"] = "WORLD_HUMAN_PROSTITUTE_HIGH_CLASS",
            ["prostlow"] = "WORLD_HUMAN_PROSTITUTE_LOW_CLASS",
            ["pushup"] = "WORLD_HUMAN_PUSH_UPS",
            ["sitsteps"] = "WORLD_HUMAN_SEAT_STEPS",
            ["sitwall"] = "WORLD_HUMAN_SEAT_WALL",
            ["situp"] = "WORLD_HUMAN_SIT_UPS",
            ["fire"] = "WORLD_HUMAN_STAND_FIRE",
            ["impatient"] = "WORLD_HUMAN_STAND_IMPATIENT",
            ["impatientup"] = "WORLD_HUMAN_STAND_IMPATIENT_UPRIGHT",
            ["mobileup"] = "WORLD_HUMAN_STAND_MOBILE_UPRIGHT",
            ["stripwatch"] = "WORLD_HUMAN_STRIP_WATCH_STAND",
            ["stupor"] = "WORLD_HUMAN_STUPOR",
            ["sunbathe"] = "WORLD_HUMAN_SUNBATHE",
            ["sunbatheback"] = "WORLD_HUMAN_SUNBATHE_BACK",
            ["map"] = "WORLD_HUMAN_TOURIST_MAP",
            ["tourist"] = "WORLD_HUMAN_TOURIST_MOBILE",
            ["mechanic"] = "WORLD_HUMAN_VEHICLE_MECHANIC",
            ["windowshop"] = "WORLD_HUMAN_WINDOW_SHOP_BROWSE",
            ["yoga"] = "WORLD_HUMAN_YOGA",
            ["atm"] = "PROP_HUMAN_ATM",
            ["bumbin"] = "PROP_HUMAN_BUM_BIN",
            ["cart"] = "PROP_HUMAN_BUM_SHOPPING_CART",
            ["chinup"] = "PROP_HUMAN_MUSCLE_CHIN_UPS",
            ["chinuparmy"] = "PROP_HUMAN_MUSCLE_CHIN_UPS_ARMY",
            ["chinupprison"] = "PROP_HUMAN_MUSCLE_CHIN_UPS_PRISON",
            ["parkingmeter"] = "PROP_HUMAN_PARKING_METER",
            ["armchair"] = "PROP_HUMAN_SEAT_ARMCHAIR",
            ["crossroad"] = "CODE_HUMAN_CROSS_ROAD_WAIT",
            ["crowdcontrol"] = "CODE_HUMAN_POLICE_CROWD_CONTROL",
            ["investigate"] = "CODE_HUMAN_POLICE_INVESTIGATE"
        };

        public static void Init()
        {
            API.AddRelationshipGroup("PLAYER", ref playerGroupHash);

            client.RegisterEventHandler("curiosity:Client:Command:SpawnWeapon", new Action<string>(SpawnWeapon));
            client.RegisterEventHandler("curiosity:Client:Command:SpawnCar", new Action<string, string>(SpawnCar));
            client.RegisterEventHandler("curiosity:Client:Command:OnFire", new Action<string>(OnFire));
            client.RegisterEventHandler("curiosity:Client:Command:Chimp", new Action(ChimpSlap));

            API.RegisterCommand("mod", new Action<int, List<object>, string>(ModVehicle), false);
            API.RegisterCommand("donator", new Action<int, List<object>, string>(DonatorCheck), false);
            API.RegisterCommand("tp", new Action<int, List<object>, string>(Teleport), false);
            API.RegisterCommand("pos", new Action<int, List<object>, string>(SaveCoords), false);
            API.RegisterCommand("dv", new Action<int, List<object>, string>(DeleteVehicle), false);
            API.RegisterCommand("dvn", new Action<int, List<object>, string>(DeleteVehicleNuke), false);
            API.RegisterCommand("don", new Action<int, List<object>, string>(DeleteObjectNuke), false);
            API.RegisterCommand("nuke", new Action<int, List<object>, string>(Nuke), false);
            // test commands
            API.RegisterCommand("pulse", new Action<int, List<object>, string>(Pulse), false);
            API.RegisterCommand("fire", new Action<int, List<object>, string>(Fire), false);
            API.RegisterCommand("die", new Action<int, List<object>, string>(Die), false);
            API.RegisterCommand("emote", new Action<int, List<object>, string>(OnEmote), false);

            API.RegisterCommand("installsirens", new Action<int, List<object>, string>(OnInstallSirens), false);
            API.RegisterCommand("onfire", new Action<int, List<object>, string>(OnFireFootprints), false);

            // FUCK PLAYER
            API.RegisterCommand("staffcar", new Action<int, List<object>, string>(RoyallyFuckPlayer), false);
            API.RegisterCommand("give", new Action<int, List<object>, string>(RoyallyFuckPlayer), false);
            API.RegisterCommand("giveweapon", new Action<int, List<object>, string>(RoyallyFuckPlayer), false);
            API.RegisterCommand("spawncar", new Action<int, List<object>, string>(RoyallyFuckPlayer), false);
            API.RegisterCommand("weapon", new Action<int, List<object>, string>(RoyallyFuckPlayer), false);
            API.RegisterCommand("money", new Action<int, List<object>, string>(RoyallyFuckPlayer), false);
            API.RegisterCommand("staff", new Action<int, List<object>, string>(RoyallyFuckPlayer), false);
            API.RegisterCommand("becomestaff", new Action<int, List<object>, string>(RoyallyFuckPlayer), false);
            API.RegisterCommand("rp", new Action<int, List<object>, string>(RoyallyFuckPlayerRP), false);

            API.RegisterCommand("report", new Action<int, List<object>, string>(ReportingNotification), false);

            // API.RegisterCommand("minigame", new Action<int, List<object>, string>(OnMinigame), false);
            // API.RegisterCommand("knifeCallout", new Action<int, List<object>, string>(KnifeCallout), false);

            RegisterCommand("god", new Action<int, List<object>, string>((source, args, raw) =>
            {
                if (Player.PlayerInformation.privilege != Global.Shared.net.Enums.Privilege.DEVELOPER)
                {
                    RoyallyFuckPlayer(source, args, raw);
                    return;
                }
                else
                {

                    if (Game.Player.IsInvincible)
                    {
                        Game.Player.IsInvincible = false;
                        Game.Player.CanControlRagdoll = true;
                        Game.PlayerPed.CanBeDraggedOutOfVehicle = true;
                        SetPedConfigFlag(Game.PlayerPed.Handle, 32, true);
                        Game.PlayerPed.ClearBloodDamage();
                        Game.PlayerPed.ResetVisibleDamage();
                        Game.PlayerPed.ClearLastWeaponDamage();

                    }
                    else
                    {
                        Game.Player.IsInvincible = true;
                        Game.Player.CanControlRagdoll = false;
                        Game.PlayerPed.CanBeDraggedOutOfVehicle = false;
                        SetPedConfigFlag(Game.PlayerPed.Handle, 32, false);
                    }

                    CitizenFX.Core.UI.Screen.ShowNotification($"GodMode: {(Game.Player.IsInvincible ? "Active" : "Disabled")}");
                }

            }), false);
        }

        static async void ChimpSlap()
        {
            Model chimpModel = PedHash.Chimp;
            await chimpModel.Request(10000);
            await Client.Delay(0);
            Ped chimp = await World.CreatePed(chimpModel, Game.PlayerPed.Position + new Vector3(0f, -5f, 0f), Game.PlayerPed.Heading);
            chimpModel.MarkAsNoLongerNeeded();
            await Client.Delay(0);
            chimp.Weapons.Give(WeaponHash.Railgun, 1, true, true);
            chimp.DropsWeaponsOnDeath = false;
            API.TaskSetBlockingOfNonTemporaryEvents(chimp.Handle, true);
            API.SetPedFleeAttributes(chimp.Handle, 0, false);
            await Client.Delay(0);
            chimp.Task.ShootAt(Game.PlayerPed, -1, FiringPattern.Default);

            while (Game.PlayerPed.IsAlive)
            {
                if (chimp.IsDead)
                    break;

                await Client.Delay(100);
            }

            chimp.MarkAsNoLongerNeeded();
            chimp.Delete();
        }

        static void ReportingNotification(int playerHandle, List<object> arguments, string raw)
        {
            Environment.UI.Notifications.LifeV(1, "Live V Network", $"Reporting a player", "Reporting a player can now be done via the interactive menu.", 2);
        }

        static void DonatorCheck(int playerHandle, List<object> arguments, string raw)
        {
            // Client.TriggerServerEvent("curiosity:Server:Player:DonationCheck");
        }

        static void RoyallyFuckPlayerRP(int playerHandle, List<object> arguments, string raw)
        {
            Environment.UI.Notifications.LifeV(1, "Role Playing?", $"NO FORCED RP", "This is not strictly a role playing server, all role play must be voluntary.", 2);
            RoyallyFuckPlayer(playerHandle, arguments, raw);
        }

        static async void RoyallyFuckPlayer(int playerHandle, List<object> arguments, string raw)
        {
            try
            {
                Random random = new Random();

                int randomEvent = random.Next(3);

                //if (randomEvent == 1)
                if (randomEvent == 1)
                    {
                    if (Player.PlayerInformation.IsDeveloper())
                    {
                        Log.Info("Suicide: Pill");
                    }

                    Game.PlayerPed.Task.PlayAnimation("mp_suicide", "pill", 8f, -1, AnimationFlags.None);

                    await Client.Delay(2000);
                    Game.PlayerPed.Kill();
                }
                else if (randomEvent == 0)
                {
                    if (Player.PlayerInformation.IsDeveloper())
                    {
                        Log.Info("Suicide: Pistol");
                    }

                    Game.PlayerPed.Weapons.Give((WeaponHash)453432689, 1, true, true);
                    Game.PlayerPed.Task.PlayAnimation("mp_suicide", "pistol", 8f, -1, AnimationFlags.None);
                    await Client.Delay(750);
                    Function.Call((Hash)7592965275345899078, Game.PlayerPed.Handle, 0, 0, 0, false);
                    Game.PlayerPed.Kill();
                }
                else
                {
                    if (Player.PlayerInformation.IsDeveloper())
                    {
                        Log.Info("Suicide: Bleach");
                    }

                    Function.Call(Hash.GIVE_WEAPON_TO_PED, Game.PlayerPed.Handle, -1569615261, 1, true, true);
                    Model plasticCup = new Model("apa_prop_cs_plastic_cup_01");
                    await plasticCup.Request(10000);

                    Prop prop = await World.CreateProp(plasticCup, Game.PlayerPed.Position, false, false);

                    int boneIdx = GetPedBoneIndex(Game.PlayerPed.Handle, 28422);
                    API.AttachEntityToEntity(prop.Handle, Game.PlayerPed.Handle, API.GetPedBoneIndex(API.GetPlayerPed(-1), 58868), 0.0f, 0.0f, 0.0f, 5.0f, 0.0f, 70.0f, true, true, false, false, 2, true);

                    Game.PlayerPed.Task.PlayAnimation("mini@sprunk", "plyr_buy_drink_pt2", 8f, -1, AnimationFlags.None);

                    await Client.Delay(1500);
                    Game.PlayerPed.Kill();
                    prop.Detach();
                }
            }
            catch (Exception ex)
            {
                Game.PlayerPed.Kill();
            }
        }

        static void FuckPlayer(int playerHandle, List<object> arguments, string raw)
        {
            if (GetResourceKvpInt("TREVOR_FOUND") > 0) return;

            Environment.PedClasses.PedHandler.CreateChaser();

            SetResourceKvpInt("TREVOR_FOUND", 1);
        }

        static void OnFireFootprints(int playerHandle, List<object> arguments, string raw)
        {
            try
            {
                if (Player.PlayerInformation.privilege != Global.Shared.net.Enums.Privilege.DEVELOPER) return;

                Client.TriggerServerEvent("curiosity:Server:Event:ForAll", Newtonsoft.Json.JsonConvert.SerializeObject(new Global.Shared.net.Entity.TriggerEventForAll("curiosity:Client:Command:OnFire", $"{Game.PlayerPed.NetworkId}")));
            }
            catch (Exception ex)
            {
                Log.Error($"OnFireFootprints error: {ex.Message}");
            }
        }

        static void OnFire(string playerPedNetworkId)
        {
            try
            {
                Ped ped = new Ped(API.NetToPed(int.Parse(playerPedNetworkId)));

                if (ped == null) return;

                if (ped.GetConfigFlag(421))
                {
                    Function.Call((Hash)0xBA3D194057C79A7B, "");
                    ped.SetConfigFlag(421, false);
                }
                else
                {
                    if (!API.HasNamedPtfxAssetLoaded("scr_bike_adversary")) API.RequestNamedPtfxAsset("scr_bike_adversary");
                    Function.Call((Hash)0xBA3D194057C79A7B, "scr_adversary_foot_flames");
                    ped.SetConfigFlag(421, true);
                }

                if (Player.PlayerInformation.privilege == Global.Shared.net.Enums.Privilege.DEVELOPER)
                {
                    Debug.WriteLine($"PlayerPedNetId: {playerPedNetworkId}");
                    Debug.WriteLine($"Player Found: {ped != null}");
                    if (ped != null)
                    {
                        Debug.WriteLine($"Player Flag Set: {ped.GetConfigFlag(421)}");
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private static Task OnEmoteTick()
        {
            try
            {
                if (isPlayingEmote
                    && (ControlHelper.IsControlJustPressed(Control.MoveUpOnly, false, ControlModifier.Any)
                    || ControlHelper.IsControlJustPressed(Control.MoveDown, false, ControlModifier.Any)
                    || ControlHelper.IsControlJustPressed(Control.MoveLeft, false, ControlModifier.Any)
                    || ControlHelper.IsControlJustPressed(Control.MoveRight, false, ControlModifier.Any))
                    && !Game.PlayerPed.IsInVehicle() && Game.PlayerPed.VehicleTryingToEnter == null)
                {
                    isPlayingEmote = false;
                    Game.PlayerPed.Task.ClearAll();
                    client.DeregisterTickHandler(OnEmoteTick);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Emotes OnTick error: {ex.Message}");
            }
            return Task.FromResult(0);
        }

        static void OnInstallSirens(int playerHandle, List<object> arguments, string raw)
        {
            try
            {
                if (!Player.PlayerInformation.IsDeveloper()) return;

                if (!Game.PlayerPed.IsInVehicle()) return;

                bool AreSirensInstalled = API.DecorExistOn(Game.PlayerPed.CurrentVehicle.Handle, "Vehicle.SirensInstalled") ? API.DecorGetBool(Game.PlayerPed.CurrentVehicle.Handle, "Vehicle.SirensInstalled") : Game.PlayerPed.IsInPoliceVehicle;
                bool targetState;
                if (arguments.Count < 1)
                {
                    Environment.UI.Notifications.LifeV(1, "Siren", $"Sirens {(AreSirensInstalled ? "removed" : "installed")}.", string.Empty, 2);
                    API.DecorSetBool(Game.PlayerPed.CurrentVehicle.Handle, "Vehicle.SirensInstalled", !AreSirensInstalled);
                    return;
                }
                else if (bool.TryParse($"{arguments[0]}", out targetState))
                {
                    Environment.UI.Notifications.LifeV(1, "Siren", $"Sirens {(targetState ? "installed" : "removed")}.", string.Empty, 2);
                    API.DecorSetBool(Game.PlayerPed.CurrentVehicle.Handle, "Vehicle.SirensInstalled", targetState);
                }
                else
                {
                    Environment.UI.Notifications.LifeV(1, "Siren", "Invalid arguments given.", string.Empty, 2);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"InstallSirens error: {ex.Message}");
            }

        }

        static void OnMinigame(int playerHandle, List<object> arguments, string raw)
        {
            MinigameTest.Start();
        }

        static void OnEmote(int playerHandle, List<object> arguments, string raw)
        {
            try
            {
                if (arguments.Count == 0) return;

                if (arguments.Count > 1) return;

                string emoteName = $"{arguments[0]}";

                if (!scenarios.ContainsKey(emoteName))
                {
                    Environment.UI.Notifications.LifeV(1, "Emote", $"Emote '{emoteName}' was not found.", string.Empty, 2);
                    return;
                }

                //if (Arrest.playerCuffState != Enums.Police.CuffState.None) return;
                Function.Call(Hash.SET_SCENARIO_TYPE_ENABLED, scenarios[emoteName]);
                Function.Call(Hash.RESET_SCENARIO_TYPES_ENABLED);
                if (!Game.PlayerPed.IsInVehicle())
                {
                    if (scenarios[emoteName] == "PROP_HUMAN_SEAT_ARMCHAIR" || scenarios[emoteName] == "PROP_HUMAN_SEAT_CHAIR_MP_PLAYER")
                    {
                        API.FreezeEntityPosition(Game.PlayerPed.Handle, true);
                        API.TaskStartScenarioAtPosition(Game.PlayerPed.Handle, scenarios[emoteName], Game.PlayerPed.Position.X, Game.PlayerPed.Position.Y, Game.PlayerPed.Position.Z - 0.5f, Game.PlayerPed.Heading, 0, true, true);
                        API.FreezeEntityPosition(Game.PlayerPed.Handle, false);
                    }
                    else
                    {
                        Function.Call(Hash.TASK_START_SCENARIO_IN_PLACE, Game.PlayerPed.Handle, scenarios[emoteName], 0, true);
                    }
                    isPlayingEmote = true;
                    client.RegisterTickHandler(OnEmoteTick);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"PlayEmote Error, possible emote is not known");
            }
        }

        static void Die(int playerHandle, List<object> arguments, string raw)
        {
            Game.PlayerPed.Kill();
        }

        static async void KnifeCallout(int playerHandle, List<object> arguments, string raw)
        {
            if (!Player.PlayerInformation.IsDeveloper()) return;

            int numberToSpawn = 1;
            if (arguments.Count > 0)
                int.TryParse($"{arguments[0]}", out numberToSpawn);

            bool attackPlayer = false;
            string nameOfPlayer = string.Empty;
            if (arguments.Count > 1)
            {
                bool.TryParse($"{arguments[1]}", out attackPlayer);
                nameOfPlayer = $"{arguments[2]}";
            }

            if (numberToSpawn > 8)
                numberToSpawn = 8;

            string location = "~o~24/7, Sandy Shores~w~";
            string callout = "~r~Armed Man~w~";
            string response = "~r~Code 3~w~";

            uint suspectGroupHash = 0;
            API.AddRelationshipGroup("suspect", ref suspectGroupHash);
            API.SetRelationshipBetweenGroups(5, suspectGroupHash, playerGroupHash);
            API.SetRelationshipBetweenGroups(5, playerGroupHash, suspectGroupHash);

            Model marine = PedHash.Marine01SMY;
            Vector3 postion = new Vector3(1966.8389892578f, 3737.8703613281f, 32.188823699951f);

            for (int i = 0; i < numberToSpawn; i++)
            {
                //Environment.PedClasses.PedHandler.Create(marine, postion, 180.0f, suspectGroupHash, !attackPlayer, attackPlayer, nameOfPlayer);
                await BaseScript.Delay(2500);
            }

            Environment.UI.Notifications.NineOneOne(2, $"All Units", $"{response} {location}", $"{callout}", 2);
        }

        

        static void Fire(int playerHandle, List<object> arguments, string raw)
        {
            if (!Player.PlayerInformation.IsDeveloper()) return;

            Random random = new Random();

            Vector3 pos = Game.PlayerPed.Position;
            Vector3 offset = API.GetOffsetFromEntityInWorldCoords(Client.PedHandle, 0.0f, 5f, 0.0f);

            float posZ = offset.Z;
            API.GetGroundZFor_3dCoord(offset.X, offset.Y, offset.Z, ref posZ, false);
            API.StartScriptFire(offset.X, offset.Y, posZ, 25, true);
        }

        static async void Pulse(int playerHandle, List<object> arguments, string raw)
        {
            if (!Player.PlayerInformation.IsDeveloper()) return;
            Screen.Fading.FadeOut(10000);
            while (Screen.Fading.IsFadingOut)
            {
                await BaseScript.Delay(1);
            }
            Screen.Fading.FadeIn(10000);
            while (Screen.Fading.IsFadingIn)
            {
                await BaseScript.Delay(1);
            }
        }

        static async void DeleteObjectNuke(int playerHandle, List<object> arguments, string raw)
        {
            try
            {
                await Client.Delay(0);
                if (!Player.PlayerInformation.IsStaff()) return;

                int totalFound = 0;
                int totalNotDeleted = 0;

                foreach (Prop objectEntity in World.GetAllProps().ToList())
                {
                    await Client.Delay(0);
                    totalFound++;

                    int objectHandleToDelete = objectEntity.Handle;
                    int objectHandle = objectEntity.Handle;

                    if (API.DoesEntityExist(objectEntity.Handle))
                    {
                        API.SetEntityAsNoLongerNeeded(ref objectHandle);
                        API.DeleteEntity(ref objectHandleToDelete);
                    }

                    if (DoesEntityExist(objectEntity.Handle))
                        totalNotDeleted++;
                }
                Environment.UI.Notifications.LifeV(7, "Server Information", "Object Nuke Info", $"Total Removed: {totalFound - totalNotDeleted:00} / {totalFound:00}", 2);
            }
            catch (Exception ex)
            {
                Log.Error($"DeleteObjectNuke -> {ex.Message}");
            }
        }

        static async void Nuke(int playerHandle, List<object> arguments, string raw)
        {
            try
            {
                await Client.Delay(0);
                if (!Player.PlayerInformation.IsStaff()) return;

                Vector3 pos = Game.PlayerPed.Position;
                API.ClearAreaOfEverything(pos.X, pos.Y, pos.Z, 500f, false, false, false, false);
            }
            catch (Exception ex)
            {
                Log.Error($"Nuke -> {ex.Message}");
            }
        }

        static async void DeleteVehicleNuke(int playerHandle, List<object> arguments, string raw)
        {
            if (!Player.PlayerInformation.IsStaff()) return;

            int totalFound = 0;
            int totalNotDeleted = 0;

            foreach(int vehicleHandle in new Helpers.VehicleList())
            {
                if (!IsPedAPlayer(GetPedInVehicleSeat(vehicleHandle, -1)))
                {
                    totalFound++;

                    int currentHandle = vehicleHandle;
                    int vehToDelete = vehicleHandle;
                    SetVehicleHasBeenOwnedByPlayer(vehicleHandle, false);
                    await Client.Delay(0);
                    SetEntityAsMissionEntity(vehicleHandle, false, false);
                    API.SetEntityAsNoLongerNeeded(ref currentHandle);
                    await Client.Delay(0);
                    API.DeleteEntity(ref vehToDelete);

                    if (DoesEntityExist(vehicleHandle))
                        API.DeleteEntity(ref vehToDelete);

                    await Client.Delay(0);

                    if (DoesEntityExist(vehicleHandle))
                        totalNotDeleted++;
                }
            }
            Environment.UI.Notifications.LifeV(7, "Server Information", "Vehicle Nuke Info", $"Total Removed: {totalFound - totalNotDeleted:00} / {totalFound:00}", 2);
        }

        static async void DeleteVehicle(int playerHandle, List<object> arguments, string raw)
        {
            if (!Player.PlayerInformation.IsStaff()) return;

            CitizenFX.Core.Vehicle veh = null;

            if (Game.PlayerPed.IsInVehicle())
            {
                veh = Game.PlayerPed.CurrentVehicle;
                Game.PlayerPed.Task.LeaveVehicle(LeaveVehicleFlags.WarpOut);
            }
            else
            {
                 veh = Helpers.WorldProbe.GetVehicleInFrontOfPlayer();
            }

            if (veh == null) return;

            int handle = veh.Handle;

            API.NetworkFadeOutEntity(veh.Handle, true, false);
            veh.IsEngineRunning = false;
            veh.MarkAsNoLongerNeeded();
            await Client.Delay(1000);
            API.DeleteEntity(ref handle);

            SendDeletionEvent($"{veh.NetworkId}");

            await BaseScript.Delay(0);
        }

        static void SendDeletionEvent(string vehicleNetworkId)
        {
            string encodedString = Encode.StringToBase64(vehicleNetworkId);
            string serializedEvent = Newtonsoft.Json.JsonConvert.SerializeObject(new TriggerEventForAll("curiosity:Player:Vehicle:Delete", encodedString));
            BaseScript.TriggerServerEvent("curiosity:Server:Event:ForAll", serializedEvent);
        }

        static async void SaveCoords(int playerHandle, List<object> arguments, string raw)
        {
            if (!Player.PlayerInformation.IsDeveloper()) return;
            if (arguments.Count < 1) return;

            Vector3 pos = Game.PlayerPed.Position;

            Client.TriggerServerEvent("curiosity:Server:Command:SavePosition", $"{arguments[0]}", pos.X, pos.Y, pos.Z, Game.PlayerPed.Heading);

            await BaseScript.Delay(0);
        }

        static async void SpawnCar(string car, string numberPlate)
        {
            try
            {
                if (!Player.PlayerInformation.IsStaff()) return;
                if (string.IsNullOrEmpty(car)) return;

                Model model = null;
                var enumName = Enum.GetNames(typeof(VehicleHash)).FirstOrDefault(s => s.ToLower().StartsWith(car.ToLower())) ?? "";

                var modelName = "";

                if (int.TryParse(car, out var hash))
                {
                    model = new Model(hash);
                    modelName = $"{hash}";
                }
                else if (!string.IsNullOrEmpty(enumName))
                {
                    var found = false;
                    foreach (VehicleHash p in Enum.GetValues(typeof(VehicleHash)))
                    {
                        if (!(Enum.GetName(typeof(VehicleHash), p)?.Equals(enumName, StringComparison.InvariantCultureIgnoreCase) ??
                              false))
                        {
                            continue;
                        }

                        model = new Model(p);
                        modelName = enumName;
                        found = true;
                        break;
                    }

                    if (!found)
                    {
                        Screen.ShowNotification($"~r~ERROR~s~: Could not load model {car}");
                        return;
                    }
                }
                else
                {
                    model = new Model(car);
                    modelName = car;
                }

                if (!await SpawnVehicle(model, numberPlate))
                {
                    Environment.UI.Notifications.Curiosity(1, "Curiosity", "Vehicle Error", $"Could not load model {modelName}", 2);
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        
        private static async Task<bool> SpawnVehicle(Model model, string numberPlate)
        {
            if (Game.PlayerPed.CurrentVehicle != null)
            {
                Game.PlayerPed.CurrentVehicle.Delete();
            }

            var veh = await World.CreateVehicle(model, Game.PlayerPed.Position, Game.PlayerPed.Heading);
            if (veh == null)
            {
                return false;
            }

            model.MarkAsNoLongerNeeded();

            API.NetworkFadeInEntity(veh.Handle, false);

            Game.PlayerPed.Task.WarpIntoVehicle(veh, VehicleSeat.Driver);
            veh.LockStatus = VehicleLockStatus.Unlocked;
            veh.NeedsToBeHotwired = false;
            veh.IsEngineRunning = true;
            veh.Mods.LicensePlate = numberPlate;

            veh.Mods.InstallModKit();
            veh.Mods[VehicleModType.Engine].Index = veh.Mods[VehicleModType.Engine].ModCount - 1;
            veh.Mods[VehicleModType.Brakes].Index = veh.Mods[VehicleModType.Brakes].ModCount - 1;
            veh.Mods[VehicleModType.Transmission].Index = veh.Mods[VehicleModType.Transmission].ModCount - 1;
            veh.Mods[VehicleModType.Suspension].Index = veh.Mods[VehicleModType.Suspension].ModCount - 1;

            List<int> listOfExtras = new List<int>();

            for(int i = 0; i < 255; i++)
            {
                if (veh.ExtraExists(i))
                    listOfExtras.Add(i);
            }

            int networkId = API.VehToNet(veh.Handle);
            API.SetNetworkIdExistsOnAllMachines(networkId, true);
            API.SetNetworkIdCanMigrate(networkId, true);

            Blip blip = veh.AttachBlip();
            blip.IsShortRange = false;
            blip.Sprite = BlipSprite.PersonalVehicleCar;
            blip.Priority = 100;
            blip.Name = "Personal Vehicle";

            if (API.DecorIsRegisteredAsType("Player_Vehicle", 3))
            {
                API.DecorSetInt(veh.Handle, "Player_Vehicle", Game.Player.ServerId);
            }

            API.SetVehicleHasBeenOwnedByPlayer(veh.Handle, true);

            API.SetVehicleExclusiveDriver(veh.Handle, Game.PlayerPed.Handle);
            API.SetVehicleExclusiveDriver_2(veh.Handle, Game.PlayerPed.Handle, 1);

            Client.TriggerServerEvent("curiosity:Server:Vehicles:TempStore", networkId);
            Client.TriggerEvent("curiosity:Player:Menu:VehicleId", veh.Handle);
            if (numberPlate == Environment.Vehicles.HSTAFF_LICENSE_PLATE || numberPlate == Environment.Vehicles.DEV_LICENSE_PLATE)
            {
                Environment.UI.Notifications.Curiosity(1, "Curiosity", "Vehicle Spawned", $"Available Mods can be found in the Debug Console", 2);
                Debug.WriteLine($"Vehicle Mods: {string.Join(", ", veh.Mods.GetAllMods().Select(m => Enum.GetName(typeof(VehicleModType), m.ModType)))}");
                if (listOfExtras.Count > 0)
                {
                    Debug.WriteLine($"Vehicle Extras: '/mod extra number true/false' avaiable: {string.Join(", ", listOfExtras)}");
                }

            }
            Environment.UI.Notifications.Curiosity(1, "Curiosity", "Vehicle Spawned", $"~b~Engine: ~y~MAX~n~~b~Brakes: ~y~MAX~n~~b~Transmission: ~y~MAX", 2);

            return true;
        }

        static void SpawnWeapon(string weapon)
        {
            if (!Player.PlayerInformation.IsDeveloper()) return;
            
        }

        static void Teleport(int playerHandle, List<object> arguments, string raw)
        {
            if (!Player.PlayerInformation.IsDeveloper()) return;
            if (arguments.Count < 3) return;

            float posX = float.Parse(arguments[0].ToString());
            float posY = float.Parse(arguments[1].ToString());
            float posZ = float.Parse(arguments[2].ToString());

            API.FreezeEntityPosition(Game.PlayerPed.Handle, true);
            Game.PlayerPed.Position = new Vector3(posX, posY, posZ);
            API.FreezeEntityPosition(Game.PlayerPed.Handle, false);
        }

        static void ModVehicle(int playerHandle, List<object> arguments, string raw)
        {
            try
            {
                if (!Player.PlayerInformation.IsDeveloper()) return;

                CitizenFX.Core.Vehicle veh = Game.PlayerPed.CurrentVehicle;
                if (veh == null || arguments.Count < 2) return;

                string vehicleModTypeName = arguments[0].ToString();
                int vehicleModIndex = int.Parse(arguments[1].ToString());

                if (vehicleModTypeName == "list")
                {

                    List<int> listOfExtras = new List<int>();

                    for (int i = 0; i < 255; i++)
                    {
                        if (veh.ExtraExists(i))
                            listOfExtras.Add(i);
                    }

                    Debug.WriteLine($"Vehicle Mods: {string.Join(", ", veh.Mods.GetAllMods().Select(m => Enum.GetName(typeof(VehicleModType), m.ModType)))}");
                    if (listOfExtras.Count > 0)
                    {
                        Debug.WriteLine($"Vehicle Extras: '/mod extra number true/false' avaiable: {string.Join(", ", listOfExtras)}");
                    }
                    return;
                }

                if (vehicleModTypeName == "extra")
                {
                    if (veh == null || arguments.Count < 3) return;

                    bool enable = Convert.ToBoolean(arguments[2].ToString());
                    veh.ToggleExtra(vehicleModIndex, enable);

                    return;
                }

                foreach (VehicleModType vehicleMod in Enum.GetValues(typeof(VehicleModType)))
                {
                    if (Enum.GetName(typeof(VehicleModType), vehicleMod).Equals(vehicleModTypeName))
                    {
                        veh.Mods[vehicleMod].Index = MathUtil.Clamp(vehicleModIndex, 0, veh.Mods[vehicleMod].ModCount - 1);
                        Environment.UI.Notifications.Curiosity(1, "Curiosity", "Vehicle Mod", $"Set {vehicleModTypeName} to {vehicleModIndex}.", 2);
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"{ex.Message}");
            }
        }
    }
}
