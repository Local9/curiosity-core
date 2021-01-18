using CitizenFX.Core;
using Curiosity.Systems.Library.Events;
using Curiosity.Systems.Library.Threading;
using Curiosity.Core.Server.Diagnostics;
using Curiosity.Core.Server.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Curiosity.Systems.Library;
using CitizenFX.Core.Native;
using System.Text;

namespace Curiosity.Core.Server.Events
{
    public class EventSystem : Manager<EventSystem>
    {
        private const string EVENT_KEY = DataKeys.SERVER_EVENT_KEY;

        public List<EventAttachment> Attachments { get; } = new List<EventAttachment>();
        public List<EventRequest> PendingRequests { get; } = new List<EventRequest>();

        public EventSystem()
        {
            Instance.EventRegistry[EVENT_KEY] += new Action<int, string>((handle, payload) =>
            {
                var wrapped = JsonConvert.DeserializeObject<Event>(payload.ToString());

                wrapped.Sender = handle;
                wrapped.Metadata.Sender = handle;

                try
                {
                    if (wrapped.Type == EventType.Request)
                    {
                        var firewall = true;

                        foreach (var attachment in Attachments.Where(self => self.Target == wrapped.Target))
                        {
                            if (attachment.Callback.GetType() == typeof(AsyncEventCallback))
                            {
                                Task.Factory.StartNew(async () =>
                                {
                                    wrapped.Type = EventType.Response;
                                    wrapped.Metadata.Write("__response",
                                        JsonConvert.SerializeObject(
                                            await ((AsyncEventCallback)attachment.Callback).AsyncTask(wrapped.Metadata)));

                                    Send(wrapped, handle);
                                });
                            }
                            else
                            {
                                wrapped.Type = EventType.Response;
                                wrapped.Metadata.Write("__response",
                                    JsonConvert.SerializeObject(attachment.Callback.Task(wrapped.Metadata)));

                                Send(wrapped, handle);
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
                                Task.Factory.StartNew(async () =>
                                {
                                    await ((AsyncEventCallback)attachment.Callback).AsyncTask(wrapped.Metadata);
                                });
                            }
                            else
                            {
                                attachment.Callback.Task(wrapped.Metadata);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Info($"[Events] An error occured when handling event `{wrapped.Target}`: {ex}");
                }
            });
        }

        public void SendAll(string target, params object[] payloads)
        {
            Send(Construct(target, -1, payloads), -1);
        }

        public void Send(string target, int handle, params object[] payloads)
        {
            Send(Construct(target, handle, payloads), handle);
        }

        public void Send(string target, int handle)
        {
            Send(Construct(target, handle, null), handle);
        }

        public void Send(Event wrapped, int handle)
        {
            string client =  handle == -1 ? "All Users" : $"{handle}";

            bool eventDebug = API.GetConvar("diagnostics_event_messages", "false") == "true";

            if (eventDebug)
            {
                Logger.Debug(
                    $"[{wrapped.Seed}] [{wrapped.Target}] Dispatching `{wrapped.Type}` operation to the client `{client}`.");
            }

            string jsonObject = JsonConvert.SerializeObject(wrapped);

            int fileSize = Encoding.UTF8.GetByteCount(jsonObject);
            bool latentEvent = fileSize > 4000;

            if (handle == -1)
            {
                if (latentEvent)
                {
                    BaseScript.TriggerLatentClientEvent(EVENT_KEY, 2000, jsonObject);
                }
                else
                {
                    BaseScript.TriggerClientEvent(EVENT_KEY, jsonObject);
                }
            }
            else
            {
                if (latentEvent)
                {
                    BaseScript.TriggerLatentClientEvent(PluginManager.PlayersList[handle], EVENT_KEY, 2000, jsonObject);
                }
                else
                {
                    BaseScript.TriggerClientEvent(PluginManager.PlayersList[handle], EVENT_KEY, jsonObject);
                }
            }
        }

        public Event Construct(string target, int handle, object[] payloads)
        {
            var wrapped = new Event
            {
                Target = target,
                Sender = -1
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

        public async Task<T> Request<T>(string target, int handle, params object[] payloads) where T : new()
        {
            var response = default(T);
            var task = new ThreadLock();
            var wrapped = new EventRequest(new EventCallback(metadata =>
            {
                Logger.Debug(
                    $"[{metadata.Inherit}] Got request response from client `{handle}` with metadata {JsonConvert.SerializeObject(metadata)}");

                try
                {
                    response = JsonConvert.DeserializeObject<T>(metadata.Find<string>("__response"));
                }
                catch (Exception)
                {
                    Logger.Info($"[{metadata.Inherit}] Event request response returned an invalid type.");
                }

                task.Unlock();

                return response;
            }))
            {
                Target = target,
                Sender = -1
            };

            if (payloads != null && payloads.Length > 0) WriteMetadata(wrapped, payloads);

            Send(wrapped, handle);

            PendingRequests.Add(wrapped);

            await task.Wait();

            return response;
        }

        public async Task<T> Request<T>(string target, int handle) where T : new()
        {
            return await Request<T>(target, handle, null);
        }

        public void Attach(string target, EventCallback callback)
        {
            Logger.Debug($"[Events] Attaching callback to `{target}`");

            Attachments.Add(new EventAttachment(target, callback));
        }
    }
}