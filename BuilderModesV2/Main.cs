using BuilderModesV2.Managers;
using Newtonsoft.Json;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Timers;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;
using static BuilderModesV2.BuilderModesUtils;
using Logger = Rocket.Core.Logging.Logger;

namespace BuilderModesV2
{
    public class Main : RocketPlugin<Config>
    {
        public Dictionary<ulong, List<ulong>> approvals;
        public List<UnturnedPlayer> BuilderOn;
        public List<UnturnedPlayer> FreecamOn;
        public List<UnturnedPlayer> F7On;
        public List<UnturnedPlayer> AntiPvPOn;

        public Dictionary<CSteamID, DateTime> lastMessageTimes = new Dictionary<CSteamID, DateTime>(); 
        
        public Timer resetTimer;

        public static Main Instance { get; private set; }
        public static Config Config { get => Instance!.Configuration.Instance; }

        public string configPath {  get; private set; }
        protected override void Load()
        {
            // Setting the Lists and Dictionaries
            Instance = this;
            BuilderOn = new();
            FreecamOn = new();
            F7On = new();
            approvals = new();
            AntiPvPOn = new();

            configPath = Path.Combine(base.Directory, $"{base.Name}.configuration.xml");

            LoadPlayerDataList();
            Managers.ConfigManager.UpdateConfig();
            Managers.WebhookManager.StartProcessing();

            // Branding and Loaded indicator
            Logger.Log("=============================================", ConsoleColor.DarkMagenta);
            Logger.Log("||          BuilderModes Loaded            ||", ConsoleColor.DarkMagenta);
            Logger.Log("||            Created by CPL               ||", ConsoleColor.DarkMagenta);
            Logger.Log("=============================================", ConsoleColor.DarkMagenta);
            Logger.Log($"           Version:{Assembly.GetName().Version}", ConsoleColor.DarkMagenta);

            // Events
            U.Events.OnPlayerConnected += OnPlayerConnected;
            U.Events.OnPlayerDisconnected += OnPlayerDisconnected;
            BarricadeManager.onTransformRequested += BarricadeMoveRequest;
            StructureManager.onTransformRequested += StructureMoveRequest;
            SaveManager.onPreSave += SavePlayerDataList;

            PlayerQuests.onGroupChanged += OnGroupChanged;
        }

        private void LoadPlayerDataList()
        {
            string filePath = Path.Combine(base.Directory, $"{base.Name}.PlayerData.json");
            if (File.Exists(filePath))
            {
                playerDataDictionary = JsonConvert.DeserializeObject<Dictionary<ulong, PlayerData>>(File.ReadAllText(filePath));
            }
            else if (File.Exists(Path.Combine(base.Directory, $"{base.Name}.PlayerData.json")))
            {
                var playerDataList = JsonConvert.DeserializeObject<List<PlayerData>>(File.ReadAllText(Path.Combine(base.Directory, $"{base.Name}.PlayerCharacterNames.json")));
                playerDataDictionary = playerDataList.ToDictionary(item => item.SteamID, item => new PlayerData
                {
                    CharacterName = item.CharacterName,
                    SteamGroupID = item.SteamGroupID,
                    SteamID = item.SteamID,
                    GroupID = item.GroupID
                });
                SavePlayerDataList();
                File.Delete(Path.Combine(base.Directory, $"{base.Name}.PlayerData.json"));
            }
            else
            {
                playerDataDictionary = new Dictionary<ulong, PlayerData>();
                SavePlayerDataList();
            }

            if (!playerDataDictionary.TryGetValue(0, out var data))
            {
                playerDataDictionary.Add(0, new PlayerData() { CharacterName = "unowned" });
            }
        }

