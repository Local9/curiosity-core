using Curiosity.Core.Client.Environment.Entities.Models;
using Curiosity.Core.Client.Managers.GameWorld.Properties.Data;

namespace Curiosity.Core.Client.Managers.GameWorld.Properties
{
    internal class PropertyHelper
    {
        public Scaleform CarStatScaleform = new Scaleform("MP_CAR_STATS_01");
        public Scaleform CarStatScaleform2 = new Scaleform("MP_CAR_STATS_02");
        public Scaleform CarStatScaleform3 = new Scaleform("MP_CAR_STATS_03");
        public Scaleform CarStatScaleform4 = new Scaleform("MP_CAR_STATS_04");
        public Scaleform CarStatScaleform5 = new Scaleform("MP_CAR_STATS_05");
        public Scaleform CarStatScaleform6 = new Scaleform("MP_CAR_STATS_06");
        public Scaleform CarStatScaleform7 = new Scaleform("MP_CAR_STATS_07");
        public Scaleform CarStatScaleform8 = new Scaleform("MP_CAR_STATS_08");
        public Scaleform CarStatScaleform9 = new Scaleform("MP_CAR_STATS_09");
        public Scaleform CarStatScaleform10 = new Scaleform("MP_CAR_STATS_10");

        public List<Vector5> GetParkingSpotByZone()
        {
            Vector3 pos = Game.PlayerPed.Position;
            string zone = GetNameOfZone(pos.X, pos.Y, pos.Z);
            switch (zone)
            {
                case "DOWNT":
                case "VINE":
                    return VechileGenerationPositions.DOWNT;
                case "GALLI":
                case "CHIL":
                    return VechileGenerationPositions.CHIL;
                case "DESRT":
                case "GREATC":
                    return VechileGenerationPositions.DESRT;
                case "CMSW":
                case "PALCOV":
                    return VechileGenerationPositions.CMSW;
                case "PBOX":
                    return VechileGenerationPositions.PBOX;
                case "SKID":
                    return VechileGenerationPositions.SKID;
                case "TEXTI":
                    return VechileGenerationPositions.TEXTI;
                case "LEGSQU":
                    return VechileGenerationPositions.LEGSQU;
                case "CANNY":
                    return VechileGenerationPositions.CANNY;
                case "DTVINE":
                    return VechileGenerationPositions.DTVINE;
                case "EAST_V":
                    return VechileGenerationPositions.EAST_V;
                case "MIRR":
                    return VechileGenerationPositions.MIRR;
                case "WVINE":
                    return VechileGenerationPositions.WVINE;
                case "ALTA":
                    return VechileGenerationPositions.ALTA;
                case "HAWICK":
                    return VechileGenerationPositions.HAWICK;
                case "RICHM":
                    return VechileGenerationPositions.RICHM;
                case "golf":
                    return VechileGenerationPositions.golf;
                case "ROCKF":
                    return VechileGenerationPositions.ROCKF;
                case "CCREAK":
                    return VechileGenerationPositions.CCREAK;
                case "RGLEN":
                    return VechileGenerationPositions.RGLEN;
                case "OBSERV":
                    return VechileGenerationPositions.OBSERV;
                case "DAVIS":
                    return VechileGenerationPositions.DAVIS;
                case "STRAW":
                    return VechileGenerationPositions.STRAW;
                case "CHAMH":
                    return VechileGenerationPositions.CHAMH;
                case "RANCHO":
                    return VechileGenerationPositions.RANCHO;
                case "BANNING":
                    return VechileGenerationPositions.BANNING;
                case "ELYSIAN":
                    return VechileGenerationPositions.ELYSIAN;
                case "TERMINA":
                    return VechileGenerationPositions.TERMINA;
                case "ZP_ORT":
                    return VechileGenerationPositions.ZP_ORT;
                case "LMESA":
                    return VechileGenerationPositions.LMESA;
                case "CYPRE":
                    return VechileGenerationPositions.CYPRE;
                case "EBURO":
                    return VechileGenerationPositions.EBURO;
                case "MURRI":
                    return VechileGenerationPositions.MURRI;
                case "VESP":
                    return VechileGenerationPositions.VESP;
                case "BEACH":
                    return VechileGenerationPositions.BEACH;
                case "VCANA":
                    return VechileGenerationPositions.VCANA;
                case "DELSOL":
                    return VechileGenerationPositions.DELSOL;
                case "DELBE":
                    return VechileGenerationPositions.DELBE;
                case "DELPE":
                    return VechileGenerationPositions.DELPE;
                case "LOSPUER":
                    return VechileGenerationPositions.LOSPUER;
                case "STAD":
                    return VechileGenerationPositions.STAD;
                case "KOREAT":
                    return VechileGenerationPositions.KOREAT;
                case "AIRP":
                    return VechileGenerationPositions.AIRP;
                case "MORN":
                    return VechileGenerationPositions.MORN;
                case "PBLUFF":
                    return VechileGenerationPositions.PBLUFF;
                case "BHAMCA":
                    return VechileGenerationPositions.BHAMCA;
                case "CHU":
                    return VechileGenerationPositions.CHU;
                case "TONGVAH":
                    return VechileGenerationPositions.TONGVAH;
                case "TONGVAV":
                    return VechileGenerationPositions.TONGVAV;
                case "TATAMO":
                    return VechileGenerationPositions.TATAMO;
                case "PALHIGH":
                    return VechileGenerationPositions.PALHIGH;
                case "NOOSE":
                    return VechileGenerationPositions.NOOSE;
                case "MOVIE":
                    return VechileGenerationPositions.MOVIE;
                case "SanAnd":
                    return VechileGenerationPositions.SanAnd;
                case "ALAMO":
                    return VechileGenerationPositions.ALAMO;
                case "JAIL":
                    return VechileGenerationPositions.JAIL;
                case "RTRAK":
                    return VechileGenerationPositions.RTRAK;
                case "SANCHIA":
                    return VechileGenerationPositions.SANCHIA;
                case "WINDF":
                    return VechileGenerationPositions.WINDF;
                case "PALMPOW":
                    return VechileGenerationPositions.PALMPOW;
                case "HUMLAB":
                    return VechileGenerationPositions.HUMLAB;
                case "ZQ_UAR":
                    return VechileGenerationPositions.ZQ_UAR;
                case "PALETO":
                    return VechileGenerationPositions.PALETO;
                case "PALFOR":
                    return VechileGenerationPositions.PALFOR;
                case "PROCOB":
                    return VechileGenerationPositions.PROCOB;
                case "HARMO":
                    return VechileGenerationPositions.HARMO;
                case "SANDY":
                    return VechileGenerationPositions.SANDY;
                case "ZANCUDO":
                    return VechileGenerationPositions.ZANCUDO;
                case "SLAB":
                    return VechileGenerationPositions.SLAB;
                case "NCHU":
                    return VechileGenerationPositions.NCHU;
                case "GRAPES":
                    return VechileGenerationPositions.GRAPES;
                case "MTGORDO":
                    return VechileGenerationPositions.MTGORDO;
                case "MTCHIL":
                    return VechileGenerationPositions.MTCHIL;
                case "GALFISH":
                    return VechileGenerationPositions.GALFISH;
                case "LAGO":
                    return VechileGenerationPositions.LAGO;
                case "ARMYB":
                    return VechileGenerationPositions.ARMYB;
                case "BURTON":
                    return VechileGenerationPositions.BURTON;
                default:
                    return new List<Vector5>();
            }
        }
    }
}
