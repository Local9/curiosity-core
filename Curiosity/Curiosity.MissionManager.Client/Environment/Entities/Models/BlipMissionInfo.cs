using Newtonsoft.Json;

namespace Curiosity.MissionManager.Client.Environment.Entities.Models
{
    public class BlipMissionInfo
    {
        public string Title;
        public int RockstarVerified;
        public dynamic Info;
        public string Money;
        public string Rp;
        public string TextureDictionary;
        public string TextureName;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }

    public class BlipMissionDetails
    {
        public int Index;
        public string Title;
        public string SubTitle;
        public int Icon;
        public int IconColor;
        public bool Completed;
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
