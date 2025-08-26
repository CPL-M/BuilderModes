using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

namespace BuilderModesV2.Commands
{
    internal class CommandSpectate : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "Spectate";

        public string Help => "Allows access to spectate/F7";

        public string Syntax => "";

        public List<string> Aliases => new() { "S", "F7" };

        public List<string> Permissions => new() { "BuilderModes.Spectate", "Spectate" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = caller as UnturnedPlayer;

            bool isSpectating = Main.Instance.F7On.Contains(player);
            player.Player.look.sendSpecStatsAllowed(!isSpectating);

            if (isSpectating)
            {
                Managers.WebhookManager.SpectateDisabled(player);
                UnturnedChat.Say(caller, Main.Instance.Translate("SpectateDisabled"), UnturnedChat.GetColorFromName(Main.Instance.Configuration.Instance.MessageColors.DisabledMessageColor, Color.red), true);
                Main.Instance.F7On.Remove(player);
            }
            else
            {
                Managers.WebhookManager.SpectateEnabled(player);
                UnturnedChat.Say(caller, Main.Instance.Translate("SpectateEnabled"), UnturnedChat.GetColorFromName(Main.Instance.Configuration.Instance.MessageColors.EnabledMessageColor, Color.cyan), true);
                Main.Instance.F7On.Add(player);
            }
        }

    }
}
