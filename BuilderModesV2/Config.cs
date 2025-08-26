using Rocket.API;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace BuilderModesV2
{
    public class Config : IRocketPluginConfiguration
    {
        public MessageColors MessageColors { get; set; }

        public Webhooks Webhooks { get; set; }

        public Modes Modes { get; set; }
        public bool DebugMode { get; set; }
        public void LoadDefaults()
        {
            Webhooks = new Webhooks
            {
                On = true,
                Builder = new() { Url = "https://discordapp.com/api/webhooks/{webhook.id}/{webhook.api}", EnabledColor = "81BE83", DisabledColor = "C8102E" },
                Freecam = new() { Url = "https://discordapp.com/api/webhooks/{webhook.id}/{webhook.api}", EnabledColor = "81BE83", DisabledColor = "C8102E" },
                Spectate = new() { Url = "https://discordapp.com/api/webhooks/{webhook.id}/{webhook.api}", EnabledColor = "81BE83", DisabledColor = "C8102E" },
                BuildableMoved = new() { Url = "https://discordapp.com/api/webhooks/{webhook.id}/{webhook.api}", Color = "81BE83"},
                BuildableMoveDeny = new() { Url = "https://discordapp.com/api/webhooks/{webhook.id}/{webhook.api}", Color = "C8102E" },
                Approved = new() { Url = "https://discordapp.com/api/webhooks/{webhook.id}/{webhook.api}", Color = "81BE83" },
                Deapproved = new() { Url = "https://discordapp.com/api/webhooks/{webhook.id}/{webhook.api}", Color = "C8102E" }
            };
            MessageColors = new MessageColors
            {
                EnabledMessageColor = "81BE83",
                DisabledMessageColor = "C8102E",
                ErrorMessageColor = "C8102E",
                DenyMessageColor = "C8102E",
                AcceptMessageColor = "81BE83"
            };
            Modes = new Modes
            {
                PrivateBuilder = new PrivateBuilder
                {
                    Settings = new PrivateBuilderSettings
                    {
                        AutoFreecam = true,
                        AllowSteamGroupBuildables = false,
                        AllowInGameGroupBuildables = false,
                        WhitelistTimerEnabled = true,
                        WhitelistTimerSeconds = 10,
                        BlacklistAllBeds = false,
                        BlacklistAllTraps = false,
                        BlacklistAllFarming = false,
                        AntiPvP = false,
                        AntiPvPTimer = 5,
                    },
                    Id = new List<PrivateBuilderBlacklist>()
                    {
                        new PrivateBuilderBlacklist()
                        {
                            Value = 1
                        },
                        new PrivateBuilderBlacklist()
                        {
                            Value = 2,
                            Bypass = "bypass"
                        }
                    }
                },
                CommunityBuilder = new CommunityBuilder
                {
                    Permission = "BuilderModes.Community",
                    AutoFreecam = true,
                    BlacklistAllBeds = false,
                    BlacklistAllTraps = false,
                    BlacklistAllFarming = false,
                    AntiPvP = false,
                    AntiPvPTimer = 5,
                    Id = new List<CommunityBuilderBlacklist>()
                    {
                        new CommunityBuilderBlacklist()
                        {
                            Value = 3
                        },
                        new CommunityBuilderBlacklist()
                        {
                            Value = 4,
                            Bypass = "bypass"
                        }
                    }
                }
            };
            DebugMode = false;
        }
    }
    // ============================= Message Colors =============================
    public class MessageColors
    {
        public string ErrorMessageColor { get; set; }
        public string EnabledMessageColor { get; set; }
        public string DisabledMessageColor { get; set; }
        public string DenyMessageColor { get; set; }
        public string AcceptMessageColor { get; set; }
    }
    // ============================= Modes =============================
    public class Modes
    {
        public PrivateBuilder PrivateBuilder { get; set; }
        public CommunityBuilder CommunityBuilder { get; set; }
    }
    // --------------------- Private Builder ---------------------
    public class PrivateBuilder
    {
        public PrivateBuilderSettings Settings { get; set; }
        [XmlArray("Blacklist")]
        [XmlArrayItem("Id")]
        public List<PrivateBuilderBlacklist> Id { get; set; }
    }
    public class PrivateBuilderSettings
    {
        public string GroupBuilderPermission { get; set; }
        public bool AllowSteamGroupBuildables { get; set; }
        public bool AllowInGameGroupBuildables { get; set; }
        public bool AutoFreecam { get; set; }
        public bool WhitelistTimerEnabled { get; set; }
        public float WhitelistTimerSeconds { get; set; }
        public bool BlacklistAllBeds { get; set; }
        public bool BlacklistAllTraps { get; set; }
        public bool BlacklistAllFarming { get; set; }
        public bool AntiPvP { get; set; }
        public float AntiPvPTimer { get; set; }
    }
    public class PrivateBuilderBlacklist
    {
        [XmlText]
        public ushort Value { get; set; }
        [XmlAttribute("Bypass")]
        public string? Bypass { get; set; }
    }
    // --------------------- Community Builder ---------------------
    public class CommunityBuilder
    {
        public string Permission { get; set; }
        public bool AutoFreecam { get; set; }
        public bool BlacklistAllBeds { get; set; }
        public bool BlacklistAllTraps { get; set; }
        public bool BlacklistAllFarming { get; set; }
        public bool AntiPvP { get; set; }
        public float AntiPvPTimer { get; set; }
        [XmlArray("Blacklist")]
        [XmlArrayItem("Id")]
        public List<CommunityBuilderBlacklist> Id { get; set; }
    }
    public class CommunityBuilderBlacklist
    {
        [XmlText]
        public ushort Value { get; set; }
        [XmlAttribute("Bypass")]
        public string? Bypass { get; set; }
    }
    // ============================= Webhooks =============================
    public class Webhooks
    {
        [XmlAttribute("Enabled")]
        public bool On { get; set; }

        public OnOffWebhookInfo Builder { get; set; }
        public OnOffWebhookInfo Freecam { get; set; }
        public OnOffWebhookInfo Spectate { get; set; }
        public MiscWebhookInfo BuildableMoved { get; set; }
        public MiscWebhookInfo BuildableMoveDeny { get; set; }
        public MiscWebhookInfo Approved { get; set; }
        public MiscWebhookInfo Deapproved { get; set; }
    }

    public class OnOffWebhookInfo
    {
        [XmlText]
        public string Url { get; set; }

        [XmlAttribute("EnabledColor")]
        public string EnabledColor { get; set; }

        [XmlAttribute("DisabledColor")]
        public string DisabledColor { get; set; }
    }
    public class MiscWebhookInfo
    {
        [XmlText]
        public string Url { get; set; }

        [XmlAttribute("Color")]
        public string Color { get; set; }
    }
}
