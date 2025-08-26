using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BuilderModesV2.Commands
{
    public class CommandBuilderList : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "ActiveBuilders";

        public string Help => "Shows how many people are actively using builder";

        public string Syntax => "";

        public List<string> Aliases => new() { "Builders" };

        public List<string> Permissions => new() { "BuilderModes.ActiveBuilders", "ActiveBuilders" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            if (!Main.Instance.BuilderOn.Any())
            {
                UnturnedChat.Say(caller, Main.Instance.Translate("NoBuildersError"), UnturnedChat.GetColorFromName(Main.Instance.Configuration.Instance.MessageColors.ErrorMessageColor, Color.red), true);
                return;
            }

            string builderNames = string.Join(", ", Main.Instance.BuilderOn.Select(player => player.CharacterName));
            string msg = $"{Main.Instance.Translate("BuildersList")} {builderNames}";

            UnturnedChat.Say(caller, msg.TrimEnd(',', ' '), UnturnedChat.GetColorFromName(Main.Instance.Configuration.Instance.MessageColors.AcceptMessageColor, Color.cyan), true);
        }
    }
}
