using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BuilderModesV2.Commands
{
    public class CommandWhitelisted : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "BWhitelisted";

        public string Help => "";

        public string Syntax => "";

        public List<string> Aliases => new();

        public List<string> Permissions => new() { "BuilderModes.Whitelisted", "Whitelisted" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = caller as UnturnedPlayer;
            if (!Main.Instance.approvals.ContainsKey(player.CSteamID.m_SteamID))
            {
                UnturnedChat.Say(caller, Main.Instance.Translate("NoWhitelistError"), UnturnedChat.GetColorFromName(Main.Instance.Configuration.Instance.MessageColors.ErrorMessageColor, Color.red), true);
                return;
            }
            Main.Instance.approvals.TryGetValue(player.CSteamID.m_SteamID, out var builderList);

            string builderNames = string.Join(", ", builderList);
            string msg = $"{Main.Instance.Translate("WhitelistList")} {builderNames}";

            UnturnedChat.Say(caller, msg.TrimEnd(',', ' '), UnturnedChat.GetColorFromName(Main.Instance.Configuration.Instance.MessageColors.AcceptMessageColor, Color.cyan), true);
        }
    }
}
