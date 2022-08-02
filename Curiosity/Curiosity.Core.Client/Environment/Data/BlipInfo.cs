namespace Curiosity.Core.Client.Environment.Data
{
    public static class BlipInfo
    {
        public static int GetBlipSprite(this Model model)
        {
            switch (model.Hash)
            {
                //case 1181327175: // "akula":
                //    return (int)BlipSprite.Akula;
                //case -1523619738: // "alphaz1":
                //    return (int)BlipSprite.AlphaZ1;
                case 562680400: // "apc":
                    return 558; // (int)BlipSprite.APC; radar_gr_wvm_1
                case 682434785: // "boxville5":
                    return 529; // (int)BlipSprite.ArmoredBoxville;
                case -214455498: // "stockade3":
                case 1747439474: // "stockade":
                    return 67; // (int)BlipSprite.ArmoredTruck;
                case -2118308144: // "avenger":
                    return 589; // (int)BlipSprite.Avenger;
                case 1692272545: // "strikeforce":
                    return 640; // (int)BlipSprite.B11StrikeForce;
                //case "barrage":
                //    return (int)BlipSprite.Barrage;
                case -646416997: // "blimp":
                    return 401; //(int)BlipSprite.Blimp;
                case -1117353854: // "blimp2":
                case -307958377: // "blimp3":
                    return 638; // radar_blimp_2;
                case -32878452: // "bombushka":
                    return 585; // (int)BlipSprite.Bombushka;
                case 668439077: // "bruiser":
                case -1694081890: // "bruiser2":
                case -2042350822: // "bruiser3":
                    return 659; // (int)BlipSprite.Bruiser; 
                case 2139203625: // "brutus":
                case -1890996696: // "brutus2":
                case 2038858402: // "brutus3":
                    return 659; // (int)BlipSprite.Brutus; // radar_arena_brutus
                case 22097311: // "bus":
                case -362198621: // "airbus":
                case -2072933068: // "coach":
                    return 513; // radar_bus
                case -956048545: // "taxi":
                    return 198; // (int)BlipSprite.Cab;
                //case "caracara":
                //    return (int)BlipSprite.Caracara;
                case 638132286: // "cargobob":
                case 1621617168: // "cargobob2":
                case 1270502190: // "cargobob3":
                case -2046933067: // "cargobob4":
                    return 481; // (int)BlipSprite.Cargobob;
                //case "cerberus":
                //case "cerberus2":
                //case "cerberus3":
                //    return (int)BlipSprite.Cerberus;
                case -692292317: // "chernobog":
                    return 603; // (int)BlipSprite.Chernobog;
                //case "deathbike":
                //case "deathbike2":
                //case "deathbike3":
                //    return (int)BlipSprite.Deathbike;
                case 1483171323: // "deluxo":
                    return 596; // (int)BlipSprite.Deluxo; radar_nhp_wp2
                case -1693834937: // "dinghy":
                case 276773164: // "dinghy2":
                case 509498602: // "dinghy3":
                case 867467158: // "dinghy4":
                    return 404; // (int)BlipSprite.Dinghy;
                case -980573366: // dinghy5
                    return 754; // radar_dinghy2
                case -986944621: // "dominator3":
                    return 662; // (int)BlipSprite.Dominator;
                //case "dune3":
                //    return (int)BlipSprite.DuneFAV;
                case 345756458: // "pbus2":
                    return 631; // radar_bat_pbus
                case -1860900134: // "insurgent":
                case 2071877360: // "insurgent2":
                case -1924433270: // "insurgent3":
                    return 426; // (int)BlipSprite.GunCar;
                //case "halftrack":
                //    return (int)BlipSprite.HalfTrack;
                //case "havok":
                //    return (int)BlipSprite.Havok;
                //case "howard":
                //    return (int)BlipSprite.HowardNX25;
                //case "hunter":
                //    return (int)BlipSprite.Hunter;
                //case "impaler":
                //case "impaler2":
                //case "impaler3":
                //case "impaler4":
                //    return (int)BlipSprite.Impaler;
                //case "imperator":
                //case "imperator2":
                //case "imperator3":
                //    return (int)BlipSprite.Imperator;
                //case "issi3":
                //case "issi4":
                //case "issi5":
                //case "issi6":
                //    return (int)BlipSprite.Issi;
                case -1435527158: // "khanjali":
                    return 598; // BlipSprite.Khanjali; radar_nhp_wp4
                case -114627507: // "limo":
                    return 724; // (int)BlipSprite.Limo;
                //case "menacer":
                //    return (int)BlipSprite.Menacer;
                //case "mogul":
                //    return (int)BlipSprite.Mogul;
                //case "mule4":
                //    return (int)BlipSprite.MuleCustom;
                case 884483972: // "oppressor":
                    return 559; // (int)BlipSprite.Oppressor;
                case 2069146067: //"oppressor2":
                    return 639; // (int)BlipSprite.OppressorMkII;
                //case "nokota":
                //    return (int)BlipSprite.P45Nokota;
                case -1649536104:
                    return 528; // radar_ex_vech_1
                //case "pounder2":
                //    return (int)BlipSprite.PounderCustom;
                //case "pyro":
                //    return (int)BlipSprite.Pyro;
                case -827162039: // "dune4":
                case -312295511: // "dune5":
                    return 531; // radar_ex_vech_4
                //case "riot2":
                //    return (int)BlipSprite.RCV;
                case -286046740: // "rcbandito":
                    return 646; // (int)BlipSprite.RCVehicle;
                //case "voltic2":
                //    return (int)BlipSprite.RocketVoltic;
                //case "rogue":
                //    return (int)BlipSprite.Rogue;
                //case "ruiner2":
                //    return (int)BlipSprite.Ruiner2000;
                case 1721676810: // "monster3":
                case 840387324: // "monster4":
                case -715746948: // "monster5":
                    return 666; // (int)BlipSprite.Sasquatch;
                case -1146969353: // "scarab":
                case 1542143200: // "scarab2":
                case -579747861: // "scarab3":
                    return 667; // (int)BlipSprite.Scarab;
                case -638562243: // "scramjet":
                    return 634; //(int)BlipSprite.Scramjet;
                case -392675425: // "seabreeze":
                    return 584; // (int)BlipSprite.Seabreeze;
                case -1030275036: // "seashark":
                case -616331036: // "seashark2":
                case -311022263: // "seashark3":
                    return 471; // (int)BlipSprite.Seashark;
                case -726768679: // "seasparrow":
                case 1229411063: // "seasparrow2":
                case 1593933419: // "seasparrow3":
                    return 753; // (int)BlipSprite.SeaSparrow;
                //case "slamvan4":
                //case "slamvan5":
                //case "slamvan6":
                //    return (int)BlipSprite.Slamvam;
                //case "speedo4":
                //    return (int)BlipSprite.SpeedoCustom;
                //case "starling":
                //    return (int)BlipSprite.Starling;
                //case "stromberg":
                //    return (int)BlipSprite.Stromberg;
                case 771711535: // "submersible":
                case -1066334226: // "submersible2":
                    return 308; // (int)BlipSprite.Sub;
                case 976373367: // "rhino":
                    return 421; // (int)BlipSprite.Tank;
                //case "technical2":
                //    return (int)BlipSprite.TechnicalAqua;
                //case "terbyte":
                //    return (int)BlipSprite.Terrorbyte;
                //case "thruster":
                //    return (int)BlipSprite.Thruster;
                //case "towtruck":
                //    return (int)BlipSprite.TowTruck;
                //case "towtruck2":
                //    return (int)BlipSprite.TowTruck2;
                //case "tula":
                //    return (int)BlipSprite.Tula;
                //case "limo2":
                //    return (int)BlipSprite.TurretedLimo;
                //case "microlight":
                //    return (int)BlipSprite.Ultralight;
                //case "molotok":
                //    return (int)BlipSprite.V65Molotok;
                case -857356038: // "veto":
                    return 747;
                case -1492917079: // "veto2":
                    return 748;
                //case "volatol":
                //    return (int)BlipSprite.Volatol;
                //case "wastelander":
                //    return (int)BlipSprite.Wastelander;
                //case "tampa3":
                //    return (int)BlipSprite.WeaponizedTampa;
                case -1881846085: // "trailersmall2":
                    return 563; // (int)BlipSprite.WeaponizedTrailer;
                //case 540101442: // "zr380":
                //case -1106120762: // "zr3802":
                //case -1478704292: // "zr3803":
                //    return 262; // (int)BlipSprite.ZR380;
                case -210308634: // winky
                    return 745;
                case -808457413: // patriot
                case -420911112: // patriot
                case -670086588: // patriot
                    return 818; // radar_patriot3
                case 461465043: // jubilee
                    return 820; // radar_jubilee
                case -452768186: // granger
                case -261346873: // granger2
                    return 821; // radar_granger2
                case 1532171089: // deity
                    return 823; // radar_deity
                case -915234475: // champion
                    return 824; // radar_d_champion
                case -619930876: // buffalo4
                    return 825; // radar_buffalo4
                case 343511227:
                    return 800; // radar_crusader
                case 833469436:
                    return 799; // radar_slamvan2
                case -102335483: // squaddie
                    return 799; // radar_squadee
                case -276744698: // patrolboat
                    return 799; // radar_patrol_boat
            }

            if (model.IsBike)
                return 348;

            if (model.IsBoat)
                return 427;

            if (model.IsHelicopter)
                return 422;

            if (model.IsPlane)
                return 423;

            if (model.IsTrain)
                return 795; // radar_train

            return 225;
        }
    }
}
