namespace Curiosity.Systems.Library.Enums
{
    /// <summary>
    /// The reason a mission ended, this is exclusively used for the Stop function
    /// </summary>
    public enum EndState
    {
        Unknown,
        Fail,
        Pass,
        Error,
        ForceEnd,
        Restart,
        FailPlayersDead,
        TrafficStop,
    }
}
