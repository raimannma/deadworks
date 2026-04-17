# Item Rotation Plugin

A plugin for Deadlock that assigns each player a set of items and rotates them on a timer.

## How It Works

When the game is started via `/ir_start`, every connected player has their items cleared and receives a set of items from the config. On a configurable interval, the item sets rotate — old items are removed and new ones are given. Each player cycles through the item sets either sequentially or randomly.

Late joiners are automatically assigned a set when they connect during an active game.

When items rotate, players can optionally receive a HUD announcement and a sound effect. Both are configurable.

## Chat Commands

| Command | Description |
|---------|-------------|
| `/ir_start` | Start the item rotation game |
| `/ir_swap` | Force an immediate item rotation without waiting for the timer |
| `/ir_reset` | Stop the game and remove all items from players |
| `/ir_sets` | Print all configured item sets to chat |

## Config

The config file is managed by the framework and is automatically created at `configs/ItemRotationPlugin/ItemRotationPlugin.jsonc` on first load.

```jsonc
{
  "ConfigVersion": 1,
  "swapIntervalSeconds": 60,
  "selectionMode": "sequential",
  "allowDuplicateSets": true,
  "showRotationAnnouncement": true,
  "announcementTitle": "Items Changed!",
  "announcementDescription": "<item_set_name>",
  "playRotationSound": true,
  "rotationSound": "Mystical.Piano.AOE.Warning",
  "itemSets": [
    { "name": "Speed Demons", "items": ["upgrade_sprint_booster", "upgrade_kinetic_sash", "upgrade_improved_stamina"] },
    { "name": "Cardio Kings", "items": ["upgrade_fleetfoot_boots", "upgrade_cardio_calibrator", "upgrade_improved_stamina"] }
  ]
}
```

### Config Values

| Key | Type | Default | Description |
|-----|------|---------|-------------|
| `swapIntervalSeconds` | int | `60` | Time in seconds between each item set rotation (minimum 1) |
| `selectionMode` | string | `"sequential"` | How players progress through sets. `"sequential"` goes in order (set 1 -> 2 -> 3 -> ...), `"random"` picks a random set each rotation |
| `allowDuplicateSets` | bool | `true` | Whether multiple players can have the same item set at the same time. If `false` and there are more players than sets, `/ir_start` will fail with an error |
| `showRotationAnnouncement` | bool | `true` | Whether to show a HUD game announcement to each player when items rotate |
| `announcementTitle` | string | `"Items Changed!"` | Title text displayed in the HUD announcement. Supports `<item_set_name>` placeholder |
| `announcementDescription` | string | `"<item_set_name>"` | Description text displayed in the HUD announcement. Supports `<item_set_name>` placeholder — it will be replaced with the name of the item set the player received |
| `playRotationSound` | bool | `true` | Whether to play a sound effect on each player when items rotate |
| `rotationSound` | string | `"Mystical.Piano.AOE.Warning"` | The sound event name to play on rotation |
| `itemSets` | array | 6 default sets | List of item sets. Each set has a `name` (display name) and an `items` array of Deadlock item class names |

### Item Set Object

| Key | Type | Description |
|-----|------|-------------|
| `name` | string | Display name for the item set, shown in chat messages and used in the `<item_set_name>` announcement placeholder |
| `items` | array | List of Deadlock item class names (e.g. `"upgrade_sprint_booster"`) |
