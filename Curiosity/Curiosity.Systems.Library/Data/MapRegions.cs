using System.Collections.Generic;

namespace Curiosity.Systems.Library.Data
{
    public class MapRegions
    {
        public readonly static Dictionary<SubRegion, Region> RegionBySubRegion = new Dictionary<SubRegion, Region>()
        {
            { SubRegion.AIRP, Region.SouthLosSantos },
            { SubRegion.ALAMO, Region.GrandSenoraDesert },
            { SubRegion.ALTA, Region.NorthLosSantos },
            { SubRegion.ARMYB, Region.Zancudo },
            { SubRegion.BANHAMC, Region.BeachCoastal },
            { SubRegion.BANHAMCA, Region.BeachCoastal },
            { SubRegion.BANNING, Region.SouthLosSantos },
            { SubRegion.BEACH, Region.BeachLosSantos },
            { SubRegion.BRADP, Region.NorthernMoutains },
            { SubRegion.BRADT, Region.NorthernMoutains },
            { SubRegion.BURTON, Region.NorthLosSantos },
            { SubRegion.CALAFAB, Region.GrandSenoraDesert },
            { SubRegion.CANNY, Region.Zancudo },
            { SubRegion.CCREAK, Region.Zancudo },
            { SubRegion.CHIL, Region.NorthLosSantosHills },
            { SubRegion.CHU, Region.BeachCoastal },
            { SubRegion.CMSW, Region.Paleto },
            { SubRegion.CYPRE, Region.SouthLosSantos },
            { SubRegion.DELBE, Region.BeachLosSantos },
            { SubRegion.DELPE, Region.NorthLosSantos },
            { SubRegion.DELSOL, Region.SouthLosSantos },
            { SubRegion.DESRT, Region.GrandSenoraDesert },
            { SubRegion.DTVINE, Region.NorthLosSantos },
            { SubRegion.EAST_V, Region.NorthLosSantos },
            { SubRegion.EBURO, Region.EasternValley },
            { SubRegion.ELGORL, Region.NorthernMoutains },
            { SubRegion.ELYSIAN, Region.SouthLosSantos },
            { SubRegion.GALFISH, Region.NorthernMoutains },
            { SubRegion.GOLF, Region.NorthLosSantos },
            { SubRegion.GRAPES, Region.GrandSenoraDesert },
            { SubRegion.GREATC, Region.NorthLosSantosHills },
            { SubRegion.HARMO, Region.GrandSenoraDesert },
            { SubRegion.HAWICK, Region.NorthLosSantos },
            { SubRegion.HORS, Region.NorthLosSantos },
            { SubRegion.HUMLAB, Region.GrandSenoraDesert },
            { SubRegion.JAIL, Region.GrandSenoraDesert },
            { SubRegion.KOREAT, Region.CentralLosSantos },
            { SubRegion.LACT, Region.NorthLosSantos },
            { SubRegion.LAGO, Region.Zancudo },
            { SubRegion.LDAM, Region.NorthLosSantos },
            { SubRegion.LEGSQU, Region.CentralLosSantos },
            { SubRegion.LMESA, Region.CentralLosSantos },
            { SubRegion.MIRR, Region.NorthLosSantos },
            { SubRegion.MORN, Region.NorthLosSantos },
            { SubRegion.MOVIE, Region.NorthLosSantos },
            { SubRegion.MTCHIL, Region.NorthernMoutains },
            { SubRegion.MTGORDO, Region.NorthernMoutains },
            { SubRegion.MTJOSE, Region.Zancudo },
            { SubRegion.MURRI, Region.CentralLosSantos },
            { SubRegion.NCHU, Region.Zancudo },
            { SubRegion.NOOSE, Region.EasternValley },
            { SubRegion.OCEANA, Region.Paleto },
            { SubRegion.PALCOV, Region.Paleto },
            { SubRegion.PALETO, Region.Paleto },
            { SubRegion.PALFOR, Region.Paleto },
            { SubRegion.PALHIGH, Region.EasternValley },
            { SubRegion.PALMPOW, Region.GrandSenoraDesert },
            { SubRegion.PBLUFF, Region.BeachLosSantos },
            { SubRegion.PBOX, Region.CentralLosSantos },
            { SubRegion.PROCOB, Region.Paleto },
            { SubRegion.RANCHO, Region.SouthLosSantos },
            { SubRegion.RGLEN, Region.NorthLosSantosHills },
            { SubRegion.RICHM, Region.NorthLosSantos },
            { SubRegion.ROCKF, Region.NorthLosSantos },
            { SubRegion.RTRACK, Region.GrandSenoraDesert },
            { SubRegion.SANAND, Region.SouthLosSantos },
            { SubRegion.SANCHIA, Region.GrandSenoraDesert },
            { SubRegion.SANDY, Region.GrandSenoraDesert },
            { SubRegion.SKID, Region.CentralLosSantos },
            { SubRegion.SLAB, Region.GrandSenoraDesert },
            { SubRegion.STRAW, Region.SouthLosSantos },
            { SubRegion.TATAMO, Region.EasternValley },
            { SubRegion.TERMINA, Region.SouthLosSantos },
            { SubRegion.TEXTI, Region.CentralLosSantos },
            { SubRegion.TONGVAH, Region.BeachCoastal },
            { SubRegion.TONGVAV, Region.NorthLosSantosHills },
            { SubRegion.VCANA, Region.BeachLosSantos },
            { SubRegion.VESP, Region.BeachLosSantos },
            { SubRegion.WINDF, Region.GrandSenoraDesert },
            { SubRegion.WVINE, Region.NorthLosSantos },
            { SubRegion.ZQ_UAR, Region.GrandSenoraDesert },
            { SubRegion.PROL, Region.NorthYankton },
            { SubRegion.ISHeist, Region.CayoPericoIsland }
        };
    }

