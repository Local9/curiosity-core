﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Curiosity.Systems.Library.Data
{
    public class MusicEvent
    {
        public eMusicEvents Start;
        public eMusicEvents Stop;

        public MusicEvent(eMusicEvents start, eMusicEvents end)
        {
            Start = start;
            Stop = end;
        }
    }

    public static class MusicEvents
    {
        public const Data.eMusicEvents DEFAULT_STOP = Data.eMusicEvents.AH1_STOP;

        public static List<MusicEvent> LoadingEvents = new List<MusicEvent>()
        {
            new MusicEvent(Data.eMusicEvents.AH1_START, Data.eMusicEvents.AH1_STOP),
            new MusicEvent(Data.eMusicEvents.AH3A_START, Data.eMusicEvents.AH3A_STOP),
            new MusicEvent(Data.eMusicEvents.AHP1_START, Data.eMusicEvents.AHP1_STOP),
            new MusicEvent(Data.eMusicEvents.APT_SUDDEN_DEATH_START_MUSIC, Data.eMusicEvents.APT_SUDDEN_DEATH_MUSIC_END),
            new MusicEvent(Data.eMusicEvents.APT_YA_START_ATTACK, Data.eMusicEvents.APT_YA_STOP),
            new MusicEvent(Data.eMusicEvents.APT_YA_START_DEFEND, Data.eMusicEvents.APT_YA_STOP),
            new MusicEvent(Data.eMusicEvents.BG_ASSAULT_START, Data.eMusicEvents.BG_ASSAULT_STOP),
            new MusicEvent(Data.eMusicEvents.BG_FINDERS_KEEPERS_START, Data.eMusicEvents.BG_FINDERS_KEEPERS_STOP),
            new MusicEvent(Data.eMusicEvents.BG_SIGHTSEER_START_ATTACK, Data.eMusicEvents.BG_FINDERS_KEEPERS_STOP),
            new MusicEvent(Data.eMusicEvents.BST_START, Data.eMusicEvents.BST_STOP),
            new MusicEvent(Data.eMusicEvents.DH1_START, Data.eMusicEvents.DH1_STOP),
            new MusicEvent(Data.eMusicEvents.DHP1_START, Data.eMusicEvents.DHP1_STOP),
            new MusicEvent(Data.eMusicEvents.EPS1_START, Data.eMusicEvents.EPS1_STOP),
            new MusicEvent(Data.eMusicEvents.EPS2_START, Data.eMusicEvents.EPS2_STOP),
            new MusicEvent(Data.eMusicEvents.EPS3_START, Data.eMusicEvents.EPS3_STOP),
            new MusicEvent(Data.eMusicEvents.EPS4_START, Data.eMusicEvents.EPS4_STOP),
            new MusicEvent(Data.eMusicEvents.EPS5_START, Data.eMusicEvents.EPS5_STOP),
            new MusicEvent(Data.eMusicEvents.EPS6_START, Data.eMusicEvents.EPS6_STOP),
            new MusicEvent(Data.eMusicEvents.EPS7_START, Data.eMusicEvents.EPS7_STOP),
            new MusicEvent(Data.eMusicEvents.FHPRA_START, Data.eMusicEvents.FHPRA_STOP),
            new MusicEvent(Data.eMusicEvents.FHPRB_START, Data.eMusicEvents.FHPRB_STOP),
            new MusicEvent(Data.eMusicEvents.FHPRD_START, Data.eMusicEvents.FHPRD_STOP),
            new MusicEvent(Data.eMusicEvents.JHP1B_START, Data.eMusicEvents.JHP1B_STOP),
            new MusicEvent(Data.eMusicEvents.JHP2A_START, Data.eMusicEvents.JHP2A_STOP),
            new MusicEvent(Data.eMusicEvents.MGPS_START, Data.eMusicEvents.MGPS_STOP),
            new MusicEvent(Data.eMusicEvents.MGSR_START, Data.eMusicEvents.MGSR_STOP),
            new MusicEvent(Data.eMusicEvents.MP_MC_START, Data.eMusicEvents.MP_MC_STOP),
            new MusicEvent(Data.eMusicEvents.OJBJ_START, Data.eMusicEvents.OJBJ_STOP),
            new MusicEvent(Data.eMusicEvents.PAP2_START, Data.eMusicEvents.PAP2_STOP),
            new MusicEvent(Data.eMusicEvents.PROP_INTRO_START, Data.eMusicEvents.PROP_INTRO_STOP),
            new MusicEvent(Data.eMusicEvents.PTP_START, Data.eMusicEvents.PTP_STOP),
            new MusicEvent(Data.eMusicEvents.RAMPAGE_1_START, Data.eMusicEvents.RAMPAGE_STOP),
            new MusicEvent(Data.eMusicEvents.RAMPAGE_2_START, Data.eMusicEvents.RAMPAGE_STOP),
            new MusicEvent(Data.eMusicEvents.RAMPAGE_3_START, Data.eMusicEvents.RAMPAGE_STOP),
            new MusicEvent(Data.eMusicEvents.RAMPAGE_4_START, Data.eMusicEvents.RAMPAGE_STOP),
            new MusicEvent(Data.eMusicEvents.RAMPAGE_5_START, Data.eMusicEvents.RAMPAGE_STOP),
            new MusicEvent(Data.eMusicEvents.RC18A_START, Data.eMusicEvents.RC18A_STOP),
        };

        public static List<eMusicEvents> eMusicEvents => Enum.GetValues(typeof(eMusicEvents)).Cast<eMusicEvents>().ToList();
    }

    public enum eMusicEvents
    {
        AC_DELIVERED,
        AC_DONE,
        AC_EN_ROUTE_CULT,
        AC_END,
        AC_LEFT_AREA,
        AC_START,
        AC_STOP,
        AH1_BACK_IN_CAR,
        AH1_FAIL,
        AH1_HOLE_RESTART,
        AH1_RESTART,
        AH1_START,
        AH1_STOP,
        AH2A_EXIT_SITE,
        AH2A_FIRST_FLOOR_RESTART,
        AH2A_FLEE_SITE,
        AH2A_MISSION_FAIL,
        AH2A_MISSION_START,
        AH3A_2ND_STAIRWELL,
        AH3A_ABSEIL_DONE,
        AH3A_ABSEIL_RT,
        AH3A_ABSEILING,
        AH3A_BLOW_BACK,
        AH3A_C4_PLANTED,
        AH3A_DETONATE,
        AH3A_DOOR_OPEN,
        AH3A_DOORS_EXPLODE,
        AH3A_DOWN_ONE,
        AH3A_ELEV_CS,
        AH3A_EXIT,
        AH3A_EXIT_LIFT,
        AH3A_EXIT_TRUCK,
        AH3A_FIB_DOCS_RT,
        AH3A_FIRE_FLOOR_RT,
        AH3A_FIRST_BOMB,
        AH3A_FLOOR_CRACK,
        AH3A_GET_DOCS,
        AH3A_GET_TO_ELEV,
        AH3A_HEAD_TO_LOO,
        AH3A_INTO_FLAMES,
        AH3A_LAST_BOMB,
        AH3A_LEAVING,
        AH3A_LIFT_CCTV,
        AH3A_LIFT_LOOK,
        AH3A_MOP_RETURNED,
        AH3A_MOP_RT,
        AH3A_MORE_MOPPING,
        AH3A_RUBBLE_RT,
        AH3A_SKYLIGHT,
        AH3A_STAIRWELL,
        AH3A_START,
        AH3A_START_ESCAPE,
        AH3A_STOP,
        AH3A_TRUCK_ENTERED,
        AH3B_AFTER_HELI_CS,
        AH3B_ALARM,
        AH3B_BURNTOUT_RT,
        AH3B_BURNTOUT_TWO_RT,
        AH3B_CHOPPER_APPEARS,
        AH3B_CHOPPER_DEAD,
        AH3B_COPS,
        AH3B_DATA_FINISHED,
        AH3B_DEAD,
        AH3B_DOOR_52,
        AH3B_DOWNLOADING_RT,
        AH3B_ENEMIES_ARRIVE,
        AH3B_ENEMY_DOWN,
        AH3B_ENTERED_BURN,
        AH3B_EVADE_COPS_RT,
        AH3B_GET_TO_VAN_RT,
        AH3B_HACK_RT,
        AH3B_HACKED_PC,
        AH3B_HALF_RAPPEL,
        AH3B_HELI_CRASHED,
        AH3B_HELI_CS,
        AH3B_HELI_FALLS,
        AH3B_HELI_HIT,
        AH3B_HELI_LIFT_OFF,
        AH3B_HELI_SHOOTS_HELI,
        AH3B_INSIDE_BUILDING,
        AH3B_JUMPED,
        AH3B_LANDED,
        AH3B_LOCKED_DOOR,
        AH3B_LOST_COPS,
        AH3B_LOST_COPS_VEH,
        AH3B_NO_WANTED_ESCAPE_RT,
        AH3B_ON_FLOOR,
        AH3B_ON_GROUND,
        AH3B_ON_PC,
        AH3B_RAPPEL_CS,
        AH3B_RAPPEL_RT,
        AH3B_RAPPEL_STARTS,
        AH3B_SKYDIVE_RT,
        AH3B_STAIRWELL,
        AH3B_STAIRWELL_RT,
        AH3B_VAN_ENTERED,
        AH3B_VAN_ENTERED_WANTED,
        AH3B_VAN_READY,
        AHP1_FAIL,
        AHP1_START,
        AHP1_STOP,
        APT_COUNTDOWN_30S,
        APT_COUNTDOWN_30S_KILL,
        APT_FADE_IN_RADIO,
        APT_PRE_COUNTDOWN_STOP,
        APT_SUDDEN_DEATH_MUSIC_END,
        APT_SUDDEN_DEATH_MUSIC_KILL,
        APT_SUDDEN_DEATH_START_MUSIC,
        APT_YA_ACTION,
        APT_YA_ATTACK,
        APT_YA_DEFENDING,
        APT_YA_START_ATTACK,
        APT_YA_START_DEFEND,
        APT_YA_STOP,
        APT_YA_STOP_LEAVE,
        ARM1_RADIO_OFF,
        ARM1_RADIO_ON,
        ARM2_MISSION_FAIL,
        ARM3_CALL,
        ARM3_CAR,
        ARM3_CS,
        ARM3_FAIL,
        ARM3_GARAGE_STOP,
        ARM3_HIT,
        ARM3_HIT_STOP,
        ARM3_MIC,
        ARM3_RESTART_1,
        ARM3_RESTART_2,
        ARM3_RESTART_3,
        ARM3_RESTART_4,
        ARM3_RESTART_5,
        ARM3_RESTART_6,
        ARM3_RESTART_7,
        ARM3_RESTART_8,
        ARM3_SPEED,
        ARM3_START,
        ARM3_WINDOW,
        ASS1_ALERT,
        ASS1_FAIL,
        ASS1_LOST,
        ASS1_RESTART1,
        ASS1_START,
        ASS1_STOP,
        ASS3_COPS,
        ASS3_FAIL,
        ASS3_KILL,
        ASS3_RADIO_FADE_OUT,
        ASS3_RADIO_PASS,
        ASS3_RESTART1,
        ASS3_RESTART2,
        ASS3_START,
        ASS5_DRIVE,
        ASS5_FAIL,
        ASS5_KILL,
        ASS5_LIFT,
        ASS5_RESTART1,
        ASS5_RESTART2,
        ASS5_RESTART3,
        ASS5_ROOF,
        ASS5_START,
        ASS5_STOP,
        ASS5_TOP,
        BG_ASSAULT_COLLECT,
        BG_ASSAULT_DELIVER,
        BG_ASSAULT_START,
        BG_ASSAULT_STOP,
        BG_FINDERS_KEEPERS_START,
        BG_FINDERS_KEEPERS_STOP,
        BG_HUNT_STOP,
        BG_SIGHTSEER_FINAL,
        BG_SIGHTSEER_MID,
        BG_SIGHTSEER_START,
        BG_SIGHTSEER_START_ATTACK,
        BG_SIGHTSEER_STOP,
        BLUE_DOOR,
        BST_START,
        BST_STOP,
        CAR1_APPROACH,
        CAR1_BRIDGE,
        CAR1_CHASE_RESTART,
        CAR1_CHASE_START,
        CAR1_COP_BIKES,
        CAR1_COPS_RESTART,
        CAR1_MISSION_FAIL,
        CAR1_MISSION_RESTART,
        CAR1_MISSION_START,
        CAR1_PULL_OVER,
        CAR2_MISSION_FAIL,
        CAR2_STOP,
        CAR3_CAR_RESTART,
        CAR3_DELIVER,
        CAR3_DELIVER_RESTART,
        CAR3_DRIVE,
        CAR3_DROP,
        CAR3_ESCAPE_RESTART,
        CAR3_MISSION_FAIL,
        CAR3_MISSION_START,
        CAR3_SET_ALERT,
        CAR3_STOP_TRACK,
        CAR3_TRAILER,
        CAR3_TRAILER_RESTART,
        CAR4_CLIMB,
        CAR4_MISSION_FAIL,
        CAR4_RADIO_1,
        CAR4_RADIO_2,
        CAR4_RADIO_2_START_TRACK,
        CAR4_REVERSE,
        CAR4_TRUCK_RESTART,
        CHN1_AFTER_GRENADE_RT,
        CHN1_BACK_ROOF,
        CHN1_CAR_ARRIVES,
        CHN1_CS_SKIP,
        CHN1_ENEMIES_FLEE,
        CHN1_FAIL,
        CHN1_FINAL_CS,
        CHN1_FINAL_CS_SKIP,
        CHN1_FIRST_FLOOR,
        CHN1_FROM_LEFT,
        CHN1_G_LAUNCHER,
        CHN1_GAMEPLAY_STARTS,
        CHN1_HEAD_TO_BACK,
        CHN1_ICE_BIN,
        CHN1_LAST_GUYS,
        CHN1_NOW,
        CHN1_OUTSIDE_RT,
        CHN1_START,
        CHN1_WAVE_3_RT,
        CHN1_WAVE_ZERO_RT,
        CHN2_MISSION_FAIL,
        CHN2_TREV_RADIO_1_FRTA,
        CHN2_TREV_RADIO_2_FRTA,
        CUT_PIPE_END,
        CUT_PIPE_START,
        debug_stop_oneshot,
        DH1_START,
        DH1_STOP,
        DH2A_1ST_BOMB_RT,
        DH2A_1ST_SWITCH,
        dh2a_2nd_bomb_planted,
        DH2A_2ND_BOMB_RT,
        dh2a_3rd_bomb_planted,
        DH2A_3RD_BOMB_RT,
        DH2A_4TH_BOMB_RT,
        DH2A_ALL_CLEAR,
        DH2A_CHOPPER_DEAD,
        dh2a_clear_path,
        dh2a_dead,
        DH2A_DETONATE,
        DH2A_DETONATE_RT,
        DH2A_DIVER,
        dh2a_double_guards,
        dh2a_early_alarm,
        DH2A_FINAL_EXP,
        DH2A_FRANK_JUMPS,
        DH2A_GOODS_RT,
        dh2a_main_alarm,
        DH2A_MINISUB,
        DH2A_MISSION_COMPLETE,
        dh2a_mission_complete,
        DH2A_ON_BRIDGE,
        DH2A_READY_FOR_2ND,
        DH2A_RIB,
        DH2A_SNIPE_GUARDS_RT,
        DH2A_START,
        DH2A_WAY_OUT_RT,
        DH2B_ALL_DEAD,
        DH2B_BOATS,
        DH2B_CLEAR_MERRY,
        DH2B_DROP_SUB_RT,
        DH2B_FAIL,
        DH2B_FIND_CONT_RT,
        DH2B_FLY_AWAY_RT,
        DH2B_FLY_SEA_RT,
        DH2B_GET_SURFACE_RT,
        DH2B_GOT_CONTAINER,
        DH2B_GOT_SUB,
        DH2B_GOT_SUB_2ND,
        DH2B_HELIS_ARRIVE,
        DH2B_PICK_SUB_RT,
        DH2B_PICK_UP_RT,
        DH2B_START,
        DH2B_SUB_RETURNED,
        DH2B_TREV_SUB,
        DHP1_ATTACKED,
        DHP1_FAIL,
        DHP1_RELEASED,
        DHP1_START,
        DHP1_STOP,
        DHP1_SUB,
        DHP1_VEHICLE_ARRIVE,
        DROPZONE_ACTION,
        DROPZONE_ACTION_HIGH,
        DROPZONE_HELI,
        DROPZONE_JUMP,
        DROPZONE_LAND,
        EPS1_FAIL,
        EPS1_START,
        EPS1_STOP,
        EPS2_FAIL,
        EPS2_START,
        EPS2_STOP,
        EPS3_START,
        EPS3_STOP,
        EPS4_START,
        EPS4_STOP,
        EPS5_START,
        EPS5_STOP,
        EPS6_START,
        EPS6_STOP,
        EPS7_START,
        EPS7_STOP,
        EPS8_ESCAPE,
        EPS8_FAIL,
        EPS8_PASS,
        EPS8_START,
        EPS_FAIL,
        EXL1_CARGO_DOORS_OPEN,
        EXL1_JUMPED,
        EXL1_LAND_IN_CARGO_PLANE,
        EXL1_MISSION_FAILED,
        EXL2_DEER,
        EXL2_FLY_HELI_RT,
        EXL2_HELI_CS,
        EXL2_HELI_LIFT,
        EXL2_MISSION_FAIL,
        EXL2_ON_FOOT,
        EXL2_ON_FOOT_RT,
        EXL2_RPG_DEAD,
        EXL2_RPG_FIRED,
        EXL2_RPG_HELI,
        EXL2_SNIPE_RT,
        EXL2_SNIPE_START,
        EXL2_SWITCH_START,
        EXL2_TRUCK,
        EXL3_BIKE_LAND,
        EXL3_FIGHT_OS,
        EXL3_MISSION_FAIL,
        EXL3_RAPIDS_START,
        EXL3_SEE_TRAIN,
        EXL3_STOP,
        EXL3_SWITCH_1,
        EXL3_TUNNEL_EXIT,
        EXT4_JUMPED_OS,
        EXTREME1_BIKE,
        EXTREME1_CYCLE,
        EXTREME1_FAIL,
        EXTREME1_JUMP,
        EXTREME1_LAND,
        EXTREME1_RESTART1,
        EXTREME1_START,
        EXTREME1_STOP,
        EXTREME2_ENTER,
        EXTREME2_FAIL,
        EXTREME2_JUMP,
        EXTREME2_PARA,
        EXTREME2_READY,
        EXTREME2_RESTART1,
        EXTREME2_RESTART2,
        EXTREME2_RUNWAY,
        EXTREME2_START,
        EXTREME2_STOP,
        EXTREME3_FAIL,
        EXTREME3_RESTART1,
        EXTREME3_START,
        EXTREME3_STOP,
        FAM1_1ST_ENEMY_OS,
        FAM1_BROKE_CAR,
        FAM1_CLOSE_YACHT,
        FAM1_DO_CHASE_RT,
        FAM1_FADE_RADIO,
        FAM1_FAIL,
        FAM1_FRANK_JUMPS_RT,
        FAM1_FRANK_LEAPS,
        FAM1_FRANKLIN_JUMPS,
        FAM1_JIMMY_APPEARS_RT,
        FAM1_JIMMY_BOOM,
        FAM1_JIMMY_LANDS,
        FAM1_RADIO_START,
        FAM1_START,
        FAM2_CHASE_RT,
        FAM2_COMING,
        FAM2_COMPLETE,
        FAM2_CS_SKIP,
        FAM2_GRAB_NECK,
        FAM2_LOST_HIM,
        FAM2_NEAR_YACHT,
        FAM2_NECK_GRAB,
        FAM2_ON_JETSKI,
        FAM2_SHOOTING,
        FAM2_SPLASH,
        FAM2_STOP,
        FAM3_ARRIVE_HOUSE,
        FAM3_BALCONY,
        FAM3_CHASE_RESTART,
        FAM3_CHASE_START,
        FAM3_END,
        FAM3_HOOKED_UP,
        FAM3_HOUSE_COLLAPSE,
        FAM3_HOUSE_RESTART,
        FAM3_MEX_CHASE,
        FAM3_MEX_LOST,
        FAM3_MEX_RESTART,
        FAM3_MISSION_FAIL,
        FAM3_MISSION_START,
        FAM3_PULL_RESTART,
        FAM3_TRUCK_PULL,
        FAM4_CHASE_RESTART,
        FAM4_CHASE_START,
        FAM4_MISSION_FAIL,
        FAM4_MISSION_START,
        FAM4_STOP_TRACK,
        FAM5_YOGA_MOVE_START,
        FAM5_YOGA_MUSIC_CHANGE,
        FAM5_YOGA_MUSIC_CHANGE_DOWN,
        FAM5_YOGA_MUSIC_ENDS,
        FAM5_YOGA_MUSIC_RESTART,
        FAM5_YOGA_MUSIC_START,
        FANATIC2_FAIL,
        FANATIC2_RESTART1,
        FANATIC2_START,
        FANATIC2_STOP,
        FANATIC3_CYCLE,
        FANATIC3_FAIL,
        FANATIC3_RESTART1,
        FANATIC3_RESTART2,
        FANATIC3_RESTART3,
        FANATIC3_RUN,
        FANATIC3_START,
        FBI1_2ND_STAIRWELL,
        FBI1_ALARM,
        FBI1_CALL_NORTON,
        FBI1_COPS_LOST,
        FBI1_DEAD,
        FBI1_ESCAPE,
        FBI1_FIND_BODY,
        FBI1_GET_GUN,
        FBI1_JUMP,
        FBI1_LAND_TRUCK,
        FBI1_LEAK,
        FBI1_LIFT_ENEMY,
        FBI1_LOSE_COPS_START,
        FBI1_OUTSIDE_CORONERS_RT,
        FBI1_RADIO,
        FBI1_SHOOTOUT_HALFWAY_RT,
        FBI1_SHOOTOUT_RT,
        FBI1_STAIRWELL,
        FBI1_THREE_DEAD,
        FBI1_TOP_FLOOR,
        FBI1_WAKE_UP,
        FBI1_WAKE_UP_RT,
        FBI3_BACK_TO_MICHAEL,
        FBI3_FAIL,
        FBI3_MICHAEL_ARRIVES_1,
        FBI3_MICHAEL_ARRIVES_2,
        FBI3_MICHAEL_MUSIC_1,
        FBI3_MICHAEL_MUSIC_2,
        FBI3_START,
        FBI3_TORTURE,
        FBI3_TORTURE_DONE,
        FBI3_TORTURE_RT,
        FBI3_TORTURE_START,
        FBI4_COVER_RESTART,
        fbi4_DEPOT_STOP_TRACK,
        FBI4_ENTER_VEHICLE_MA,
        fbi4_EXPLODE_MA,
        FBI4_EXPLODE_RESTART_ST,
        FBI4_GETAWAY_RESTART,
        FBI4_MISSION_FAIL,
        fbi4_PARK_AMBULANCE_OS,
        FBI4_PETROL,
        FBI4_PETROL_EXPLODE,
        fbi4_PLANT_BOMB_MA,
        fbi4_PRE_TRUCK_RAM_MA,
        FBI4_RAM_OS,
        fbi4_SHOOTOUT_MA,
        fbi4_SHOOTOUT_MID_MA,
        FBI4_SWITCH_1,
        FBI4_SWITCH_BINOC_ST,
        fbi4_TRUCK_RAM_MA,
        FBI4_TRUCK_RAM_MA,
        fbi4_TRUCK_RAM_RESTART_ST,
        FBI5A_ALARM_MA,
        FBI5A_BLUE_DOOR,
        FBI5A_CHEM_START,
        FBI5A_CONTAINER,
        FBI5A_CUT_PIPE_END,
        FBI5A_CUT_PIPE_RESTART,
        FBI5A_CUT_PIPE_START,
        FBI5A_CUT_SWIM_UP,
        FBI5A_DIVE_OUT_MA,
        FBI5A_ENTER_LAB_STEALTH_ST,
        FBI5A_ENTER_LAB_STOP_TRACK,
        FBI5A_FIGHT_END_MA,
        FBI5A_FIGHT_RAMP_UP_MA,
        FBI5A_FIGHT_RESTART,
        FBI5A_FIGHT_START_MA,
        FBI5A_FLY_RESTART,
        FBI5A_FORKLIFT_RESTART,
        FBI5A_GET_CHEMICALS_MA,
        FBI5A_HELI_OS,
        FBI5A_HELI_RESTART,
        FBI5A_LIFT_EXIT,
        FBI5A_LIFT_RESTART,
        FBI5A_LOAD_CRATE_MA,
        FBI5A_MISSION_FAIL,
        FBI5A_MISSION_START_ST,
        FBI5A_STOP_TRACK,
        FBI5A_SWIM_UP,
        FBI5A_SWITCH_HELI_MA,
        FBI5A_TO_AIRPORT,
        FBI5A_TREV_RADIO_FRTA,
        FBI_04_HEAT_SOUNDS,
        FH1_END,
        FH1_FAIL,
        FH1_ONION86,
        FH1_TRUCKS,
        FH1_TRUCKS_2,
        FH2A_ACCESS_BANK_MA,
        FH2A_ACCESS_BANK_RESTART,
        FH2A_ARRIVE_BANK,
        FH2A_ARRIVE_BANK_2,
        FH2A_BANK_MID,
        FH2A_BANK_MID_RESTART,
        FH2A_CARS,
        FH2A_CARTS_MA,
        FH2A_CARTS_RESTART,
        FH2A_ENTER_BANK_MA,
        FH2A_ENTER_LIFT,
        FH2A_ENTER_TRUCK,
        FH2A_ENTER_TUNNEL,
        FH2A_FIGHT_DROP,
        FH2A_FIGHT_END,
        FH2A_FIGHT_MID,
        FH2A_FIGHT_PRE,
        FH2A_FIGHT_RESTART,
        FH2A_FINAL_DRIVE_RADIO,
        FH2A_GETAWAY_DRIVE_MA,
        FH2A_GETAWAY_RESTART,
        FH2A_GOLD,
        FH2A_JUMP_LAND_MA,
        FH2A_JUMP_START,
        FH2A_LEAVE_BANK_MA,
        FH2A_LEAVE_BANK_RESTART,
        FH2A_MISSION_END,
        FH2A_MISSION_FAIL,
        FH2A_MISSION_RESTART,
        FH2A_MISSION_START_OS,
        FH2A_MISSION_START_ST,
        FH2A_RADIO_FADE_OUT,
        FH2A_STOP_TRACK,
        FH2A_TRAFFIC_END,
        FH2A_TRAFFIC_RESTART,
        FH2A_TRAFFIC_START,
        FH2A_VAN_RESTART,
        FH2A_VAN_ST,
        FH2B_BOMBS_RESTART,
        FH2B_CARPARK,
        FH2B_CREW_ESCAPE,
        FH2B_DRILL_RESTART,
        FH2B_DRILL_START,
        FH2B_DROP_GOLD_RESTART,
        FH2B_DROPPED_RESTART,
        FH2B_ENTER_VEHICLE,
        FH2B_ESCAPE_RESTART,
        FH2B_EXPLODE,
        FH2B_FIGHT_1_RESTART,
        FH2B_FIGHT_START,
        FH2B_GOLD_DROPPED,
        FH2B_HELI_ARRIVE,
        FH2B_HELI_CHASE_RESTART,
        FH2B_HELI_RESTART,
        FH2B_LEAVE_BANK,
        FH2B_LEAVE_RESTART,
        FH2B_MISSION_END,
        FH2B_MISSION_FAIL,
        FH2B_MISSION_START,
        FH2B_NOOSE_FIGHT,
        FH2B_NOOSE_FIGHT_RESTART,
        FH2B_PARK_CUTTER,
        FH2B_PLANT_BOMBS,
        FH2B_SWITCH_2,
        FH2B_SWITCH_3,
        FH2B_TANKER,
        FH2B_VAN_START,
        FH2B_WALL_SMASH,
        FHPRA_FAIL,
        FHPRA_START,
        FHPRA_STOP,
        FHPRA_VAN,
        FHPRB_COPS,
        FHPRB_LOST,
        FHPRB_START,
        FHPRB_STOP,
        FHPRB_TRUCK,
        FHPRC_FAIL,
        FHPRD_END,
        FHPRD_FAIL,
        FHPRD_RESTART_1,
        FHPRD_RESTART_2,
        FHPRD_RESTART_3,
        FHPRD_SIDINGS,
        FHPRD_START,
        FHPRD_STOP,
        FHPRD_TRAIN,
        FIB2_DEATH_FAIL,
        FIB2_HELICOPTERS_APPROACHING,
        FIN1_AT_VEHICLES,
        FIN1_BEFORE_GUNS,
        FIN1_CS_SKIP,
        FIN1_FAIL,
        FIN1_GUNS_DONE,
        FIN1_PREP,
        FIN1_RADIO_FADE,
        FIN1_SHOOTOUT_1,
        FIN1_SHOOTOUT_2,
        FIN1_SHOOTOUT_3,
        FIN1_SHOOTOUT_4,
        FIN1_SO_1_RT,
        FIN1_SO_2_RT,
        FIN1_SO_3_RT,
        FIN1_SO_4_RT,
        FIN1_START,
        FIN1_SWAT_ARRIVE,
        FIN1_TREV_HELPED,
        FINA_CHASE,
        FINA_END,
        FINA_FAIL,
        FINA_KILL_RESTART,
        FINA_NITRO_CRASH,
        FINA_RESTART_CHASE,
        FINA_START,
        FINB_ARRIVE,
        FINB_CHASE,
        FINB_CHOOSE,
        FINB_CLIMB,
        FINB_FAIL,
        FINB_RESTART_ARRIVE,
        FINB_RESTART_CHASE,
        FINB_RESTART_CLIMB,
        FINB_RESTART_STEPS,
        FINB_START,
        FINB_STEPS,
        FINB_TOWER,
        FINC2_FAIL,
        FM_COUNTDOWN_10S,
        FM_COUNTDOWN_20S,
        FM_COUNTDOWN_30S,
        FM_COUNTDOWN_30S_FIRA,
        FM_COUNTDOWN_30S_KILL,
        FM_INTRO_DRIVE_END,
        FM_INTRO_DRIVE_START,
        FM_INTRO_START,
        FM_PRE_COUNTDOWN_30S,
        FM_SUDDEN_DEATH_START_MUSIC,
        FM_SUDDEN_DEATH_STOP_MUSIC,
        FRA0_BADDY,
        FRA0_BOY,
        FRA0_DISMOUNT,
        FRA0_FENCE,
        FRA0_FOUND,
        FRA0_MISSION_FAIL,
        FRA0_MOUNT,
        FRA0_OPEN_CAR,
        FRA0_SWITCH_1,
        FRA1_FIGHT_LEAVE,
        FRA1_FIGHT_LEAVE_RESTART,
        FRA1_FIGHT_RESTART,
        FRA1_FIGHT_START,
        FRA1_HUSTLER,
        FRA1_MISSION_FAIL,
        FRA1_MISSION_START,
        FRA1_SPEED,
        FRA1_SPEED_RESTART,
        FRA1_STOP_TRACK,
        FRA1_WATER_ARRIVE,
        FRA2_ALERTED,
        FRA2_ATTACK_RT,
        FRA2_CUT_LAMAR_RT,
        FRA2_END_ON_FOOT,
        FRA2_END_VEHICLE,
        FRA2_FAIL,
        FRA2_FLEE_RT,
        FRA2_GET_TO_LAMAR,
        FRA2_GOT_LAMAR,
        FRA2_HEAD_TO_POS,
        FRA2_IN_POSITION,
        FRA2_RETURN_LAMAR,
        FRA2_START,
        GA_KILL_ALERTED,
        GA_KILL_ALERTED_RS,
        GA_KILL_COMPLETE,
        GA_KILL_HALF,
        GA_KILL_HALF_RS,
        GA_KILL_LEAVE,
        GA_KILL_LEAVE_RS,
        GA_KILL_START,
        GA_LEAVE_AREA,
        GA_STOP,
        GLOBAL_KILL_MUSIC,
        GLOBAL_KILL_MUSIC_FADEIN_RADIO,
        GTA_ONLINE_STOP_SCORE,
        HALLOWEEN_FAST_STOP_MUSIC,
        HALLOWEEN_START_MUSIC,
        HEIST_CELEB_APARTMENT,
        HEIST_CELEB_STRIP_CLUB,
        HEIST_STATS_SCREEN_START,
        HEIST_STATS_SCREEN_STOP,
        HEIST_STATS_SCREEN_STOP_PREP,
        HELI_OS,
        JH1_FAIL,
        JH1_RESTART_1,
        JH1_RESTART_2,
        JH1_RESTART_3,
        JH1_START,
        JH1_STOP_TRACK_ACTION,
        JH1_STORE,
        JH2A_ARRIVE_DRAIN_MA,
        JH2A_ARRIVE_STOP_TRACK,
        JH2A_ENTER_SHOP_MA,
        JH2A_ENTER_SHOP_RESTART,
        JH2A_ENTER_TRUCK,
        JH2A_ENTER_TUNNEL_MA,
        JH2A_EXIT_SHOP_MA,
        JH2A_EXIT_SHOP_RESTART,
        JH2A_EXIT_TUNNEL_MA,
        JH2A_EXIT_TUNNEL_RESTART,
        JH2A_GAS_SHOP_MA,
        JH2A_GAS_SHOP_OS,
        JH2A_GAS_SHOP_RESTART,
        JH2A_JUMP_OS,
        JH2A_MISSION_FAIL,
        JH2A_MISSION_START_OS,
        JH2A_MISSION_START_ST,
        JH2A_ONTO_BIKE_MA,
        JH2A_ONTO_BIKE_RESTART,
        JH2A_RADIO_FADE,
        JH2A_STORM_DRAIN_MA,
        JH2A_TUNNEL_MID,
        JH2A_VEHICLE,
        JH2B_ARRIVE_STOP_TRACK,
        JH2B_ENTER_SHOP_MA,
        JH2B_ENTER_SHOP_RESTART,
        JH2B_ENTER_TRUCK,
        JH2B_ENTER_TUNNEL_MA,
        JH2B_EXIT_SHOP_MA,
        JH2B_EXIT_TUNNEL_MA,
        JH2B_EXIT_TUNNEL_RESTART,
        JH2B_JUMP_OS,
        JH2B_MISSION_FAIL,
        JH2B_MISSION_RESTART,
        JH2B_MISSION_START_ST,
        JH2B_ONTO_BIKE_MA,
        JH2B_ONTO_BIKE_RESTART,
        JH2B_RADIO_FADE,
        JH2B_SECURITY_MA,
        JH2B_START,
        JH2B_STORM_DRAIN_MA,
        JH2B_TUNNEL_MID,
        JH2B_VEHICLE,
        JHP1A_ATTACK,
        JHP1A_FAIL,
        JHP1A_RADIO_1,
        JHP1A_RADIO_2,
        JHP1A_START,
        JHP1A_VAN,
        JHP1A_WAREHOUSE,
        JHP1B_FAIL,
        JHP1B_START,
        JHP1B_STOP,
        JHP1B_VAN,
        JHP2A_FAIL,
        JHP2A_START,
        JHP2A_STOP,
        JOSH3_COPS,
        JOSH3_COPS_LOST,
        JOSH3_COPS_LOST_RADIO,
        JOSH3_HOUSE_EXPLODE,
        JOSH3_MISSION_FAIL,
        JOSH3_PETROL,
        JOSH3_RESTART1,
        JOSH3_START,
        JOSH4_ACTION,
        JOSH4_COPS_LOST,
        JOSH4_COPS_LOST_RADIO,
        JOSH4_MISSION_FAIL,
        JOSH4_RESTART1,
        JOSH4_START,
        JOSH4_VEHICLE,
        KILL_LIST_START_MUSIC,
        KILL_LIST_STOP_MUSIC,
        LIFT_EXIT,
        LM1_COPS_LOST_RADIO,
        LM1_TERMINADOR_1ST_DOOR_EXPLODES,
        LM1_TERMINADOR_2ND_DOOR_EXPLODES,
        LM1_TERMINADOR_ALL_WAREHOUSE,
        LM1_TERMINADOR_CLIMB_LADDER,
        LM1_TERMINADOR_CLIMB_LADDER_RESTART,
        LM1_TERMINADOR_CLUMSY_ASS,
        LM1_TERMINADOR_CORRIDOR,
        LM1_TERMINADOR_CS_DOORS,
        LM1_TERMINADOR_DOORS_OPEN,
        LM1_TERMINADOR_ENTER_CAR,
        LM1_TERMINADOR_ENTER_WAREHOUSE,
        LM1_TERMINADOR_ENTER_WAREHOUSE_RESTART,
        LM1_TERMINADOR_ENTERED_ROOM,
        LM1_TERMINADOR_EXIT_WAREHOUSE,
        LM1_TERMINADOR_GAMEPLAY_BEGINS,
        LM1_TERMINADOR_GAMEPLAY_BEGINS_RESTART,
        LM1_TERMINADOR_HALF_WAREHOUSE,
        LM1_TERMINADOR_HEAD_SHOT,
        LM1_TERMINADOR_LANDED,
        LM1_TERMINADOR_LOST_ON_FOOT,
        LM1_TERMINADOR_MISSION_FAIL,
        LM1_TERMINADOR_MISSION_START,
        LM1_TERMINADOR_SMOKE,
        LOWRIDER_FINALE_START_MUSIC,
        LOWRIDER_START_MUSIC,
        MGGF_START,
        MGGF_STOP,
        MGPS_FAIL,
        MGPS_START,
        MGPS_STOP,
        MGSP_END,
        MGSP_FAIL,
        MGSP_START,
        MGSR_BACK_ON,
        MGSR_FELL_OFF,
        MGSR_START,
        MGSR_STOP,
        MGTN_END,
        MGTN_START,
        MGTR_COMPLETE,
        MGTR_LAST_CYCLE,
        MGTR_LAST_FOOT,
        MGTR_LAST_SWIM,
        MGTR_MUSIC_START,
        MGTR_ON_BIKE,
        MGTR_ON_FOOT,
        MGTR_STOP,
        MGYG_END,
        MGYG_POSITION_COMPLETE,
        MGYG_START,
        MIC1_1ST_VAN,
        MIC1_ALERTED,
        MIC1_ARGUE_CS_SKIP,
        MIC1_ARRIVED_CHURCH,
        MIC1_DRIVE_TO_GRAVEYARD,
        MIC1_FAIL,
        MIC1_FIRST_TWO_DEAD,
        MIC1_FLIGHT_ARRIVING,
        MIC1_FLIGHT_LANDED,
        MIC1_FLY_HOME_RT,
        MIC1_GAMEPLAY_STARTS,
        MIC1_GRAVE_CS,
        MIC1_INTRO_CS_BEGINS,
        MIC1_KIDNAPPED,
        MIC1_LEFT_HOUSE,
        MIC1_PRE_MISSION_MUSIC,
        MIC1_READY_TO_FLY,
        MIC1_SHOOTOUT_RT,
        MIC1_SHOOTOUT_START,
        MIC1_SKIPPED_TO_KIDNAP,
        MIC1_TRAIN,
        MIC1_TREVOR_PLANE,
        MIC2_ABATTOIR_PROGRESS,
        MIC2_ACID_BATH_OS,
        MIC2_BACK_TO_FRANK,
        MIC2_DEAD,
        MIC2_FIGHT_BEGINS,
        MIC2_FIGHT_BEGINS_RT,
        MIC2_FIGHT_CONT,
        MIC2_FIND_A_WAY,
        MIC2_FIND_MIKE_RT,
        MIC2_FRANK_SAVED,
        MIC2_FRANK_VEH,
        MIC2_HANGING_MICHAEL,
        MIC2_HANGING_RT,
        MIC2_LOSE_TRIADS,
        MIC2_MICHAEL_ESCAPE_RT,
        MIC2_MULCHED,
        MIC2_OVER,
        MIC2_RADIO_SETUP,
        MIC2_SPINNING_BLADES,
        MIC2_START,
        MIC2_SWITCHED,
        MIC2_TRIADS_CHASE_RT,
        MIC2_TRIADS_LOST,
        MIC2_VEHICLE_READY,
        MIC3_CRASH,
        MIC3_DAVE_ESCAPES_RESTART,
        MIC3_ESCAPE,
        MIC3_ESCAPE_RESTART,
        MIC3_FIGHT_RESTART,
        MIC3_FIGHT_START,
        MIC3_FOUNTAIN_RESTART,
        MIC3_FRANK_DOWN,
        MIC3_HELI,
        MIC3_INTRO,
        MIC3_MEET,
        MIC3_MISSION_FAIL,
        MIC3_MISSION_START,
        MIC3_MT_FIGHT_RESTART,
        MIC3_SNIPE,
        MIC3_STEVE_SHOT,
        MIC3_STOP_TRACK,
        MIC3_TREV_HELI_RESTART,
        MIC3_VEHICLE_ESCAPE_RESTART,
        MICHAELS_HOUSE,
        MICHAELS_HOUSE_STOP,
        MM1_FAIL,
        MM1_STOP,
        MM2_FAIL,
        MM2_RESTART1,
        MM2_START_FORA,
        MM2_START_STA,
        MM2_STOP,
        MM3_FAIL,
        MM3_RESTART1,
        MM3_START_FORA,
        MM3_START_STA,
        MM3_STOP,
        MM3_TRACTOR,
        MP_DM_COUNTDOWN_30_SEC,
        MP_DM_COUNTDOWN_30_SEC_FIRA,
        MP_DM_COUNTDOWN_60_SEC_FIRA,
        MP_DM_COUNTDOWN_KILL,
        MP_DM_LAST,
        MP_DM_START_ALL,
        MP_DM_STOP_TRACK,
        MP_GLOBAL_RADIO_FADE_IN,
        MP_LTS,
        MP_MC_ACTION_HPREP,
        MP_MC_DANGERZONE,
        MP_MC_DZ_FADE_OUT_RADIO,
        MP_MC_DZ_FIRA,
        MP_MC_FAIL,
        MP_MC_GENERAL_1,
        MP_MC_RADIO_FADE,
        MP_MC_RADIO_OUT_SCORE_IN,
        MP_MC_START,
        MP_MC_START_BEYOND_4,
        MP_MC_START_BURNING_BAR_8,
        MP_MC_START_CAR_STEAL_CHIPS_2,
        MP_MC_START_CHOP_8,
        MP_MC_START_CITY,
        MP_MC_START_CITY_8,
        MP_MC_START_COCK_SONG_1,
        MP_MC_START_COUNTRY,
        MP_MC_START_DARK_ROBBERY_8,
        MP_MC_START_DEBUNKED_8,
        MP_MC_START_DIAMOND_DIARY_8,
        MP_MC_START_DR_DESTRUCTO_8,
        MP_MC_START_DRAGONER_8,
        MP_MC_START_EYE_IN_SKY_3,
        MP_MC_START_FUNK_JAM_3,
        MP_MC_START_FUNK_JAM_TWO_4,
        MP_MC_START_GREYHOUND_8,
        MP_MC_START_GUN_NOVEL_8,
        MP_MC_START_HEIST_4,
        MP_MC_START_HEIST_8,
        MP_MC_START_HEIST_FIN_NEW,
        MP_MC_START_HEIST_PREP_NEW,
        MP_MC_START_MEATY_8,
        MP_MC_START_MISSION_SEVEN_8,
        MP_MC_START_NINE_BLURT_8,
        MP_MC_START_NT_DEF_8,
        MP_MC_START_NT_ELC_8,
        MP_MC_START_NT_TKB_4,
        MP_MC_START_PB1_8,
        MP_MC_START_PB2_PUSSYFACE_8,
        MP_MC_START_SCRAP_YARD_8,
        MP_MC_START_SILVER_PUSSY_8,
        MP_MC_START_STREETS_OF_FORTUNE_8,
        MP_MC_START_TOUGHT_SEA_RACE_1,
        MP_MC_START_TRACK_EIGHT_8,
        MP_MC_START_VACUUM_8,
        MP_MC_START_VINEGAR_TITS_8,
        MP_MC_START_VODKA_8,
        MP_MC_START_WAVERY_1,
        MP_MC_STOP,
        MP_MC_VEHICLE_CHASE_HFIN,
        MP_PRE_COUNTDOWN_RADIO,
        MP_RADIO_FADE_IN,
        MP_RADIO_FADE_OUT,
        NIGEL1C_END,
        NIGEL1C_FAIL,
        NIGEL1C_FORA,
        NIGEL1C_START,
        NIGEL2_JUMP,
        OJBJ_JUMPED,
        OJBJ_JUMPED_MA,
        OJBJ_LANDED,
        OJBJ_START,
        OJBJ_STOP,
        OJBJ_STOP_TRACK,
        OJDA1_1ST_DROPPED,
        OJDA1_2ND_DROPPED,
        OJDA1_AIRBORNE,
        OJDA1_HATCH_OPEN,
        OJDA1_READY_2ND,
        OJDA1_START,
        OJDA1_TAXI,
        OJDA2_1ST_DROPPED,
        OJDA2_AIRBORNE,
        OJDA2_HEAD_BACK,
        OJDA2_READY_1ST,
        OJDA2_START,
        OJDA3_AIRBORNE,
        OJDA3_BOMB_HIT,
        OJDA3_HATCH,
        OJDA3_LAST_ONE,
        OJDA3_START,
        OJDA4_1_LEFT,
        OJDA4_AIRBORNE,
        OJDA4_BOATS,
        OJDA4_RETURN,
        OJDA4_START,
        OJDA4_TRAIN,
        OJDA4_TRAIN_HIT,
        OJDA5_AIRBORNE,
        OJDA5_AT_BASE,
        OJDA5_BASE_DESTROYED,
        OJDA5_FIRST_BOMBS,
        OJDA5_START,
        OJDA_COMPLETE,
        OJDA_STOP,
        OJDG1_ENEMIES_DEAD,
        OJDG1_GOING_LOST,
        OJDG1_GOING_WANTED,
        OJDG1_PACKAGE,
        OJDG1_SAFE_PACKAGE,
        OJDG1_START,
        OJDG2_1ST_SET_DEAD,
        OJDG2_FIRST_ENEMIES_DEAD,
        OJDG2_MORE_DEAD,
        OJDG2_MORE_ENEMIES,
        OJDG2_PACKAGE_OBTAINED,
        OJDG2_PACKAGE_STOLEN,
        OJDG2_START,
        OJDG2_TREV_FIRST,
        OJDG_COMPLETE,
        OJDG_STOP,
        PAP2_CAR,
        PAP2_CAR_RESTART,
        PAP2_FAIL,
        PAP2_SPOTTED,
        PAP2_SPOTTED_RESTART,
        PAP2_START,
        PAP2_STOP,
        PAP3_FAIL,
        PAP3_START,
        PAP3_START_FORA,
        PAP3_STOP,
        PENNED_IN_70_PERCENT,
        PENNED_IN_START_MUSIC,
        PENNED_IN_STOP_MUSIC,
        PEYOTE_TRIPS_START,
        PEYOTE_TRIPS_STOP,
        PRE_MP_DM_COUNTDOWN_30_SEC,
        PROLOGUE_TEST_AFTER_TRAIN,
        PROLOGUE_TEST_BLAST_DOORS_EXPLODE,
        PROLOGUE_TEST_BRAD_DOWN,
        PROLOGUE_TEST_CAR_CHASE,
        PROLOGUE_TEST_COLLECT_CASH,
        PROLOGUE_TEST_COLLECT_MONEY,
        PROLOGUE_TEST_COP_GUNFIGHT,
        PROLOGUE_TEST_COP_GUNFIGHT_PROGRESS,
        PROLOGUE_TEST_COP_GUNFIGHT_RT,
        PROLOGUE_TEST_COVER_AT_BLAST_DOORS,
        PROLOGUE_TEST_FAIL,
        PROLOGUE_TEST_FINAL_CUTSCENE,
        PROLOGUE_TEST_FINAL_CUTSCENE_MA,
        PROLOGUE_TEST_FINALE_RT,
        PROLOGUE_TEST_GETAWAY_CUTSCENE,
        PROLOGUE_TEST_GETAWAY_RT,
        PROLOGUE_TEST_GRAB_WOMAN,
        PROLOGUE_TEST_GUARD_HOSTAGE,
        PROLOGUE_TEST_GUARD_HOSTAGE_OS,
        PROLOGUE_TEST_GUARD_HOSTAGE_RT,
        PROLOGUE_TEST_GUARD_SWITCH,
        PROLOGUE_TEST_HEAD_TO_GETAWAY_VEHICLE,
        PROLOGUE_TEST_HEAD_TO_SECURITY_ROOM_MA,
        PROLOGUE_TEST_HOSTAGES,
        PROLOGUE_TEST_KILL_ONESHOT,
        PROLOGUE_TEST_MISSION_CLEANUP,
        PROLOGUE_TEST_MISSION_END,
        PROLOGUE_TEST_MISSION_START,
        PROLOGUE_TEST_POLICE_CAR_CHASE,
        PROLOGUE_TEST_POLICE_CAR_CHASE_OS,
        PROLOGUE_TEST_POLICE_CAR_CRASH,
        PROLOGUE_TEST_POLICE_DRIVE_BY,
        PROLOGUE_TEST_PRE_SAFE_EXPLOSION,
        PROLOGUE_TEST_ROADBLOCK_WARNING,
        PROLOGUE_TEST_SHUTTER_OPEN_OS,
        PROLOGUE_TEST_TRAIN_CRASH,
        PROP_INTRO_START,
        PROP_INTRO_STOP,
        PTP_START,
        PTP_STOP,
        RAMPAGE_1_OS,
        RAMPAGE_1_START,
        RAMPAGE_2_OS,
        RAMPAGE_2_START,
        RAMPAGE_3_OS,
        RAMPAGE_3_START,
        RAMPAGE_4_OS,
        RAMPAGE_4_START,
        RAMPAGE_5_OS,
        RAMPAGE_5_START,
        RAMPAGE_FAIL,
        RAMPAGE_STOP,
        RC18A_CS_SKIP_AFTER,
        RC18A_CS_SKIP_BEFORE,
        RC18A_END_OS,
        RC18A_INCREASE,
        RC18A_RESTART,
        RC18A_START,
        RC18A_STOP,
        RC18B_END,
        RC18B_START,
        RC6A_FAIL,
        RC6A_FINISH,
        RC6A_START,
        RE14A_FAIL,
        RE14A_PIPES,
        RE14A_SAFE,
        RE14A_START,
        RE20_END,
        RE20_FADE_RADIO_OUT,
        RE20_FAIL,
        RE20_START,
        RE28_OS,
        RE35_OS,
        RE51A_SHOP,
        RE6_BOTH_DEAD,
        RE6_BOTH_DEAD_OS,
        RE6_END,
        RE6_START,
        RE9_SPOTTED,
        RH1_FAIL,
        RH1_RACE,
        RH1_START,
        RH2A_BANK_RESTART,
        RH2A_CLUCK_ARRIVE,
        RH2A_CLUCK_ARRIVE_RESTART,
        RH2A_CLUCK_FIGHT_START,
        RH2A_ENTER_BANK,
        RH2A_ENTER_GATE,
        RH2A_FENCE,
        RH2A_FIGHT_MID,
        RH2A_FIGHT_PAUSE,
        RH2A_FIGHT_RAMP_UP,
        RH2A_FIGHT_START,
        RH2A_HELI_ARRIVE_RESTART,
        RH2A_MISSION_FAIL,
        RH2A_MISSION_START,
        RH2A_MOVE_AWAY_MA,
        RH2A_PAUSE_RESTART,
        RH2A_PICK_UP,
        RH2A_PLATFORM,
        RH2A_POST_HELI_CRASH_MA,
        RH2A_RADIO_ARRIVAL,
        RH2A_RESCUE_RESTART,
        RH2A_SHOOT_TANK,
        RH2A_STOP_TRACK,
        RH2A_SWITCH_1,
        RH2A_SWITCH_1_RESTART,
        RH2A_SWITCH_2,
        RH2A_SWITCH_2_RESTART,
        RH2A_SWITCH_3,
        RH2A_TRAIN,
        RH2A_TREV_DOOR,
        RH2A_TREV_FACE,
        RHP1_END,
        RHP1_FAIL,
        RHP1_START,
        RHP1_TRUCK,
        SOL1_1ST_ENEMY,
        SOL1_AIR_TRAFFIC,
        SOL1_ALMOST_CRASHED,
        SOL1_APP_ACTIVE,
        SOL1_BEGIN,
        SOL1_BUS_JUMP,
        SOL1_CHASE_PLANE_RT,
        SOL1_CRASH,
        SOL1_CRASHED_PLANE_RT,
        SOL1_DRIVE_TO_OBS_RT,
        SOL1_END,
        SOL1_ENDS,
        SOL1_ENGINE_HIT,
        SOL1_FAIL,
        SOL1_FIGHT_DONE,
        SOL1_FIGHT_RT,
        SOL1_FIST_FIGHT,
        SOL1_FRANKLIN_STARTS,
        SOL1_GAMEPLAY,
        SOL1_GET_SOL_RT,
        SOL1_GOT_IT,
        SOL1_HELI_ROOF,
        SOL1_SCARED_THEM,
        SOL1_SHOOT_PLANE_RT,
        SOL1_SNIPER_READY,
        SOL1_START,
        SOL1_START_FIGHT,
        SOL1_STEALTH_RT,
        SOL1_TAKE_OFF,
        SOL1_TRAIN_JUMP,
        SOL1_VEH,
        SOL2_CAR,
        SOL2_FAIL,
        SOL2_RESTART1,
        SOL2_START,
        SOL2_STOP,
        SOL5_AMANDA_SAVED,
        SOL5_BACK_TO_TRACEY,
        SOL5_BAD_GUYS,
        SOL5_ENDING_CS,
        SOL5_ENTER_HOUSE_RT,
        SOL5_FAIL,
        SOL5_FIGHT_BAD_RT,
        SOL5_FRONT_DOORS,
        SOL5_GAMEPLAY_RT,
        SOL5_GAMEPLAY_STARTS,
        SOL5_GROUND_FLOOR,
        SOL5_HOSTAGE_DEAD,
        SOL5_HOSTAGE_TAKER,
        SOL5_IN_DRIVEWAY,
        SOL5_LIMO_ENTERED,
        SOL5_LIMO_RADIO,
        SOL5_MICHAEL_CLOBBERED,
        SOL5_MORE_MERRY,
        SOL5_SAVE_A_RT,
        SOL5_SAVE_T_RT,
        SOL5_START,
        START_ELECTRONIC,
        START_RANDOM,
        START_ROCK,
        START_URBAN,
        SWIM_UP,
        TRV1_AT_CARAVAN,
        TRV1_BIKERS_FLEE,
        TRV1_CARAVAN_RT,
        TRV1_CHASE_BIKERS_RT,
        TRV1_CHASE_CS_SKIP,
        TRV1_CHASE_STARTS,
        TRV1_CHASING,
        TRV1_DRIVE_TRAILER_RT,
        TRV1_END_TRUCK,
        TRV1_EXPLODE,
        TRV1_FAIL,
        TRV1_ORTEGA_RT,
        TRV1_PUSH_TRAILER_RT,
        TRV1_RAM_TRAILER,
        TRV1_START,
        TRV1_THREATEN,
        TRV1_TRAILER,
        TRV1_TRAILER_SMASHED,
        TRV1_TRUCK,
        TRV2_FIGHT_START,
        TRV2_FLY,
        TRV2_FLY_RESTART,
        TRV2_GO_TO_RON,
        TRV2_MISSION_END,
        TRV2_MISSION_FAIL,
        TRV2_MISSION_START,
        TRV2_RACE,
        TRV2_RACE_RESTART,
        TRV2_SNIPE_RESTART,
        TRV2_STEAL_PLANE_RESTART,
        TRV2_TO_PLANE,
        TRV2_TOWER_RESTART,
        TRV2_WING_PLANE,
        TRV2_WING_RESTART,
        TRV3_FAIL,
        TRV4_AIRPORT_ENTERED,
        TRV4_CAR_ENTERED,
        TRV4_CHASE,
        TRV4_COPS_LOST,
        TRV4_EVADE_RT,
        TRV4_EXIT_CARS,
        TRV4_FAIL,
        TRV4_FOOT_CHASE_RT,
        TRV4_GAMEPLAY_START,
        TRV4_JET_ENTERED,
        TRV4_LOSE_COPS,
        TRV4_RUN,
        TRV4_START,
        TRV4_START_CS_SKIP,
        TRV4_START_RT,
        TRV4_SUCK_CS,
        VAL2_COUNTDOWN_30S,
        VAL2_COUNTDOWN_30S_KILL,
        VAL2_FADE_IN_RADIO,
        VAL2_PRE_COUNTDOWN_STOP
    }
}
