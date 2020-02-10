namespace Curiosity.Systems.Library.Models
{
    public class CharacterLifeStyle
    {
        public const string CHAR_CREATOR_SLEEP = "CHAR_CREATOR_SLEEP";
        public const string CHAR_CREATOR_FAMILY = "CHAR_CREATOR_FAMILY";
        public const string CHAR_CREATOR_SPORT = "CHAR_CREATOR_SPORT";
        public const string CHAR_CREATOR_LEGAL = "CHAR_CREATOR_LEGAL";
        public const string CHAR_CREATOR_TV = "CHAR_CREATOR_TV";
        public const string CHAR_CREATOR_PARTY = "CHAR_CREATOR_PARTY";
        public const string CHAR_CREATOR_ILLEGAL = "CHAR_CREATOR_ILLEGAL";

        public int Asleep { get; set; } = 2;
        public int Family { get; set; } = 0;
        public int Sport { get; set; } = 0;
        public int Legal { get; set; } = 0;
        public int Tv { get; set; } = 0;
        public int Party { get; set; } = 0;
        public int Illegal { get; set; } = 0;
    }
}