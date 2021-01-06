using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Environment;
using Curiosity.Core.Client.Environment.Entities.Models;
using Curiosity.Core.Client.Events;
using Curiosity.Systems.Library.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Extensions
{
    public static class CharacterExtensions
    {

        public static Dictionary<int, KeyValuePair<string, string>> HairOverlays = new Dictionary<int, KeyValuePair<string, string>>()
            {
                { 0, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_a") },
                { 1, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 2, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 3, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_003_a") },
                { 4, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 5, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 6, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 7, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 8, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_008_a") },
                { 9, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 10, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 11, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 12, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 13, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 14, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_long_a") },
                { 15, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_long_a") },
                { 16, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_z") },
                { 17, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_a") },
                { 18, new KeyValuePair<string, string>("mpbusiness_overlays", "FM_Bus_M_Hair_000_a") },
                { 19, new KeyValuePair<string, string>("mpbusiness_overlays", "FM_Bus_M_Hair_001_a") },
                { 20, new KeyValuePair<string, string>("mphipster_overlays", "FM_Hip_M_Hair_000_a") },
                { 21, new KeyValuePair<string, string>("mphipster_overlays", "FM_Hip_M_Hair_001_a") },
                { 22, new KeyValuePair<string, string>("multiplayer_overlays", "FM_M_Hair_001_a") },
            };
        public static Position DefaultPosition { get; } = new Position(-1042.24f, -2745.336f, 21.3594f, 332.7626f);
        public static Position TaxiPosition { get; } = new Position(-1050.467f, -2720.67f, 19.68902f, 240.8357f);

        public static RotatablePosition[] CameraViews { get; } =
        {
            new RotatablePosition(402.8294f, -1002.45f, -98.80403f, 357.6219f, -7f, 0f),
            new RotatablePosition(402.8294f, -998.8467f, -98.80403f, 357.1697f, -7f, 0f),
            new RotatablePosition(402.8294f, -997.967f, -98.35f, 357.1697f, -7f, 0f)
        };

        public static string[] HeadComponents { get; } =
        {
            "Face", "Wrinkles", "Freckles", "EyeColor", "Sunburn", "Complexion", "Hair", "Eyebrows", "Beard", "Blush",
            "Makeup", "Lipstick"
        };

        public static Position RegistrationPosition { get; } =
            new Position(405.9247f, -997.2114f, -99.00401f, 86.36787f);

        //public static void Synchronize(this CuriosityCharacter character)
        //{
        //    var player = Cache.Player;

        //    player.User.Characters.RemoveAll(self => self.CharacterId == character.CharacterId);
        //    player.User.Characters.Add(character);
        //}

        public static async Task Save(this CuriosityCharacter character)
        {
            character.LastPosition = Cache.Entity.Position;

            await EventSystem.GetModule().Request<object>("character:save", character);

            // Logger.Info($"[Characters] Saved `{character.CharacterId}` and it's changes.");
        }

        public static void SetupStat(string stat, int value)
        {
            API.StatSetInt((uint)API.GetHashKey(stat), value, true);
        }

        public static void SetupStats(this CuriosityCharacter character)
        {
            foreach (KeyValuePair<string, int> keyValuePair in character.Stats)
            {
                API.StatSetInt((uint)API.GetHashKey(keyValuePair.Key), keyValuePair.Value, true);
            }
        }
    }
}