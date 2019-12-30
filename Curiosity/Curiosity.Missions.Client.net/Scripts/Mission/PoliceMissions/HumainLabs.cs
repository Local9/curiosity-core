using CitizenFX.Core;
using Curiosity.Missions.Client.net.DataClasses.Mission;
using Curiosity.Missions.Client.net.MissionPeds;
using Curiosity.Missions.Client.net.Scripts.PedCreators;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Curiosity.Shared.Client.net.Extensions;
using static CitizenFX.Core.Native.API;
using CitizenFX.Core.Native;
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Global.Shared.net;
using Curiosity.Shared.Client.net.Helper.Area;
using CitizenFX.Core.UI;
using System.Drawing;
using Curiosity.Shared.Client.net.Helper;
using Curiosity.Missions.Client.net.DataClasses;
using Curiosity.Missions.Client.net.Exceptions;

namespace Curiosity.Missions.Client.net.Scripts.Mission.PoliceMissions
{
    class HumainLabs
    {
        private const string TRIGGER1 = "hlTrigger1";
        private const string TRIGGER2 = "hlTrigger2";
        private const string TRIGGER3 = "hlTrigger3";
        private const string TRIGGER4 = "hlTrigger4";
        private const string TRIGGER5 = "hlTrigger5";
        private const string TRIGGER6 = "hlTrigger6";
        private const string TRIGGER7 = "hlTrigger7";
        private const string TRIGGER8 = "hlTrigger8";
        private const string TRIGGER9 = "hlTrigger9";
        private const string TRIGGER10 = "hlTrigger10";
        private const string MISSIONTRIGGER = "hlTriggerMissionLocation";
        static Client client = Client.GetInstance();

        static ConcurrentDictionary<string, AreaSphere> _missionTriggers = new ConcurrentDictionary<string, AreaSphere>();

        static Vector3 _location = Vector3.Zero;
        static Blip _blip;

        static bool DebugAreas = false;
        static bool MissionActive = false;

        public static void Init()
        {
            API.RegisterCommand("hlmission", new Action<int, List<object>, string>(CommandHlMission), false);
            // API.RegisterCommand("item", new Action<int, List<object>, string>(StartItemPreview), false);
            // API.RegisterCommand("boss", new Action<int, List<object>, string>(BossTest), false);

            client.RegisterEventHandler("curiosity:missions:player:spawn", new Action(CreateMission));
            client.RegisterEventHandler("curiosity:missions:player:invalid", new Action(InvalidMission));
            client.RegisterEventHandler("curiosity:missions:player:clean", new Action(CleanMission));

            client.RegisterEventHandler("curiosity:Client:Player:Environment:OnEnterArea", new Action<string, dynamic>(OnAreaEnter));
            client.RegisterEventHandler("curiosity:Client:Player:Environment:OnExitArea", new Action<string, dynamic>(OnAreaExit));

            client.RegisterEventHandler("curiosity:Client:Player:Environment:DrawAreas", new Action<bool>(OnDrawAreas));
        }

        private static async void BossTest(int playerHandle, List<object> arguments, string raw)
        {
            if (!Classes.PlayerClient.ClientInformation.IsDeveloper()) return;

            Vector3 offset = Game.PlayerPed.Position + new Vector3(0f, 3f, 0f);

            Model model = API.GetHashKey("u_m_y_juggernaut_01");
            await model.Request(10000);
            Ped ped = await World.CreatePed(model, offset);
            model.MarkAsNoLongerNeeded();

            int type = 1;

            if (arguments.Count > 0)
                type = int.Parse($"{arguments[0]}");

            if (type == 1)
            {
                SetPedPropIndex(ped.Handle, 0, 0, 0, false);
                SetPedComponentVariation(ped.Handle, 0, 0, 1, 0);
                SetPedComponentVariation(ped.Handle, 3, 0, 0, 0);
                SetPedComponentVariation(ped.Handle, 4, 0, 0, 0);
                SetPedComponentVariation(ped.Handle, 5, 0, 0, 0);
                SetPedComponentVariation(ped.Handle, 8, 0, 1, 0);
                SetPedComponentVariation(ped.Handle, 10, 0, 1, 0);
                return;
            }

            if (type == 2)
            {
                SetPedPropIndex(ped.Handle, 0, 0, 0, false);
                SetPedComponentVariation(ped.Handle, 0, 0, 0, 0);
                SetPedComponentVariation(ped.Handle, 3, 0, 1, 0);
                SetPedComponentVariation(ped.Handle, 4, 0, 0, 0);
                SetPedComponentVariation(ped.Handle, 5, 0, 0, 0);
                SetPedComponentVariation(ped.Handle, 8, 0, 0, 0);
                SetPedComponentVariation(ped.Handle, 10, 0, 0, 0);
                return;
            }

            if (type == 3)
            {
                ClearPedProp(ped.Handle, 0);
                SetPedComponentVariation(ped.Handle, 0, 0, 0, 0);
                SetPedComponentVariation(ped.Handle, 3, 0, 1, 0);
                SetPedComponentVariation(ped.Handle, 4, 0, 0, 0);
                SetPedComponentVariation(ped.Handle, 5, 0, 0, 0);
                SetPedComponentVariation(ped.Handle, 8, 0, 0, 0);
                SetPedComponentVariation(ped.Handle, 10, 0, 0, 0);
                return;
            }


        }

