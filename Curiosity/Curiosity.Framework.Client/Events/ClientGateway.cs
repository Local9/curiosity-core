﻿using Lusive.Events;
using Lusive.Events.Diagnostics;
using Lusive.Events.Message;
using Lusive.Events.Serialization;
using Lusive.Events.Serialization.Implementations;

namespace Curiosity.Framework.Client.Events
{
    public class ClientGateway : BaseGateway
    {
        public List<NetworkMessage> Buffer { get; } = new List<NetworkMessage>();
        protected override ISerialization Serialization { get; }
        private string _signature;

        public ClientGateway()
        {
            Serialization = new BinarySerialization();
            DelayDelegate = async delay => await BaseScript.Delay(delay);
            PrepareDelegate = PrepareAsync;
            PushDelegate = Push;

            PluginManager.Instance.AddEventHandler(EventConstant.InboundPipeline, new Action<byte[]>(async serialized =>
            {
                try
                {
                    await ProcessInboundAsync(new ServerId(), serialized);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                }
            }));

            PluginManager.Instance.AddEventHandler(EventConstant.OutboundPipeline, new Action<byte[]>(serialized =>
            {
                try
                {
                    ProcessOutbound(serialized);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                }
            }));

            PluginManager.Instance.AddEventHandler(EventConstant.SignaturePipeline, new Action<string>(signature => _signature = signature));

            BaseScript.TriggerServerEvent(EventConstant.SignaturePipeline);
        }

        public async Task PrepareAsync(string pipeline, ISource source, IMessage message)
        {
            if (_signature == null)
            {
                var stopwatch = StopwatchUtil.StartNew();

                while (_signature == null)
                    await BaseScript.Delay(0);

                //Client.Logger.Debug($"[{message}] Halted {stopwatch.Elapsed.TotalMilliseconds}ms due to signature retrieval.");
            }

            message.Signature = _signature;
        }

        public void Push(string pipeline, ISource source, byte[] buffer)
        {
            if (source.Handle != -1)
                throw new Exception(
                    $"The client can only target server events. (arg {nameof(source)} is not matching -1)");

            BaseScript.TriggerServerEvent(pipeline, buffer);
        }


        public async void Send(string endpoint, params object[] args)
        {
            await SendInternal(EventFlowType.Straight, new ServerId(), endpoint, args);
        }

        public async Task<T> Get<T>(string endpoint, params object[] args)
        {
            return await GetInternal<T>(new ServerId(), endpoint, args);
        }
    }
}
