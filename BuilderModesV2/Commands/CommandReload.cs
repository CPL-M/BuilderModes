using Rocket.API;
using Rocket.Unturned.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BuilderModesV2.Commands
{
    public class CommandReload : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;

        public string Name => "BMConfigReload";

        public string Help => "";

        public string Syntax => "";

        public List<string> Aliases => new() { "BMCR" };

        public List<string> Permissions => new() { "BuilderModes.ConfigReload" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            Main.Instance.Configuration.Load();
            UnturnedChat.Say(caller, "[BuilderModes] Reloaded Config File", UnturnedChat.GetColorFromName(Main.Instance.Configuration.Instance.MessageColors.EnabledMessageColor, Color.green), true);
            Managers.ConfigManager.UpdateConfig();
        }
    }
}
