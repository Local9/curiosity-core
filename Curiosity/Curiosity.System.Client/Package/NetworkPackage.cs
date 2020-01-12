using Curiosity.System.Client.Managers;
using Curiosity.System.Library.Events;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Curiosity.System.Client.Package
{
    public sealed class NetworkPackage : Manager<NetworkPackage>
    {
        public Dictionary<string, object> Payloads { get; set; } = new Dictionary<string, object>();
        public event Action<NetworkPackage, string> Imports;

        public NetworkPackage()
        {
            EventSystem.Attach("network:package:receive", new EventCallback(metadata =>
            {
                var index = metadata.Find<string>(0);
                var payload = JsonConvert.DeserializeObject<NetworkPayload<object>>(metadata.Find<string>(1));

                payload.Index = index;
                payload.Origin = payload.Get().GetType().FullName;

                Commit(payload);
                OnImports(index);

                return null;
            }));
        }

        public NetworkPayload<T> GetLoad<T>(string index)
        {
            Payloads.TryGetValue(index, out var load);

            NetworkPayload<T> final = null;

            try
            {
                final = (NetworkPayload<T>) load;
            }
            catch (Exception)
            {
                if (load != null)
                {
                    final = JsonConvert.DeserializeObject<NetworkPayload<T>>(JsonConvert.SerializeObject(
                        new Dictionary<string, object>
                        {
                            ["Origin"] = typeof(T).FullName,
                            ["Index"] = index,
                            ["Object"] =
                                JsonConvert.DeserializeObject<Dictionary<object, object>>(
                                    JsonConvert.SerializeObject(load))
                                [
                                    "Object"]
                        }));
                }
            }

            if (final == null)
            {
                final = new NetworkPayload<T>();
            }

            final.Index = index;

            return final;
        }

        public void Commit<T>(NetworkPayload<T> load)
        {
            if (Payloads.ContainsKey(load.Index)) Payloads[load.Index] = load;
            else Payloads.Add(load.Index, load);
        }

        private void OnImports(string index)
        {
            if (Imports == null) return;

            foreach (var import in Imports.GetInvocationList())
            {
                ((Action<NetworkPackage, string>) import).Invoke(this, index);
            }
        }
    }
}