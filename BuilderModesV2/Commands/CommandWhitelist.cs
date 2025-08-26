using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using UnityEngine;

namespace BuilderModesV2.Commands
{
    internal class CommandWhitelist : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "BWhitelist";

        public string Help => "Allows a specified player with private builder to move your buildables";

        public string Syntax => "<name>";

        public List<string> Aliases => new List<string> { "Whitelist" };

        public List<string> Permissions => new List<string> { "BuilderModes.Whitelist", "Whitelist" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length != 1)
            {
                UnturnedChat.Say(caller, Main.Instance.Translate("WhitelistSyntaxError"), UnturnedChat.GetColorFromName(Main.Config.MessageColors.ErrorMessageColor, Color.red), true);
                return;
            }

            UnturnedPlayer whitelister = caller as UnturnedPlayer;
            UnturnedPlayer whitelisted = UnturnedPlayer.FromName(command[0]);

            if (whitelisted == null)
            {
                UnturnedChat.Say(caller, Main.Instance.Translate("WhitelistError"), UnturnedChat.GetColorFromName(Main.Config.MessageColors.ErrorMessageColor, Color.red), true);
                return;
            }

            var approvals = Main.Instance.approvals;
            var whitelisterID = whitelister.CSteamID.m_SteamID;
            var whitelistedID = whitelisted.CSteamID.m_SteamID;

            if (!approvals.ContainsKey(whitelisterID))
            {
                approvals[whitelisterID] = new List<ulong>();
            }

            approvals[whitelisterID].Add(whitelistedID);
            whitelisted.Player.ServerShowHint(Main.Instance.Translate("Whitelisted", whitelisted.CharacterName), 1);

            if (Main.Config.Modes.PrivateBuilder.Settings.WhitelistTimerEnabled)
            {
                Main.Instance.StartCoroutine(Main.Instance.WhitelistedPlayer(whitelister, whitelisted));
            }

            UnturnedChat.Say(caller, Main.Instance.Translate("WhitelistGiven", whitelisted.CharacterName), UnturnedChat.GetColorFromName(Main.Config.MessageColors.AcceptMessageColor, Color.cyan), true);
            Managers.WebhookManager.BuilderWhitelisted(whitelister, whitelisted);
        }
    }
}
