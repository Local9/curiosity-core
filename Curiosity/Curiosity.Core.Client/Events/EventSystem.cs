using CitizenFX.Core;
using CitizenFX.Core.Native;
using Curiosity.Core.Client.Diagnostics;
using Curiosity.Core.Client.Managers;
using Curiosity.Systems.Library;
using Curiosity.Systems.Library.Events;
using MsgPack.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Curiosity.Core.Client.Events
{
    public class EventSystem : Manager<EventSystem>
    {
        private const string EVENT_KEY = DataKeys.SERVER_EVENT_KEY;

        public List<EventAttachment> Attachments { get; } = new List<EventAttachment>();
        public List<EventRequest> PendingRequests { get; } = new List<EventRequest>();

        public EventSystem()
        {
            Instance.EventRegistry[EVENT_KEY] += new Action<byte[]>(payload =>
            {
                var wrapped = Deserialize<Event>(payload);

                if (wrapped.Type == EventType.Request)
                {
                    var firewall = true;

                    foreach (var attachment in Attachments.Where(self => self.Target == wrapped.Target))
                    {
                        if (attachment.Callback.GetType() == typeof(AsyncEventCallback))
                        {
                            wrapped.Type = EventType.Response;
                            wrapped.Metadata.Write("__response",
                                JsonConvert.SerializeObject(
                                    ((AsyncEventCallback)attachment.Callback).AsyncTask(wrapped.Metadata)));

                            Send(wrapped);
                        }
                        else
                        {
                            wrapped.Type = EventType.Response;
                            wrapped.Metadata.Write("__response",
                                JsonConvert.SerializeObject(attachment.Callback.Task(wrapped.Metadata)));

                            Send(wrapped);
                        }

                        firewall = false;

                        break;
                    }

                    if (firewall)
                        Logger.Info(
                            $"[{wrapped.Seed}] [{wrapped.Target}] [FIREWALL] Request did not get managed by the attacher.");
                }
                else if (wrapped.Type == EventType.Response)
                {
                    var found = PendingRequests.Where(self => self.Seed == wrapped.Seed).ToList();

                    found.ForEach(self => self.Callback.Task(wrapped.Metadata));

                    PendingRequests.RemoveAll(self => self.Seed == wrapped.Seed);
                }
                else if (wrapped.Type == EventType.Send)
                {
                    foreach (var attachment in Attachments.Where(self => self.Target == wrapped.Target))
                    {
                        if (attachment.Callback.GetType() == typeof(AsyncEventCallback))
                        {
#pragma warning disable 4014
                            ((AsyncEventCallback)attachment.Callback).AsyncTask(wrapped.Metadata);
#pragma warning restore 4014
                        }
                        else
                        {
                            attachment.Callback.Task(wrapped.Metadata);
                        }
                    }
                }
            });
        }

        public void Send(string target, params object[] payloads)
        {
            Send(Construct(target, payloads));
        }

        public void Send(string target)
        {
            Send(Construct(target, null));
        }

        public void Send(Event wrapped)
        {
            //Logger.Debug(
            //    $"[{wrapped.Seed}] [{wrapped.Target}] Dispatching `{wrapped.Type}` operation to the server-side.");

            byte[] package = Serialize<Event>(wrapped);

            BaseScript.TriggerServerEvent(EVENT_KEY, API.GetPlayerServerId(API.PlayerId()), package);
        }

        public Event Construct(string target, object[] payloads)
        {
            var wrapped = new Event
            {
                Target = target,
                Sender = API.GetPlayerServerId(API.PlayerId())
            };

            if (payloads != null) WriteMetadata(wrapped, payloads);

            return wrapped;
        }

        public void WriteMetadata(Event wrapped, IEnumerable<object> payloads)
        {
            var index = 0;

            foreach (var payload in payloads)
            {
                wrapped.Metadata.Write(index, payload);

                index++;
            }
        }

        public async Task<T> Request<T>(string target, params object[] payloads) where T : new()
        {
            var response = default(T);
            var completed = false;
            var wrapped = new EventRequest(new EventCallback(metadata =>
            {
                //Logger.Debug(
                //    $"[{metadata.Inherit}] Got request response from server-side with metadata {JsonConvert.SerializeObject(metadata)}");

                try
                {
                    response = JsonConvert.DeserializeObject<T>(metadata.Find<string>("__response"));
                }
                catch (Exception)
                {
                    Logger.Info($"[{metadata.Inherit}] Event request response returned an invalid type.");
                }

                completed = true;

                return response;
            }))
            {
                Target = target,
                Sender = Game.Player.ServerId
            };


            if (payloads != null && payloads.Length > 0) WriteMetadata(wrapped, payloads);

            Send(wrapped);

            PendingRequests.Add(wrapped);

            while (!completed)
            {
                await BaseScript.Delay(10);
            }

            return response;
        }

        public async Task<T> Request<T>(string target) where T : new()
        {
            return await Request<T>(target, null);
        }

        public void Attach(string target, EventCallback callback)
        {
            Logger.Debug($"[Events] Attaching callback to `{target}`");

            Attachments.Add(new EventAttachment(target, callback));
        }

        public byte[] Serialize<T>(T obj)
        {
            try
            {
                SerializationContext context = new SerializationContext { SerializationMethod = SerializationMethod.Map };
                MessagePackSerializer<T> serializer = MessagePackSerializer.Get<T>(context);
                using MemoryStream byteStream = new();
                serializer.Pack(byteStream, obj);
                return byteStream.ToArray();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                return default;
            }
        }

        public T Deserialize<T>(byte[] serializedObject)
        {
            try
            {
                SerializationContext context = new SerializationContext { SerializationMethod = SerializationMethod.Map };
                MessagePackSerializer<T> serializer = MessagePackSerializer.Get<T>(context);
                using MemoryStream byteStream = new(serializedObject);
                T result = serializer.Unpack(byteStream);
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                return default;
            }
        }
    }
}