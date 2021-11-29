# Prismatic Helper

## Entities

### Multi-lock door
Multi-lock doors are doors that require multiple keys (at least one) to unlock. Keys are only consumed when all of the required keys are collected by the player, and the player is within 60 pixels of the door and has a line of sight to its centre.

## Cutscenes
Prismatic Helper allows you to use custom cutscene triggers in dialog, which allow you to include some fancy effects in your regular dialog. The available triggers are limited to what is defined by Prismatic Helper (or other mods that add triggers), and more complex cutscenes may require Lua Cutscenes. Additionally, these triggers are not currently called when skipping the cutscene, so you should avoid including gameplay effects for now.

The syntax for custom triggers is `{ph_trigger id param_1 param_2 ... param_n}`, where `id` is the ID of the trigger you're running, and the parameters are space seperated strings. You may specify only as many paramaters as you want to use; all triggers have default values for all parameters. All parameters are positional; to specify a parameter, you must specify all parameters before it.

Example: `{ph_trigger camera_zoom}` is equivalent to `{ph_trigger camera_zoom 2 0.05}`.

### Triggers:

#### `walk`, `amount = 8`
Make the player walk the specified number of pixels to the right. 1 tile = 8 pixels, negative values go left.

#### `run`, `amount = 8`
Make the player run the specified number of pixels to the right. 1 tile = 8 pixels, negative values go left.

#### `look`, `direction = left`
Make the player look in the specified direction, either `left` or `right`. Other values are assumed to be right.

#### `camera_zoom`, `zoom = 2`, `speed = 0.05`
Zooms the camera to the specified zoom level, with higher values zooming in. Zooming out further than `1` won't render outside of the regular screen size. `speed` controls the speed at which the camera zooms.  

Note that the camera will not automatically zoom to the normal scale after the cutscene; you will have to reset it yourself.

#### `camera_pan`, `x = 0`, `y = 0`
Pans the camera by the specified number of pixels on the X and Y axis relative to its current position.

#### `camera_pan_to`, `x = 0`, `y = 0`
Pans the camera to the specified X and Y position.

#### `attach_camera_to_player`
Reattaches the camera to the player after panning the camera. This is done automatically when a cutscene ends, and this is only needed when moving the player after panning the camera.

#### `player_animation`, `anim = idle`, `mode = start`
Makes the player play the specified animation. See `Sprites.xml` for the available animation. If `mode` is `play`, the cutscene waits for the animation to end; otherwise, the animation continues while other triggers and dialog occurs.

### `player_inventory`, `inventory = Default`
Sets the player's inventory to the specified inventory, from: `Default`, `CH6End`, `Core`, `OldSite`, `Prologue`, `TheSummit` and `Farewell`. Note that the player's inventory is *not* currenty set if the player skips the cutscene, and you will want to use an inventory trigger if the chosen inventory affects gameplay.