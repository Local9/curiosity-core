﻿using Lusive.Events;
using Lusive.Events.Diagnostics;
using Lusive.Events.Message;
using Lusive.Events.Serialization;
using Lusive.Events.Serialization.Implementations;
using Lusive.Snowflake;

namespace Curiosity.Framework.Client.Events
{
    public class ClientGateway : BaseGateway
    {
        public const string SignaturePipeline = "moonlight_event_sig";

        protected override ISerialization Serialization { get; }
#nullable enable
        private string? _signature;
#nullable disable
        public ClientGateway(PluginManager client)
        {
            short playerId = (short)GetPlayerServerId(PlayerId());
            SnowflakeGenerator.Create(playerId);

            Serialization = new BinarySerialization();
            DelayDelegate = async delay => await BaseScript.Delay(delay);
            PrepareDelegate = PrepareAsync;
            PushDelegate = Push;

            client.Hook(EventConstant.InboundPipeline,
                new Action<byte[]>(async serialized => { await ProcessInboundAsync(new ServerId(), serialized); }));
            client.Hook(EventConstant.OutboundPipeline, new Action<byte[]>(ProcessOutbound));
            client.Hook(SignaturePipeline, new Action<string>(signature => _signature = signature));

            BaseScript.TriggerServerEvent(SignaturePipeline);
        }

        public async Task PrepareAsync(string pipeline, ISource source, IMessage message)
        {
            if (_signature == null)
            {
                var stopwatch = StopwatchUtil.StartNew();

                while (_signature == null)
                    await Common.MoveToMainThread();

                Logger.Debug($"[{message}] Signature fetch took {stopwatch.Elapsed.TotalMilliseconds}ms.");
            }

            message.Signature = _signature;
        }

        public void Push(string pipeline, ISource source, byte[] buffer)
        {
            if (source.Handle != -1)
                throw new Exception(
                    $"The client can only target the server. (arg {nameof(source)} is not matching -1)");

            BaseScript.TriggerServerEvent(pipeline, buffer);
        }

        public async void Send(string endpoint, params object[] args)
        {
            await SendInternal(EventFlowType.Straight, ServerId.Instance, endpoint, args);
        }

        public async Task<T> Get<T>(string endpoint, params object[] args)
        {
            return await GetInternal<T>(ServerId.Instance, endpoint, args);
        }
    }
}
