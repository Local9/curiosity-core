using Curiosity.System.Client.Events;
using Newtonsoft.Json;
using System;

namespace Curiosity.System.Client.Package
{
    public class NetworkPayload<T>
    {
        public string Index { get; set; }
        public string Origin { get; set; }

        [JsonIgnore] private object Object { get; set; }

        [JsonProperty("Object")]
        public string Updator
        {
            get => JsonConvert.SerializeObject(Object);
            set
            {
                Object = JsonConvert.DeserializeObject(value, Type.GetType(Origin));
                Origin = Object.GetType().FullName;
            }
        }

        public T Get()
        {
            return Object != null ? (T)Object : default(T);
        }

        public void Update(T obj)
        {
            Object = obj;
            Origin = typeof(T).FullName;
        }

        public void UpdateAndCommit(T obj)
        {
            Update(obj);
            Commit();
        }

        public void Commit()
        {
            var package = NetworkPackage.GetModule();

            package.Commit(this);

            EventSystem.GetModule().Send("network:package:update", Index, JsonConvert.SerializeObject(this));
        }
    }
}