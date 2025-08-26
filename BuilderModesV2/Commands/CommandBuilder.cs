using Rocket.API;
using Rocket.Unturned.Player;
using System.Collections.Generic;
using Rocket.Unturned.Chat;
using UnityEngine;

namespace BuilderModesV2.Commands
{
    public class CommandBuilder : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "Builder";

        public string Help => "Allows access to Builder";

        public string Syntax => "";

        public List<string> Aliases => new() { "B" };

        public List<string> Permissions => new() { "BuilderModes.Builder", "Builder", "BuilderModes.Community" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = caller as UnturnedPlayer;
            string mode = player.HasPermission(Main.Instance.Configuration.Instance.Modes.CommunityBuilder.Permission) ? "community" : "private";

            if (Main.Instance.AntiPvPOn.Contains(player))
            {
                player.Player.look.sendWorkzoneAllowed(false);
                player.Player.look.sendFreecamAllowed(false);
                UnturnedChat.Say(player, Main.Instance.Translate("AntiPvP"), UnturnedChat.GetColorFromName(Main.Instance.Configuration.Instance.MessageColors.DisabledMessageColor, Color.red));
            }

            if (Main.Instance.BuilderOn.Contains(player))
            {
                player.Player.look.sendWorkzoneAllowed(false);
                player.Player.look.sendFreecamAllowed(false);
                Managers.WebhookManager.BuilderDisabled(player, mode);
                UnturnedChat.Say(player, Main.Instance.Translate("BuilderDisabled"), UnturnedChat.GetColorFromName(Main.Instance.Configuration.Instance.MessageColors.DisabledMessageColor, Color.red));
                Main.Instance.BuilderOn.Remove(player);
            }
            else
            {
                player.Player.look.sendWorkzoneAllowed(true);
                bool autoFreecam = mode == "community"
                    ? Main.Instance.Configuration.Instance.Modes.CommunityBuilder.AutoFreecam
                    : Main.Instance.Configuration.Instance.Modes.PrivateBuilder.Settings.AutoFreecam;

                if (autoFreecam)
                {
                    player.Player.look.sendFreecamAllowed(true);
                }

                UnturnedChat.Say(player, Main.Instance.Translate("BuilderEnabled"), UnturnedChat.GetColorFromName(Main.Instance.Configuration.Instance.MessageColors.EnabledMessageColor, Color.cyan));
                Managers.WebhookManager.BuilderEnabled(player, mode);
                Main.Instance.BuilderOn.Add(player);
            }
        }
    }
}
