using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Curiosity.Shared.Client.net.Models
{
    public class UiPlayerCharacter
    {
        [JsonProperty(PropertyName = "username")]
        public string Username;

        [JsonProperty(PropertyName = "userId")]
        public int UserId;

        [JsonProperty(PropertyName = "money")]
        public UiMoney Finance = new UiMoney();
    }

    public class UiMoney
    {
        [JsonProperty(PropertyName = "cash")]
        public int Cash;

        [JsonProperty(PropertyName = "bank")]
        public int Bank;
    }
}
