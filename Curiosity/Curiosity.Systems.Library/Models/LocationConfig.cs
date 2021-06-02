using Curiosity.Systems.Library.Enums;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Curiosity.Systems.Library.Models
{
    [DataContract]
    public class LocationConfig
    {
        [DataMember(Name = "locations")]
        public List<Location> Locations = new List<Location>();
    }

    [DataContract]
    public class Location : LocationBlip
    {
        [DataMember(Name = "jobRequirement")]
        public string JobRequirement;

        [DataMember(Name = "markers")]
        public List<Marker> Markers = new List<Marker>();
    }

    [DataContract]
    public class LocationBlip
    {
        private int _priority = 1;
        private int _category = 1;

        [DataMember(Name = "positions")]
        public List<Position> Positions;

        [DataMember(Name = "spawns")]
        public List<Position> Spawners;

        [DataMember(Name = "name")]
        public string Name;

        [DataMember(Name = "sprite")]
        public int Sprite;

        [DataMember(Name = "color")]
        public int Color;

        [DataMember(Name = "isShortRange")]
        public bool IsShortRange;

        [DataMember(Name = "category")]
        public int Category
        {
            get
            {
                return _category;
            }
            set
            {
                switch (value)
                {
                    case 1:
                    case 2:
                    case 7:
                    case 10:
                    case 11:
                        _category = value;
                        break;
                    default:
                        _category = 1;
                        break;
                }
            }
        }

        [DataMember(Name = "scale")]
        public float Scale;

        [DataMember(Name = "priority")]
        public int Priority
        {
            get
            {
                return _priority;
            }
            set
            {
                if (value > 255)
                {
                    _priority = 255;
                }
                else if (value < 0)
                {
                    _priority = 0;
                }
                else
                {
                    _priority = value;
                }
            }
        }

        [DataMember(Name = "spawnType")]
        public SpawnType SpawnType;
    }
}
