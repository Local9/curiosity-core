using Curiosity.Framework.Server.Events;
using Curiosity.Framework.Server.Models;
using Curiosity.Framework.Server.Models.Database;
using Curiosity.Framework.Server.Web.Discord.API;
using Curiosity.Framework.Shared.Enums;
using Curiosity.Framework.Shared.SerializedModels;
using FxEvents;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Curiosity.Framework.Server.Managers
{
    public class UserManager : Manager<UserManager>
    {
        long _lastTimeCleanupRan = 0;
        const int TWO_MINUTES = (1000 * 60) * 2;
        const int BASE_LOGIN_BUCKET = 5000;

        public override void Begin()
        {
            Logger.Debug($"INIT USER MANAGER");

            Event("playerConnecting", new Action<Player, string, CallbackDelegate, dynamic>(OnPlayerConnectingAsync));
            Event("playerJoining", new Action<Player, string>(OnPlayerJoiningAsync));
            Event("playerDropped", new Action<Player, string>(OnPlayerDropped));
            Event("onResourceStop", new Action<string>(OnResourceStop));

            Event("helloWorld", new Action(OnHelloWorld));

            EventDispatcher.Mount("user:active", new Func<ClientId, int, Task<User>>(OnUserActiveAsync));
        }

        private void OnHelloWorld()
        {
            Logger.Debug($"Hello World: {GetInvokingResource()}");
        }

        private async void OnPlayerConnectingAsync([FromSource] Player player, string name, CallbackDelegate denyWithReason, dynamic deferrals)
        {
            try
            {
                deferrals.defer();

                await BaseScript.Delay(500);
                // First, check to see if we can get their discord information, else reject them with a card.

                deferrals.update(ServerConfiguration.GetTranslation("user:check:identity", "🔍 Checking users identity."));
                await BaseScript.Delay(100);

                string strDiscordId = player?.Identifiers["discord"] ?? string.Empty;
                ulong discordId = 0;

                if (string.IsNullOrEmpty(strDiscordId) || !ulong.TryParse(strDiscordId, out discordId))
                {
                    ShowAdaptiveCard("data/cards/discord-error.json", deferrals);
                    return;
                }

                deferrals.update(ServerConfiguration.GetTranslation("user:check:discord", "🔍 Checking users Discord information."));
                await BaseScript.Delay(100);

                HttpClientHandler httpClientHandler = new HttpClientHandler
                {
                    ClientCertificateOptions = ClientCertificateOption.Automatic,
                    UseProxy = true,
                    UseDefaultCredentials = true
                };
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                using (var discordAPI = new HttpClient(httpClientHandler))
                {
                    Discord discordConfig = ServerConfiguration.GetDiscordConfig;

                    discordAPI.DefaultRequestHeaders.Accept.Clear();
                    discordAPI.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    discordAPI.DefaultRequestHeaders.Add("Authorization", $"Bot {discordConfig.BotKey}");

                    HttpResponseMessage memberObjRes = discordAPI.GetAsync($"https://discord.com/api/v9/guilds/{discordConfig.GuildId}/members/{discordId}").GetAwaiter().GetResult();

                    if (!memberObjRes.IsSuccessStatusCode)
                    {
                        if (memberObjRes.StatusCode == HttpStatusCode.NotFound)
                        {
                            ShowAdaptiveCard("data/cards/discord-unverified.json", deferrals);
                            return;
                        }
                        else
                        {
                            DefferAndKick("connection:error", "Something went wrong while trying to connect to the server.", denyWithReason, deferrals);
                            var memberErrObjStr = await memberObjRes.Content.ReadAsStringAsync();
                            Logger.Error($"Error {memberObjRes.StatusCode}: {memberErrObjStr}");
                        }
                    }

                    var memberObjStr = await memberObjRes.Content.ReadAsStringAsync();

                    DiscordMember discordMember = JsonConvert.DeserializeObject<DiscordMember>(memberObjStr);

                    bool isWhitelisted = false;

                    foreach (string role in discordMember.Roles)
                    {
                        foreach (string whitelistRole in discordConfig.Whitelist)
                        {
                            if (role.Equals(whitelistRole))
                            {
                                isWhitelisted = true;
                                break;
                            }
                        }
                    }

                    if (!isWhitelisted)
                    {
                        ShowAdaptiveCard("data/cards/discord-unverified.json", deferrals);
                        return;
                    }

                    // Check the database if they have accepted the rules, else display them.
                    // This will allow us to set a rule version, and if its changed, it will update
                    // RuleVersionAccepted != RuleVersionCurrent
                    string rulesTemplate = LoadResourceFile(GetCurrentResourceName(), "data/cards/join-server.json");

                    await BaseScript.Delay(500);

                    deferrals.presentCard(rulesTemplate, new Action<dynamic, string>(async (data, rawData) =>
                    {
                        Logger.Debug($"Deferral Data: {rawData}");

                        if (data.acceptRules != "true")
                        {
                            DefferAndKick("user:failedToAcceptRules", "You must accept the servers rules to connect.", denyWithReason, deferrals);
                            return;
                        }

                        deferrals.update(ServerConfiguration.GetTranslation("user:creation:check", "🔍 Checking for existing user information."));
                        await BaseScript.Delay(500);

                        DataStoreUser user = await DataStoreUser.GetUserAsync(player.Name, discordId, false);

                        if (user is null)
                        {
                            DefferAndKick("user:error:creation", "Failed to create a new user, please contact support.", denyWithReason, deferrals);
                            return;
                        }

                        deferrals.update(ServerConfiguration.GetTranslation("user:creation:existing", "🎉 Found an existing user."));
                        await BaseScript.Delay(1000);
                        deferrals.done();
                    }));
                }

                Logger.Debug($"Resource: {GetCurrentResourceName()} was invoked by {player.Name} ({player.Handle}) via {GetInvokingResource()}");
            }
            catch (Exception ex)
            {
                DefferAndKick("connection:error", "Something went wrong while trying to connect to the server.", denyWithReason, deferrals);
                Logger.Fatal("OnPlayerConnectingAsync");
                Logger.Fatal($"{ex}");
            }
        }

        private async void OnPlayerJoiningAsync([FromSource] Player player, string oldId)
        {
            try
            {
                await PluginManager.IsReady();

                string strDiscordId = player?.Identifiers["discord"] ?? string.Empty;
                ulong discordId;

                if (string.IsNullOrEmpty(strDiscordId) || !ulong.TryParse(strDiscordId, out discordId))
                {
                    player.Drop(ServerConfiguration.GetTranslation("user:join:error", "‼️ Something went wrong when joining the server."));
                    return;
                }

                DataStoreUser user = await DataStoreUser.GetUserAsync(player.Name, discordId, true);

                if (user is null)
                {
                    player.Drop(ServerConfiguration.GetTranslation("user:join:error", "‼️ Something went wrong when joining the server."));
                    return;
                }

                string msg = $"Player [{discordId}] '{user.Username}#{user.UserID}' is connecting to the server with {user.Characters.Count} character(s).";
                Logger.Info(msg);
            }
            catch (Exception ex)
            {
                player.Drop(ServerConfiguration.GetTranslation("connection:error", "Something went wrong while trying to connect to the server."));
                Logger.Fatal("OnPlayerJoiningAsync");
                Logger.Fatal($"{ex}");
            }
        }

        private void OnPlayerDropped([FromSource] Player player, string reason)
        {
            Logger.Debug($"Player '{player.Name}' dropped, reason; {reason}.");
            int playerId = int.Parse(player.Handle);
            if (UserSessions.ContainsKey(playerId))
                UserSessions.TryRemove(playerId, out ClientId user);
        }

        private void OnResourceStop(string resourceName)
        {
            if (resourceName != GetCurrentResourceName()) return;

        }

        // return the user from sessions with the characters
        private async Task<User> OnUserActiveAsync(ClientId client, int serverId)
        {
            try
            {
                if (client.Handle != serverId) return null;

                // SetPlayerRoutingBucket($"{client.Handle}", BASE_LOGIN_BUCKET + client.Handle);

                User userResult = client.User;

                Logger.Debug($"User with server handle '{client.Handle}' is updating their session state.");

                if (userResult == null)
                {
                    string strDiscordId = client.Player?.Identifiers["discord"] ?? string.Empty;
                    ulong discordId;

                    if (!ulong.TryParse(strDiscordId, out discordId))
                    {
                        client.Player.Drop(ServerConfiguration.GetTranslation("user:join:error", "‼️ Something went wrong when joining the server."));
                        return userResult;
                    }

                    DataStoreUser user = await DataStoreUser.GetUserAsync(client.Player.Name, discordId, true);

                    if (user is null)
                    {
                        client.Player.Drop(ServerConfiguration.GetTranslation("user:join:error", "‼️ Something went wrong when joining the server."));
                        return userResult;
                    }

                    ClientId clientId = new ClientId(serverId);

                    UserSessions.AddOrUpdate(client.Handle, clientId, (key, oldValue) => oldValue = clientId);

                    userResult = new User()
                    {
                        Handle = client.Handle,
                        UserID = user.UserID,
                        Username = client.Player.Name,
                        Role = (eRole)user.RoleId,
                    };

                    foreach (DataStoreCharacter dataStoreCharacter in user.Characters)
                    {
                        Character character = new();
                        character.CharacterId = dataStoreCharacter.CharacterId;
                        //character.Cash = dataStoreCharacter.Cash;
                        //character.CharacterJson = dataStoreCharacter.CharacterJson;
                        userResult.Characters.Add(character);
                    }

                    clientId.StoreUser = user;

                    Logger.Debug($"User {userResult.Username}#{userResult.UserID} is newly added to the User Sessions.");
                    Logger.Debug($"Number of Sessions: {UserSessions.Count}");
                }

                bool stateSetup = client.Player.State.Get("player:server:setup") ?? false;
                if (!stateSetup)
                {
                    client.Player.State.Set("player:spawned", false, true);
                    client.Player.State.Set("player:server:setup", true, true);
                    Logger.Debug($"User {userResult.Username}#{userResult.UserID} is setup.");
                }
                else
                {
                    Logger.Debug($"User {userResult.Username}#{userResult.UserID} is already setup.");
                }

                Logger.Info($"User {userResult.Username}#{userResult.UserID} is now active.");

                return userResult;
            }
            catch (Exception ex)
            {
                client.Player.Drop(ServerConfiguration.GetTranslation("connection:error", "Something went wrong while trying to connect to the server."));
                Logger.Fatal("OnPlayerJoiningAsync");
                Logger.Fatal($"{ex}");
                return null;
            }
        }

        private void DefferAndKick(string languageKey, string defaultMessage, CallbackDelegate denyWithReason, dynamic deferrals)
        {
            string message = ServerConfiguration.GetTranslation(languageKey, defaultMessage);
            deferrals.done(message);
            denyWithReason.Invoke(message);
        }

        private async void ShowAdaptiveCard(string cardLocation, dynamic deferrals)
        {
            string discordTemplate = LoadResourceFile(GetCurrentResourceName(), cardLocation);
            await BaseScript.Delay(100);
            deferrals.presentCard(discordTemplate);
        }
    }
}