        private static void StartItemPreview(int playerHandle, List<object> arguments, string raw)
        {
            if (!Classes.PlayerClient.ClientInformation.IsDeveloper()) return;

            string prop = "prop_bin_beach_01d";

            if (arguments.Count > 0)
                prop = $"{arguments[0]}";

            ItemPreview.StartPreview(prop, new Vector3(0f, 0f, 1f), false);
        }

        private static void CommandHlMission(int playerHandle, List<object> arguments, string raw)
        {
            if (!Classes.PlayerClient.ClientInformation.IsDeveloper()) return;

            if (arguments.Count > 0)
                DebugAreas = true;

            CreateMission();
        }

        private static void OnAreaExit(string identifier, dynamic data)
        {
            if (!identifier.Contains("hlTrigger")) return;

            if (identifier == MISSIONTRIGGER)
            {
                // CLEAR
            }
        }

        private static async void OnAreaEnter(string identifier, dynamic data)
        {
            try
            {
                if (!identifier.Contains("hlTrigger")) return;

                if (DebugAreas)
                    Screen.ShowNotification($"Trigger: {identifier}");

                if (identifier == MISSIONTRIGGER)
                {
                    // _missionTriggers.TryRemove(MISSIONTRIGGER, out AreaSphere areaSphere); // REMOVE IT, NO NEED TO TRIGGER IT AGAIN

                    await CreatePed(3611.027f, 3728.94f, 29.68939f, 308.7826f);
                    await CreatePed(3623.014f, 3728.33f, 28.69011f, 355.1138f);
                    await CreatePed(3609.912f, 3745.168f, 28.6901f, 304.7286f);
                    await CreatePed(3605.438f, 3735.31f, 28.69009f, 325.1797f);
                    await CreatePed(3610.265f, 3713.552f, 29.6894f, 319.5776f);
                    await CreatePed(3601.898f, 3707.934f, 29.6894f, 6.761765f);
                }

                if (identifier == TRIGGER1)
                {
                    await CreatePed(3600.922f, 3716.216f, 29.68941f, 160.6745f);
                    await CreatePed(3598.575f, 3714.219f, 29.6894f, 183.3985f);

                    _missionTriggers.TryRemove(TRIGGER1, out AreaSphere areaSphere); // REMOVE IT, NO NEED TO TRIGGER IT AGAIN
                }

                if (identifier == TRIGGER2)
                {
                    CheckTriggerIsRemoved(TRIGGER1);

                    await CreatePed(3596.407f, 3718.502f, 29.68941f, 328.133f);
                    await CreatePed(3593.634f, 3706.342f, 29.68941f, 236.0788f);

                    _missionTriggers.TryRemove(TRIGGER2, out AreaSphere areaSphere); // REMOVE IT, NO NEED TO TRIGGER IT AGAIN
                }

                if (identifier == TRIGGER3)
                {
                    CheckTriggerIsRemoved(TRIGGER1);
                    CheckTriggerIsRemoved(TRIGGER2);

                    await CreatePed(3597.94f, 3690.484f, 28.8214f, 20.90239f);
                    await CreatePed(3594.008f, 3695.081f, 28.82139f, 43.89056f);
                    await CreatePed(3602.753f, 3688.477f, 28.82139f, 49.60534f);

                    _missionTriggers.TryRemove(TRIGGER3, out AreaSphere areaSphere); // REMOVE IT, NO NEED TO TRIGGER IT AGAIN
                }

                if (identifier == TRIGGER4)
                {
                    CheckTriggerIsRemoved(TRIGGER1);
                    CheckTriggerIsRemoved(TRIGGER2);
                    CheckTriggerIsRemoved(TRIGGER3);

                    await CreatePed(3585.394f, 3691.344f, 27.12185f, 258.396f);
                    await CreatePed(3567.439f, 3700.764f, 28.12148f, 186.5913f);
                    await CreatePed(3560.268f, 3696.225f, 30.12151f, 253.0495f);
                    await CreatePed(3590.57f, 3685.909f, 27.62151f, 260.2165f);
                    await CreatePed(3582.299f, 3694.177f, 27.12185f, 228.9425f);

                    _missionTriggers.TryRemove(TRIGGER4, out AreaSphere areaSphere); // REMOVE IT, NO NEED TO TRIGGER IT AGAIN
                }

                if (identifier == TRIGGER5)
                {
                    CheckTriggerIsRemoved(TRIGGER1);
                    CheckTriggerIsRemoved(TRIGGER2);
                    CheckTriggerIsRemoved(TRIGGER3);
                    CheckTriggerIsRemoved(TRIGGER4);

                    await CreatePed(3558.471f, 3666.603f, 28.12189f, 341.537f);
                    await CreatePed(3562.876f, 3683.196f, 28.12189f, 321.4918f);
                    await CreatePed(3560.752f, 3678.526f, 28.12187f, 55.01857f);
                    await CreatePed(3551.098f, 3664.566f, 28.12189f, 208.1347f);

                    _missionTriggers.TryRemove(TRIGGER5, out AreaSphere areaSphere); // REMOVE IT, NO NEED TO TRIGGER IT AGAIN
                }

                if (identifier == TRIGGER6)
                {
                    CheckTriggerIsRemoved(TRIGGER1);
                    CheckTriggerIsRemoved(TRIGGER2);
                    CheckTriggerIsRemoved(TRIGGER3);
                    CheckTriggerIsRemoved(TRIGGER4);
                    CheckTriggerIsRemoved(TRIGGER5);

                    await CreatePed(3545.969f, 3645.399f, 28.12189f, 352.4461f);
                    await CreatePed(3552.627f, 3656.957f, 28.12189f, 91.69083f);

                    _missionTriggers.TryRemove(TRIGGER6, out AreaSphere areaSphere); // REMOVE IT, NO NEED TO TRIGGER IT AGAIN
                }

                if (identifier == TRIGGER7)
                {
                    CheckTriggerIsRemoved(TRIGGER1);
                    CheckTriggerIsRemoved(TRIGGER2);
                    CheckTriggerIsRemoved(TRIGGER3);
                    CheckTriggerIsRemoved(TRIGGER4);
                    CheckTriggerIsRemoved(TRIGGER5);
                    CheckTriggerIsRemoved(TRIGGER6);

                    await CreatePed(3534.463f, 3672.313f, 28.12114f, 133.2211f);
                    await CreatePed(3530.158f, 3673.195f, 28.12114f, 172.5429f);
                    await CreatePed(3547.29f, 3641.687f, 28.12189f, 70.07915f);
                    await CreatePed(3531.382f, 3650.277f, 27.52158f, 274.3347f);

                    _missionTriggers.TryRemove(TRIGGER7, out AreaSphere areaSphere); // REMOVE IT, NO NEED TO TRIGGER IT AGAIN
                }

                if (identifier == TRIGGER8)
                {
                    CheckTriggerIsRemoved(TRIGGER1);
                    CheckTriggerIsRemoved(TRIGGER2);
                    CheckTriggerIsRemoved(TRIGGER3);
                    CheckTriggerIsRemoved(TRIGGER4);
                    CheckTriggerIsRemoved(TRIGGER5);
                    CheckTriggerIsRemoved(TRIGGER6);
                    CheckTriggerIsRemoved(TRIGGER7);

                    _missionTriggers.TryRemove(TRIGGER8, out AreaSphere areaSphere); // REMOVE IT, NO NEED TO TRIGGER IT AGAIN
                }

                if (identifier == TRIGGER9)
                {
                    CheckTriggerIsRemoved(TRIGGER1);
                    CheckTriggerIsRemoved(TRIGGER2);
                    CheckTriggerIsRemoved(TRIGGER3);
                    CheckTriggerIsRemoved(TRIGGER4);
                    CheckTriggerIsRemoved(TRIGGER5);
                    CheckTriggerIsRemoved(TRIGGER6);
                    CheckTriggerIsRemoved(TRIGGER7);
                    CheckTriggerIsRemoved(TRIGGER8);

                    await CreatePed(3535.261f, 3672.048f, 20.99179f, 263.4342f);
                    await CreatePed(3524.487f, 3681.816f, 20.99179f, 172.2374f);
                    await CreatePed(3527.599f, 3693.367f, 20.99179f, 83.60178f);

                    _missionTriggers.TryRemove(TRIGGER9, out AreaSphere areaSphere); // REMOVE IT, NO NEED TO TRIGGER IT AGAIN
                }

                if (identifier == TRIGGER10)
                {
                    CheckTriggerIsRemoved(TRIGGER1);
                    CheckTriggerIsRemoved(TRIGGER2);
                    CheckTriggerIsRemoved(TRIGGER3);
                    CheckTriggerIsRemoved(TRIGGER4);
                    CheckTriggerIsRemoved(TRIGGER5);
                    CheckTriggerIsRemoved(TRIGGER6);
                    CheckTriggerIsRemoved(TRIGGER7);
                    CheckTriggerIsRemoved(TRIGGER8);
                    CheckTriggerIsRemoved(TRIGGER9);

                    await CreatePed(3524.206f, 3711.529f, 20.99178f, 171.4173f, API.GetHashKey("u_m_y_juggernaut_01"));
                    await CreatePed(3528.076f, 3711.494f, 20.99179f, 180.5266f);
                    await CreatePed(3521.714f, 3712.648f, 20.99179f, 200.29f);

                    _missionTriggers.TryRemove(TRIGGER10, out AreaSphere areaSphere); // REMOVE IT, NO NEED TO TRIGGER IT AGAIN
                }
            }
            catch(InvalidOrderException ex)
            {
                // KILL THE MISSION
                KillTheMission(true);
            }
        }

