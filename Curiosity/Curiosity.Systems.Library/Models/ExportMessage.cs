﻿using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Curiosity.Systems.Library.Models
{
    [DataContract]
    public class ExportMessage
    {
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        [DataMember(Name = "success", Order = 0)]
        public bool Success
        {
            get
            {
                return Error.Length == 0;
            }
        }

        [DataMember(Name = "error", Order = 1, EmitDefaultValue = false)]
        public string Error;

        [DataMember(Name = "newNumberValue", Order = 1, EmitDefaultValue = false)]
        public int NewNumberValue;
    }
}