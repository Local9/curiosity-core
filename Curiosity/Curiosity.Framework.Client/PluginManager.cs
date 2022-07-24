using Curiosity.Framework.Client.Engine;
using Curiosity.Framework.Client.Events;
using Curiosity.Framework.Client.Scripts;
using Lusive.Snowflake;

namespace Curiosity.Framework.Client
{
    public class PluginManager : BaseScript
    {
        public static PluginManager Instance { get; private set; }
        public ClientGateway ClientGateway;
        internal SoundEngine SoundEngine;
        
     //   public readonly DiscordRichPresence DiscordRichPresence =
     //new DiscordRichPresence(
     //    GetResourceMetadata(GetCurrentResourceName(), "discord_rich_presence_asset", 0),
     //    GetResourceMetadata(GetCurrentResourceName(), "discord_rich_presence_asset_text", 0)
     //    )
     //{
     //    SmallAsset = GetResourceMetadata(GetCurrentResourceName(), "discord_rich_presence_small_asset", 0),
     //    SmallAssetText = GetResourceMetadata(GetCurrentResourceName(), "discord_rich_presence_small_asset_text", 0),
     //    Status = "Connecting..."
     //};

        public PluginManager()
        {
            Instance = this;
            ClientGateway = new ClientGateway();

            SoundEngine = new SoundEngine();

            SnowflakeGenerator.Create(-1);

            EventHandlers["onResourceStart"] += new Action<string>(OnResourceStart);
            EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings { MaxDepth = 128 };
        }

        private void OnResourceStart(string resourceName)
        {
            if (resourceName != GetCurrentResourceName()) return;
            InitiationScript.Startup();
            new CharacterScript().Init();
        }

        private void OnResourceStop(string resourceName)
        {
            if (resourceName != GetCurrentResourceName()) return;
        }

        public void AddEventHandler(string eventName, Delegate @delegate)
        {
            EventHandlers[eventName] += @delegate;
        }

        public void AttachTickHandler(Func<Task> task)
        {
            Tick += task;
        }

        public void DetachTickHandler(Func<Task> task)
        {
            Tick -= task;
        }
    }
}