        private static void KillTheMission(bool teleportPlayers = false)
        {
            // remove all triggers

            if (_missionTriggers.ContainsKey(TRIGGER1))
                _missionTriggers.TryRemove(TRIGGER1, out AreaSphere t1);
            if (_missionTriggers.ContainsKey(TRIGGER2))
                _missionTriggers.TryRemove(TRIGGER2, out AreaSphere t2);
            if (_missionTriggers.ContainsKey(TRIGGER3))
                _missionTriggers.TryRemove(TRIGGER3, out AreaSphere t3);
            if (_missionTriggers.ContainsKey(TRIGGER4))
                _missionTriggers.TryRemove(TRIGGER4, out AreaSphere t4);
            if (_missionTriggers.ContainsKey(TRIGGER5))
                _missionTriggers.TryRemove(TRIGGER5, out AreaSphere t5);
            if (_missionTriggers.ContainsKey(TRIGGER6))
                _missionTriggers.TryRemove(TRIGGER6, out AreaSphere t6);
            if (_missionTriggers.ContainsKey(TRIGGER7))
                _missionTriggers.TryRemove(TRIGGER7, out AreaSphere t7);
            if (_missionTriggers.ContainsKey(TRIGGER8))
                _missionTriggers.TryRemove(TRIGGER8, out AreaSphere t8);
            if (_missionTriggers.ContainsKey(TRIGGER9))
                _missionTriggers.TryRemove(TRIGGER9, out AreaSphere t9);
            if (_missionTriggers.ContainsKey(TRIGGER10))
                _missionTriggers.TryRemove(TRIGGER10, out AreaSphere t10);

            ClearAreaOfPeds(_location.X, _location.Y, _location.Z, 300f, 1);
            ClearAreaOfCops(_location.X, _location.Y, _location.Z, 300f, 0);
            ClearAreaOfProjectiles(_location.X, _location.Y, _location.Z, 300f, true);

            if (teleportPlayers)
            {
                GetPlayersInArea().ForEach(p =>
                {
                    string encoded = "";
                    BaseScript.TriggerServerEvent("curiosity:Server:Mission:MovePlayer", encoded);
                });
            }
        }

