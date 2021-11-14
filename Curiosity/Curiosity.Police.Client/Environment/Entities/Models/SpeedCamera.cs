using System.Runtime.Serialization;

namespace Curiosity.Police.Client.Environment.Entities.Models
{
    [DataContract]
    public class SpeedCamera
    {
        [DataMember(Name = "street")]
        public string Street;

        [DataMember(Name = "limit")]
        public int Limit;
    }
}
