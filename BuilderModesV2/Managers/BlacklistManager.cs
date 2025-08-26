using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace BuilderModesV2.Managers
{
    public class BlacklistManager
    {
        public static void PrivateBuilderBuildables(UnturnedPlayer builder, BuilderModesUtils.PlayerData ownerData, BarricadeDrop drop, out bool shouldAllow)
        {
            var settings = Main.Config.Modes.PrivateBuilder.Settings;
            var denyConditions = new List<Func<bool>>
            {
                () => settings.BlacklistAllBeds && drop.asset.build == EBuild.BED,
                () => settings.BlacklistAllTraps && (drop.asset.build == EBuild.CHARGE || drop.asset.build == EBuild.WIRE || drop.asset.build == EBuild.SPIKE),
                () => settings.BlacklistAllFarming && drop.asset.build == EBuild.FARM
            };

            if (denyConditions.Any(condition => condition()))
            {
                WebhookManager.BuildableMoveDeny(builder, drop, ownerData);
                shouldAllow = false;

                DateTime now = DateTime.Now;
                if (!Main.Instance.lastMessageTimes.TryGetValue(builder.CSteamID, out DateTime lastMessageTime) || (now - lastMessageTime).TotalMilliseconds > 500)
                {
                    UnturnedChat.Say(builder, Main.Instance.Translate("BuildableMoveDeny"), UnturnedChat.GetColorFromName(Main.Instance.Configuration.Instance.MessageColors.DenyMessageColor, Color.cyan));
                    Main.Instance.lastMessageTimes[builder.CSteamID] = now;
                    Main.Instance.StartResetTimer(builder.CSteamID);
                    Main.Instance.resetTimer.Start();
                }

                if (Main.Config.DebugMode == true)
                    Logger.Log($"{builder.CSteamID.m_SteamID} couldn't move {drop.asset.id}", ConsoleColor.Cyan);
                return;
            }

            var builderBlacklist = Main.Config.Modes.PrivateBuilder.Id.FirstOrDefault(bl => bl.Value == drop.asset.id);

            if (builderBlacklist != null)
            {
                if (Main.Config.DebugMode == true)
                    Logger.Log($"Blacklist Match Found: {builderBlacklist.Value}", ConsoleColor.Cyan);

                if (builder.HasPermission($"BuilderModes.{builderBlacklist.Bypass}"))
                {
                    WebhookManager.BuildableMoved(builder, drop, ownerData);
                    shouldAllow = true;
                    if (Main.Config.DebugMode == true)
                        Logger.Log($"{builder.CSteamID.m_SteamID} moved {drop.asset.id}", ConsoleColor.Cyan);
                }
                else
                {
                    WebhookManager.BuildableMoveDeny(builder, drop, ownerData);
                    shouldAllow = false;

                    DateTime now = DateTime.Now;
                    if (!Main.Instance.lastMessageTimes.TryGetValue(builder.CSteamID, out DateTime lastMessageTime) || (now - lastMessageTime).TotalMilliseconds > 500)
                    {
                        UnturnedChat.Say(builder, Main.Instance.Translate("BarricadeMoveDeny"), UnturnedChat.GetColorFromName(Main.Instance.Configuration.Instance.MessageColors.DenyMessageColor, Color.cyan));
                        Main.Instance.lastMessageTimes[builder.CSteamID] = now;
                        Main.Instance.StartResetTimer(builder.CSteamID);
                        Main.Instance.resetTimer.Start();
                    }

                    if (Main.Config.DebugMode == true)
                        Logger.Log($"{builder.CSteamID.m_SteamID} couldn't move {drop.asset.id}", ConsoleColor.Cyan);
                }
                return;
            }
            else
            {
                if (Main.Config.DebugMode == true)
                    Logger.Log($"No Blacklist Match Found for {drop.asset.id}", ConsoleColor.Cyan);

                WebhookManager.BuildableMoved(builder, drop, ownerData);
                shouldAllow = true;
                if (Main.Config.DebugMode == true)
                    Logger.Log($"{builder.CSteamID.m_SteamID} moved {drop.asset.id}", ConsoleColor.Cyan);
            }
        }
        public static void CommunityBuilderBuildables(UnturnedPlayer builder, BuilderModesUtils.PlayerData ownerData, BarricadeDrop drop, out bool shouldAllow)
        {
            var settings = Main.Config.Modes.CommunityBuilder;
            var denyConditions = new List<Func<bool>>
            {
                () => settings.BlacklistAllBeds && drop.asset.build == EBuild.BED,
                () => settings.BlacklistAllTraps && (drop.asset.build == EBuild.CHARGE || drop.asset.build == EBuild.WIRE || drop.asset.build == EBuild.SPIKE),
                () => settings.BlacklistAllFarming && drop.asset.build == EBuild.FARM
            };

            if (denyConditions.Any(condition => condition()))
            {
                WebhookManager.BuildableMoveDeny(builder, drop, ownerData);
                shouldAllow = false;

                DateTime now = DateTime.Now;
                if (!Main.Instance.lastMessageTimes.TryGetValue(builder.CSteamID, out DateTime lastMessageTime) || (now - lastMessageTime).TotalMilliseconds > 500)
                {
                    UnturnedChat.Say(builder, Main.Instance.Translate("BarricadeMoveDeny"), UnturnedChat.GetColorFromName(Main.Instance.Configuration.Instance.MessageColors.DenyMessageColor, Color.cyan));
                    Main.Instance.lastMessageTimes[builder.CSteamID] = now;
                    Main.Instance.StartResetTimer(builder.CSteamID);
                    Main.Instance.resetTimer.Start();
                }

                if (Main.Config.DebugMode == true)
                    Logger.Log($"{builder.CSteamID.m_SteamID} couldn't move {drop.asset.id}", ConsoleColor.Cyan);
                return;
            }

            var builderBlacklist = Main.Config.Modes.CommunityBuilder.Id.FirstOrDefault(bl => bl.Value == drop.asset.id);

            if (builderBlacklist != null)
            {
                if (Main.Config.DebugMode == true)
                    Logger.Log($"Blacklist Match Found: {builderBlacklist.Value}", ConsoleColor.Cyan);

                if (builderBlacklist.Bypass == null)
                {
                    WebhookManager.BuildableMoveDeny(builder, drop, ownerData);
                    shouldAllow = false;

                    DateTime now = DateTime.Now;
                    if (!Main.Instance.lastMessageTimes.TryGetValue(builder.CSteamID, out DateTime lastMessageTime) || (now - lastMessageTime).TotalMilliseconds > 500)
                    {
                        UnturnedChat.Say(builder, Main.Instance.Translate("BarricadeMoveDeny"), UnturnedChat.GetColorFromName(Main.Instance.Configuration.Instance.MessageColors.DenyMessageColor, Color.cyan));
                        Main.Instance.lastMessageTimes[builder.CSteamID] = now;
                        Main.Instance.StartResetTimer(builder.CSteamID);
                        Main.Instance.resetTimer.Start();
                    }

                    if (Main.Config.DebugMode == true)
                        Logger.Log($"{builder.CSteamID.m_SteamID} couldn't move {drop.asset.id}", ConsoleColor.Cyan);
                    return;
                }

                if (builder.HasPermission($"BuilderModes.{builderBlacklist.Bypass}"))
                {
                    WebhookManager.BuildableMoved(builder, drop, ownerData);
                    shouldAllow = true;
                    if (Main.Config.DebugMode == true)
                        Logger.Log($"{builder.CSteamID.m_SteamID} moved {drop.asset.id} due to bypass", ConsoleColor.Cyan);
                }
                else
                {
                    WebhookManager.BuildableMoveDeny(builder, drop, ownerData);
                    shouldAllow = false;

                    DateTime now = DateTime.Now;
                    if (!Main.Instance.lastMessageTimes.TryGetValue(builder.CSteamID, out DateTime lastMessageTime) || (now - lastMessageTime).TotalMilliseconds > 500)
                    {
                        UnturnedChat.Say(builder, Main.Instance.Translate("BarricadeMoveDeny"), UnturnedChat.GetColorFromName(Main.Instance.Configuration.Instance.MessageColors.DenyMessageColor, Color.cyan));
                        Main.Instance.lastMessageTimes[builder.CSteamID] = now;
                        Main.Instance.StartResetTimer(builder.CSteamID);
                        Main.Instance.resetTimer.Start();
                    }

                    if (Main.Config.DebugMode == true)
                        Logger.Log($"{builder.CSteamID.m_SteamID} couldn't move {drop.asset.id}", ConsoleColor.Cyan);
                }
            }
            else
            {
                if (Main.Config.DebugMode == true)
                    Logger.Log($"No Blacklist Match Found for {drop.asset.id}", ConsoleColor.Cyan);

                WebhookManager.BuildableMoved(builder, drop, ownerData);
                shouldAllow = true;
                if (Main.Config.DebugMode == true)
                    Logger.Log($"{builder.CSteamID.m_SteamID} moved {drop.asset.id}", ConsoleColor.Cyan);
            }
        }
        public static void PrivateBuilderStructures(UnturnedPlayer builder, BuilderModesUtils.PlayerData ownerData, StructureDrop drop, out bool shouldAllow)
        {
            var builderBlacklist = Main.Config.Modes.PrivateBuilder.Id.FirstOrDefault(bl => bl.Value == drop.asset.id);

            if (builderBlacklist != null)
            {
                if (Main.Config.DebugMode == true)
                    Logger.Log($"Blacklist Match Found: {builderBlacklist.Value}", ConsoleColor.Cyan);

                if (builder.HasPermission($"BuilderModes.{builderBlacklist.Bypass}"))
                {
                    WebhookManager.StructureMoved(builder, drop, ownerData);
                    shouldAllow = true;
                    if (Main.Config.DebugMode == true)
                        Logger.Log($"{builder.CSteamID.m_SteamID} moved {drop.asset.id}", ConsoleColor.Cyan);
                }
                else
                {
                    WebhookManager.StructureMoveDeny(builder, drop, ownerData);
                    shouldAllow = false;

                    DateTime now = DateTime.Now;
                    if (!Main.Instance.lastMessageTimes.TryGetValue(builder.CSteamID, out DateTime lastMessageTime) || (now - lastMessageTime).TotalMilliseconds > 500)
                    {
                        UnturnedChat.Say(builder, Main.Instance.Translate("StructureMoveDeny"), UnturnedChat.GetColorFromName(Main.Instance.Configuration.Instance.MessageColors.DenyMessageColor, Color.cyan));
                        Main.Instance.lastMessageTimes[builder.CSteamID] = now;
                        Main.Instance.StartResetTimer(builder.CSteamID);
                        Main.Instance.resetTimer.Start();
                    }

                    if (Main.Config.DebugMode == true)
                        Logger.Log($"{builder.CSteamID.m_SteamID} couldn't move {drop.asset.id}", ConsoleColor.Cyan);
                }
                return;
            }
            else
            {
                if (Main.Config.DebugMode == true)
                    Logger.Log($"No Blacklist Match Found for {drop.asset.id}", ConsoleColor.Cyan);

                WebhookManager.StructureMoved(builder, drop, ownerData);
                shouldAllow = true;
                if (Main.Config.DebugMode == true)
                    Logger.Log($"{builder.CSteamID.m_SteamID} moved {drop.asset.id}", ConsoleColor.Cyan);
            }
        }
        public static void CommunityBuilderStructures(UnturnedPlayer builder, BuilderModesUtils.PlayerData ownerData, StructureDrop drop, out bool shouldAllow)
        {
            var builderBlacklist = Main.Config.Modes.CommunityBuilder.Id.FirstOrDefault(bl => bl.Value == drop.asset.id);

            if (builderBlacklist != null)
            {
                if (Main.Config.DebugMode == true)
                    Logger.Log($"Blacklist Match Found: {builderBlacklist.Value}", ConsoleColor.Cyan);

                if (builder.HasPermission($"BuilderModes.{builderBlacklist.Bypass}"))
                {
                    WebhookManager.StructureMoved(builder, drop, ownerData);
                    shouldAllow = true;
                    if (Main.Config.DebugMode == true)
                        Logger.Log($"{builder.CSteamID.m_SteamID} moved {drop.asset.id}", ConsoleColor.Cyan);
                }
                else
                {
                    WebhookManager.StructureMoveDeny(builder, drop, ownerData);
                    shouldAllow = false;

                    DateTime now = DateTime.Now;
                    if (!Main.Instance.lastMessageTimes.TryGetValue(builder.CSteamID, out DateTime lastMessageTime) || (now - lastMessageTime).TotalMilliseconds > 500)
                    {
                        UnturnedChat.Say(builder, Main.Instance.Translate("StructureMoveDeny"), UnturnedChat.GetColorFromName(Main.Instance.Configuration.Instance.MessageColors.DenyMessageColor, Color.cyan));
                        Main.Instance.lastMessageTimes[builder.CSteamID] = now;
                        Main.Instance.StartResetTimer(builder.CSteamID);
                        Main.Instance.resetTimer.Start();
                    }

                    if (Main.Config.DebugMode == true)
                        Logger.Log($"{builder.CSteamID.m_SteamID} couldn't move {drop.asset.id}", ConsoleColor.Cyan);
                }
            }
            else
            {
                if (Main.Config.DebugMode == true)
                    Logger.Log($"No Blacklist Match Found for {drop.asset.id}", ConsoleColor.Cyan);

                WebhookManager.StructureMoved(builder, drop, ownerData);
                shouldAllow = true;
                if (Main.Config.DebugMode == true)
                    Logger.Log($"{builder.CSteamID.m_SteamID} moved {drop.asset.id}", ConsoleColor.Cyan);
            }
        }
    }
}