        private static void CheckTriggerIsRemoved(string trigger)
        {
            if (_missionTriggers.ContainsKey(trigger))
            {
                throw new InvalidOrderException("Invalid order of triggers");
            }
        }

        static void OnDrawAreas(bool state)
        {
            if (!Classes.PlayerClient.ClientInformation.IsDeveloper()) return;

            DebugAreas = state;
        }

        private static void InvalidMission()
        {
            CleanMission();
            Client.TriggerEvent("curiosity:Client:Player:UpdateExtraFlags");
        }

        private static void CleanMission()
        {
            if (_blip != null)
            {
                if (_blip.Exists())
                {
                    _blip.Delete();
                }
            }
        }

        private static void AddTrigger(string identifier, Vector3 position, float radius = 5f)
        {
            AddTrigger(identifier, position, Color.FromArgb(255, 0, 0), radius);
        }

        private static void AddTrigger(string identifier, Vector3 position, System.Drawing.Color color, float radius = 5f)
        {
            AreaSphere areaSphere = new AreaSphere();
            areaSphere.Pos = position;
            areaSphere.Radius = radius;
            areaSphere.Identifier = identifier;
            areaSphere.Color = color;
            _missionTriggers.GetOrAdd(areaSphere.Identifier, areaSphere);
        }