        private void OnPlayerDamaged(Player player, byte damage, Vector3 force, EDeathCause cause, ELimb limb, CSteamID killer)
        {
            UnturnedPlayer Uplayer = UnturnedPlayer.FromPlayer(player);
            if (!Uplayer.HasPermission("Buildermodes.builder") || !Uplayer.HasPermission("Builder") || !Uplayer.HasPermission(Main.Instance.Configuration.Instance.Modes.CommunityBuilder.Permission))
            {
                return;
            }
            Uplayer.Player.look.sendFreecamAllowed(false);
            Uplayer.Player.look.sendSpecStatsAllowed(false);
            Uplayer.Player.look.sendWorkzoneAllowed(false);
            if (!AntiPvPOn.Contains(Uplayer))
                AntiPvPOn.Add(Uplayer);
            AntiPvP(Uplayer);
        }

        private void OnGroupChanged(PlayerQuests sender, CSteamID oldGroupID, EPlayerGroupRank oldGroupRank, CSteamID newGroupID, EPlayerGroupRank newGroupRank)
        {
            var SteamID = sender.player.channel.owner.playerID.steamID.m_SteamID;

            if (playerDataDictionary.TryGetValue(SteamID, out var data))
            {
                data.GroupID = newGroupID.m_SteamID;
            }
        }

        protected override void Unload()
        {
            // Branding and Unloaded indicator
            Logger.Log("=============================================", ConsoleColor.DarkMagenta);
            Logger.Log("||         BuilderModes Unloaded           ||", ConsoleColor.DarkMagenta);
            Logger.Log("||            Created by CPL               ||", ConsoleColor.DarkMagenta);
            Logger.Log("=============================================", ConsoleColor.DarkMagenta);
            // Events
            U.Events.OnPlayerConnected -= OnPlayerConnected;
            U.Events.OnPlayerDisconnected -= OnPlayerDisconnected;
            BarricadeManager.onTransformRequested -= BarricadeMoveRequest;
            StructureManager.onTransformRequested -= StructureMoveRequest;
            SaveManager.onPreSave -= SavePlayerDataList;

            PlayerQuests.onGroupChanged -= OnGroupChanged;

            SavePlayerDataList();
        }
        private void SavePlayerDataList()
        {
            File.WriteAllText(Path.Combine(base.Directory, $"{base.Name}.PlayerData.json"), JsonConvert.SerializeObject(playerDataDictionary));
            if (Config.DebugMode == true)
                Logger.Log($"Player Data List Saved", ConsoleColor.Cyan);
        }
        // -------------------------- Player Connect/Disconnect --------------------------
        private void OnPlayerConnected(UnturnedPlayer player)
        {
            if (Config.DebugMode == true)
                Logger.Log($"[Buildermodes] {player.CharacterName} ({player.CSteamID.m_SteamID}) connected to server", ConsoleColor.Cyan);

            CSteamID CSteamID = player.CSteamID;

            if (playerDataDictionary.TryGetValue(CSteamID.m_SteamID, out var data))
            {
                data.CharacterName = player.CharacterName;
                data.SteamGroupID = player.SteamGroupID.m_SteamID;
                data.SteamID = CSteamID.m_SteamID;
                data.GroupID = player.Player.quests.groupID.m_SteamID;
                if (Config.DebugMode == true)
                    Logger.Log($"{player.CharacterName} ({player.CSteamID.m_SteamID}) added to playerdata", ConsoleColor.Cyan);
            }
            else
            {
                playerDataDictionary.Add(CSteamID.m_SteamID, new PlayerData() { CharacterName = player.CharacterName, SteamID = CSteamID.m_SteamID, SteamGroupID = player.SteamGroupID.m_SteamID, GroupID = player.Player.quests.groupID.m_SteamID });
                if (Config.DebugMode == true)
                    Logger.Log($"{player.CharacterName} ({player.CSteamID.m_SteamID}) added to playerdata", ConsoleColor.Cyan);
            }
            if (Config.Modes.PrivateBuilder.Settings.AntiPvP == true || Config.Modes.CommunityBuilder.AntiPvP == true)
            {
                player.Player.life.onHurt += OnPlayerDamaged;
            }
        }
        private void OnPlayerDisconnected(UnturnedPlayer player)
        {
            // Reset player modes
            player.Player.look.sendFreecamAllowed(false);
            player.Player.look.sendSpecStatsAllowed(false);
            player.Player.look.sendWorkzoneAllowed(false);

            F7On!.Remove(player);
            FreecamOn!.Remove(player);
            BuilderOn!.Remove(player);
            approvals!.Remove(player.CSteamID.m_SteamID);
            AntiPvPOn.Remove(player);

            if (Config.DebugMode == true)
                Logger.Log($"{player.CharacterName}({player.CSteamID.m_SteamID}) disconnected from server", ConsoleColor.Cyan);
            player.Player.life.onHurt -= OnPlayerDamaged;
        }

