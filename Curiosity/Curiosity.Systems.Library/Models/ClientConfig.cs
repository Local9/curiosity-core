using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Curiosity.Systems.Library.Models
{
    [DataContract]
    public class ClientConfig
    {
        [DataMember(Name = "jobs")]
        public List<Job> Jobs;

        [DataMember(Name = "outfits")]
        public List<Outfit> Outfits;

        [DataMember(Name = "suppressedVehicles")]
        public List<string> VehiclesToSuppress;

        [DataMember(Name = "partyPeds")]
        public List<string> PartyPeds;

        [DataMember(Name = "supporter")]
        public Supporter Supporter;

        [DataMember(Name = "milos")]
        public Milos Milos;

        [DataMember(Name = "propsToDelete")]
        public List<string> PropsToDelete;

        [DataMember(Name = "eletricVehicles")]
        public List<string> EletricVehicles;
    }

    [DataContract]
    public class Supporter
    {
        [DataMember(Name = "companions")]
        public List<Companion> Companions;

        [DataMember(Name = "models")]
        public List<SupporterModel> SupporterModels;
    }

    [DataContract]
    public class Outfit
    {
        [DataMember(Name = "label")]
        public string Label;

        [DataMember(Name = "male")]
        public bool IsMale;

        [DataMember(Name = "components")]
        public List<OutfitComponent> Components;

        [DataMember(Name = "props")]
        public List<OutfitProp> Props;

        public override string ToString()
        {
            return Label;
        }
    }

    [DataContract]
    public class OutfitComponent
    {
        [DataMember(Name = "component")]
        public int Component;

        [DataMember(Name = "drawable")]
        public int Drawable;

        [DataMember(Name = "texture")]
        public int Texture;
    }

    [DataContract]
    public class OutfitProp
    {
        [DataMember(Name = "index")]
        public int Index;

        [DataMember(Name = "drawable")]
        public int Drawable;

        [DataMember(Name = "texture")]
        public int Texture;
    }


    [DataContract]
    public class Companion
    {
        [DataMember(Name = "label")]
        public string Label;

        [DataMember(Name = "hash")]
        public string Hash;

        [DataMember(Name = "human")]
        public bool Human;
    }


    [DataContract]
    public class SupporterModel
    {
        [DataMember(Name = "label")]
        public string Label;

        [DataMember(Name = "hash")]
        public string Hash;
    }

    [DataContract]
    public class Milos
    {
        [DataMember(Name = "activeIpls")]
        public List<string> ActiveIpls;

        [DataMember(Name = "cayoLOD")]
        public List<string> CayoLOD;

        [DataMember(Name = "losSantosLOD")]
        public List<string> LosSantosLOD;

        [DataMember(Name = "northYankton")]
        public List<string> NorthYankton;
    }

    public class Job
    {
        [DataMember(Name = "label")]
        public string Label;

        [DataMember(Name = "jobEvent")]
        public string JobEvent;

        [DataMember(Name = "legacyEvent")]
        public bool LegacyEvent;

        public override string ToString()
        {
            return this.Label;
        }
    }
}
