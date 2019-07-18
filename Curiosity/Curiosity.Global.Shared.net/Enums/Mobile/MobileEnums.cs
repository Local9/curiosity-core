namespace Curiosity.Global.Shared.net.Enums.Mobile
{
    public enum View
    {
        Any = 0,
        HomeMenu = 1,
        Contacts = 2,
        CallScreen = 4,
        MessageList = 6,
        MessageView = 7,
        EmailList = 8,
        EmailView = 9,
        Settings = 13,
        ToDoList = 14,
        ToDoView = 15,
        MissionRepeat = 18,
        MissionStats = 19,
        JobList = 20,
        EmailResponse = 21,
        XYZData = 24,
        BossJobList = 25,
        BossJobView = 26,
        SecuroServHackingView = 27
    }

    public enum HomescreenNumber
    {
        One,
        Two,
        Three
    }

    public enum HomescreenLocation
    {
        TopLeft,
        TopMiddle,
        TopRight,
        MiddleLeft,
        Middle,
        MiddleRight,
        BottomLeft,
        BottomMiddle,
        BottomRight
    }
    public enum HomeIcon
    {
        Camera = 1,
        TextMessage,
        Calendar,
        Email,
        Call,
        Eyefind,
        Map,
        Apps,
        Media = 9,
        NewContact = 11,
        BAWSAQ = 13,
        Multiplayer,
        Music,
        GPS,
        Spare = 17,
        Settings2 = 24,
        MissedCall = 27,
        UnreadEmail,
        ReadEmail,
        ReplyEmail,
        ReplayMission,
        ShitSkip,
        UnreadSMS,
        ReadSMS,
        PlayerList,
        CopBackup,
        GangTaxi,
        RepeatPlay = 38,
        Sniper = 40,
        ZitIT,
        Trackify,
        Save,
        AddTag,
        RemoveTag,
        Location,
        Party = 47,
        Broadcast = 49,
        Gamepad = 50,
        InvitesPending = 52,
        OnCall,
    }

    public enum ListIcons
    {
        Attachment = 10,
        SideTasks = 12,
        RingTone = 18,
        TextTone = 19,
        VibrateOn = 20,
        VibrateOff = 21,
        Volume = 22,
        Settings1 = 23,
        Profile = 25,
        SleepMode = 26,
        Checklist = 39,
        Ticked = 48,
        Silent = 51
    }

    public enum Theme
    {
        LightBlue = 1,
        Green,
        Red,
        Orange,
        Grey,
        Purple,
        Pink,
        Black
    }

    public enum Direction
    {
        Up = 1,
        Right,
        Down,
        Left,
    }
    public enum BackgroundImage
    {
        Default = 0,
        None = 1, //2, 3
        PurpleGlow = 4,
        GreenSquares = 5,
        OrangeHerringBone = 6,
        OrangeHalfTone = 7,
        GreenTriangles = 8,
        GreenShards = 9,
        BlueAngles = 10,
        BlueShards = 11,
        BlueCircles = 12,
        Diamonds = 13,
        GreenGlow = 14,
        Orange8Bit = 15,
        OrangeTriangles = 16,
        PurpleTartan = 17
    }

    public enum SoftKey
    {
        Left = 1,
        Middle,
        Right
    }

    public enum SoftkeyIcon
    {
        Blank = 1,
        Select = 2,
        Pages = 3,
        Back = 4,
        Call = 5,
        Hangup = 6,
        Hangup_Human = 7,
        Hide_Phone = 8,
        Keypad = 9,
        Open = 10,
        Reply = 11,
        Delete = 12,
        Yes = 13,
        No = 14,
        Sort = 15,
        Website = 16,
        Police = 17,
        Ambulance = 18,
        Fire = 19,
        Pages2 = 20
    }
}