        private static async void CreateMission()
        {
            await BaseScript.Delay(0);

            _location = new Vector3(3611.552f, 3720.873f, 29.68941f);

            ClearAreaOfPeds(_location.X, _location.Y, _location.Z, 300f, 1);
            ClearAreaOfCops(_location.X, _location.Y, _location.Z, 300f, 0);
            ClearAreaOfProjectiles(_location.X, _location.Y, _location.Z, 300f, true);

            AddTrigger(MISSIONTRIGGER, _location, Color.FromArgb(0, 255, 0), 250f);

            AddTrigger(TRIGGER1, new Vector3(3611.552f, 3720.873f, 29.68941f), Color.FromArgb(0, 0, 255));
            AddTrigger(TRIGGER2, new Vector3(3599.848f, 3716.641f, 29.68941f), Color.FromArgb(0, 0, 255));
            AddTrigger(TRIGGER3, new Vector3(3589.642f, 3707.065f, 29.68545f), Color.FromArgb(0, 0, 255));
            AddTrigger(TRIGGER4, new Vector3(3597.859f, 3690.435f, 28.82138f), Color.FromArgb(0, 0, 255));
            AddTrigger(TRIGGER5, new Vector3(3569.469f, 3694.422f, 28.12245f), Color.FromArgb(0, 0, 255));
            AddTrigger(TRIGGER6, new Vector3(3555.890f, 3676.128f, 28.12187f), Color.FromArgb(0, 0, 255));
            AddTrigger(TRIGGER7, new Vector3(3547.526f, 3645.309f, 28.12189f), Color.FromArgb(0, 0, 255));
            AddTrigger(TRIGGER8, new Vector3(3529.248f, 3654.237f, 27.52158f), Color.FromArgb(0, 0, 255));
            AddTrigger(TRIGGER9, new Vector3(3540.52f, 3675.612f, 20.99179f), Color.FromArgb(0, 0, 255));
            AddTrigger(TRIGGER10, new Vector3(3524.037f, 3690.067f, 20.9918f), Color.FromArgb(0, 0, 255));

            Alert();
            CreateMissionBlip();

            client.RegisterTickHandler(OnTriggerCheck);
            client.RegisterTickHandler(OnCompleteCheck);
        }

