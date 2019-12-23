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
using Curiosity.Shared.Client.net.Enums;
using Curiosity.Global.Shared.net;

namespace Curiosity.Missions.Client.net.Scripts.Mission.PoliceMissions
{
    class HumainLabs
    {
        static Client client = Client.GetInstance();

        static ConcurrentDictionary<string, MissionPed> _missionPedList = new ConcurrentDictionary<string, MissionPed>();

        static Vector3 _location = Vector3.Zero;
        static Blip _blip;

        public static void Init()
        {
            client.RegisterEventHandler("curiosity:missions:player:spawn", new Action(CreateMission));
            client.RegisterEventHandler("curiosity:missions:player:invalid", new Action(InvalidMission));
            client.RegisterEventHandler("curiosity:missions:player:clean", new Action(CleanMission));
        }

        private static void InvalidMission()
        {
            CleanMission();
            Client.TriggerEvent("curiosity:Client:Player:UpdateExtraFlags");
        }

        private static void CleanMission()
        {
            ConcurrentDictionary<string, MissionPed> _missionPedListCopy = _missionPedList;
            foreach(KeyValuePair<string, MissionPed> kvp in _missionPedListCopy)
            {
                if (kvp.Value.Exists())
                    kvp.Value.Delete();

                _missionPedList.TryRemove(kvp.Key, out MissionPed empty);
            }

            if (_blip != null)
            {
                if (_blip.Exists())
                {
                    _blip.Delete();
                }
            }
        }

        private static async void CreateMission()
        {
            await CreatePeds();

            Alert();
            CreateMissionBlip();

            client.RegisterTickHandler(OnCompleteCheck);
        }

        private static async Task OnCompleteCheck()
        {
            await Task.FromResult(0);
            if (AreAllPedsAreCleared())
            {
                CleanMission();

                List<Player> players = GetPlayersInArea();


                string json = "";
                BaseScript.TriggerServerEvent("curiosity:Server:Missions:CompletedGroupMission", Encode.StringToBase64(json));
            }
        }

        private static void Alert()
        {
            Client.TriggerEvent("curiosity:Client:Notification:Advanced", $"{NotificationCharacter.CHAR_CALL911}", 2, "Code 3", $"Humain Labs", "Humain labs is being raided!", 2);
            PlaySoundFrontend(-1, "Menu_Accept", "Phone_SoundSet_Default", true);
            SoundManager.PlayAudio(
                $"RESIDENT/DISPATCH_INTRO_0{Client.Random.Next(1, 3)}" +
                $" ATTENTION_ALL_UNITS/ATTENTION_ALL_UNITS_{Client.Random.Next(1, 6)}" +
                $" WE_HAVE/WE_HAVE_0{Client.Random.Next(1, 3)}" +
                $" CRIMES/CRIME_GUNFIRE_0{Client.Random.Next(1, 4)}" +
                $" CONJUNCTIVES/AT_0{Client.Random.Next(1, 3)}" +
                $" AREAS/AREA_HUMANE_LABS");
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

        private static async Task CreatePeds() // Maybe brake this down to set checkpoints.
        {
            _missionPedList.GetOrAdd("humainRaidPed1", await CreatePed(3611.027f, 3728.94f, 29.68939f, 308.7826f));
            _missionPedList.GetOrAdd("humainRaidPed2", await CreatePed(3623.014f, 3728.33f, 28.69011f, 355.1138f));
            _missionPedList.GetOrAdd("humainRaidPed3", await CreatePed(3609.912f, 3745.168f, 28.6901f, 304.7286f));
            _missionPedList.GetOrAdd("humainRaidPed4", await CreatePed(3605.438f, 3735.31f, 28.69009f, 325.1797f));
            _missionPedList.GetOrAdd("humainRaidPed5", await CreatePed(3610.265f, 3713.552f, 29.6894f, 319.5776f));
            _missionPedList.GetOrAdd("humainRaidPed6", await CreatePed(3601.898f, 3707.934f, 29.6894f, 6.761765f));
            _missionPedList.GetOrAdd("humainRaidPed7", await CreatePed(3600.922f, 3716.216f, 29.68941f, 160.6745f));
            _missionPedList.GetOrAdd("humainRaidPed8", await CreatePed(3596.407f, 3718.502f, 29.68941f, 328.133f));
            _missionPedList.GetOrAdd("humainRaidPed9", await CreatePed(3593.634f, 3706.342f, 29.68941f, 236.0788f));
            _missionPedList.GetOrAdd("humainRaidPed10", await CreatePed(3597.94f, 3690.484f, 28.8214f, 20.90239f));
            _missionPedList.GetOrAdd("humainRaidPed11", await CreatePed(3585.394f, 3691.344f, 27.12185f, 258.396f));
            _missionPedList.GetOrAdd("humainRaidPed12", await CreatePed(3567.439f, 3700.764f, 28.12148f, 186.5913f));
            _missionPedList.GetOrAdd("humainRaidPed13", await CreatePed(3560.268f, 3696.225f, 30.12151f, 253.0495f));
            _missionPedList.GetOrAdd("humainRaidPed14", await CreatePed(3558.471f, 3666.603f, 28.12189f, 341.537f));
            _missionPedList.GetOrAdd("humainRaidPed15", await CreatePed(3545.969f, 3645.399f, 28.12189f, 352.4461f));
            _missionPedList.GetOrAdd("humainRaidPed16", await CreatePed(3534.463f, 3672.313f, 28.12114f, 133.2211f));
            _missionPedList.GetOrAdd("humainRaidPed17", await CreatePed(3530.158f, 3673.195f, 28.12114f, 172.5429f));
        }

        private static async Task<MissionPed> CreatePed(float x, float y, float z, float heading)
        {
            Vector3 position = new Vector3(x, y, z);
            Model model = Client.Random.Next(2) == 1 ? PedHash.Lost01GMY : Client.Random.Next(2) == 1 ? PedHash.Lost03GMY : PedHash.Lost02GMY;
            await model.Request(10000);
            Ped spawnedPed = await World.CreatePed(model, position, heading);
            // settings
            spawnedPed.Weapons.Give(WeaponHash.AssaultRifle, 1, true, true);
            spawnedPed.DropsWeaponsOnDeath = false;
            spawnedPed.Armor = 100;
            // mission maker
            MissionPed missionPed = MissionPedCreator.Ped(spawnedPed, Extensions.Alertness.FullyAlert, Extensions.Difficulty.HurtMePlenty);
            model.MarkAsNoLongerNeeded();
            return missionPed;
        }

        private static bool AreAllPedsAreCleared()
        {
            if (_missionPedList.Count == 0)
            {
                return true;
            }

            int countOfPeds = _missionPedList.Count;

            ConcurrentDictionary<string, MissionPed> MissionPedListCopy = _missionPedList;

            MissionPedListCopy.ToList().ForEach(item =>
            {
                if (item.Value.Exists())
                {
                    if (item.Value.IsDead)
                        countOfPeds--;
                }
                else
                {
                    _missionPedList.TryRemove(item.Key, out MissionPed missionPed); // Cause... NO ONE CARES!
                }
            });

            return countOfPeds == 0;
        }

        private static List<Player> GetPlayersInArea()
        {
            return Client.players.Select(p => p).Where(x => x.Character.Position.Distance(_location) <= 250).ToList();
        }
    }
}
