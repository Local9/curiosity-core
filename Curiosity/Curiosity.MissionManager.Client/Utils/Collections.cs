using CitizenFX.Core;
using System.Collections.Generic;

namespace Curiosity.MissionManager.Client.Utils
{
    internal static class Collections
    {
        internal static class Peds
        {
            internal static readonly List<PedHash> ALL = new List<PedHash>()
            {
                PedHash.Ammucity01SMY,
                PedHash.AmmuCountrySMM,
                PedHash.ArmBoss01GMM,
                PedHash.ArmGoon01GMM,
                PedHash.ArmGoon02GMY,
                PedHash.Autoshop01SMM,
                PedHash.BallaEast01GMY,
                PedHash.BallaOrig01GMY,
                PedHash.Bartender01SFY,
                PedHash.Bevhills01AFY,
                PedHash.Bevhills01AMM,
                PedHash.Bevhills03AFY,
                PedHash.Bouncer01SMM,
            };
        }

        internal static class PoliceCars
        {
            internal static readonly List<VehicleHash> ALL = new List<VehicleHash>()
            {
                VehicleHash.Police,
                VehicleHash.Police2,
                VehicleHash.Police3,
                VehicleHash.Police4,
                VehicleHash.Sheriff,
                VehicleHash.Sheriff2,
                VehicleHash.Policeb,
                VehicleHash.FBI,
                VehicleHash.FBI2,
                VehicleHash.Riot
            };

            internal static readonly List<VehicleHash> URBAN = new List<VehicleHash>()
            {
                VehicleHash.Police,
                VehicleHash.Police2,
                VehicleHash.Police3
            };

            internal static readonly List<VehicleHash> RURAL = new List<VehicleHash>()
            {
                VehicleHash.Sheriff,
                VehicleHash.Sheriff2
            };

            internal static readonly List<VehicleHash> HIGHWAY = new List<VehicleHash>()
            {
                VehicleHash.Sheriff,
                VehicleHash.Policeb
            };

            internal static readonly List<VehicleHash> UNDERCOVER = new List<VehicleHash>()
            {
                VehicleHash.FBI,
                VehicleHash.FBI2,
                VehicleHash.Police4
            };

            internal static readonly List<VehicleHash> NOOSE = new List<VehicleHash>()
            {
                VehicleHash.FBI2,
                VehicleHash.Riot
            };
        }

        internal static class PolicePeds
        {
            internal static List<PedHash> URBAN = new List<PedHash>() { PedHash.Cop01SFY, PedHash.Cop01SMY };
            internal static List<PedHash> RURAL = new List<PedHash>() { PedHash.Sheriff01SFY, PedHash.Sheriff01SFY };
            internal static List<PedHash> HIGHWAY = new List<PedHash>() { PedHash.Cop01SFY, PedHash.Cop01SMY, PedHash.Sheriff01SFY, PedHash.Sheriff01SFY };
        }

        internal static class PoliceWeapons
        {
            internal static List<WeaponHash> WEAPONS = new List<WeaponHash>() { WeaponHash.CarbineRifle, WeaponHash.APPistol, WeaponHash.PumpShotgun };
        }

        internal enum RelationshipHash : uint
        {
            Player = 0x6F0783F5,
            Civmale = 0x02B8FA80,
            Civfemale = 0x47033600,
            Cop = 0xA49E591C,
            SecurityGuard = 0xF50B51B7,
            PrivateSecurity = 0xA882EB57,
            Fireman = 0xFC2CA767,
            Gang1 = 0x4325F88A,
            Gang2 = 0x11DE95FC,
            Gang9 = 0x8DC30DC3,
            Gang10 = 0x0DBF2731,
            AmbientGangLost = 0x90C7DA60,
            AmbientGangMexican = 0x11A9A7E3,
            AmbientGangFamily = 0x45897C40,
            AmbientGangBallas = 0xC26D562A,
            AmbientGangMarabunte = 0x7972FFBD,
            AmbientGangCult = 0x783E3868,
            AmbientGangSalva = 0x936E7EFB,
            AmbientGangWeicheng = 0x6A3B9F86,
            AmbientGangHillbilly = 0xB3598E9C,
            Dealer = 0x8296713E,
            HatesPlayer = 0x84DCFAAD,
            Hen = 0xC01035F9,
            WildAnimal = 0x7BEA6617,
            Shark = 0x229503C8,
            Cougar = 0xCE133D78,
            NoRelationship = 0xFADE4843,
            Special = 0xD9D08749,
            Mission2 = 0x80401068,
            Mission3 = 0x49292237,
            Mission4 = 0x5B4DC680,
            Mission5 = 0x270A5DFA,
            Mission6 = 0x392C823E,
            Mission7 = 0x024F9485,
            Mission8 = 0x14CAB97B,
            Army = 0xE3D976F3,
            GuardDog = 0x522B964A,
            AggressiveInvestigate = 0xEB47D4E0,
            Medic = 0xB0423AA0,
            Prisoner = 0x7EA26372,
            DomesticAnimal = 0x72F30F6E,
            Deer = 0x31E50E10
        }

        public enum RawVehicleDrivingFlags : int
        {
            StopBeforeVehicle = 1,
            StopBeforePed = 2,
            AvoidVehicles = 4,
            AvoidEmptyVehicles = 8,
            AvoidPeds = 16,
            AvoidObjects = 32,
            StopAtTrafficLight = 128,
            UseBlinkers = 256,
            AllowGoingWrongWayIfNeeded = 512,
            DriveReverse = 1024,
            UseShortestPath = 262144,
            IgnoreRoads = 4194304,
            DontUsePathing = 16777216,
            AvoidHighwaysWhenPossible = 536870912
        }

        public enum CombinedVehicleDrivingFlags : int
        {
            Normal = RawVehicleDrivingFlags.StopBeforeVehicle | RawVehicleDrivingFlags.StopBeforePed |
                     RawVehicleDrivingFlags.AvoidEmptyVehicles | RawVehicleDrivingFlags.AvoidObjects |
                     RawVehicleDrivingFlags.StopAtTrafficLight | RawVehicleDrivingFlags.UseBlinkers,

            Hurry = RawVehicleDrivingFlags.AvoidVehicles | RawVehicleDrivingFlags.AvoidEmptyVehicles |
                    RawVehicleDrivingFlags.AvoidObjects | RawVehicleDrivingFlags.StopAtTrafficLight |
                    RawVehicleDrivingFlags.UseBlinkers | RawVehicleDrivingFlags.AllowGoingWrongWayIfNeeded,

            Fleeing = RawVehicleDrivingFlags.AvoidVehicles | RawVehicleDrivingFlags.AvoidEmptyVehicles |
                      RawVehicleDrivingFlags.AvoidPeds | RawVehicleDrivingFlags.AvoidObjects |
                      RawVehicleDrivingFlags.AllowGoingWrongWayIfNeeded,

            Emergency = RawVehicleDrivingFlags.AvoidVehicles | RawVehicleDrivingFlags.AvoidEmptyVehicles |
                        RawVehicleDrivingFlags.AvoidPeds | RawVehicleDrivingFlags.AvoidObjects |
                        RawVehicleDrivingFlags.AllowGoingWrongWayIfNeeded | RawVehicleDrivingFlags.UseShortestPath
        }
    }
}