        private static async Task OnTriggerCheck()
        {
            await Task.FromResult(0);

            ConcurrentDictionary<string, AreaSphere> copy = _missionTriggers;

            foreach (KeyValuePair<string, AreaSphere> pair in copy)
            {
                pair.Value.Check();

                if (DebugAreas)
                {
                    NativeWrappers.Draw3DText(pair.Value.Pos.X, pair.Value.Pos.Y, pair.Value.Pos.Z, pair.Value.Identifier);
                    pair.Value.Draw();
                }
            }
        }

        private static async Task OnCompleteCheck()
        {
            await Task.FromResult(0);
            //if (AreAllPedsAreCleared())
            //{
            //    CleanMission();

            //    List<Player> players = GetPlayersInArea();


            //    string json = "";
            //    BaseScript.TriggerServerEvent("curiosity:Server:Missions:CompletedGroupMission", Encode.StringToBase64(json));
            //}
        }

        private static void Alert()
        {
            Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 2, "Code 3", $"Humain Labs", "Humain labs is being raided!", 2);
            PlaySoundFrontend(-1, "Menu_Accept", "Phone_SoundSet_Default", true);
            SoundManager.PlayAudio($"RESIDENT/DISPATCH_INTRO_0{Client.Random.Next(1, 3)} ATTENTION_ALL_UNITS/ATTENTION_ALL_UNITS_0{Client.Random.Next(1, 5)} WE_HAVE/WE_HAVE_0{Client.Random.Next(1, 3)} CRIMES/CRIME_GUNFIRE_0{Client.Random.Next(1, 4)} CONJUNCTIVES/AT_0{Client.Random.Next(1, 3)} AREAS/AREA_HUMANE_LABS");
        }

        private static void CreateMissionBlip()
        {
            _blip = World.CreateBlip(_location);
            _blip.Sprite = BlipSprite.BigCircle;
            _blip.Scale = 0.5f;
            _blip.Color = (BlipColor)5;
            _blip.Alpha = 126;
            _blip.ShowRoute = true;
            _blip.Priority = 9;
            _blip.IsShortRange = true;

            SetBlipDisplay(_blip.Handle, 5);
        }

        private static async Task CreatePed(float x, float y, float z, float heading)
        {
            Model model = Client.Random.Next(2) == 1 ? PedHash.Lost01GMY : Client.Random.Next(2) == 1 ? PedHash.Lost03GMY : PedHash.Lost02GMY;
            await CreatePed(x, y, z, heading, model);
        }

        private static async Task CreatePed(float x, float y, float z, float heading, Model selectedModel)
        {
            await BaseScript.Delay(10);
            if (DebugAreas)
            {
                AddTrigger($"npc_{Client.Random.Next(999999999)}", new Vector3(x, y, z), Color.FromArgb(255, 0, 0), 2f);
                return;
            }

            Vector3 position = new Vector3(x, y, z);
            Model model = selectedModel;
            await model.Request(10000);

            while (!model.IsLoaded)
            {
                await BaseScript.Delay(0);
            }

            Ped spawnedPed = await World.CreatePed(model, position, heading);
            // settings
            WeaponHash weaponHash = Client.Random.Next(2) == 1 ? WeaponHash.SawnOffShotgun : Client.Random.Next(2) == 1 ? WeaponHash.AssaultRifle : WeaponHash.MicroSMG;
            spawnedPed.Armor = 100;

            if (spawnedPed.Model.Hash == API.GetHashKey("u_m_y_juggernaut_01"))
            {
                spawnedPed.Health = 2000;
                spawnedPed.CanRagdoll = false;
                spawnedPed.CanSufferCriticalHits = false;
                spawnedPed.FiringPattern = FiringPattern.FullAuto;

                weaponHash = WeaponHash.Minigun;
                
            }

            spawnedPed.Weapons.Give(weaponHash, 999, true, true);
            spawnedPed.DropsWeaponsOnDeath = false;
            // mission maker
            MissionPedCreator.Ped(spawnedPed, Extensions.Alertness.FullyAlert, Extensions.Difficulty.HurtMePlenty);
            model.MarkAsNoLongerNeeded();
        }

        private static List<Player> GetPlayersInArea()
        {
            return Client.players.Select(p => p).Where(x => x.Character.Position.Distance(_location) <= 250).ToList();
        }
    }
}
