using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using UnityEngine;

namespace BuilderModesV2.Commands
{
    internal class CommandFreecam : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "Freecam";

        public string Help => "Allows access to Freecam";

        public string Syntax => "";

        public List<string> Aliases => new() { "FC" };

        public List<string> Permissions => new() { "Buildermodes.Freecam", "Freecam" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = caller as UnturnedPlayer;

            bool isFreecamEnabled = Main.Instance.FreecamOn.Contains(player);
            player.Player.look.sendFreecamAllowed(!isFreecamEnabled);

            if (isFreecamEnabled)
            {
                Managers.WebhookManager.FreecamDisabled(player);
                UnturnedChat.Say(caller, Main.Instance.Translate("FreecamDisabled"), UnturnedChat.GetColorFromName(Main.Instance.Configuration.Instance.MessageColors.DisabledMessageColor, Color.red), true);
                Main.Instance.FreecamOn.Remove(player);
            }
            else
            {
                Managers.WebhookManager.FreecamEnabled(player);
                UnturnedChat.Say(caller, Main.Instance.Translate("FreecamEnabled"), UnturnedChat.GetColorFromName(Main.Instance.Configuration.Instance.MessageColors.EnabledMessageColor, Color.cyan), true);
                Main.Instance.FreecamOn.Add(player);
            }
        }

    }
}
