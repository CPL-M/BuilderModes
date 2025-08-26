using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using UnityEngine;

namespace BuilderModesV2.Commands
{
    internal class CommandWhitelistRemove : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "UWhitelistBuilder";

        public string Help => "";

        public string Syntax => "";

        public List<string> Aliases => new() { "UnWhitelist" };

        public List<string> Permissions => new() { "BuilderModes.Whitelist", "Whitelist" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (command.Length != 1)
            {
                UnturnedChat.Say(caller, Main.Instance.Translate("DeWhitelistSyntaxError"), UnturnedChat.GetColorFromName(Main.Config.MessageColors.ErrorMessageColor, Color.red), true);
                return;
            }

            UnturnedPlayer unwhitelister = (UnturnedPlayer)caller;
            UnturnedPlayer unwhitelisted = UnturnedPlayer.FromName(command[0]);

            if (unwhitelisted == null)
            {
                UnturnedChat.Say(caller, Main.Instance.Translate("DeWhitelistError"), UnturnedChat.GetColorFromName(Main.Config.MessageColors.ErrorMessageColor, Color.red), true);
                return;
            }

            if (Main.Instance.approvals.ContainsKey(unwhitelister.CSteamID.m_SteamID) && Main.Instance.approvals[unwhitelister.CSteamID.m_SteamID].Contains(unwhitelisted.CSteamID.m_SteamID))
            {
                Main.Instance.approvals[unwhitelister.CSteamID.m_SteamID].Remove(unwhitelisted.CSteamID.m_SteamID);
                UnturnedChat.Say(caller, Main.Instance.Translate("WhitelistRemoved", unwhitelisted.CharacterName), UnturnedChat.GetColorFromName(Main.Config.MessageColors.AcceptMessageColor, Color.cyan), true);
                Managers.WebhookManager.BuilderUnwhitelisted(unwhitelister, unwhitelisted);
                unwhitelisted.Player.ServerShowHint(Main.Instance.Translate("Unwhitelisted", unwhitelister.DisplayName), 1);
            }
            else
            {
                UnturnedChat.Say(caller, Main.Instance.Translate("DeWhitelistError"), UnturnedChat.GetColorFromName(Main.Config.MessageColors.ErrorMessageColor, Color.red), true);
            }
        }
    }
}
