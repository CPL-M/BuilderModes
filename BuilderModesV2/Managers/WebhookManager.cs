using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Logger = Rocket.Core.Logging.Logger;

namespace BuilderModesV2.Managers
{
    public class WebhookManager
    {
        private static readonly ConcurrentQueue<(string key, string message)> _queue = new ConcurrentQueue<(string key, string message)>();
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private static readonly Dictionary<string, List<string>> _messageBuffer = new Dictionary<string, List<string>>();
        private static readonly TimeSpan _batchInterval = TimeSpan.FromSeconds(7);
        private static readonly int _batchSize = 40;
        private static readonly HttpClient _client = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };

        public static void StartProcessing()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(_batchInterval);
                    await ProcessQueue();
                }
            });
        }
        public static void SendWebhookToQueue(string key, string message)
        {
            _queue.Enqueue((key, message));
        }
        private static async Task ProcessQueue()
        {
            await _semaphore.WaitAsync();

            try
            {
                while (_queue.TryDequeue(out var item))
                {
                    if (item.key == null)
                    {
                        Logger.LogError($"key is null in ProcessQueue (line 52)");
                    }
                    if (!_messageBuffer.ContainsKey(item.key))
                    {
                        _messageBuffer[item.key] = new List<string>();
                    }
                    _messageBuffer[item.key].Add(item.message);

                    if (_messageBuffer[item.key].Count >= _batchSize)
                    {
                        await SendBatchedMessages(item.key);
                    }
                }

                if (_messageBuffer == null)
                {
                    Logger.LogError($"messagebuffer is null in ProcessQueue (line 68)");
                }

                if (_messageBuffer.Keys == null)
                {
                    Logger.LogError($"messagebuffer.keys is null in ProcessQueue (line 73)");
                }

                var keys = _messageBuffer.Keys.ToList();
                foreach (var key in keys)
                {
                    if (key == null)
                    {
                        Logger.LogError($"key is null in ProcessQueue Foreach (line 76)");
                        continue;
                    }
                    if (_messageBuffer[key].Count > 0)
                    {
                        if (key == null)
                        {
                            Logger.LogError($"key is null in ProcessQueue Foreach (line 88)");
                        }
                        await SendBatchedMessages(key);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.LogError("BuilderModes: Exception sending webhook:");
                Logger.LogException(exception);
            }
            finally
            {
                _semaphore.Release();
            }
        }
        private static async Task SendBatchedMessages(string key)
        {
            if (key == null)
            {
                Logger.LogError($"key is null in SendBatchedMessages (line 108)");
                return;
            }
            var webhookURL = "0";
            int color = (250 << 16) + (235 << 8) + 215;

            var webhookURLMap = new Dictionary<string, WebhookInfo>
            {
                { "Builder Enabled", new WebhookInfo { Url = Main.Config.Webhooks.Builder.Url, Color = Main.Config.Webhooks.Builder.EnabledColor } },
                { "Builder Disabled", new WebhookInfo { Url = Main.Config.Webhooks.Builder.Url, Color = Main.Config.Webhooks.Builder.DisabledColor } },
                { "Freecam Enabled", new WebhookInfo { Url = Main.Config.Webhooks.Freecam.Url, Color = Main.Config.Webhooks.Freecam.EnabledColor } },
                { "Freecam Disabled", new WebhookInfo { Url = Main.Config.Webhooks.Freecam.Url, Color = Main.Config.Webhooks.Freecam.DisabledColor } },
                { "Spectate Enabled", new WebhookInfo { Url = Main.Config.Webhooks.Spectate.Url, Color = Main.Config.Webhooks.Spectate.EnabledColor } },
                { "Spectate Disabled", new WebhookInfo { Url = Main.Config.Webhooks.Spectate.Url, Color = Main.Config.Webhooks.Spectate.DisabledColor } },
                { "Buildables Moved", new WebhookInfo { Url = Main.Config.Webhooks.BuildableMoved.Url, Color = Main.Config.Webhooks.BuildableMoved.Color } },
                { "Buildables Denied", new WebhookInfo { Url = Main.Config.Webhooks.BuildableMoveDeny.Url, Color = Main.Config.Webhooks.BuildableMoveDeny.Color } },
                { "Builder Whitelisted", new WebhookInfo { Url = Main.Config.Webhooks.Approved.Url, Color = Main.Config.Webhooks.Approved.Color } },
                { "Builder Dewhitelisted", new WebhookInfo { Url = Main.Config.Webhooks.Deapproved.Url, Color = Main.Config.Webhooks.Deapproved.Color } }
            };
            webhookURLMap.TryGetValue(key, out var webhookInfo);
            webhookURL = webhookInfo.Url;

            var colorhex = webhookInfo.Color;
            colorhex = colorhex.Replace("#", "");
            if (colorhex.Length == 6)
            {
                color = int.Parse(colorhex, System.Globalization.NumberStyles.HexNumber);
            }
            else
            {
                color = 0xFFFFFF;
            }

            if (webhookURL == null || webhookURL == "0")
            {
                Logger.LogError($"No webhook URL found for key: {key}");
                return;
            }

            var messages = _messageBuffer[key];
            _messageBuffer[key] = new List<string>();

            var embeds = new List<object>();
            var currentEmbedMessages = new List<string>();
            foreach (var message in messages)
            {
                if (currentEmbedMessages.Sum(m => m.Length) + message.Length >= 2000)
                {
                    embeds.Add(new
                    {
                        title = key.ToString(),
                        color = color,
                        description = string.Join("\n", currentEmbedMessages)
                    });
                    currentEmbedMessages.Clear();
                }
                currentEmbedMessages.Add(message);
            }
            if (currentEmbedMessages.Count > 0)
            {
                embeds.Add(new
                {
                    title = key.ToString(),
                    color = color,
                    description = string.Join("\n", currentEmbedMessages)
                });
            }

            var combinedMessage = new
            {
                embeds = embeds.ToArray()
            };

            if (combinedMessage == null)
            {
                Logger.LogError($"CombinedMessage is null");
                return;
            }

            var combinedJson = JsonConvert.SerializeObject(combinedMessage);

            var content = new StringContent(combinedJson, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(webhookURL, content);
            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Logger.LogError($"BuilderModes: Failed to send webhook. Status code: {response.StatusCode}");
                Logger.LogError($"Response: {responseContent}");

                if (response.StatusCode == (HttpStatusCode)429)
                {
                    var retryAfter = JsonConvert.DeserializeObject<RateLimitResponse>(responseContent).RetryAfter;
                    await Task.Delay((int)(retryAfter * 1000));
                    foreach (var message in messages)
                    {
                        _queue.Enqueue((key, message));
                    }
                }
            }
        }

        public static void TestHook(string title, string description)
        {
            if (Main.Instance.Configuration.Instance.Webhooks.On == false)
                return;

            string key = "Buildables Denied";
            string currentTime = DateTime.UtcNow.ToString("HH:mm:ss");
            string message = $"- {currentTime} | Test";

            WebhookManager.SendWebhookToQueue(key, message);
        }
        public static void BuildableMoveDeny(UnturnedPlayer builder, BarricadeDrop drop, BuilderModesUtils.PlayerData ownerData)
        {
            if (Main.Instance.Configuration.Instance.Webhooks.On == false)
                return;

            string key = "Buildables Denied";
            string currentTime = DateTime.UtcNow.ToString("HH:mm:ss");

            string message = $"- **{currentTime}** | Builder: {builder.CharacterName} ({builder.CSteamID}), Owner: {ownerData.CharacterName} ({ownerData.SteamID}), Buildable: {drop.asset.FriendlyName} ({drop.asset.id})";

            WebhookManager.SendWebhookToQueue(key, message);
        }
        public static void BuildableMoved(UnturnedPlayer builder, BarricadeDrop drop, BuilderModesUtils.PlayerData ownerData)
        {
            if (Main.Instance.Configuration.Instance.Webhooks.On == false)
                return;

            string key = "Buildables Moved";
            string currentTime = DateTime.UtcNow.ToString("HH:mm:ss");

            string message = $"- **{currentTime}** | Builder: {builder.CharacterName} ({builder.CSteamID}), Owner: {ownerData.CharacterName} ({ownerData.SteamID}), Buildable: {drop.asset.FriendlyName} ({drop.asset.id})";

            WebhookManager.SendWebhookToQueue(key, message);
        }
        public static void StructureMoved(UnturnedPlayer builder, StructureDrop drop, BuilderModesUtils.PlayerData ownerData)
        {
            if (Main.Instance.Configuration.Instance.Webhooks.On == false)
                return;

            string key = "Buildables Moved";
            string currentTime = DateTime.UtcNow.ToString("HH:mm:ss");

            string message = $"- ** {currentTime} ** | Builder: {builder.CharacterName} ({builder.CSteamID}), Owner: {ownerData.CharacterName} ({ownerData.SteamID}), Buildable: {drop.asset.FriendlyName} ({drop.asset.id})";

            WebhookManager.SendWebhookToQueue(key, message);
        }
        public static void StructureMoveDeny(UnturnedPlayer builder, StructureDrop drop, BuilderModesUtils.PlayerData ownerData)
        {
            if (Main.Instance.Configuration.Instance.Webhooks.On == false)
                return;

            string key = "Buildables Denied";
            string currentTime = DateTime.UtcNow.ToString("HH:mm:ss");

            string message = $"- ** {currentTime} ** | Builder: {builder.CharacterName} ({builder.CSteamID}), Owner: {ownerData.CharacterName} ({ownerData.SteamID}), Buildable: {drop.asset.FriendlyName} ({drop.asset.id})";

            WebhookManager.SendWebhookToQueue(key, message);
        }
        public static void BuilderEnabled(UnturnedPlayer builder, string mode)
        {
            if (Main.Instance.Configuration.Instance.Webhooks.On == false)
                return;

            string key = "Builder Enabled";
            string currentTime = DateTime.UtcNow.ToString("HH:mm:ss");

            string message = $"- **{currentTime}** | Player: {builder.CharacterName} ({builder.CSteamID})";

            WebhookManager.SendWebhookToQueue(key, message);
        }
        public static void BuilderDisabled(UnturnedPlayer builder, string mode)
        {
            if (Main.Instance.Configuration.Instance.Webhooks.On == false)
                return;

            string key = "Builder Disabled";
            string currentTime = DateTime.UtcNow.ToString("HH:mm:ss");

            string message = $"- **{currentTime}** | Player: {builder.CharacterName} ({builder.CSteamID})";

            WebhookManager.SendWebhookToQueue(key, message);
        }
        public static void FreecamEnabled(UnturnedPlayer builder)
        {
            if (Main.Instance.Configuration.Instance.Webhooks.On == false)
                return;

            string key = "Freecam Enabled";
            string currentTime = DateTime.UtcNow.ToString("HH:mm:ss");

            string message = $"- ** {currentTime} ** | Player: {builder.CharacterName} ({builder.CSteamID})";

            WebhookManager.SendWebhookToQueue(key, message);
        }
        public static void FreecamDisabled(UnturnedPlayer builder)
        {
            if (Main.Instance.Configuration.Instance.Webhooks.On == false)
                return;

            string key = "Freecam Disabled";
            string currentTime = DateTime.UtcNow.ToString("HH:mm:ss");

            string message = $"- ** {currentTime} ** | Player: {builder.CharacterName} ({builder.CSteamID})";

            WebhookManager.SendWebhookToQueue(key, message);
        }
        public static void SpectateEnabled(UnturnedPlayer builder)
        {
            if (Main.Instance.Configuration.Instance.Webhooks.On == false)
                return;

            string key = "Spectate Enabled";
            string currentTime = DateTime.UtcNow.ToString("HH:mm:ss");

            string message = $"- ** {currentTime} ** | Player: {builder.CharacterName} ({builder.CSteamID})";

            WebhookManager.SendWebhookToQueue(key, message);
        }
        public static void SpectateDisabled(UnturnedPlayer builder)
        {
            if (Main.Instance.Configuration.Instance.Webhooks.On == false)
                return;

            string key = "Spectate Disabled";
            string currentTime = DateTime.UtcNow.ToString("HH:mm:ss");

            string message = $"- **  {currentTime}  ** | Player: {builder.CharacterName} ({builder.CSteamID})";

            WebhookManager.SendWebhookToQueue(key, message);
        }
        public static void BuilderWhitelisted(UnturnedPlayer owner, UnturnedPlayer approved)
        {
            if (Main.Instance.Configuration.Instance.Webhooks.On == false)
                return;

            string key = "Builder Whitelisted";
            string currentTime = DateTime.UtcNow.ToString("HH:mm:ss");

            string message = $"- **{currentTime}** | Owner: {owner.CharacterName} ({owner.CSteamID}), Approved: {approved.CharacterName} ({approved.CSteamID})";

            WebhookManager.SendWebhookToQueue(key, message);
        }

        public static void BuilderUnwhitelisted(UnturnedPlayer owner, UnturnedPlayer deapproved)
        {
            if (Main.Instance.Configuration.Instance.Webhooks.On == false)
                return;

            string key = "Builder Dewhitelisted";
            string currentTime = DateTime.UtcNow.ToString("HH:mm:ss");

            string message = $"- **{currentTime}** | Owner: {owner.CharacterName} ({owner.CSteamID}), DeWhitelisted: {deapproved.CharacterName} ({deapproved.CSteamID})";

            WebhookManager.SendWebhookToQueue(key, message);
        }
    }
    public class RateLimitResponse
    {
        [JsonProperty("retry_after")]
        public double RetryAfter { get; set; }
    }
    public class WebhookInfo
    {
        public string Url { get; set; }
        public string Color { get; set; }
    }
}