        // -------------------------- Move Requests --------------------------
        private void BarricadeMoveRequest(CSteamID instigator, byte x, byte y, ushort plant, uint instanceID, ref UnityEngine.Vector3 point, ref byte angle_x, ref byte angle_y, ref byte angle_z, ref bool shouldAllow)
        {
            if (BarricadeManager.tryGetRegion(x, y, plant, out BarricadeRegion region))
            {
                var drop = region.drops.FirstOrDefault(d => d.instanceID == instanceID);
                if (drop == null) return;

                ulong owner = drop.GetServersideData().owner;
                CSteamID ownerID = new CSteamID(owner);

                UnturnedPlayer builder = UnturnedPlayer.FromCSteamID(instigator);
                playerDataDictionary.TryGetValue(ownerID.m_SteamID, out var ownerData);

                if (builder.HasPermission(Main.Instance.Configuration.Instance.Modes.CommunityBuilder.Permission))
                {
                    Managers.BlacklistManager.CommunityBuilderBuildables(builder, ownerData, drop, out shouldAllow);
                    if (Config.DebugMode == true)
                    {
                        Logger.Log("CommunityBuilderBuildabledBlacklistRun", ConsoleColor.Cyan);
                        Logger.Log($"{shouldAllow}", ConsoleColor.Cyan);
                    }
                    return;
                }
                else // Private Builder
                {
                    var settings = Main.Config.Modes.PrivateBuilder.Settings;
                    if (builder.CSteamID.m_SteamID != ownerID.m_SteamID)
                    {
                        bool isAllowed = false;

                        if (settings.AllowSteamGroupBuildables && builder.SteamGroupID.m_SteamID == ownerData.SteamGroupID)
                        {
                            isAllowed = true;
                        }
                        else if (builder.HasPermission(Config.Modes.PrivateBuilder.Settings.GroupBuilderPermission) && builder.SteamGroupID.m_SteamID == ownerData.SteamGroupID)
                        {
                            isAllowed = true;
                        }
                        else if (settings.AllowInGameGroupBuildables && builder.Player.quests.groupID.m_SteamID == ownerData.GroupID)
                        {
                            isAllowed = true;
                        }
                        else if (builder.HasPermission(Config.Modes.PrivateBuilder.Settings.GroupBuilderPermission) && builder.Player.quests.groupID.m_SteamID == ownerData.GroupID)
                        {
                            isAllowed = true;
                        }
                        else if (IsApprovedToMove(ownerData.SteamID, builder.CSteamID.m_SteamID))
                        {
                            isAllowed = true;
                        }

                        if (isAllowed)
                        {
                            Managers.BlacklistManager.PrivateBuilderBuildables(builder, ownerData, drop, out shouldAllow);
                        }
                        else
                        {
                            Managers.WebhookManager.BuildableMoveDeny(builder, drop, ownerData);
                            shouldAllow = false;

                            DateTime now = DateTime.Now;
                            if (!lastMessageTimes.TryGetValue(builder.CSteamID, out DateTime lastMessageTime) || (now - lastMessageTime).TotalMilliseconds > 500)
                            {
                                UnturnedChat.Say(builder, Translate("BarricadeMoveDeny"), UnturnedChat.GetColorFromName(Instance.Configuration.Instance.MessageColors.DenyMessageColor, Color.cyan));
                                builder.Player.ServerShowHint(Translate("BarricadeMoveDeny"), 1);
                                lastMessageTimes[builder.CSteamID] = now;
                                StartResetTimer(builder.CSteamID);
                                resetTimer.Start();
                            }

                            return;
                        }
                    }
                    else
                    {
                        Managers.BlacklistManager.PrivateBuilderBuildables(builder, ownerData, drop, out shouldAllow);
                    }
                }
            }
        }
        public bool IsApprovedToMove(ulong ownerID, ulong moverID)
        {
            return Main.Instance.approvals.ContainsKey(ownerID) && Main.Instance.approvals[ownerID].Contains(moverID);
        }
        private void StructureMoveRequest(CSteamID instigator, byte x, byte y, uint instanceID, ref UnityEngine.Vector3 point, ref byte angle_x, ref byte angle_y, ref byte angle_z, ref bool shouldAllow)
        {
            StructureDrop drop = null;

            foreach (var region in StructureManager.regions)
            {
                drop = region.drops.FirstOrDefault(d => d.instanceID == instanceID);
                if (drop != null)
                    break;
            }

            if (drop == null) return;

            ulong owner = drop.GetServersideData().owner;
            CSteamID ownerID = new CSteamID(owner);

            UnturnedPlayer builder = UnturnedPlayer.FromCSteamID(instigator);
            playerDataDictionary.TryGetValue(ownerID.m_SteamID, out var ownerData);

            if (builder.HasPermission(Main.Instance.Configuration.Instance.Modes.CommunityBuilder.Permission))
            {
                Managers.BlacklistManager.CommunityBuilderStructures(builder, ownerData, drop, out shouldAllow);
            }
            else // Private
            {
                var settings = Main.Config.Modes.PrivateBuilder.Settings;
                if (instigator.m_SteamID != ownerID.m_SteamID)
                {
                    bool isAllowed = false;

                    if (settings.AllowSteamGroupBuildables && builder.SteamGroupID.m_SteamID == ownerData.SteamGroupID)
                    {
                        isAllowed = true;
                    }
                    else if (builder.HasPermission(Config.Modes.PrivateBuilder.Settings.GroupBuilderPermission) && builder.SteamGroupID.m_SteamID == ownerData.SteamGroupID)
                    {
                        isAllowed = true;
                    }
                    else if (settings.AllowInGameGroupBuildables && builder.Player.quests.groupID.m_SteamID == ownerData.GroupID && builder.Player.quests.groupID != CSteamID.Nil)
                    {
                        isAllowed = true;
                    }
                    else if (builder.HasPermission(Config.Modes.PrivateBuilder.Settings.GroupBuilderPermission) && builder.Player.quests.groupID.m_SteamID == ownerData.GroupID && builder.Player.quests.groupID != CSteamID.Nil)
                    {
                        isAllowed = true;
                    }
                    else if (IsApprovedToMove(ownerData.SteamID, builder.CSteamID.m_SteamID))
                    {
                        isAllowed = true;
                    }

                    if (isAllowed)
                    {
                        Managers.BlacklistManager.PrivateBuilderStructures(builder, ownerData, drop, out shouldAllow);
                    }
                    else
                    {
                        Managers.WebhookManager.StructureMoveDeny(builder, drop, ownerData);
                        shouldAllow = false;

                        DateTime now = DateTime.Now;
                        if (!lastMessageTimes.TryGetValue(builder.CSteamID, out DateTime lastMessageTime) || (now - lastMessageTime).TotalMilliseconds > 500)
                        {
                            UnturnedChat.Say(builder, Translate("StructureMoveDeny"), UnturnedChat.GetColorFromName(Instance.Configuration.Instance.MessageColors.DenyMessageColor, Color.cyan));
                            builder.Player.ServerShowHint(Translate("StructureMoveDeny"), 1);
                            lastMessageTimes[builder.CSteamID] = now;
                            StartResetTimer(builder.CSteamID);
                            resetTimer.Start();
                        }

                        return;
                    }
                }
                else
                {
                    Managers.BlacklistManager.PrivateBuilderStructures(builder, ownerData, drop, out shouldAllow);
                }
            }
        }
        public IEnumerator<WaitForSeconds> WhitelistedPlayer(UnturnedPlayer Whitelister, UnturnedPlayer Whitelisted)
        {
            yield return new WaitForSeconds(Configuration.Instance.Modes.PrivateBuilder.Settings.WhitelistTimerSeconds);

            if (approvals != null && approvals.TryGetValue(Whitelister.CSteamID.m_SteamID, out var whitelistedPlayers) && whitelistedPlayers.Contains(Whitelisted.CSteamID.m_SteamID))
            {
                whitelistedPlayers.Remove(Whitelisted.CSteamID.m_SteamID);
                Managers.WebhookManager.BuilderUnwhitelisted(Whitelister, Whitelisted);
                UnturnedChat.Say(Whitelister, Main.Instance.Translate("WhitelistRemoved", Whitelisted.CharacterName), UnturnedChat.GetColorFromName(Main.Config.MessageColors.AcceptMessageColor, Color.cyan), true);
                Whitelisted.Player.ServerShowHint(Main.Instance.Translate("Unwhitelisted", Whitelister.DisplayName), 1);
            }
        }
        public IEnumerator<WaitForSeconds> AntiPvP(UnturnedPlayer Builder)
        {
            yield return new WaitForSeconds(Configuration.Instance.Modes.PrivateBuilder.Settings.AntiPvPTimer);

            if (AntiPvPOn != null && AntiPvPOn.Contains(Builder))
            {
                AntiPvPOn.Remove(Builder);
            }
        }
        // -------------------------- Translations --------------------------
        public override TranslationList DefaultTranslations => new TranslationList
        {
            {"BuilderDisabled", "[Builder] Disabled"},
            {"BuilderEnabled", "[Builder] Enabled"},
            {"FreecamDisabled", "[Freecam] Disabled"},
            {"FreecamEnabled", "[Freecam] Enabled"},
            {"SpectateDisabled", "[Spectate] Disabled"},
            {"SpectateEnabled", "[Spectate] Enabled"},
            {"StructureMoveDeny", "[Error] You cannot Move this Structure"},
            {"BarricadeMoveDeny", "[Error] You cannot Move this Barricade"},
            {"PlayerBlacklisted", "[Error] You are blacklisted from using Builder"},
            {"BuildersList", "[Active Builders] "},
            {"NoBuildersError", "[Builder Error] There is nobody with builder active"},
            {"WhitelistSyntaxError", "[Syntax] /Whitelist <name>"},
            {"WhitelistError", "[Builder Error] There are no players with that name"},
            {"WhitelistGiven", "[Builder] {0} added to your Whitelist"},
            {"DeWhitelistError", "[Builder Error] There is nobody in your whitelist with that name"},
            {"DeWhitelistSyntaxError", "[Syntax] /UnWhitelist <name>"},
            {"WhitelistRemoved", "[Builder] {0} removed from your Whitelist"},
            {"WhitelistList", "[Whitelisted Builders] "},
            {"NoWhitelistError", "[Builder Error] There is nobody whitelisted"},
            {"Whitelisted", "[Builder] You have been added to {0}'s whitelist"},
            {"Unwhitelisted", "[Builder] You have been removed from {0}'s whitelist"},
            {"AntiPvP", "[Builder] You are still in PvP"}
        };
        // -------------------------- Timer --------------------------
        public void StartResetTimer(CSteamID steamID)
        {
            resetTimer = new Timer(500);
            resetTimer.Elapsed += (sender, e) => OnResetTimerElapsed(steamID);
            resetTimer.AutoReset = false;
        }
        public void OnResetTimerElapsed(CSteamID steamID)
        {
            if (lastMessageTimes.ContainsKey(steamID))
            {
                lastMessageTimes.Remove(steamID);
            }
        }
    }

    // -------------------------- MISC --------------------------
    public class BuilderModesUtils
    {
        public class PlayerData
        {
            public string CharacterName;
            public ulong SteamID;
            public ulong? SteamGroupID;
            public ulong? GroupID;
        }
        public static Dictionary<ulong, PlayerData> playerDataDictionary = new Dictionary<ulong, PlayerData>();
    }
}
