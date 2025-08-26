# BuilderModes
A relatively simple plugin I made for a few servers I was developer on. It allows you to limit admin edit to certain permissions and adds a lot of customization.
## Features
- Private Builder Mode | Only allows the player to edit their own buildings 
- Community Builder Mode | Normal admin edit
- Whitelist System | Players can run a command to let a private builder edit their buildings
- Buildable Blacklist System | Easy to use blacklists + an ID based blacklist. Private and Community have seperate blacklists
- Blacklist Bypass System | Only works for the ID based blacklist but you can make a permission that bypasses a set blacklist
- AntiPVP System | Turns all builder tools off for X seconds if you get damaged
- Freecam and F7 Commands
- Webhook Logging | Every command, buildable move, etc can be logged to discord
- Custom Message Colors
- Builder List Command | Lists all active builders
- Config Reloader | Reload the config without restarting the server
## Commands
| Command | Description | Default Permission |
| --- | --- | --- |
| Builder | Turns On/Off Builder Mode | "BuilderModes.Builder", "BuilderModes.Community", "Builder", "Community" |
| Freecam | Turns On/Off Freecam Mode | "BuilderModes.Freecam", "Freecam" |
| Spectate | Turns On/Off Spectate/F7 Mode | "BuilderModes.Spectate", "Spectate" |
| ActiveBuilders | Tells you who is actively using buildermode | "BuilderModes.ActiveBuilders", "ActiveBuilders" |
| Whitelist | Whitelists a builder so they can edit your buildables | "BuilderModes.Whitelist", "Whitelist" |
| Whitelisted | Tells you who you have whitelisted | "BuilderModes.Whitelisted", "Whitelisted" |
| UnWhitelist | UnWhitelists a builder in your whitelist | "BuilderModes.Whitelist", "Whitelist" |
| BMConfigReload | Reloads the plugin's config file | "BuilderModes.ConfigReload" |
> [!NOTE]
> You must use the respective F keys to use Builder, Freecam, and Spectate after running the commands. It does not automatically turn them on