    public enum Region
    {
        SouthLosSantos,
        CentralLosSantos,
        NorthLosSantos,
        BeachLosSantos,
        EasternValley,
        BeachCoastal,
        NorthLosSantosHills,
        GrandSenoraDesert,
        NorthernMoutains,
        Zancudo,
        Paleto,
        NorthYankton,
        CayoPericoIsland
    }

    public enum SubRegion
    {
        TERMINA,
        ELYSIAN,
        AIRP,
        BANNING,
        DELSOL,
        RANCHO,
        STRAW,
        CYPRE,
        SANAND,
        MURRI,
        LMESA,
        SKID,
        LEGSQU,
        TEXTI,
        PBOX,
        KOREAT,
        MIRR,
        EAST_V,
        DTVINE,
        ALTA,
        HAWICK,
        BURTON,
        ROCKF,
        MOVIE,
        DELPE,
        MORN,
        RICHM,
        GOLF,
        WVINE,
        HORS,
        LACT,
        LDAM,
        BEACH,
        VESP,
        VCANA,
        DELBE,
        PBLUFF,
        EBURO,
        PALHIGH,
        NOOSE,
        TATAMO,
        BANHAMC,
        BANHAMCA,
        CHU,
        TONGVAH,
        CHIL,
        GREATC,
        RGLEN,
        TONGVAV,
        PALMPOW,
        WINDF,
        RTRACK,
        JAIL,
        HARMO,
        DESRT,
        SANDY,
        ZQ_UAR,
        HUMLAB,
        SANCHIA,
        GRAPES,
        ALAMO,
        SLAB,
        CALAFAB,
        MTGORDO,
        ELGORL,
        BRADP,
        BRADT,
        MTCHIL,
        GALFISH,
        LAGO,
        ARMYB,
        NCHU,
        CANNY,
        MTJOSE,
        CCREAK,
        CMSW,
        PALCOV,
        OCEANA,
        PALFOR,
        PALETO,
        PROCOB,
        PROL,
        ISHeist,
        UNKNOWN
    }

    public enum SouthLosSantos
    {
        TERMINA,
        ELYSIAN,
        AIRP,
        BANNING,
        DELSOL,
        RANCHO,
        STRAW,
        CYPRE,
        SANAND
    }

    public enum CentralLosSantos
    {
        MURRI,
        LMESA,
        SKID,
        LEGSQU,
        TEXTI,
        PBOX,
        KOREAT
    }

    public enum NorthLosSantos
    {
        MIRR,
        EAST_V,
        DTVINE,
        ALTA,
        HAWICK,
        BURTON,
        ROCKF,
        MOVIE,
        DELPE,
        MORN,
        RICHM,
        GOLF,
        WVINE,
        HORS,
        LACT,
        LDAM
    }

    public enum BeachLosSantos
    {
        BEACH,
        VESP,
        VCANA,
        DELBE,
        PBLUFF
    }

    public enum EasternValley
    {
        EBURO,
        PALHIGH,
        NOOSE,
        TATAMO
    }

    public enum BeachCoastal
    {
        BANHAMC,
        BANHAMCA,
        CHU,
        TONGVAH
    }

    public enum NorthLosSantosHills
    {
        CHIL,
        GREATC,
        RGLEN,
        TONGVAV
    }

    public enum GrandSenoraDesert
    {
        PALMPOW,
        WINDF,
        RTRACK,
        JAIL,
        HARMO,
        DESRT,
        SANDY,
        ZQ_UAR,
        HUMLAB,
        SANCHIA,
        GRAPES,
        ALAMO,
        SLAB,
        CALAFAB
    }

    public enum NorthernMoutains
    {
        MTGORDO,
        ELGORL,
        BRADP,
        BRADT,
        MTCHIL,
        GALFISH
    }

    public enum Zancudo
    {
        LAGO,
        ARMYB,
        NCHU,
        CANNY,
        MTJOSE,
        CCREAK
    }

    public enum Paleto
    {
        CMSW,
        PALCOV,
        OCEANA,
        PALFOR,
        PALETO,
        PROCOB
    }

    public enum NorthYankton
    {
        PROL
    }

    public enum CayoPericoIsland
    {
        ISHeist
    }
}
